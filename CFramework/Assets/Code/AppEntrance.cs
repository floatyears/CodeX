using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppEntrance : MonoBehaviour {

	private CGameCore gameCore;

	// Use this for initialization
	void Start () {
		gameCore = new CGameCore();
		gameCore.Init();
	}
	
	// Update is called once per frame
	void Update () {
		gameCore.Update();
	}
}
