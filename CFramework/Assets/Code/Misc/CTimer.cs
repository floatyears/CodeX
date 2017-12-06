using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CTimer : CModule {

	private List<TimerItem> timers;

	private int updateCount;	

	private static CTimer instance;

	


	public static CTimer Instance{
		get
		{
			if(instance == null) throw new UnityException("CDatabase is not inited!");
			return instance;
		}
	}


	public override void Init()
	{
		instance = new CTimer();
	}


	public void AddTimer(Action action, float time)
	{
		timers.Add(new TimerItem(action, time));
	}

	public void AddTimer(Action action, float interval, int times = 0)
	{
		timers.Add(new TimerItem(action, interval, times));
	}

	public override void Update()
	{
		float deltaTime = Time.deltaTime;
		updateCount = timers.Count - 1;
		for(int i = updateCount; i >= 0 ; i--)
		{
			var timer = timers[i];
			if(timer.times > 0 )
			{
				timer.delay -= deltaTime;
				if(timer.delay < 0)
				{
					timer.times -= 1;
					timer.action();
				}
			}else
			{
				timers.RemoveAt(i);
			}
			
		}

		
	}

}

internal class TimerItem{

	public Action action;

	public float delay;

	public int times;

	public TimerItem(Action action, float delay, int times = 1)
	{
		this.action = action;
		this.delay = delay;
		this.times = times;
	}

}
