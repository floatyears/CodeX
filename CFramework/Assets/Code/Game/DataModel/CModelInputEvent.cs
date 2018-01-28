using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;

//所有的输入事件，比如网络消息和用户输入。
public class CModelInputEvent : CModelBase {

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

	private int[] mouseDx;

	private int[] mouseDy;

	private int mouseIndex;

	private int[] joystickAxis;

	private KButton[] inButtons;


	// Use this for initialization
	public override void Init () {
		pushedEvents = new SysEvent[CConstVar.MAX_PUSHED_EVENTS];
		eventQueue = new SysEvent[CConstVar.MAX_PUSHED_EVENTS];

		inButtons = new KButton[16];
		for (int i = 0; i < 16; i++)
		{
			inButtons[i] = new KButton();	
		}
		joystickAxis = new int[CConstVar.MAX_JOYSTICK_AXIS];
		mouseDx = new int[2];
		mouseDy = new int[2];
		mouseIndex = 0;

		inSpeed = new KButton();
		inRight = new KButton();
		inLeft = new KButton();
		inForward = new KButton();
		inBack = new KButton();
		inLookdown = new KButton();
		inLookup = new KButton();
		inStrafe = new KButton();

		update = Update;
	}

	public void Update()
	{

		//EventLoop:
		int time = (int)(Time.realtimeSinceStartup*1000);
		
		ProcessMouseKeyEvent();
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
					if(Server.Instance.ServerRunning){
						server.RunServerPacket(remote, packet);
					}
				}
				break;
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

		//Send CMD:
		if(CDataModel.Connection.state < ConnectionState.CONNECTED){
			return;
		}
		CreateNewUserCommands();
		if(!CNetwork.Instance.ReadyToSendPacket()){
			if(CConstVar.ShowNet > 0){
				// CLog.Info("send no msg.");
			}
			return;
		}
		CNetwork.Instance.WritePacket();
		
	}
	
	// Update is called once per frame
	public override void Dispose () {
		
	}

	private void CreateNewUserCommands(){
		int cmdNum;

		if(CDataModel.Connection.state < ConnectionState.PRIMED){
			return;
		}

		var cl = CDataModel.GameState.ClActive;
		cl.cmdNum++;
		cmdNum = cl.cmdNum & CConstVar.CMD_MASK;
		cl.cmds[cmdNum] = CreateCmd();
	}

	private UserCmd CreateCmd(){
		UserCmd cmd = new UserCmd();
		var clientActive = CDataModel.GameState.ClActive;
		Vector3 oldAngles = clientActive.viewAngles;

		// AdjustAngles();

		KeyMove(ref cmd);
		cmd.serverTime = clientActive.serverTime;

		return cmd;
	}

	private void KeyMove(ref UserCmd cmd){
		int moveSpeed = 0;
		if(inSpeed.active){
			moveSpeed = 127;
			cmd.buttons &= ~16;//BUTTON_WALKING
		}else{
			cmd.buttons |= 16;
			moveSpeed = 64;
		}

		int forward = 0;
		int side = 0;
		int up = 0;
		// if(inStrafe.active){
		// 	side += (int)(moveSpeed * KeyState(inRight));
		// 	side -= (int)(moveSpeed * KeyState(inLeft));
		// }

		side += (int)(moveSpeed * KeyState(inRight));
		side -= (int)(moveSpeed * KeyState(inLeft));
		// if(side != 0){
		// 	CLog.Info("move side : {0}", side);
		// }

		cmd.forwardmove = CUtils.ClampChar(forward);
		cmd.rightmove = CUtils.ClampChar(side);
		cmd.upmove = CUtils.ClampChar(up);
	}

	private void AdjustAngles(){
		float speed;

		if(inSpeed.active){
			speed = 0.001f * CDataModel.GameState.frameTime * CConstVar.AnglesSpeedKey;
		}else{
			speed = 0.001f * CDataModel.GameState.frameTime;
		}

		var clientActive = CDataModel.GameState.ClActive;
		if(!inStrafe.active){
			clientActive.viewAngles[CConstVar.YAW] -= speed * CConstVar.YawSpeed * KeyState(inRight);
			clientActive.viewAngles[CConstVar.YAW] += speed * CConstVar.YawSpeed * KeyState(inLeft);
		}

		clientActive.viewAngles[CConstVar.PITCH] -= speed * CConstVar.PitchSpeed * KeyState(inLookup);
		clientActive.viewAngles[CConstVar.PITCH] += speed * CConstVar.PitchSpeed * KeyState(inLookdown);
	}

	private float KeyState(KButton key){
		int msec = key.msec;
		key.msec = 0;

		int time = CDataModel.GameState.realTime;
		if(key.active){
			//仍然按下
			if(key.downtime == 0){
				msec = time;
			}else{
				msec += time - key.downtime;
			}
			key.downtime = time;
		}

		float val = (float)msec / CDataModel.GameState.frameTime;
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

	public int TimVal(int minMsec){
		int timeVal = Milliseconds() - CDataModel.GameState.frameTime;
		if(timeVal >= minMsec){
			return 0;
		}else{
			return minMsec - timeVal;
		}
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
				// File.WriteAllText(CPath.demoPath,"test");
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

	//这里应该是用通用的字符串来解析指令，这样就可以处理所有的输入事件而不只是键盘鼠标的指令。
	private void ProcessMouseKeyEvent(){
		if(Input.GetMouseButton(1))
		{
			mouseIndex = 1;
			mouseDx[mouseIndex] = 1;
			mouseDx[mouseIndex] = 1;
			// QueueEvent(time, SysEventType.MOUSE, 0, 0);
		}

		if(Input.GetKeyDown(KeyCode.A)){
			inLeft.down[0] = (int)KeyCode.A;
			KeyDown(inLeft);
		}else if(Input.GetKeyUp(KeyCode.A)){
			inLeft.down[0] = (int)KeyCode.A;
			KeyUp(inLeft);
		}

		if(Input.GetKeyDown(KeyCode.D)){
			inLeft.down[0] = (int)KeyCode.D;
			KeyDown(inRight);
		}else if(Input.GetKeyUp(KeyCode.D)){
			inLeft.down[0] = (int)KeyCode.D;
			KeyUp(inRight);
		}

	}

	private void KeyDown(KButton b)
	{
		if(b.active){ //一直按着
			return;
		}

		b.downtime = Milliseconds();
		b.active = true;
		b.wasPressed = true;
	}

	private void KeyUp(KButton b){
		b.active = false;

		int t = Milliseconds();
		if(t > 0){
			b.msec += t - b.downtime;
		}else{
			b.msec += CDataModel.GameState.frameTime / 2;
		}
		b.active = false;
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

public class KButton{

	//当前
	public int[] down;

	public int downtime;

	public int msec;

	public bool active;

	public bool wasPressed;

	public KButton(){
		down = new int[2];
	}
}