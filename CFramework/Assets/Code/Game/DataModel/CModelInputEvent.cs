﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

//所有的输入事件，比如网络消息和用户输入。
public class CModelInputEvent : IModel {

	private int pushedEventsHead;
	
	private int pushedEventsTail;

	private SysEvent[] pushedEvents;

	private int eventHead;

	private int eventTail;

	private SysEvent[] eventQueue;

	// Use this for initialization
	public void Init () {
		pushedEvents = new SysEvent[CConstVar.MAX_PUSHED_EVENTS];
		eventQueue = new SysEvent[CConstVar.MAX_PUSHED_EVENTS];
	}

	public void Update()
	{
		int time = (int)(Time.realtimeSinceStartup*1000);
		if(Input.GetMouseButton(1))
		{
			QueueEvent(time, SysEventType.MOUSE, 0, 0);
		}

		SysEvent ev;
		//在获取到一个空事件之前，一直获取事件，并将它们添加到队列中
		do{
			ev = GetRealEvent();
			if(ev.eventType != SysEventType.NONE){
				PushEvent(ev);
			}

		}while(ev.eventType != SysEventType.NONE);
	}
	
	// Update is called once per frame
	public void Dispose () {
		
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