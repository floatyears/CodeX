using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class Server : CModule {

	private bool inited;

	private int time;

	private int snapFlagServerBit;

	private ClientNode[] clients;

	private int numSnapshotEntities;

	private int nextSnapshotEntities;

	private EntityState[] snapshotEntities;

	private int nextHeartbeatTime;

	private SvChanllenge[] challenges;

	private IPEndPoint redirectAddress;

	private IPEndPoint authorizeAddress;

	private static Server instance;

	public static Server Instance{
		get{
			return instance;
		}
	}

	public override void Init()
	{
		instance = this;
	}

	public void RunServerPacket(IPEndPoint from, MsgPacket packet)
	{
		int t1,t2,msec;
		t1 = 0;

		var input = CDataModel.InputEvent;
		if(CConstVar.ComSpeeds > 0)
		{
			t1 = input.Milliseconds();
		}

		SV_PacketEvent(from, packet);

		if(CConstVar.ComSpeeds > 0)
		{
			t2 = input.Milliseconds();
			msec = t2 - t1;
			if(CConstVar.ComSpeeds == 3)
			{
				CLog.Info("SV_PacketEvent time: %d", msec);
			}
		}
	}

	public void SV_PacketPrcess(MsgPacket packet, IPEndPoint remote)
	{

	}

	public void ParseServerInfo(string systemInfo)
	{

	}

	public void SystemInfoChanged(string systemInfo)
	{

	}

	public void SV_PacketEvent(IPEndPoint from, MsgPacket packet)
	{
		int i;
		int qport;

		if(packet.CurSize >= 4 && packet.ReadInt() == -1)
		{
			SV_ConnectionlessPacket(from, packet);
		}

		packet.BeginReadOOB();
		packet.ReadInt(); //sequence number

		qport = packet.ReadShot() & 0xffff;

		for(i = 0; i < CConstVar.maxClient; i ++)
		{
			var cl = clients[i];
			if(cl.state == ClientState.FREE){
				continue;
			}

			if(from != cl.netChan.remoteAddress)
			{
				continue;
			}

			//一个IP对应多个客户端，用qport来区分
			if(cl.netChan.qport != qport)
			{
				continue;
			}

			if(cl.netChan.remoteAddress.Port != from.Port)
			{
				CLog.Info("SV_PacketEvent: fixing up a translated port");
				cl.netChan.remoteAddress.Port = from.Port;
			}

			if(SV_NetChanProcess(ref cl.netChan, packet))
			{
				if(cl.state != ClientState.ZOMBIE){
					cl.lastPacketTime = time;
					SV_ExecuteClientMessage(cl, packet);
				}
			}
			return;
		}
	}

	private bool SV_NetChanProcess(ref NetChan netChan, MsgPacket msg)
	{
		return CNetwork.NetChanProcess(ref netChan, msg);
	}

	public void SV_ExecuteClientMessage(ClientNode cl, MsgPacket msg)
	{

	}

	public void SV_ConnectionlessPacket(IPEndPoint from, MsgPacket packet)
	{

		packet.BeginReadOOB();
		packet.ReadInt(); //skip -1 marker

		if(packet.ReadChars(7) == "connect")
		{
			HuffmanMsg.Decompress(packet, 12);
		}

		string s = packet.ReadStringLine();
		var cmd = CDataModel.CmdBuffer;
		cmd.TokenizeString(s, false);

		string c = cmd.Argv(0);
		CLog.Info("SV packet %s : %s", from, c);

		switch(c)
		{
			case "getstatus":
				SVCStatus(from);
				break;
			case "getinfo":
				SVCInfo(from);
				break;
			case "connect":
				DerectConnect(from);
				break;
			case "rcon":
				RemoteCommand(from, packet);
				break;
			case "disconnect":
				break;
			default:
				CLog.Info("bad connectionless packet from %s: %s", from, s);
				break;
		}
	}

	private void SVCStatus(IPEndPoint from)
	{

	}

	private void SVCInfo(IPEndPoint from)
	{

	}

	private void DerectConnect(IPEndPoint from)
	{

	}

	private void RemoteCommand(IPEndPoint from, MsgPacket msg)
	{

	}

	//模块更新
	public override void Update()
	{
		
	}

	//释放
	public override void Dispose()
	{

	}
}

public struct ClientNode
{
	public ClientState state;
	public string userInfo;

	public string[] reliableCommands;

	public int reliableSequence; //最后添加的可靠消息，不必发送或者已知

	public int reliableAcknowledge; //最后已知的可靠消息

	public int reliableSent; //最后发送的可靠消息，不必是已知的

	public int messageAcknowledge;

	public int gamestateMessageNum; //netchan.outgoingSequence

	public int challenge;

	public UserCmd lastUserCmd;

	public int lastMessageNum; //用来增量压缩

	public int lastClientCommand; //可靠的客户端消息sequence

	public string lastClientString;

	public SharedEntity gentity;

	public string name;

	public int deltaMessage;
	public int nextReliableTime;
	public int lastPacketTime;
	public int lastConnectTime;
	public int lastSnapshotTime;
	public bool rateDelayed;
	public int timeoutCount;

	public SvClientSnapshot[] frames;

	public int ping;

	public int rate;

	public int snapshotMsec;

	public int pureAuthentic;

	public bool gotCP;

	public NetChan netChan;

	public Queue<NetChanBuffer> netChanStartQueue;

	public Queue<NetChanBuffer> netChanEndQueue;

	public int oldServerTime;

	public bool[] csUpdated;
}

public class NetChanBuffer{
	public MsgPacket msg;

	public byte[] bytes;
}

public enum ClientState
{
	FREE = 1,
	ZOMBIE,
	CONNECTED,
	PRIMED,
	ACTIVE,
}

public struct SvClientSnapshot
{
	public PlayerState playerState;
	public int numEntities;

	public int firstEntity;

	public int messageSent;

	public int messageAcked;

	public int messageSize; //用来
}

public struct SvChanllenge
{
	public IPEndPoint adr;

	public int challenge;

	public int clientChallenge;

	public int time;

	public int pingTime;

	public int firstTime;

	public bool wasrefused;

	public bool connected;
}