using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;

public class CSocketUDP {

	private IPAddress ip;
	
	private int port;

	private Socket socket;

	private byte[] buffer;

	private int bufferLimit = 1024;

	private int socketArgsLimit = 2;

	private int socketArgsIndex = 0;

	private EndPoint remoteEP;

	public void Init()
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		buffer = new byte[bufferLimit];
		socketAsyncEventArgs = new SocketAsyncEventArgs[socketArgsLimit];
		remoteEP = new IPEndPoint(IPAddress.Any, 0); //可以接受任何ip和端口的消息
	}

	public void Connect(string ip, int port)
	{
		this.ip = IPAddress.Parse(ip);
		this.port = port;
		//socket.Connect(new IPEndPoint(this.ip, this.port));
		socket.Bind(new IPEndPoint(this.ip, this.port));
	}

	public void BeginReceive()
	{
		socket.BeginReceiveFrom(buffer, 0, bufferLimit, SocketFlags.None,ref remoteEP, ReceiveCallback, null);

		// if(!socket.ReceiveFromAsync(socketAsyncEventArgs[socketArgsIndex]))
		// {

		// }
	}

	private void ReceiveCallback(IAsyncResult ar)
	{
		int count = socket.EndReceive(ar);
		if(count > 0)
		{

		}else{

		}
	}

	public void Send()
	{
		// socket.SendTo();
	}
}
