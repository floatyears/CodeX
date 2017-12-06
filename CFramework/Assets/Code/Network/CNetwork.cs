using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using FlatBuffers;
using System;

//CNetwork主要负责对收到的协议进行派发和处理，不关注协议的解析部分
public class CNetwork : CModule{

	public delegate void ConnectOKCallback();

	public delegate void ReconnectCallback();

	public delegate void ErrorCallback();

	public delegate void SendCallback(FlatBufferBuilder builder);

	private static CNetwork instance;

	private CSocket csocket;

	private Stack<SendCallback> sendStack;

	private List<FlatBufferBuilder> msgBuilders;

	private static CMessageID msgIDs;

	private const int builderLimit = 3;


	public static CNetwork Instance
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
	}


	public void Connect()
	{
		csocket.Connect("127.0.0.1", 11111);
	}
	
	// Update is called once per frame
	public override void Update () 
	{
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
	}

	public void SendMsg(SendCallback callback)
	{
		sendStack.Push(callback);
		//var msg = ScPlayerBasic.GetRootAsScPlayerBasic(new ByteBuffer(fb.SizedByteArray())); 
		//CLog.Info(msg.MsgID.ToString());
	}

	private FlatBufferBuilder GetFreeBufferBuilder()
	{
		for(int i = 0; i < builderLimit; i++ )
		{
			if(msgBuilders[i].VtableSize < 0)
			{
				return msgBuilders[i];
			}
		}

		return null;
	}

	public void Reconnect()
	{
		csocket.Reconnect();
	}

	public void Disconnect()
	{
		csocket.Disconnect();
	}

	public override void Dispose()
	{
		csocket.Dipose();
	}
}
