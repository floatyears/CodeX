using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

public class CModelConnection : CModelBase {

	private bool inited = false;

	public ConnectionState state;

	public int clientNum;

	public int lastPacketSentTime; //for retransmits during connection

	public int lastPacketTime; //for timeouts

	public IPEndPoint serverAddress;

	public int connectTime; // for connection retransmits

	public int connectPacketCount; //

	public int challenge;

	public int checksumFeed;

	public int reliableSequence;

	public int reliableAcknowledge; //服务器执行的最后一个

	public string[] reliableCommands;

	public int serverMessageSequence;

	//server message(非可靠)和command(可靠) sequence数量不会在关卡改变的时候改变
	//只要connection连上，就会增加
	//从服务器接收的可靠消息
	public int serverCommandSequence;

	private int lastExecutedServerCommand; //获取或者执行的server command

	public string[] serverCommands;

	private int incommingSequence;

	private int outgoingSequence;

	// private int fragmentSequence;

	// private int fragmentLength;

	// private int fra

	private string demoName;

	private bool spDemoRecording;

	public bool demoRecording;

	public bool demoPlaying;

	public bool demoWaiting;

	private bool firstDemoFrameSkipped;

	private int timeDemoFrames;

	private int timeDemoStart;

	private int timeDemoBaseTime;

	private int timeDemoLastFrame;

	private int timeDemoMinDuration;

	private int timeDemoMaxDuration;

	private string[] timeDemoDurations;

	public IPAddress ServerIP = IPAddress.Parse("127.0.0.1:8001");


	private NetChan netChan;

	public NetChan NetChan{
		get
		{
			return netChan;
		}
	}

	private bool serverRunning;

	public bool ServerRunning{
		get{
			return serverRunning;
		}
	}

	// Use this for initialization
	public override void Init()
	{
		inited = true;
	}
	
	public void NetChanSetup(NetSrc src, IPEndPoint from, int qport, int challenge)
	{
		netChan.src = src;
		netChan.remoteAddress = from;
		netChan.qport = qport;
		netChan.incomingSequence = 0;
		netChan.outgoingSequence = 1;
		netChan.challenge = challenge;
	}

	public override void Dispose()
	{
		inited = false;
	}
}

public enum ConnectionState
{
	UNINITIALIZED = 1,

	DISCONNECTED, //没有连上服务器

	CONNECTING, //发送request packets到服务器

	CHALLENGING, //发送challenge packets到服务器

	CONNECTED, //建立了netchan，正在获取gamestate

	LOADING, //只在初始化，没有在主循环中

	PRIMED, //收到了gamestate，等待第一帧

	ACTIVE, //已经在主循环中了

	CINEMATIC,
}

public struct NetChan{

	public NetSrc src;
	public int dropped; //between last packet and previous

	public IPEndPoint remoteAddress;

	public int qport; //qport value to write when transmitting

	//sequence variables
	public int incomingSequence;

	public int outgoingSequence;

	//incomming fragment buffer
	public int fragementSequence;

	public int fragmentLength;

	public byte[] fragmentBuffer;

	//outgoing fragment buffer
	//需要为最大的fragmet messages留出空间
	public bool unsentFragments;

	public int unsentFragmentStart;

	public int unsentLength;

	public byte[] unsentBuffer;

	public int challenge;

	public int lastSentTime;

	public int lastSentSize;

	public void WriteFragment(byte[] data, int start, int length)
	{
		Array.Copy(data,start,fragmentBuffer,fragmentLength,length);
		fragmentLength += length;
	}
}

public enum NetSrc
{
	CLIENT = 0,

	SERVER,
}