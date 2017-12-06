using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSceneBase {

	private CSceneState state;

	private CTableScene data;



	public CSceneState State{
		get{
			return state;
		}
		set{
			state = value;
		}
	}

	public CTableScene Data{
		get{
			return data;
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
