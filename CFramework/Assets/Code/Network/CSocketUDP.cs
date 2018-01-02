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

		packetBuffer = new MsgPacket[2];
		curPacket = 0;

		InitHuffmanMsg();
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
				if(remoteEP.Address != connection.NetChan.remoteAddress)
				{
					CLog.Error(string.Format("%s:sequence packet without connection",remoteEP.Address));
				}else{
					var packet = packetBuffer[~curPacket];
					packet.GetData(buffer, count);
					if(connection.ServerRunning){
						SV_PacketPrcess(packet, remoteEP);
					}else{

						connection.lastPacketTime = CDataModel.GameState.realTime;
						if(packet.CurSize >= 4 && packet.ReadInt() == -1)
						{
							ConnectionlessPacket(remoteEP, packet);
							return;
						}
						if(connection.state < ConnectionState.CONNECTED)
						{
							return;
						}
						if(packet.CurSize < 4)
						{
							CLog.Info("%s: wrong packet", remoteEP.Address);
							return;
						}

						if(remoteEP.Address != connection.NetChan.remoteAddress)
						{
							CLog.Info("%s: sequenced packet without connection", remoteEP);
							return;
						}

						if(!PacketProcess(packet, remoteEP))
						{
							return;
						}

						//可靠消息和不可靠消息的头是不同的
						int headerBytes = packet.CurPos;

						//记录最后接收到的消息，这样它可以在客户端信息中返回，允许服务器检测丢失的gamestate
						connection.serverMessageSequence = LittleInt(packet.ReadInt(0));
						connection.lastPacketTime = CDataModel.GameState.time;

						ParseMessage(packet);

						//在解析完packet之后，不知道是否能保存demo message
						if(connection.demoRecording && !connection.demoWaiting)
						{
							WriteDemoMessage(packet, headerBytes);
						}
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

	private bool PacketProcess(MsgPacket packet, IPEndPoint remote)
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

		var netChan = CDataModel.Connection.NetChan;

		//如果是服务器，那就读取qport
		if(netChan.src == NetSrc.SERVER)
		{
			packet.ReadShot(); //
		}

		int checkSum = packet.ReadInt();

		//UDP欺骗保护
		if(CheckSum(netChan.challenge, checkSum) != checkSum)
			return false;

		//读取fragment信息
		if(fragmented)
		{
			fragmentStart = packet.ReadShot();
			fragmentLength = packet.ReadShot();
		}else
		{
			fragmentStart = 0;
			fragmentLength = 0;
		}

		if(CConstVar.ShowPacket > 0)
		{
			if(fragmented)
			{
				CLog.Info("%s recv %d : s=%d fragment=%d,%d", netChan.src, packet.CurSize, sequence, fragmentStart, fragmentLength);

			}else
			{
				CLog.Info("%s recv %d : s=%d", netChan.remoteAddress, packet.CurSize, sequence);
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
				CLog.Info("%s: Dropped %d packets at %d", remoteEP, netChan.dropped, sequence);
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
					CLog.Info("%s:Dropped a message fragment", netChan.remoteAddress);
				}
				return false;
			}

			//复制fragment到fragment buffer
			if(fragmentLength < 0 || packet.CurPos + fragmentLength > packet.CurSize || netChan.fragmentLength + fragmentLength > netChan.fragmentBuffer.Length)
			{
				if(CConstVar.ShowPacket > 0 || CConstVar.ShowNet > 0)
				{
					CLog.Info("%s:illegal fragment length", netChan.remoteAddress);
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
					CLog.Info("$s:fragmentLength %d > packet.Data Length", netChan.remoteAddress, netChan.fragmentLength);
				}
				return false;
			}

			//保证前面的还是sequence
			packet.WriteInt(CSocketUDP.LittleInt(sequence), 0);
			packet.WriteData(netChan.fragmentBuffer, 4, netChan.fragmentLength);
			packet.CurSize = netChan.fragmentLength + 4;
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
			CLog.Info("%s --------------\n", packet.CurSize);
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
					CLog.Info("%s: BAD CMD %d", packet.CurPos, cmd);
				}else{
					CLog.Info("%s packet", ((SVCCmd)cmd).ToString());
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
					ParseGamestate(packet);
					break;
				case SVCCmd.SNAPSHOT:
					ParseSnapshot(packet);
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
	private void ConnectionlessPacket(IPEndPoint from, MsgPacket msg)
	{

	}

	private void WriteDemoMessage(MsgPacket packet, int headerBytes)
	{

	}

	private void SV_PacketPrcess(MsgPacket packet, IPEndPoint remote)
	{

	}

	//如果snapshot解析正确，它会被复制到snap中，并被存储在snapshots[]
	//如果snapshot因为任何原因不合适，不会任何改变任何state
	private void ParseSnapshot(MsgPacket packet)
	{
		int len;
		ClientSnapshot old = new ClientSnapshot();
		ClientSnapshot newSnap = new ClientSnapshot();
		int deltaNum;
		int oldMessageNum;
		int i, packetNum;

		var connection = CDataModel.Connection;

		//获取可靠的acknowledge number
		//所有从服务器发送到客户端的消息reliableAcknowledge = ReadLong();
		//将新的snapshot读取到临时缓冲中
		//如果合适就复制到snap中
		newSnap.serverCommandNum = connection.serverCommandSequence;
		newSnap.serverTime = packet.ReadInt();

		CDataModel.GameState.paused = 0;

		newSnap.messageNum = connection.serverMessageSequence;

		deltaNum = packet.ReadByte();
		if(deltaNum == 0){
			newSnap.deltaNum = -1;
		}else{
			newSnap.deltaNum = newSnap.messageNum - deltaNum;
		}
		newSnap.snapFlags = (SnapFlags)packet.ReadByte();

		var clientActive = CDataModel.GameState.ClientActive;
		if(newSnap.deltaNum <= 0)
		{
			newSnap.valid = true;
			old = null;
			connection.demoWaiting = false;
		}else{
			old = clientActive.snapshots[newSnap.deltaNum & CConstVar.PACKET_MASK];
			if(!old.valid){
				CLog.Error("old snapshot is invalid");
			}else if(old.messageNum != newSnap.deltaNum){
				CLog.Info("Delta frame too old");
			}else if(clientActive.parseEntitiesIndex - old.parseEntitiesIndex > CConstVar.MAX_PARSE_ENTITIES - CConstVar.MAX_SNAPSHOT_ENTITIES){
				CLog.Info("Delta parseEntitiesNum too old");
			}else{
				newSnap.valid = true;
			}
		}

		CLog.Info("playerstate:%d", packet.CurPos);
		if(old != null)
		{
			ReadDeltaPlayerstate(packet, old.playerState, newSnap.playerState);
		}else{
			// PlayerState tmpP = default(PlayerState);
			ReadDeltaPlayerstate(packet, null, newSnap.playerState);
		}

		CLog.Info("packet entities:%d", packet.CurPos);
		ParseEntities(packet, old, newSnap);

		//如果不合适，就弹出所有的内容，因为它已经被读出。
		if(!newSnap.valid)
		{
			return;
		}

		//清除最后接收到的帧和当前帧之间的所有帧的valid标记，所以如果有丢掉的消息
		//它看起来就不会像是从buffer中增量更新而得到的合适的帧。
		oldMessageNum = clientActive.snap.messageNum + 1;

		if(newSnap.messageNum - oldMessageNum >= CConstVar.PACKET_BACKUP)
		{
			oldMessageNum = newSnap.messageNum - (CConstVar.PACKET_BACKUP - 1);
		}
		for(; oldMessageNum < newSnap.messageNum; oldMessageNum++)
		{
			clientActive.snapshots[oldMessageNum & CConstVar.PACKET_MASK].valid = false;
		}
		
		clientActive.snap = newSnap;
		clientActive.snap.ping	 = 999;
		for(i = 0; i < CConstVar.PACKET_BACKUP; i++)
		{
			packetNum = (connection.NetChan.outgoingSequence - 1 - i) & CConstVar.PACKET_MASK;
			if(clientActive.snap.playerState.commandTime >= clientActive.outPackets[packetNum].serverTime)
			{
				clientActive.snap.ping = CDataModel.GameState.realTime - clientActive.outPackets[packetNum].realTime;
				break;
			}
		}

		//保存当前帧在缓冲中，用来后来做增量比较
		clientActive.snapshots[clientActive.snap.messageNum & CConstVar.PACKET_MASK] = clientActive.snap;

		if(CConstVar.ShowNet == 3)
		{
			CLog.Info("snapshot: %d delta: %d ping:%d", clientActive.snap.messageNum, clientActive.snap.deltaNum, clientActive.snap.ping);
		}

		clientActive.newSnapshots = true;

	}

	private void ParseGamestate(MsgPacket packet)
	{

	}

	private void ParseEntities(MsgPacket packet, ClientSnapshot from, ClientSnapshot to)
	{

	}

	private void ParseCommandString(MsgPacket packet)
	{

	}

	private void ReadDeltaPlayerstate(MsgPacket packet, PlayerState from, PlayerState to)
	{
		int i,lc;
		int bits;

	}

	public void Send()
	{
		// socket.SendTo();
	}

	public static int CheckSum(int challenge, int sequence)
	{
		return challenge ^(sequence * challenge);
	}

	public static int LittleInt(int value)
	{
		return 0;
	}
}

public class MsgPacket{
	bool allowOverflow;

	bool overflowed;

	bool oob; //out of band(带外数据)

	byte[] bytes;

	int curSize;

	int curPos;

	int bit;

	public MsgPacket()
	{
		this.bytes = new byte[CConstVar.BUFFER_LIMIT];
		this.curPos = 0;
	}

	public void GetData(byte[] data, int length)
	{
		Array.Copy(data, 0, bytes, 0, length);
		curSize = length;
	}

	public void WriteData(byte[] data, int start, int length)
	{
		Array.Copy(bytes, start, data, 0, length);
	}

	public void BeginRead()
	{
		curPos = 0;
		bit = 0;
		oob = false;
	}

	public int CurSize
	{
		get{
			return curSize;
		}
		set{
			curSize = value;
		}
	}

	public int Bit{
		set{
			bit = value;
		}
	}

	public bool Oob{
		set{
			oob = value;
		}
	}

	public byte[] Data{
		get{
			return bytes;
		}
	}

	public int CurPos{
		get{
			return curPos;
		}
		set{
			curPos = value;
		}
	}

	public long ReadLong()
	{
		return 0L;
	}

	public int ReadInt(int start = -1)
	{
		if(start < 0)
		{
			start = curPos;
		}else{

		}
		return 0;
	}

	public int ReadByte()
	{
		return 0;
	}

	public int ReadBits(int bits)
	{
		int value, get, i, nbits;
		bool sgn;

		value = 0;
		if(bits < 0)
		{
			bits = -bits;
			sgn = true;
		}else{
			sgn = false;
		}
		if(oob)
		{
			if(bits == 8)
			{
				value = bytes[curPos];
				curPos++;
				bit += 8;
			}else if(bits == 16)
			{
				short temp;

			}else if(bits == 32)
			{

			}else{
				CLog.Error("can't read %d bits", bits);
			}
		}else{
			nbits = 0;
			if((bits & 7) != 0)
			{
				nbits = bits & 7;
				for(i = 0; i < nbits; i++)
				{
					// value |= (Huff)
				}
				bits = bits - nbits;
			}
			if(bits != 0)
			{
				for(i=0; i < bits; i+= 8)
				{
					// value |= (get << (i + nbits));
				}
			}
		}

		if(sgn)
		{
			if((value & ( 1 << (bits - 1))) != 0){
				value |= -1 ^ ((1 - bits) - 1);
			}
		}
		return value;
	}

	public short ReadShot()
	{
		return 0;
	}

	public void WriteInt(int value, int pos = -1)
	{
		if(pos < 0)
		{
			pos = curPos;
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

