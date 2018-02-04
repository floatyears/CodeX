using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSceneBase {

	private string name;

	public string Name{
		get{
			return name;
		}
	}

	private CSceneState state;

	public CSceneState State{
		get{
			return state;
		}
		set{
			state = value;
		}
	}

	public virtual void Init()
	{
		state = CSceneState.None;
	}

	public virtual void OnLoaded()
	{
		
	}

	public virtual void Dispose()
	{
		//data = null;
		state = CSceneState.Disposed;
	}
}
