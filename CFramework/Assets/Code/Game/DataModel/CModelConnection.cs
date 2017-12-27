using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class CModelConnection : IModel {

	public ConnectionState state;

	private int clientNum;

	private int lastPacketSentTime; //for retransmits during connection

	private int lastPacketTime; //for timeouts

	// private IPAddress serverAddress;

	private int connectTime; // for connection retransmits

	private int connectPacketCount; //

	private int challenge;

	private int checksumFeed;

	private int reliableSequence;

	private int reliableAcknowledge; //服务器执行的最后一个

	private string[] reliableCommands;

	// private int serverMessageSequence;

	//server message(非可靠)和command(可靠) sequence数量不会在关卡改变的时候改变
	//只要connection连上，就会增加
	//从服务器接收的可靠消息
	private int serverCommandSequence;

	private int lastExecutedServerCommand; //获取或者执行的server command

	private string[] serverCommands;

	private int incommingSequence;

	private int outgoingSequence;

	// private int fragmentSequence;

	// private int fragmentLength;

	// private int fra

	private string demoName;

	private bool spDemoRecording;

	private bool demoRecording;

	private bool demoPlaying;

	private bool demoWaiting;

	private bool firstDemoFrameSkipped;

	private int timeDemoFrames;

	private int timeDemoStart;

	private int timeDemoBaseTime;

	private int timeDemoLastFrame;

	private int timeDemoMinDuration;

	private int timeDemoMaxDuration;

	private string[] timeDemoDurations;


	// Use this for initialization
	public void Init()
	{

	}
	
	public void Dispose()
	{

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

	ACTIVE, //

	CINEMATIC,
}
