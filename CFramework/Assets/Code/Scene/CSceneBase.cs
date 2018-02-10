using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSceneBase {

	private string name;

	private Camera camera;

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
		state = CSceneState.Inited;

		camera = Camera.main;
	}

	public virtual void OnLoaded()
	{

	}

	public virtual void Update()
	{
		camera.transform.position = CDataModel.GameState.ClActive.snap.playerState.origin;
	}

	public virtual void Dispose()
	{
		//data = null;
		state = CSceneState.Deactive;
	}
}
