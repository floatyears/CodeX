using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandFrameManager : CModule {

	//记录了服务器返回的
	private Dictionary<int, List<EntityCommand>> commandFrames;

	private	List<EntityCommand> exeCommand;

	private static CommandFrameManager instance;

	public static CommandFrameManager Instance{
		get{
			return instance;
		}
	}

	public override void Init()
	{
		instance = this;

	}
	
}
