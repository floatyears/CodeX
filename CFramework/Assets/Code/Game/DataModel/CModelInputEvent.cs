using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;

//所有的输入事件，比如网络消息和用户输入。
public class CModelInputEvent : IModel {

	private int pushedEventsHead;
	
	private int pushedEventsTail;

	private SysEvent[] pushedEvents;

	private int eventHead;

	private int eventTail;

	private SysEvent[] eventQueue;

	private int frame_msec;

	private KButton inSpeed;

	private KButton inRight;

	private KButton inLeft;

	private KButton inForward;

	private KButton inBack;

	private KButton inLookup;

	private KButton inLookdown;

	private KButton inStrafe;

	private KButton[] inButtons;

	// Use this for initialization
	public void Init () {
		pushedEvents = new SysEvent[CConstVar.MAX_PUSHED_EVENTS];
		eventQueue = new SysEvent[CConstVar.MAX_PUSHED_EVENTS];
		
		inButtons = new KButton[16];
	}

	public void Update()
	{
		int time = (int)(Time.realtimeSinceStartup*1000);
		if(Input.GetMouseButton(1))
		{
			QueueEvent(time, SysEventType.MOUSE, 0, 0);
		}

		SysEvent ev;
		IPEndPoint remote;
		MsgPacket packet;
		var network = CNetwork.Instance;
		var server = Server.Instance;
		while(true)
		{
			ev = GetEvent();
			if(ev.eventType == SysEventType.NONE)
			{
				while(network.GetLoopPacket(NetSrc.CLIENT, out remote, out packet)){
					network.PacketEvent(packet, remote);
				}

				while(network.GetLoopPacket(NetSrc.SERVER, out remote, out packet)){
					if(CDataModel.Connection.ServerRunning){
						server.RunServerPacket(remote, packet);
					}
				}
				// return ev.eventTime;
			}

			switch(ev.eventType)
			{
				case SysEventType.KEY:
					break;
				case SysEventType.CHAR:
					break;
				case SysEventType.MOUSE:
					break;
				case SysEventType.JOYSTICK_AXIS:
					break;
				default:
					CLog.Error("Event Update: bad type %s", ev.eventType);
					break;
			}
		}

		CreateNewUserCommands();
	}
	
	// Update is called once per frame
	public void Dispose () {
		
	}

	private void CreateNewUserCommands(){
		int cmdNum;

		if(CDataModel.Connection.state < ConnectionState.PRIMED){
			return;
		}

		var cl = CDataModel.GameState.ClientActive;
		cl.cmdNum++;
		cmdNum = cl.cmdNum & CConstVar.CMD_MASK;
		cl.cmds[cmdNum] = CreateCmd();

	}

	private UserCmd CreateCmd(){
		UserCmd cmd;
		var clientActive = CDataModel.GameState.ClientActive;
		Vector3 oldAngles = clientActive.viewAngles;

		AdjustAngles();

		cmd = new UserCmd();


		return cmd;
	}

	private void AdjustAngles(){
		float speed;

		if(inSpeed.active){
			speed = 0.001f * CDataModel.GameState.frameTime * CConstVar.AnglesSpeedKey;
		}else{
			speed = 0.001f * CDataModel.GameState.frameTime;
		}

		var clientActive = CDataModel.GameState.ClientActive;
		if(!inStrafe.active){
			clientActive.viewAngles[CConstVar.YAW] -= speed * CConstVar.YawSpeed * KeyState(ref inRight);
			clientActive.viewAngles[CConstVar.YAW] += speed * CConstVar.YawSpeed * KeyState(ref inLeft);
		}

		clientActive.viewAngles[CConstVar.PITCH] -= speed * CConstVar.PitchSpeed * KeyState(ref inLookup);
		clientActive.viewAngles[CConstVar.PITCH] += speed * CConstVar.PitchSpeed * KeyState(ref inLookdown);
	}

