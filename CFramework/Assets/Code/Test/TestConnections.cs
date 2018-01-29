using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Runtime.InteropServices;

public class TestConnections : MonoBehaviour {

	private static string log;

	private GUIStyle logStyle;

	// private System.Net.Sockets.UdpClient udp;
	private UdpClient udp;

	private bool isServer;
	

	// Use this for initialization
	void Start () {
		int i = 0;
		float a = 1.424f;
		i =  BitConverter.ToInt32(BitConverter.GetBytes(a), 0);
		a = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
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
		isServer = Server.Instance.ServerRunning;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void AddLog(string content){
		log += content + "\n";
	}



	private void OnGUI() {
		GUILayout.BeginHorizontal();

		GUILayout.BeginVertical(GUILayout.Width(300));
		GUILayout.Label("连接状态" + CDataModel.Connection.state);
		if(GUILayout.Button("刷新服务器",GUILayout.Height(40), GUILayout.Width(300))){
			CDataModel.GameState.GetLocalServers();
		}
		if(GUILayout.Button("重置",GUILayout.Height(40), GUILayout.Width(300))){
			Server.Instance.ClearClients();
			CDataModel.Connection.state = ConnectionState.DISCONNECTED;
		}
		if(isServer){
			if(GUILayout.Button("关闭服务器",GUILayout.Height(40), GUILayout.Width(300))){
				isServer = !isServer;
				Server.Instance.ServerRunning = isServer;
			}
		}else{
			if(GUILayout.Button("开启服务器",GUILayout.Height(40), GUILayout.Width(300))){
				isServer = !isServer;
				Server.Instance.ServerRunning = isServer;
				// HuffmanMsg.Init();
			}
		}
		GUILayout.TextArea(log);
		GUILayout.EndVertical();

		GUILayout.BeginVertical(GUILayout.Width(300));

		int count = CDataModel.GameState.localServers.Length;
		for(int i = 0; i < count; i++){
			var server = CDataModel.GameState.localServers[i];
			if(server != null){

				GUILayout.BeginHorizontal();
				GUILayout.Label("服务器：" + server.hostName, GUILayout.Width(100));
				if(CDataModel.Connection.state < ConnectionState.CHALLENGING){
					if(GUILayout.Button("Challenge",GUILayout.Width(100))){
						CDataModel.GameState.ChallengeServer(server);
					}
				}else if(CDataModel.Connection.state < ConnectionState.CONNECTED){
					if(GUILayout.Button("Connect",GUILayout.Width(100))){
						CDataModel.GameState.ConnectServer(server);
					}
				}
				GUILayout.EndHorizontal();
			}
		}

		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
}