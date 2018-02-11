using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using FlatBuffers;
using System;
using System.Net;
using System.Text;

//CNetwork主要负责对收到的协议进行派发和处理，不关注协议的解析部分
//TCP和UDP可以同时使用，对延迟要求高的部分使用UDP，对延迟要求不高的部分使用TCP
public class CNetwork_Server : CNetwork{

	private new static CNetwork_Server instance;


	public new static CNetwork_Server Instance
	{
		get{
			return instance;
		}
	}

	public override void Init()
	{
		instance = this;
		if(msgIDs == null) msgIDs = new CMessageID();
		sendStack = new Stack<SendCallback>();
		msgBuilders = new List<FlatBufferBuilder>(builderLimit){};
		for(int i = 0; i < builderLimit; i++)
		{
			msgBuilders.Add(new FlatBufferBuilder(32));
		}
		csocket = new CSocket();
		csocket.Init();
		//
		//udp相关
		updSocket = new CSocketUDP();
		updSocket.Init();
		updSocket.BeginReceive();

		//loopback
		loopbacks = new Loopback[2];

		//队列
		sendPacketQueue = new CircularBuffer<PacketQueue>(10);
		needUpdate = true;

		CConstVar.ServerPort = updSocket.GetLocalPort();
		// HuffmanMsg.Init();
	}
	
	// Update is called once per frame
	public override void Update () 
	{
		// HuffmanMsg.Update();

		//flat buffer的处理
		while(csocket.HasCacheMsg) //这里一次性处理的所有的协议，可以设置每帧处理的协议数量
		{
			var byteBuffer = csocket.GetMsg();
			//直接派发数据
			CDataModel.Instance.DispatchMessage(byteBuffer);
		}
		while(sendStack.Count > 0)
		{
			var builder = GetFreeBufferBuilder();
			if(builder != null)
			{
				sendStack.Pop().Invoke(builder);
			}
		}

		while(!updSocket.packetBuffer.IsEmpty){
			var packet = updSocket.packetBuffer.Dequeue();
			if(Server.Instance.ServerRunning){ //服务器接收到消息
				Server.Instance.RunServerPacket(packet.remoteEP, packet);
			}
		}

		//写入packet
		// WritePacket();

		//udp 的处理
		FlushPacketQueue();
	}

	/*-----------Flatbuffer相关-----------*/


	/*---------------LOOPBACK---------------*/


	/* ----------------UDP START--------------------- */
}

