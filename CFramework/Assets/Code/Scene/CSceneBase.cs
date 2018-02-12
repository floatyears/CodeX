using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSceneBase {

	private string name;

	private Transform camTrans;

	private ClientActive clientActive;

	private BaseEntity[] npcList;

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

		camTrans = Camera.main.transform.parent;
		npcList = new BaseEntity[CConstVar.MAX_ENTITIES_IN_SNAPSHOT];
	}

	public virtual void OnLoaded()
	{
		clientActive = CDataModel.GameState.ClActive;
	}

	public virtual void Update()
	{
		camTrans.position = CDataModel.GameState.ClActive.snap.playerState.origin;

		for(int i = 0; i < clientActive.snap.numEntities; i++){ //表示当前帧的entity
			int idx = clientActive.snap.parseEntitiesIndex + i;
			var ent = clientActive.parseEntities[idx & (CConstVar.MAX_PARSE_ENTITIES - 1)];
			
			
		}
		// clientActive.snap.parseEntitiesIndex
		// clientActive.parseEntities[clientActive.snap.parseEntitiesIndex + clientActive.snap.numEntities]
	}

	public virtual void Dispose()
	{
		//data = null;
		state = CSceneState.Deactive;
	}
}
