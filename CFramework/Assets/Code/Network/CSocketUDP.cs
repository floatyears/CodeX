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

	private int socketArgsLimit = 2;

	private MsgPacket[] packetBuffer;

	private int curPacket;

	private IPEndPoint remoteEP;

	private IPEndPoint destEP;

	private HuffmanMsg huffmanMsg;

	public void Init()
	{
		recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		
		buffer = new byte[CConstVar.BUFFER_LIMIT];
		remoteEP = new IPEndPoint(IPAddress.Any, 0); //可以接受任何ip和端口的消息
		recvSocket.Bind(remoteEP);

		// IPAddress.IsLoopback(remoteEP.Address);
		packetBuffer = new MsgPacket[2];
		curPacket = 0;

		InitHuffmanMsg();
	}

	public void Dispose()
	{

	}
	
	public void InitHuffmanMsg()
	{
		huffmanMsg = new HuffmanMsg();
		huffmanMsg.decompresser.loc = new HuffmanNode[CConstVar.HUFF_MAX+1];
		huffmanMsg.compresser.loc = new HuffmanNode[CConstVar.HUFF_MAX+1];

		huffmanMsg.decompresser.tree = huffmanMsg.decompresser.lhead = huffmanMsg.decompresser.ltail = 
			huffmanMsg.decompresser.loc[CConstVar.HUFF_MAX] = huffmanMsg.decompresser.nodeList[huffmanMsg.decompresser.blocNode++];

		huffmanMsg.decompresser.tree.symbol = CConstVar.HUFF_MAX;
		huffmanMsg.decompresser.tree.weight = 0;
		huffmanMsg.decompresser.lhead.next = huffmanMsg.decompresser.lhead.prev = null;
		huffmanMsg.decompresser.tree.parent = huffmanMsg.decompresser.tree.left = huffmanMsg.decompresser.tree.right = null;

		huffmanMsg.compresser.tree = huffmanMsg.compresser.lhead = 
			huffmanMsg.decompresser.loc[CConstVar.HUFF_MAX] = huffmanMsg.decompresser.nodeList[huffmanMsg.decompresser.blocNode++];

		huffmanMsg.compresser.tree.symbol = CConstVar.HUFF_MAX;
		huffmanMsg.decompresser.tree.weight = 0;
		huffmanMsg.decompresser.lhead.next = huffmanMsg.decompresser.lhead.prev = null;
		huffmanMsg.decompresser.tree.parent = huffmanMsg.decompresser.tree.left = huffmanMsg.decompresser.tree.right = null;

	}

	public void Connect(string ip, int port)
	{
		this.ip = IPAddress.Parse(ip);
		this.port = port;
		//socket.Connect(new IPEndPoint(this.ip, this.port));
		destEP = new IPEndPoint(this.ip, this.port);
		sendSocket.Bind(destEP);
	}

	public void BeginReceive()
	{
		var tmpEP = remoteEP as EndPoint;
		recvSocket.BeginReceiveFrom(buffer, 0, CConstVar.BUFFER_LIMIT, SocketFlags.None,ref tmpEP, ReceiveCallback, recvSocket);

		// if(!socket.ReceiveFromAsync(socketAsyncEventArgs[socketArgsIndex]))
		// {

		// }
	}

	private void ReceiveCallback(IAsyncResult ar)
	{
		int count = recvSocket.EndReceive(ar);
		if(count > 0)
		{
			var connection = CDataModel.Connection;
			if(connection.state < ConnectionState.CONNECTED)
			{
				return; //网络还没连上，不处理消息
			}
			//随机丢包处理
			if(CConstVar.NET_DROP_SIM > 0 && CConstVar.NET_DROP_SIM < 100 && UnityEngine.Random.Range(1,100) < CConstVar.NET_DROP_SIM)
			{
				//已经丢掉的包，不处理
			}else
			{
				if(remoteEP != connection.NetChan.remoteAddress)
				{
					CLog.Error(string.Format("%s:sequence packet without connection",remoteEP.Address));
				}else{
					var packet = packetBuffer[~curPacket];
					packet.WriteData(buffer, 0, count); //把缓冲内的数据写入到packet

					var network = CNetwork.Instance;
					if(network == null) return;
					if(CDataModel.Connection.ServerRunning){ //服务器接收到消息
						Server.Instance.RunServerPacket(remoteEP, packet);
					}else{
						network.PacketEvent(packet, remoteEP);
					}
				}
			}
			
			if(remoteEP == null) {
				remoteEP = new IPEndPoint(IPAddress.Any, 0);
			}
			else{
				remoteEP.Address = IPAddress.Any;
				remoteEP.Port = 0;
			}
			var ep = remoteEP as EndPoint;
			recvSocket.BeginReceiveFrom(buffer, 0, CConstVar.BUFFER_LIMIT, SocketFlags.None, ref ep, ReceiveCallback, recvSocket);
		}else{
			CLog.Error("udp socket has recevied no byte");
		}
	}

	//network层已经做了fragment的拆分处理
	public void SendMsg(byte[] bytes, int length, IPEndPoint to){
		sendSocket.BeginSendTo(bytes, 0, length, SocketFlags.None, to, SendCallback, sendSocket);
	}

	private void SendCallback(IAsyncResult ar){
		int count = sendSocket.EndSendTo(ar);

		if(count > 0){
			CLog.Info("Send Packet Finish. length: %d", count);
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

