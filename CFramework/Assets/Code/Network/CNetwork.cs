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
public class CNetwork : CModule{

	public delegate void ConnectOKCallback();

	public delegate void ReconnectCallback();

	public delegate void ErrorCallback();

	public delegate void SendCallback(FlatBufferBuilder builder);

	private static CNetwork instance;

	private CSocket csocket;

	private CSocketUDP updSocket;

	private Stack<SendCallback> sendStack;

	private List<FlatBufferBuilder> msgBuilders;

	private static CMessageID msgIDs;

	private const int builderLimit = 3;

	private int incommingSequence;

	private int outgoingSequence;

	private int dropped;

	private int framgmentSequence;

	/*----------LOOPBACK缓冲，用于本地玩家---------*/

	private Loopback[] loopbacks;

	//
	private CircularBuffer<PacketQueue> sendPacketQueue;


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
		
		// HuffmanMsg.Init();
	}

	public void Connect()
	{
		csocket.Connect("127.0.0.1", 11111);
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
			if(CDataModel.Connection.ServerRunning){ //服务器接收到消息
				Server.Instance.RunServerPacket(packet.remoteEP, packet);
			}else{
				var network = CNetwork.Instance;
				if(network != null) {
					network.PacketEvent(packet, packet.remoteEP);
				}
			}
		}

		//写入packet
		// WritePacket();

		//udp 的处理
		// FlushPacketQueue();
	}

	/*-----------Flatbuffer相关-----------*/
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

	/*---------------LOOPBACK---------------*/
	public bool GetLoopPacket(NetSrc src, out IPEndPoint from, out MsgPacket msg)
	{
		from = new IPEndPoint(IPAddress.Loopback, 0);
		msg = new MsgPacket();

		Loopback loop = loopbacks[(int)src];
		if(loop.send - loop.get > CConstVar.MAX_LOOPBACK){
			loop.get = loop.send - CConstVar.MAX_LOOPBACK;
		}

		if(loop.get >= loop.send)
		{
			return false;
		}

		int i = loop.get & (CConstVar.MAX_LOOPBACK - 1);
		loop.get++;
		Array.Copy(loop.msgs[i].data, msg.Data, loop.msgs[i].datalen);
		msg.CurSize = loop.msgs[i].datalen;
		
		from.Address = IPAddress.Loopback;
		return true;
	}


	/* ----------------UDP START--------------------- */

	public void PacketEvent(MsgPacket packet, IPEndPoint from)
	{
		var connection = CDataModel.Connection;

		if(connection.ServerRunning){
			Server.Instance.SV_PacketPrcess(packet, from);
		}else{

			connection.lastPacketTime = CDataModel.GameState.realTime;
			if(packet.CurSize >= 4 && packet.ReadFirstInt() == -1)
			{
				ConnectionlessPacket(from, packet);
				return;
			}
			if(connection.state < ConnectionState.CONNECTED)
			{
				return;
			}
			if(packet.CurSize < 4)
			{
				CLog.Info("{0}: wrong packet", from.Address);
				return;
			}

			if(!from.Equals(connection.NetChan.remoteAddress))
			{
				CLog.Info("{0}: sequenced packet without connection", from);
				return;
			}

			var netChan = CDataModel.Connection.NetChan;
			if(!NetChanProcess(ref netChan, packet))
			{
				return;
			}

			//可靠消息和不可靠消息的头是不同的
			int headerBytes = packet.CurPos;

			//记录最后接收到的消息，这样它可以在客户端信息中返回，允许服务器检测丢失的gamestate
			connection.serverMessageSequence = CNetwork.LittleInt(packet.ReadFirstInt());
			connection.lastPacketTime = CDataModel.GameState.time;

			ParseMessage(packet);

			//在解析完packet之后，不知道是否能保存demo message
			if(connection.demoRecording && !connection.demoWaiting)
			{
				WriteDemoMessage(packet, headerBytes);
			}
		}
	}

	public static bool NetChanProcess(ref NetChan netChan, MsgPacket packet)
	{
		bool fragmented = false;
		int fragmentStart = 0;
		int fragmentLength = 0;
		packet.BeginRead();
		int sequence = packet.ReadInt();

		//检查fragment信息
		if((sequence & CConstVar.FRAGMENT_BIT) != 0 )
		{
			sequence &= ~CConstVar.FRAGMENT_BIT;
			fragmented = true;
		}else{
			fragmented = false;
		}

		//如果是服务器，那就读取qport
		if(netChan.src == NetSrc.SERVER)
		{
			packet.ReadShort(); //
		}

		int checkSum = packet.ReadInt();

		//UDP欺骗保护
		if(CheckSum(netChan.challenge, checkSum) != checkSum)
			return false;

		//读取fragment信息
		if(fragmented)
		{
			fragmentStart = packet.ReadShort();
			fragmentLength = packet.ReadShort();
		}else
		{
			fragmentStart = 0;
			fragmentLength = 0;
		}

		if(CConstVar.ShowPacket > 0)
		{
			if(fragmented)
			{
				CLog.Info("{0} recv {1} : s={2} fragment={3},{4}", netChan.src, packet.CurSize, sequence, fragmentStart, fragmentLength);

			}else
			{
				CLog.Info("{0} recv {1} : s={2}", netChan.remoteAddress, packet.CurSize, sequence);
			}
		}
		
		//丢掉乱序或者重复的packets
		if(sequence <= netChan.incomingSequence)
		{
			if(CConstVar.ShowPacket > 0)
			{
				CLog.Error("%s:Out of order packet %d at %d", netChan.remoteAddress, netChan.dropped, sequence);
			}
			return false;
		}

		//丢包使得当前的消息不可用
		netChan.dropped = sequence - (netChan.incomingSequence + 1);
		if(netChan.dropped > 0)
		{
			if(CConstVar.ShowNet > 0 || CConstVar.ShowPacket > 0)
			{
				CLog.Info("{0}: Dropped {1} packets at {2}", netChan.remoteAddress, netChan.dropped, sequence);
			}
		}

		//如果当前是可靠消息的最后一个fragment
		//就取得incomming_reliable_sequence
		if(fragmented)
		{
			//确保以正确的顺序添加fragment，可能有packet被丢弃，或者过早地收到了这个packet
			//不会重新构造这个fragment，会等这个packet再次达到
			if(sequence != netChan.fragementSequence){
				netChan.fragementSequence = sequence;
				netChan.fragmentLength = 0;
			}

			//如果有fragment丢失，打印出日志
			if(fragmentStart != netChan.fragmentLength)
			{
				if(CConstVar.ShowPacket > 0 || CConstVar.ShowNet > 0)
				{
					CLog.Info("{0}:Dropped a message fragment", netChan.remoteAddress);
				}
				return false;
			}

			//复制fragment到fragment buffer
			if(fragmentLength < 0 || packet.CurPos + fragmentLength > packet.CurSize || netChan.fragmentLength + fragmentLength > netChan.fragmentBuffer.Length)
			{
				if(CConstVar.ShowPacket > 0 || CConstVar.ShowNet > 0)
				{
					CLog.Info("{0}:illegal fragment length", netChan.remoteAddress);
				}
				return false;
			}
			netChan.WriteFragment(packet.Data, packet.CurPos, fragmentLength);

			//如果这不是最后一个fragment，就不处理任何事情(处于中间的fragment的长度是FRAGMENT_SIZE)
			if(fragmentLength == CConstVar.FRAGMENT_SIZE)
			{
				return false;
			}

			//
			if(netChan.fragmentLength > packet.Data.Length)
			{
				if(CConstVar.ShowPacket > 0 || CConstVar.ShowNet > 0)
				{
					CLog.Info("{0}:fragmentLength {1} > packet.Data Length", netChan.remoteAddress, netChan.fragmentLength);
				}
				return false;
			}

			//保证前面的还是sequence
			packet.WriteFirstInt(CNetwork.LittleInt(sequence));
			packet.WriteData(netChan.fragmentBuffer, 4, netChan.fragmentLength);
			// packet.CurSize = netChan.fragmentLength + 4;
			netChan.fragmentLength = 0;
			packet.CurPos = 4; 	//粘贴sequence
			packet.Bit = 32;  	//粘贴sequence

			//客户端没有应答fragment信息
			netChan.incomingSequence = sequence;

			return true;
		}

		//信息现在可以从当前的信息指针中读取
		netChan.incomingSequence = sequence;
	
		return true;
	}

	private void ParseMessage(MsgPacket packet)
	{
		int cmd;
		if(CConstVar.ShowNet == 1)
		{
			CLog.Info("{0} --------------\n", packet.CurSize);
		}

		packet.Oob = false;
		
		var connection = CDataModel.Connection;
		//获得可靠的acknowledge sequence
		connection.reliableAcknowledge = packet.ReadInt();
		if(connection.reliableAcknowledge < connection.reliableSequence - CConstVar.MAX_RELIABLE_COMMANDS){
			connection.reliableAcknowledge = connection.reliableSequence;
		}

		//处理消息
		while(true)
		{
			if(packet.CurPos > packet.CurSize)
			{
				CLog.Error("Parse Message: read past end of server message");
				break;
			}
			cmd = packet.ReadInt();
			if(cmd == (int)SVCCmd.EOF){
				CLog.Info("END OF MESSAGE");
				break;
			}

			if(CConstVar.ShowNet >= 2){
				if(cmd < 0){
					CLog.Info("{0}: BAD CMD {1}", packet.CurPos, cmd);
				}else{
					CLog.Info("{0} packet", ((SVCCmd)cmd).ToString());
				}
			}

			switch((SVCCmd)cmd)
			{
				case SVCCmd.NOP:
					break;
				case SVCCmd.SERVER_COMMAND:
					ParseCommandString(packet);
					break;
				case SVCCmd.GAME_STATE:
					CDataModel.GameState.ParseGamestate(packet);
					break;
				case SVCCmd.SNAPSHOT:
					CDataModel.GameState.ParseSnapshot(packet);
					break;
				case SVCCmd.DOWNLOAD:
					break;
				default:
					CLog.Error("Parse Message: Illegile server message");
					break;
			}

		}
	}

	//处理广播消息等。
	//第一个字节用来区分game channel的消息（-1表示connectionless数据，1表示有数据）
	//而在c#中并没有暴露底层的接口来支持显式发送oob，所以用第一位来表示是否是带外数据
	private void ConnectionlessPacket(IPEndPoint from, MsgPacket msg)
	{
		msg.BeginReadOOB();
		msg.ReadInt(); //skip -1
		string s = msg.ReadStringLine();
		var cmd = CDataModel.CmdBuffer;
		cmd.TokenizeString(s, false);
		string c = cmd.Argv(0);
		CLog.Info("Client Packet {0} : {1}", from, c);

		if(c == "challengeResponse")
		{
			CDataModel.Connection.PacketChallengeResponse(from, msg);
			return;
		}

		//服务器连接
		if(c == "connectResponse")
		{
			CDataModel.Connection.PacketConnectResponse(from, msg);
			return;
		}

		//服务器返回信息
		if(c == "infoResponse")
		{
			CDataModel.GameState.ServerInfoPacket(from, msg);
			return;
		}

		if(c == "statusResponse")
		{
			CDataModel.GameState.ServerStatusResponse(from, msg);
			return;
		}

		if(c == "echo")
		{
			return;
		}

		if(c == "keyAuthorize")
		{
			return;
		}

		if(c == "getserversResponse")
		{
			CDataModel.GameState.ServerResponsePacket(from, msg, false);
			return;
		}

		if(c == "getserversExtResponse")
		{
			CDataModel.GameState.ServerResponsePacket(from, msg, true);
			return;
		}

	}

	private void WriteDemoMessage(MsgPacket packet, int headerBytes)
	{

	}

	private void ParseCommandString(MsgPacket packet)
	{
		int seq = packet.ReadInt();
		string s = packet.ReadString();
		var connection = CDataModel.Connection;
		if(connection == null) return;
		if(connection.serverCommandSequence >= seq){
			return;
		}
		connection.serverCommandSequence = seq;
		
		int index = seq & (CConstVar.MAX_RELIABLE_COMMANDS - 1);
		connection.serverCommands[index] = s;
	}

	/*-------------------UDP END------------------*/


	/*------------------发送消息-------------------*/

	public void Send()
	{
		// socket.SendTo();
	}

	public bool ReadyToSendPacket(){
		int oldPacketNum;
		int delta;

		var connection = CDataModel.Connection;
		if(connection.demoPlaying || connection.state == ConnectionState.CINEMATIC){
			return false;
		}

		int realTime = CDataModel.GameState.realTime;
		//没有合适的gamstate状态，就1s发一个包
		if(connection.state != ConnectionState.ACTIVE && connection.state != ConnectionState.PRIMED && realTime - connection.lastPacketSentTime < 1000){
			return false;
		}

		//loopback就每帧都发送
		if(IPAddress.IsLoopback(connection.NetChan.remoteAddress.Address)){
			return true;
		}

		if(CConstVar.LanForcePackets && IsLANAddress(connection.NetChan.remoteAddress.Address)){
			return true;
		}

		if(CConstVar.MaxPackets < 15) CConstVar.MaxPackets = 15;
		else if(CConstVar.MaxPackets > 125) CConstVar.MaxPackets = 125;

		oldPacketNum = (connection.NetChan.outgoingSequence - 1) & CConstVar.PACKET_MASK;
		delta = realTime - CDataModel.GameState.ClientActive.outPackets[oldPacketNum].realTime;
		if(delta < (1000 / CConstVar.MaxPackets)){
			//累积的commands会在下一个packet中发出
			return false;
		}
		return true;
	}

	/*
	创建并发送command packet到服务器
	包含可靠的commands和usercmds

	客户端packet会包含这样的格式：
	
	4	sequence number
	2	qport
	4	serverid
	4	acknowledged sequence number
	4	clc.serverCommandSequence
	<optional reliable commands>
	1	clc_move or clc_moveNoDelta
	1	command count
	<count * usercmds>
	*/
	public void WritePacket(){
		MsgPacket buf;
		byte[] data = new byte[CConstVar.MAX_MSG_LEN];
		int i,j;
		UserCmd cmd, oldcmd;
		int packetNum;
		int oldPacketNum;
		int count, key;

		var clActive = CDataModel.GameState.ClientActive;
		var connection = CDataModel.Connection;
		if(connection.demoPlaying || connection.state == ConnectionState.CINEMATIC){
			return;
		}

		UserCmd nullcmd = new UserCmd();

		oldcmd = nullcmd;
		buf = new MsgPacket();
		buf.Oob = false;
		buf.WriteInt(clActive.serverID);

		//写入我们最后收到的消息，这可以用来增量压缩，同时也可以告知我们是否丢掉了gamestate
		buf.WriteInt(connection.serverMessageSequence);

		//写入最后收到的可靠消息
		buf.WriteInt(connection.serverCommandSequence);

		//写入所有未知的client commands
		for(i = connection.reliableAcknowledge + 1; i < connection.reliableSequence; i++){
			buf.WriteByte((byte)ClientMsgType.ClientCommand);
			buf.WriteInt(i);
			buf.WriteString(connection.reliableCommands[i & (CConstVar.MAX_RELIABLE_COMMANDS - 1)]);
		}

		if(CConstVar.PacketDUP < 0) CConstVar.PacketDUP = 0;
		else if(CConstVar.PacketDUP > 5) CConstVar.PacketDUP = 5;

		oldPacketNum = (connection.NetChan.outgoingSequence - 1 - CConstVar.PacketDUP) & CConstVar.PACKET_MASK;
		count = clActive.cmdNum - clActive.outPackets[oldPacketNum].cmdNum;
		if(count > CConstVar.MAX_PACKET_USERCMDS){
			count = CConstVar.MAX_PACKET_USERCMDS;
			CLog.Info("Exceed max packet usercmds");
		}

		if(count >= 1){
			if(CConstVar.ShowNet > 0){
				CLog.Info("packet user cmd: {0}", count);
			}

			if(CConstVar.NoDelta > 0 || !clActive.snap.valid || connection.demoWaiting || connection.serverMessageSequence != clActive.snap.messageNum){
				buf.WriteByte((byte)ClientMsgType.MoveNoDelta);
			}else{
				buf.WriteByte((byte)ClientMsgType.Move);
			}

			//写入command的数量
			buf.WriteByte((byte)count);

			//使用chechsum feed内的key
			key = connection.checksumFeed;
			//使用已知的消息
			key ^= connection.serverMessageSequence;
			//使用key中最后已知的服务器command
			key ^= HasKey(connection.serverCommands[connection.serverCommandSequence & (CConstVar.MAX_RELIABLE_COMMANDS - 1)], 32);

			for(i = 0; i < count; i++){
				j = (clActive.cmdNum - count + i + 1) & CConstVar.CMD_MASK;
				cmd = clActive.cmds[j];
				buf.WirteDeltaUserCmdKey(key, ref oldcmd, ref cmd);
				oldcmd = cmd;
			}
		}

		//发送消息
		packetNum = connection.NetChan.outgoingSequence & CConstVar.PACKET_MASK;
		var opacket = clActive.outPackets[packetNum];
		var realTime = CDataModel.GameState.realTime;
		opacket.realTime = realTime;
		opacket.serverTime = oldcmd.serverTime;
		opacket.cmdNum = clActive.cmdNum;
		connection.lastPacketSentTime = realTime;

		if(CConstVar.ShowNet > 0){
			CLog.Info("send packet:{0}", buf.CurPos);
		}

		//客户端发送数据
		buf.WriteByte((byte)ClientMsgType.EOF);
		var netChan = connection.NetChan;
		NetChanTransmit(ref netChan, buf.CurSize, buf.Data);

		//下一帧传输所有的数据，没有延迟
		while(netChan.unsentFragments){
			NetChanTransmitNextFrame(ref netChan);
			CLog.Warning("unsent fragments (not supposed to happen!)");
		}

	}

	public void NetChanTransmit(ref NetChan netChan, int length, byte[] data){
		MsgPacket send = new MsgPacket();
		byte[] sendBuf = new byte[CConstVar.PACKET_MAX_LEN];
		if(length > CConstVar.PACKET_MAX_LEN){
			CLog.Info("Netchan transmit overflow, length = {0}", length);
		}
		netChan.unsentFragmentStart = 0;

		//fragment large reliable messages
		if(length >= CConstVar.FRAGMENT_SIZE){
			netChan.unsentFragments = true;
			netChan.unsentLength = length;
			Array.Copy(data, 0, netChan.unsentBuffer, 0, length);

			//只发送第一帧的数据
			NetChanTransmitNextFrame(ref netChan); 
		}

		//写入packet header
		send.WriteInt(netChan.outgoingSequence);

		//发送qport
		if(netChan.src == NetSrc.CLIENT){
			send.WriteShort((short)CConstVar.Qport);
		}

		send.WriteInt(CheckSum(netChan.challenge, netChan.outgoingSequence));
		netChan.outgoingSequence++;

		send.WriteData(data, -1, length); //data的数据写入到packet中

		//发送数据
		SendPacket(netChan.src, send.CurSize, send.Data, netChan.remoteAddress);

		//存储这个包发送的时间和大小。
		netChan.lastSentTime = CDataModel.InputEvent.Milliseconds();
		netChan.lastSentSize = send.CurSize;

		if(CConstVar.ShowPacket > 0){
			CLog.Info("{0} send {1} : outgoSeq = {2} incomSeq = {3}", netChan.src, send.CurSize, netChan.outgoingSequence - 1, netChan.incomingSequence);
		}
	}

	public void SendPacket(NetSrc src, int length, byte[] data, IPEndPoint to){
		// if(CConstVar.ShowPacket > 0){ // && *(int *)data == -1
		// 	CLog.Info("send packet {0}", length * 4);
		// }

		if(IPAddress.IsLoopback(to.Address)){
			SendLoopPacket(src, length, data, to);
			return;
		}
		// if(to.Address.)

		if(src == NetSrc.CLIENT && CConstVar.PacketDelayClient > 0){
			QueuePacket(length, data, to, CConstVar.PacketDelayClient);
		}else if(src == NetSrc.SERVER && CConstVar.PacketDelayServer > 0){
			QueuePacket(length, data, to, CConstVar.PacketDelayServer);
		}else{
			updSocket.SendMsg(data, length, to);
		}
	}

	//发送一个文字信息在out of band，用于"connect"
	//而在c#中并没有暴露底层的接口来支持显式发送oob，所以用第一位来表示是否是带外数据
	public void OutOfBandSend(NetSrc src, IPEndPoint address, string format){
		var charArr = System.Text.Encoding.Default.GetBytes(format);
		
		byte[] a = new byte[4 + charArr.Length];
		a[0] = 0xff;
		a[1] = 0xff;
		a[2] = 0xff;
		a[3] = 0xff;
		
		Array.Copy(charArr, 0, a, 4, charArr.Length);

		IPEndPoint newA = new IPEndPoint(address.Address,address.Port);
		newA.Address = IPAddress.Broadcast;
		
		SendPacket(src, a.Length, a, newA);
		// SendPacket(src, str)
	}

	private static void SendLoopPacket(NetSrc src, int length, byte[] data, IPEndPoint to){

	}

	public void NetChanTransmitNextFrame(ref NetChan netChan){
		MsgPacket send = new MsgPacket();
		send.Oob = true;
		byte[] send_buf = new byte[CConstVar.PACKET_MAX_LEN];
		int fragmentLength = 0;
		int outgoingSequence = netChan.outgoingSequence | CConstVar.FRAGMENT_BIT;
		send.WriteInt(outgoingSequence);

		//如果是客户端就发送qport
		if(netChan.src == NetSrc.CLIENT){
			send.WriteInt(CConstVar.Qport);
		}

		send.WriteInt(CheckSum(netChan.challenge, netChan.outgoingSequence));

		fragmentLength = CConstVar.FRAGMENT_SIZE;
		if(netChan.unsentFragmentStart + fragmentLength > netChan.unsentLength){
			fragmentLength = netChan.unsentLength - netChan.unsentFragmentStart;
		}

		send.WriteShort((short)netChan.unsentFragmentStart);
		send.WriteShort((short)fragmentLength);
		send.WriteData(netChan.unsentBuffer, -1, fragmentLength, netChan.unsentFragmentStart);

		//发送数据
		SendPacket(netChan.src, send.CurSize, send.Data, netChan.remoteAddress);

		//存储发送的时间和大小
		netChan.lastSentTime = CDataModel.InputEvent.Milliseconds();
		netChan.lastSentSize = send.CurSize;

		if(CConstVar.ShowPacket > 0){
			CLog.Info("%s send %d : s=%d fragment=%d,%d", netChan.src, send.CurSize, netChan.outgoingSequence, netChan.unsentFragmentStart, fragmentLength);
		}

		netChan.unsentFragmentStart += fragmentLength;
		
		//现在的情况有点戏剧，因为一个packet如果刚好是fragment的长度
		//那还需要发送第二个packet（长度为0），这样另外一端就知道是否有更多的packet。
		if(netChan.unsentFragmentStart == netChan.unsentLength && CConstVar.FRAGMENT_SIZE != fragmentLength){
			netChan.outgoingSequence++;
			netChan.unsentFragments = false;
		}
	}

	

	private void QueuePacket(int length, byte[] data, IPEndPoint to, int delay){
		if(delay > 999) delay = 999;
		var newPacket = new PacketQueue(to);
		newPacket.packet.CurSize = length;
		newPacket.to = to;
		newPacket.packet.WriteData(data, 0, length);
		// newPacket.to = to;
		newPacket.release = CDataModel.InputEvent.Milliseconds() +  (int)((float)delay/CConstVar.timeScale);

		sendPacketQueue.Enqueue(newPacket);
		// while(packetQueue.IsEmpty())
	}

	//清空缓存中的消息，必须在当前帧进行处理，超过当前帧可能会造成下一帧的数据错误
	public void FlushPacketQueue(){
		int time = CDataModel.InputEvent.Milliseconds();
		while(!sendPacketQueue.IsEmpty){
			var packet = sendPacketQueue.Peek();
			if(packet.release >= time){ //延迟操作
				break;
			}
			sendPacketQueue.Dequeue();
			updSocket.SendMsg(packet.packet.Data, packet.packet.CurSize, packet.to);
		}
	}


	/*------------------工具函数-----------------*/
	public static int CheckSum(int challenge, int sequence)
	{
		// return challenge ^(sequence * challenge);
		return sequence;
	}

	public static int LittleInt(int value)
	{
		return 0;
	}

	public static bool IsLANAddress(IPAddress address){
		return true;
	}

	public static int HasKey(string content, int maxLen){
		int hash = 0;
		for (int i = 0; i < maxLen && content[i] != '\0'; i++) {
			if ((content[i] & 0x80) > 0 || content[i] == '%')
				hash += '.' * (119 + i);
			else
				hash += content[i] * (119 + i);
		}
		hash = (hash ^ (hash >> 10) ^ (hash >> 20));
		return hash;
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
		updSocket.Dispose();

		csocket = null;
		updSocket = null;
	}
}


public struct LoopbackMsg{
	public byte[] data;
	public int datalen;
}

public struct Loopback{
	public LoopbackMsg[] msgs;
	public int get,send;
}

public enum ClientMsgType{
	Bad = 0,
	Nop = 1,

	Move,

	MoveNoDelta,

	ClientCommand,

	EOF,
}

public struct PacketQueue{

	public IPEndPoint to;

	public int release;

	public MsgPacket packet;

	public PacketQueue(IPEndPoint to){
		this.to = to;
		packet = new MsgPacket();
		release = 0;
	}

}