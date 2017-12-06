using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CModule : IDisposable {

	private string name;

	private CModuleState state;

	//是否需要每帧更新，有些Module并不需要这样的逻辑，所以可以不更新
	public bool needUpdate;
	
	public CModule()
	{
		state = CModuleState.None;
		needUpdate = false;
	}

	//模块初始化
	public virtual void Init()
	{

	}

	//模块更新
	public virtual void Update()
	{

	}

	//激活
	public virtual void Active()
	{

	}

	//
	public virtual void Deactive()
	{

	}

	//释放
	public virtual void Dispose()
	{

	}
}

public enum CModuleState
{
	//没有初始化
	None = 0,
	//初始化并激活
	Active = 1,
	//
	Deactive = 2,
	//被标记释放
	Disposed = 3,
}