	private float KeyState(ref KButton key){
		int msec = key.msec;
		key.msec = 0;

		int time = CDataModel.GameState.realTime;
		if(key.active){
			if(key.downtime > 0){
				msec = time;
			}else{
				msec += time - key.downtime;
			}
			key.downtime = time;
		}

		float val = (float)msec / CDataModel.GameState.deltaTime;
		if(val < 0){
			val = 0;
		}else if(val > 1){
			val = 1;
		}

		return val;
	}

	private void CmdButtons(ref UserCmd cmd){
		for(int i = 0; i < 15; i++){
			var btn = inButtons[i];
			if(btn.active || btn.wasPressed){
				cmd.buttons |= 1 << i;
			}
			btn.wasPressed = false;
		}

		// if()
	}

	public int Milliseconds()
	{
		//添加事件
		SysEvent ev;
		//在获取到一个空事件之前，一直获取事件，并将它们添加到队列中
		do{
			ev = GetRealEvent();
			if(ev.eventType != SysEventType.NONE){
				PushEvent(ev);
			}

		}while(ev.eventType != SysEventType.NONE);
		
		return ev.eventTime;
	}

	public SysEvent GetEvent()
	{
		if(pushedEventsHead > pushedEventsTail)
		{
			pushedEventsTail++;
			return pushedEvents[(pushedEventsTail - 1) & (CConstVar.MAX_PUSHED_EVENTS - 1)];
		}
		return GetRealEvent();
	}

	public SysEvent GetRealEvent()
	{
		SysEvent ev;

		int journal = CDataModel.GameState.journal;
		//要么从系统中读取事件，要么从文件中读取
		if(journal == 2)
		{
			ev = new SysEvent();
			File.ReadAllText("");
		}else{
			ev = GetSystemEvent();

			if(journal == 1) //写入到到文件
			{
				File.WriteAllText("","");
			}
		}
		return ev;
	}

	public SysEvent GetSystemEvent()
	{
		if(eventHead > eventTail)
		{
			eventTail++;
			return eventQueue[(eventTail - 1) & CConstVar.MASK_QUEUED_EVENTS];
		}

		//创建一个空的事件
		SysEvent ev = new SysEvent();
		ev.eventType = SysEventType.NONE;
		ev.eventTime = (int)(Time.realtimeSinceStartup*1000);

		return ev;
	}

	public void PushEvent(SysEvent ev)
	{
		if(pushedEventsHead - pushedEventsTail >= CConstVar.MAX_PUSHED_EVENTS)
		{
			CLog.Warning("InputEvent Push Event Overflow!");
			pushedEventsTail++;
		}

		pushedEvents[pushedEventsHead & (CConstVar.MAX_PUSHED_EVENTS - 1)] = ev;
		pushedEventsHead++;
	}

	public void QueueEvent(int time, SysEventType eventType, int value1, int value2)
	{
		SysEvent ev = eventQueue[eventHead & CConstVar.MAX_PUSHED_EVENTS];
		if(eventHead - eventTail >= CConstVar.MAX_PUSHED_EVENTS)
		{
			CLog.Warning("QueueEvent: overflow!");
			eventTail++;
		}
		eventHead++;

		if(time == 0)
		{
			time = (int)(Time.realtimeSinceStartup*1000);
		}
		// ev = new SysEvent();

		ev.eventTime = time;
		ev.eventType = eventType;
		ev.eventValue = value1;
		ev.eventValue2 = value2;
	}
}

public struct SysEvent
{
	public int eventTime;

	public SysEventType eventType;

	public int eventValue;

	public int eventValue2;

	public int eventPtrLength;

	public IntPtr eventPtr;
}

public enum SysEventType
{
	NONE = 0,

	KEY,

	CHAR,

	MOUSE,

	JOYSTICK_AXIS,
}

public struct KButton{

	public int[] down;

	public int downtime;

	public int msec;

	public bool active;

	public bool wasPressed;
}