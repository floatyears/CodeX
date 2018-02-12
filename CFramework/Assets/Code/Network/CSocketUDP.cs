using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;

public class CSocketUDP {

	private IPAddress ip;
	
	private int port;

	private Socket recvSocket;

	private Socket sendSocket;

	private byte[] buffer;

	private byte[] sendBuffer;

	private int socketArgsLimit = 2;

	// private MsgPacket[] packetBuffer;
	public CircularBuffer<MsgPacket> packetBuffer;

	private int curPacket;

	// private IPEndPoint remoteEP;

	private IPEndPoint destEP;

	// private HuffmanMsg huffmanMsg;

	public void Init()
	{
		recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		// recvSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
		
		sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		// sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
		if(CConstVar.IsLocalNetwork){
			sendSocket.EnableBroadcast = true;
			recvSocket.EnableBroadcast = true;
		}
		recvSocket.Blocking = false;
		sendSocket.Blocking = false;		
		
		buffer = new byte[CConstVar.BUFFER_LIMIT];
		sendBuffer = new byte[CConstVar.BUFFER_LIMIT];
		var localEP = new IPEndPoint(IPAddress.Any, CConstVar.SERVER_PORT); //可以接受任何ip和端口的消息
		
		for(int i =0; i < 10; i++){
			try{
				// CConstVar.LocalPort = 
				localEP.Port = CConstVar.SERVER_PORT + i;
				recvSocket.Bind(localEP);

				// OpenSocks(CConstVar.LocalPort);
				break;
			}catch(Exception e){

			}
		}
		
		// packetBuffer = new MsgPacket[2];
		packetBuffer = new CircularBuffer<MsgPacket>(30);
		// recvSocket.Listen();

		// remoteEP = new IPEndPoint(IPAddress.Any, 0);
		// IPAddress.IsLoopback(remoteEP.Address);
		curPacket = 0;

		HuffmanMsg.Init();
	}

	public void Dispose()
	{

	}

	public int GetLocalPort()
	{
		if(recvSocket != null){
			return (recvSocket.LocalEndPoint as IPEndPoint).Port;
		}else{
			return 0;
		}
	}

	public void OpenSocks(int port)
	{
		// this.ip = IPAddress.Parse(ip);
		// this.port = port;
		//socket.Connect(new IPEndPoint(this.ip, this.port));
		// destEP = new IPEndPoint(this.ip, this.port);
		// sendSocket.Bind(destEP);
		var server = IPAddress.Parse(CConstVar.ServerAddress);

		recvSocket.Connect(server, CConstVar.SERVER_PORT);
	}

	public void BeginReceive()
	{
		var tmpEP = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
		recvSocket.BeginReceiveFrom(buffer, 0, CConstVar.BUFFER_LIMIT, SocketFlags.None, ref tmpEP, ReceiveCallback, null);
	}

	//异步的实现，C#会自动用多线程来处理（此处需要对多线程进行判断）
	//TODO:因为多线程的关系，线程外的数据不是安全的
	private void ReceiveCallback(IAsyncResult ar)
	{
		var tmpEP = new IPEndPoint(IPAddress.Any,0);
		EndPoint tmp = tmpEP as EndPoint;
		int count = recvSocket.EndReceiveFrom(ar, ref tmp);
		// IPEndPoint _remote = ar.AsyncState as IPEndPoint;
		if(count > 0)
		{
			// CLog.Info("udp socket recieved: {0}, from adr:{1}", count, tmp);
			
#if CFRAMEWORK_DEBUG
			try{
#endif
				var connection = CDataModel.Connection;
				// if(connection.state < ConnectionState.CONNECTED)
				// {
				// 	return; //网络还没连上，不处理消息
				// }
				//随机丢包处理
				if(CConstVar.NET_DROP_SIM > 0 && CConstVar.NET_DROP_SIM < 100 && CUtils.Random() < CConstVar.NET_DROP_SIM)
				{
					//已经丢掉的包，不处理
				}else
				{
					// TODO: 线程的关系，暂时不能用缓冲，后面再改写
					// var packet = packetBuffer[~curPacket];
					// 
					MsgPacket packet = new MsgPacket();
					packetBuffer.Enqueue(packet);
					// packet.remoteEP.Port = packet.ReadPort();
					packet.WriteBufferData(buffer, 0, count); //把缓冲内的数据写入到packet
					if(buffer[0] == 0xff || buffer[1] == 0xff || buffer[2] == 0xff || buffer[3] == 0xff){ //模拟带外数据
						packet.CurPos = 0;
						packet.remoteEP = tmp as IPEndPoint;
						// packet.remoteEP.Port = packet.ReadPort();
					}else{ //oob数据，暂时是广播的数据
						packet.CurPos = 0;
						packet.remoteEP = tmp as IPEndPoint;
						// packet.remoteEP = new IPEndPoint(IPAddress.Broadcast, 0);
						
					}
				}
#if CFRAMEWORK_DEBUG
			}catch(Exception e){
				CLog.Error("packet process error:", e.Message);
			}
#endif
			
			var ep = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
			recvSocket.BeginReceiveFrom(buffer, 0, CConstVar.BUFFER_LIMIT, SocketFlags.None, ref ep, ReceiveCallback, null);
		}else{
			CLog.Error("udp socket has recevied no byte");
		}
	}

	//network层已经做了fragment的拆分处理
	public void SendMsg(byte[] bytes, int length, IPEndPoint to){
		if(CConstVar.ShowNet > 0) CLog.Info("send msg, length {0}, to {1}", length, to);
		if(to.Address == IPAddress.Broadcast){ //直接发送数据
			//Array.Copy(bytes,0,sendBuffer,0,length);
			sendSocket.BeginSendTo(bytes, 0, length, SocketFlags.None, to, SendCallback, sendSocket);
			
		}else{ //这里也不需要进行处理
			// sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName)
			// sendBuffer[0] = 0;
			// sendBuffer[1] = 0;
			// sendBuffer[2] = 0;
			// sendBuffer[3] = 1; //IP

			// Array.Copy(to.Address.GetAddressBytes(),0,sendBuffer,4, 4);
			// sendBuffer[8] = (byte)(CConstVar.Qport >> 8); //发送的是自己监听的端口
			// sendBuffer[9] = (byte)(CConstVar.LocalPort);
			// Array.Copy(bytes,0,sendBuffer, 10, length); //前10个字节是保留的
			// Array.Copy(bytes, sendBuffer, length);
			// sendSocket.BeginSendTo(sendBuffer, 0, length + 10, SocketFlags.None, to, SendCallback, sendSocket);
			sendSocket.BeginSendTo(bytes, 0, length, SocketFlags.None, to, SendCallback, sendSocket);
		}
	}


	private void SendCallback(IAsyncResult ar){
		int count = sendSocket.EndSendTo(ar);
		if(count > 0){
			CLog.Info("Send Packet Finish. length: {0}, rmeote addr:{1}", count, recvSocket.RemoteEndPoint);
		}else{
			CLog.Info("send zero bytes");
		}
	}
	
}



public enum SVCCmd
{
	BAD = 0,
	NOP,
	GAME_STATE,
	CONFIG_STRING,
	BASELINE,
	SERVER_COMMAND,
	DOWNLOAD,
	SNAPSHOT,
	EOF,
}

public enum CLC_Cmd{
	BAD = 0,
	NOP,
	MOVE,
	MoveNoDelta,
	ClientCommand,
	EOF,
}

