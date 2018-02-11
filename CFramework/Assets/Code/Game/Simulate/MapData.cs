using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData {

	private GameObject mapRoot;

	private Transform mapRootTrans;

	public void Init(){
		var res = Resources.Load("map") as GameObject;
		mapRoot = GameObject.Instantiate(res);
		CUtils.SetLayer(mapRoot, LayerMask.NameToLayer("Simulate"));

		mapRootTrans = mapRoot.transform;
	}

	public Vector3 GetSpawnPoint(int teamID){
		return mapRootTrans.Find("spawn").position;
	}

	public Vector3 SpectatorSpawnPoint(Vector3 spawnOrigin, Vector3 spawnAngles){
		return mapRootTrans.Find("spawn").position;
	}

	public Vector3 InitialSpawnPoint(Vector3 spawnOrigin, Vector3 spawnAngles){
		return mapRootTrans.Find("spawn").position;
	}

	public Vector3 SpawnPoint(Vector3 spawnOrigin, Vector3 spawnAngles){
		return mapRootTrans.Find("spawn").position;
	}
}
