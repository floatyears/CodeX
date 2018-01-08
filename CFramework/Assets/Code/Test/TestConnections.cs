using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestConnections : MonoBehaviour {

	private static string log;

	private GUIStyle logStyle;

	// Use this for initialization
	void Start () {
		logStyle = new GUIStyle();
		logStyle.border = new RectOffset(0,300,0,300);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void AddLog(string content){
		log += content + "\n";
	}



	private void OnGUI() {
		if(GUILayout.Button("刷新服务器")){
			CDataModel.GameState.LocalServers();
		}
		if(GUILayout.Button("清空")){
			log = "";
		}
		GUILayout.TextArea(log);
	}
}
