﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class TestConnections : MonoBehaviour {

	private static string log;

	private GUIStyle logStyle;

	// private System.Net.Sockets.UdpClient udp;
	private UdpClient udp;

	private bool isServer;
	

	// Use this for initialization
	void Start () {
		logStyle = new GUIStyle();
		logStyle.border = new RectOffset(0,300,0,300);

		// udp = new System.Net.Sockets.UdpClient(CConstVar.SERVER_PORT, AddressFamily.InterNetwork);
		// udp = new UdpClient(CConstVar.SERVER_PORT + 2, AddressFamily.InterNetwork);
		// System.AsyncCallback action = null;
		// action = (ar)=>{
		// 	IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
		// 	var count = udp.EndReceive(ar, ref remote);
		// 	CLog.Info("receive :{0}, adr:{1}", count.Length, remote.Address);// log += "receive :" + count.Length + " adr:" + remote.Address.ToString();
		// 	udp.BeginReceive(action, null);
		// };
		// udp.BeginReceive(action,null);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void AddLog(string content){
		log += content + "\n";
	}



	private void OnGUI() {
		if(GUILayout.Button("刷新服务器",GUILayout.Height(100), GUILayout.Width(300))){
			CDataModel.GameState.LocalServers();
		}
		if(GUILayout.Button("清空",GUILayout.Height(100), GUILayout.Width(300))){
			log = "";
		}
		if(isServer){
			if(GUILayout.Button("关闭服务器",GUILayout.Height(100), GUILayout.Width(300))){
				isServer = !isServer;
				CDataModel.Connection.ServerRunning = isServer;
			}
		}else{
			if(GUILayout.Button("开启服务器",GUILayout.Height(100), GUILayout.Width(300))){
				isServer = !isServer;
				CDataModel.Connection.ServerRunning = isServer;
			}
		}
		GUILayout.TextArea(log);
	}
}