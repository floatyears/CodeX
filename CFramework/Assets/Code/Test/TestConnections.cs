using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class TestConnections : MonoBehaviour {

	private static string log;

	private GUIStyle logStyle;

	// private System.Net.Sockets.UdpClient udp;
	private UdpClient udp;
	

	// Use this for initialization
	void Start () {
		logStyle = new GUIStyle();
		logStyle.border = new RectOffset(0,300,0,300);

		// udp = new System.Net.Sockets.UdpClient(CConstVar.SERVER_PORT, AddressFamily.InterNetwork);
		udp = new UdpClient(CConstVar.SERVER_PORT, AddressFamily.InterNetwork);
		udp.BeginReceive((ar)=>{
			IPEndPoint remote = new IPEndPoint(IPAddress.Any, CConstVar.SERVER_PORT);
			var count = udp.EndReceive(ar, ref remote);
			log += "receive :" + count.Length + " adr:" + remote.Address.ToString();
		},null);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void AddLog(string content){
		log += content + "\n";
	}



	private void OnGUI() {
		if(GUILayout.Button("刷新服务器",GUILayout.Height(200), GUILayout.Width(300))){
			CDataModel.GameState.LocalServers();
		}
		if(GUILayout.Button("清空",GUILayout.Height(200), GUILayout.Width(300))){
			log = "";
		}
		GUILayout.TextArea(log);
	}
}
