using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;
using System;

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

	//server none static
	private bool restarting;

	private int serverID;

	private int restartedServerId;

	private int checksumFeed;

	private int checksumFeedServerId;

	private int snapshotCounter;

	//每一帧的时间（1000/CConstVar.SV_FPS）
	private int timeResidual;

	private int nextFrameTime;

	private SvEntityState[] svEntities;

	private SharedEntity[] gEntities;

	private int gEntitySize;

	private int numEntities;

	// private PlayerState[] gameClients;

	private int restartTime;

	// private int time;

	private static Server instance;

	public static Server Instance{
		get{
			return instance;
		}
	}

	public override void Init()
	{
		instance = this;
		clients = new ClientNode[CConstVar.MAX_CLIENTS];
		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			clients[i] = new ClientNode();
		}
		svEntities = new SvEntityState[CConstVar.MAX_GENTITIES];

		numSnapshotEntities = CConstVar.MAX_CLIENTS * CConstVar.PACKET_BACKUP * CConstVar.MAX_SNAPSHOT_ENTITIES;
		inited = true;

	}

	public void Startup(){
		
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
				CLog.Info("SV_PacketEvent time: {0}", msec);
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

		if(packet.ReadChars(7, 4) == "connect")
		{
			HuffmanMsg.Decompress(packet, 12);
		}

		string s = packet.ReadStringLine();
		var cmd = CDataModel.CmdBuffer;
		cmd.TokenizeString(s, false);

		string c = cmd.Argv(0);
		CLog.Info("SV packet {0} : {1}", from, c);

		switch(c)
		{
			case "getstatus":
				SVCStatus(from);
				break;
			case "getinfo":
				SVCInfo(from);
				break;
			case "getchallenge":
				SVCGetChallenge(from);
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
				CLog.Info("bad connectionless packet from {0}: {1}", from, s);
				break;
		}
	}

	private void SVCStatus(IPEndPoint from)
	{
		var status = StringBuilderCache.Acquire();
		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			var cl = clients[i];
			if(cl.state >= ClientState.CONNECTED){
				var ps = cl.playerState;
				status.Append(ps.persistant[(int)PlayerStatePersistant.PERS_SCORE]).Append(" ").Append(cl.ping).Append(" ").Append(cl.name);
			}
		}

		CNetwork.Instance.OutOfBandSend(NetSrc.SERVER, from, string.Format("statusResponse\n%s\n%s", "\\infostring$", status.ToString()));
		StringBuilderCache.Release(status);
	}

	private void SVCGetChallenge(IPEndPoint from){
		
	}

	private void SVCInfo(IPEndPoint from)
	{
		var infoStr = StringBuilderCache.Acquire();
		
		int humans = 0;
		int count = 0;
		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			if(clients[i].state >= ClientState.CONNECTED){
				count++;
				if(clients[i].netChan.isBot){
					humans++;
				}
			}
		}
		infoStr.Append("infoResponse").Append("\n");
		infoStr.Append("\\").Append("challenge").Append("$").Append(CDataModel.CmdBuffer.Argv(1));
		infoStr.Append("\\").Append("protocal").Append("$").Append(CConstVar.Protocol);
		infoStr.Append("\\").Append("clients").Append("$").Append(count);
		infoStr.Append("\\").Append("humans").Append("$").Append(humans);
		infoStr.Append("\\").Append("humans").Append("$").Append(humans);
		infoStr.Append("\\").Append("sv_maxclients").Append("$").Append(CConstVar.MAX_CLIENTS);
		infoStr.Append("\\").Append("minPing").Append("$").Append(CConstVar.minPing);
		infoStr.Append("\\").Append("maxPing").Append("$").Append(CConstVar.maxPing);


		CNetwork.Instance.OutOfBandSend(NetSrc.SERVER, from, infoStr.ToString());
	}

	private void DerectConnect(IPEndPoint from)
	{
		ClientNode cl = null;
		int newClIdx = 0;
		ClientNode newcl = null;
		// ClientNode temp = new ClientNode();

		string ip;
		var userinfo = CDataModel.CmdBuffer.Argv(1);
		int version = System.Convert.ToInt32(GetValueForKey(userinfo, "protocol"));

		if(version != CConstVar.Protocol){
			CNetwork.Instance.OutOfBandSend(NetSrc.SERVER, from, string.Format("server protocol mismatches with client. server:%d, client:%d", CConstVar.Protocol, version));
			return;
		}

		int chNum = System.Convert.ToInt32(GetValueForKey(userinfo, "challenge"));
		int qport = System.Convert.ToInt32(GetValueForKey(userinfo, "qport"));

		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			cl = clients[i];
			if(cl.state == ClientState.FREE){
				continue;
			}

			if(from.Address.Equals(cl.netChan.remoteAddress.Address) && (cl.netChan.qport == qport || from.Port == cl.netChan.remoteAddress.Port)){
				if(time - cl.lastConnectTime < CConstVar.reconnectLimit * 1000){
					CLog.Info("{0}: reconnect rejected : too soon", from);
					return;
				}
				break;
			}
		}

		if(IPAddress.IsLoopback(from.Address)){
			ip = "localhost";
		}else{
			ip = from.Address.ToString();
		}

		SetValueForKey(userinfo, "ip", ip);
		if(IPAddress.IsLoopback(from.Address)){
			int ping;
			SvChanllenge ch;
			int i;
			for(i = 0; i < CConstVar.MAX_CHALLENGES; i++){
				if(from.Address.Equals(challenges[i].adr.Address)){
					if(chNum == challenges[i].challenge){
						break;
					}
				}
			}

			if(i == CConstVar.MAX_CHALLENGES){
				CNetwork.Instance.OutOfBandSend(NetSrc.SERVER, from, string.Format("no or bad challenge for your address %s", from));
				return;
			}

			ch = challenges[i];
			if(ch.wasrefused){
				return;
			}
			ping = time - ch.pingTime;

			//局域网不判断ping
			if(!CNetwork.IsLANAddress(from.Address)){
				if(CConstVar.minPing > 0 && ping < CConstVar.minPing){
					CNetwork.Instance.OutOfBandSend(NetSrc.SERVER, from, "server is for high pings only");
					ch.wasrefused = true;
					return;
				}

				if(CConstVar.maxPing > 0 && ping < CConstVar.maxPing){
					CNetwork.Instance.OutOfBandSend(NetSrc.SERVER, from, "server is for high pings only");
					ch.wasrefused = true;
					return;
				}
			}

			ch.connected = true;
		}

		//  = temp;

		Action newClient = ()=>{
			// newcl = temp;
			if(newcl == null) newcl = new ClientNode();
			SharedEntity ent = gEntities[newClIdx];
			newcl.gEntity = ent;
			newcl.challenge = chNum;
		
			newcl.netChan.SetUp(NetSrc.SERVER, from, qport, chNum);
			newcl.netChanQueue.Reset();
			newcl.userInfo = userinfo;

			//发送连接消息给客户端
			CNetwork.Instance.OutOfBandSend(NetSrc.SERVER, from, string.Format("connect response %d", chNum));

			newcl.state = ClientState.CONNECTED;
			newcl.lastSnapshotTime = 0;
			newcl.lastPacketTime = time;
			newcl.lastConnectTime = time;

			//当收到来自客户端的第一条消息，就会发现这是来自不同的serverid，而gamestate消息没有发送，强制传输
			newcl.gamestateMessageNum = -1;

			int count = 0;
			for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
				if(clients[i].state >= ClientState.CONNECTED){
					count++;
				}
			}
			if(count == 1 || count == CConstVar.MAX_CLIENTS){
				nextHeartbeatTime = -999999;
			}
		};
		
		//如果有一个这个ip的连接，重用它
		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			cl = clients[i];
			if(cl.state == ClientState.FREE){
				continue;
			}

			if(from.Address.Equals(cl.netChan.remoteAddress.Address) && (cl.netChan.qport == qport || from.Port == cl.netChan.remoteAddress.Port)){
				CLog.Info("{0}: reconnect rejected : too soon", from);
				newcl = cl;
				newClIdx = i;
				newClient();
				break;
			}
		}

		//找到一个client
		//如果CConstVar.PrivateClients > 0, 那么会为"password"设置为的客户端保留位置
		//
		string pwd = GetValueForKey(userinfo, "password");
		int startIdx = 0;
		if(pwd == CConstVar.PrivatePwd){
			//跳过预留的位置
			startIdx = CConstVar.PrivateClients;
		}

		newcl = null;
		for(int i = startIdx; i < CConstVar.MAX_CLIENTS; i++){
			cl = clients[i];
			if(cl.state == ClientState.FREE){
				newcl = cl;
				newClIdx = i;
				break;
			}
		}

		if(newcl == null){
			if(IPAddress.IsLoopback(from.Address)){
				int count = 0;
				for(int i = startIdx; i < CConstVar.MAX_CLIENTS; i++){
					cl = clients[i];
					if(cl.netChan.isBot){
						count++;
					}
				}

				//如果都是机器人
				if(count >= CConstVar.MAX_CLIENTS - startIdx){
					DropClient(clients[CConstVar.MAX_CLIENTS - 1],"only bots on server");
					newcl = clients[CConstVar.MAX_CLIENTS -1];
					newClIdx = CConstVar.MAX_CLIENTS -1;
				}else{
					CLog.Error("server is full on local connect");
					return;
				}
			}
			else{
				CNetwork.Instance.OutOfBandSend(NetSrc.SERVER, from, "server is full");
				CLog.Info("Rejected a connection. {0}", from);
				return;
			}
		}

		//有一个新的客户端，所有重置reliableSequence和reliableAcknowledge
		if(cl != null){
			cl.reliableAcknowledge = 0;
			cl.reliableSequence = 0;

		}

		newClient();
	}

	

	public static string GetValueForKey(string s, string key){
		string newkey = "//" + key;
		int start = s.IndexOf(newkey);
		if(start < 0) return "";
		int end = s.IndexOf("//", start);
		if(end < 0) end = s.Length + 1;
		return s.Substring(start + newkey.Length + 1, end - start - newkey.Length + 1);
	}

	public static void SetValueForKey(string s, string key, string value){
		string newkey = "//" + key;
		int start = s.IndexOf(newkey);
		if(start >= 0){
			int end = s.IndexOf("//", start);
			if(end < 0){
				end = s.Length + 1;
			}
			s.Replace(s.Substring(start, end), "//" + key + "$" + value);
		}else{
			s += "//" + key + "$" + value;
		}
	}

	private void RemoteCommand(IPEndPoint from, MsgPacket msg)
	{

	}

	//模块更新
	public override void Update()
	{
		//先发送缓冲的消息
		int minMsec = FrameMsec();
		int timeVal = 0;
		//TODO：需要根据帧率来发送包，但是Unity里面没办法控制Update的更新时间。
		do{
			int timeValSV = SendQueuedPackets();
			timeVal = CDataModel.InputEvent.TimVal(FrameMsec());
			if(timeValSV < timeVal){
				timeVal = timeValSV;
			}
			if(timeVal > 1){
				CLog.Info("run too fast, need slow down");
				break;
			}
		}while(CDataModel.InputEvent.TimVal(minMsec) > 0);


		int startTime;
		//可以允许暂停，直到本地客户端连上之后。
		if(CheckPaused()){
			return;
		}
		if(CConstVar.SV_FPS < 1){
			CConstVar.SV_FPS = 10;
		}
		int fMsec = (int)(1000 / CConstVar.SV_FPS * CConstVar.timeScale);
		if(fMsec < 1){
			CConstVar.timeScale = CConstVar.SV_FPS / 1000f;
			fMsec = 1;
		}

		timeResidual += (int)(Time.deltaTime*1000);

		//如果time接近于32位整数的最大值，就踢掉所有的客户端连接，而不是每个地方都进行检查
		if(time > 0x70000000){
			ShutDown("Restarting server due to time wrapping");
			return;
		}
		//如果有很多玩家一直在地图里面不断的玩，可能会出现这个问题
		if(nextSnapshotEntities >= 0x7FFFFFFE - numSnapshotEntities){
			ShutDown("Restarting server due to numSnapshotEntities wrapping");
		}

		if(restartTime > 0 && time > restartTime){
			restartTime = 0;
			return;
		}

		if(CConstVar.ComSpeeds > 0){
			startTime = CDataModel.InputEvent.Milliseconds();
		}else{
			startTime = 0;
		}

		CalcPings();

		//如果帧率过低，就需要在同一帧里面多次模拟
		int multiSimulation = 0;
		while(timeResidual > fMsec){
			timeResidual -= fMsec;
			time += fMsec;
		}

		if(CConstVar.ComSpeeds > 0){
			CConstVar.TimeGame = CDataModel.InputEvent.Milliseconds() - startTime;
		}

		CheckTimeouts();
		SendClientMessages();
	}

	private void RunGameSimulation(){

	}

	//释放
	public override void Dispose()
	{

	}

	//更新client.ping
	private void CalcPings(){
		int total = 0;
		int count = 0;
		int delta = 0;

		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			var cl = clients[i];
			if(cl.state != ClientState.ACTIVE){
				cl.ping = 999;
				continue;
			}
			if(cl.gEntity == null){
				cl.ping = 999;
				continue;
			}
			if((cl.gEntity.r.svFlags & SVFlags.BOT) != SVFlags.NONE){
				cl.ping = 0;
				continue;
			}

			
			for(int j = 0; j < CConstVar.PACKET_BACKUP; j++){
				if(cl.frames[j].messageAcked <= 0){
					continue;
				}
				delta = cl.frames[i].messageAcked - cl.frames[j].messageSent;
				count++;
				total += delta;
			}
			if(count == 0){
				cl.ping = 999;
			}else{
				cl.ping = total/count;
				if(cl.ping > 999){
					cl.ping = 999;
				}
			}

			clients[i].playerState.ping = cl.ping;
		}
	}

	private bool CheckPaused(){
		int count = 0;
		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			var cl = clients[i];
			if(cl.state >= ClientState.ACTIVE && !cl.netChan.isBot){
				count++;
			}
		}
		if(count > 1){
			CConstVar.SV_PAUSE = false;
			return false;
		}
		if(!CConstVar.SV_PAUSE){
			CConstVar.SV_PAUSE = true;
		}
		return false;
	}

	private void CheckTimeouts(){
		int droppoint = time - 1000 * CConstVar.SV_TimeOut;
		int zombiepoint = time - 1000 * CConstVar.SV_ZombieTime;

		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			var cl = clients[i];
			//在切换关卡的时候message time可能错乱
			if(cl.lastPacketTime > time){
				cl.lastPacketTime = time;
			}

			if(cl.state == ClientState.ZOMBIE && cl.lastPacketTime < zombiepoint){
				cl.state = ClientState.FREE;
				CLog.Info("Going from zombie to free for client {0}", i);
				continue;
			}

			if(cl.state == ClientState.CONNECTED && cl.lastPacketTime < droppoint){
				//等待几帧再断开，防止调试出现的等待
				if(++cl.timeoutCount > 5){
					DropClient(cl, "time out");
					cl.state = ClientState.FREE;
				}
			}else{
				cl.timeoutCount = 0;
			}
		}
	}

	//返回处理下一个服务器帧的毫米时间
	private int FrameMsec(){
		if(CConstVar.SV_FPS > 0){
			int frameMsec = (int)(1000f/CConstVar.SV_FPS);
			if(frameMsec < timeResidual){
				return 0;
			}else{
				return frameMsec - timeResidual;
			}
		}else{
			return 1;
		}
	}

	private void ShutDown(string message){

	}

	private void DropClient(ClientNode client, string message){

	}

	private int SendQueuedPackets(){
		//发送出fragments包，因为现在是空闲时间
		int delayT = SendQueueMessages();
		if(delayT >= 0){
			return delayT;
		}else{
			return 0;
		}
		// int timeVal = 0;
		// if(delayT >= 0){
		// 	timeVal = delayT;
		// }

		// if()
		// if(delayT )
	}

	private int SendQueueMessages(){
		int retval = -1;
		int nextFragT = 0;
		ClientNode cl;
		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			cl = clients[i];
			if(cl.state > 0){
				nextFragT = RateMsec(cl);
				if(nextFragT == 0){
					nextFragT = SVNetChanTransmitNextFragment(cl);
				}
				if(nextFragT >= 0 && (retval == -1 || retval > nextFragT)){
					retval = nextFragT;
				}
			}
		}
		return retval;
	}

	//发送下一个fragment和下一个队列中的packet。返回下一个消息可以被发送的毫秒数（基于收到的客户端帧率）
	//如果没有发送包，就返回-1
	private int SVNetChanTransmitNextFragment(ClientNode cl){
		if(cl.netChan.unsentFragments){
			CNetwork.Instance.NetChanTransmitNextFrame(ref cl.netChan);
			return RateMsec(cl);
		}else if(!cl.netChanQueue.IsEmpty){
			SVNetChanTransmitNextFragment(cl);
			return RateMsec(cl);
		}
		return -1;
	}

	//返回的是下一个消息可以发送的时间（基于帧率的设定，服务器的帧率和客户端的不一样）。
	private int RateMsec(ClientNode client){
		int msgSize = client.netChan.lastSentSize;
		int rate = client.rate;

		if(rate > CConstVar.SV_MAX_RATE) rate = CConstVar.SV_MAX_RATE;
		if(rate < CConstVar.SV_MIN_RATE) rate = CConstVar.SV_MIN_RATE;

		// if(client.netChan.remoteAddress.Add ipv6
		msgSize += CConstVar.UDPIP_HEADER_SIZE;
		int rateMsec = msgSize * 1000 / ((int)(rate * CConstVar.timeScale));
		rate = CDataModel.InputEvent.Milliseconds() - client.netChan.lastSentTime;

		if(rate > rateMsec){
			return 0;
		}else{
			return rateMsec - rate;
		}
	}

	private void SendMessageToClient(MsgPacket msg, ClientNode client){
		var frame = client.frames[client.netChan.outgoingSequence & CConstVar.PACKET_MASK];
		frame.messageSize = msg.CurSize;
		frame.messageSent = time;
		frame.messageAcked = -1;

		//SV_NetChan_Transimit, 发送报文
		msg.WriteByte((byte)SVCCmd.EOF);

		//如果有没有发送的fragment，把它们放入缓冲并以正确的顺序发送
		if(client.netChan.unsentFragments || client.netChanQueue.IsEmpty ){
			CLog.Info("NetChan Transimit: unsent fragments, stacked");
			NetChanBuffer netChanBuffer = new NetChanBuffer();

			//存储消息，不能编码后存储，因为编码依赖于我们还需要完成发送的内容
			msg.Copy(netChanBuffer.msg, netChanBuffer.bytes, netChanBuffer.bytes.Length); 

			//插入队列，消息会被编码并后续发送
			client.netChanQueue.Enqueue(netChanBuffer);
		}else{
			//
			CNetwork.Instance.NetChanTransmit(ref client.netChan, msg.CurSize, msg.Data);
		}
	}

	private void SendClientSnapshot(ClientNode client){
		byte[] msg_buf = new byte[CConstVar.MAX_MSG_LEN];

		//构建snapshot
		BuildClientSnapshot(client);

		//机器人有自己的构建方法，不需要发送
		if(client.gEntity != null && (client.gEntity.r.svFlags & SVFlags.BOT) != SVFlags.NONE){
			return;
		}


		MsgPacket msg = new MsgPacket();
		msg.AllowOverflow = true;

		//让客户端知道服务器已经收到的可靠消息。
		msg.WriteInt(client.lastClientCommand);

		//发送任何可靠的服务器指令
		UpdateServerCommandsToClient(client, msg);

		//发送所有的相应的entityState和playerState
		WriteSnapshotToClient(client, msg);

		if(msg.Overflowed){
			CLog.Info("WARNING: msg overflowed for %s", client.name);
			msg.Clear();
		}

		SendMessageToClient(msg, client);
	}

	//决定哪一个entity对客户端是可见的，并复制playerState。
	//这个函数处理多个递归的入口，但是渲染器不会。
	//对于其他玩家看到的视野，clent可以是client.gentity以外的东西
	private void BuildClientSnapshot(ClientNode client){
		snapshotCounter++;
		SvClientSnapshot frame = client.frames[client.netChan.outgoingSequence & CConstVar.PACKET_MASK];

		frame.numEntities = 0;
		SharedEntity clEnt = client.gEntity;
		if(client != null || client.state == ClientState.ZOMBIE){
			return;
		}

		PlayerState ps = client.playerState;
		frame.playerState = ps;

		int clientNum = frame.playerState.clientNum;
		if(clientNum < 0 || clientNum > CConstVar.MAX_GENTITIES){
			CLog.Error("SvEntity for gEntity: bad gEnt");
		}
		var svEnt = svEntities[clientNum];
		svEnt.snapshotCounter = snapshotCounter;

		Vector3 org = ps.origin;
		org[2] += ps.viewHeight;

		var entitiesIndex = new List<int>(CConstVar.MAX_SNAPSHOT_ENTITIES);

		AddEntitiesVisibleFromPoint(org, frame, entitiesIndex, false);

		entitiesIndex.Sort();

		frame.numEntities = 0;
		frame.firstEntity = nextSnapshotEntities;
		for(int i = 0; i < entitiesIndex.Count; i++){
			var ent = gEntities[i];
			snapshotEntities[nextSnapshotEntities % numSnapshotEntities] = ent.s;
			nextSnapshotEntities++;
			if(nextSnapshotEntities >= 0x7FFFFFFE){
				CLog.Error("SV: nextSnapshotEntities wraped");
			}
			frame.numEntities++;
		}

	}

	//
	private void AddEntitiesVisibleFromPoint(Vector3 origin, SvClientSnapshot frame, List<int> entitiesIndex, bool portal){
		for(int e = 0; e < numEntities; e++){
			SharedEntity ent = gEntities[e];

			//没有连入的entity就不发送
			if(!ent.r.linked){
				continue;
			}

			if(ent.s.entityIndex != e){
				CLog.Error("SV: entity index mismatch, fixing");
				ent.s.entityIndex = e;
			}

			//entities能被标记为发送给一个客户端
			if((ent.r.svFlags & SVFlags.SINGLE_CLIENT) != SVFlags.NONE){
				if(ent.r.singleClinet != frame.playerState.clientNum){
					continue;
				}
			}

			//entities可以标记为发送给每个人但是只有一个客户端
			if((ent.r.svFlags & SVFlags.NOTSINGLE_CLIENT) != SVFlags.NONE){
				if(ent.r.singleClinet == frame.playerState.clientNum){
					continue;
				}
			}

			//entities可以标记为发送给指定掩码的客户端
			if((ent.r.svFlags & SVFlags.CLIENT_MASK) != SVFlags.NONE){
				if(frame.playerState.clientNum > 32){
					CLog.Error("SVFlags.CLIENT_MASK: clientNum >= 32");
				}
				if((~ent.r.singleClinet & (1 << frame.playerState.clientNum)) >0){
					continue;
				}
			}

			if(ent == null || ent.s.entityIndex < 0 || ent.s.entityIndex >= CConstVar.MAX_GENTITIES){
				CLog.Error("SV: entity index not in range");
			}
			var svEnt = svEntities[ent.s.entityIndex];

			//不要添加两次entity
			if(svEnt.snapshotCounter == snapshotCounter){
				continue;
			}

			//广播的entities总会发送
			if((ent.r.svFlags & SVFlags.BROADCAST) != SVFlags.NONE){
				AddEntToSnapshot(svEnt, ent, entitiesIndex);
				continue;
			}

			//这里需要进行判断是否可见等
			AddEntToSnapshot(svEnt, ent, entitiesIndex);

			if((ent.r.svFlags & SVFlags.PORTAL) != SVFlags.NONE){
				Vector3 dir = ent.s.origin - origin;
				if(dir.magnitude > ent.s.generic1){
					continue;
				}

				AddEntitiesVisibleFromPoint(ent.s.origin2, frame, entitiesIndex, true);
			}
		}
	}

	private void AddEntToSnapshot(SvEntityState svEntity, SharedEntity gEnt, List<int> entitiesIndex){
		//如果已经添加了，就不再次添加
		if(svEntity.snapshotCounter == snapshotCounter){
			return;
		}
		svEntity.snapshotCounter = snapshotCounter;
		if(entitiesIndex.Count >= CConstVar.MAX_SNAPSHOT_ENTITIES){
			return;
		}
		entitiesIndex.Add(gEnt.s.entityIndex);
	}

	private void UpdateServerCommandsToClient(ClientNode client, MsgPacket msg){

	}

	private void WriteSnapshotToClient(ClientNode client, MsgPacket msg){
		SvClientSnapshot frame, oldFrame;
		int lastFrame, i;
		SnapFlags snapFlags;

		frame = client.frames[client.netChan.outgoingSequence & CConstVar.PACKET_MASK];

		if(client.deltaMessage <= 0 || client.state != ClientState.ACTIVE){
			oldFrame = null;
			lastFrame = 0;
		}else if(client.netChan.outgoingSequence - client.deltaMessage >= (CConstVar.PACKET_BACKUP - 3)){
			CLog.Info("%s Delta request from out of data packet.", client.name);
			oldFrame = null;
			lastFrame = 0;
		}else{
			oldFrame = client.frames[client.deltaMessage & CConstVar.PACKET_MASK];
			lastFrame = client.netChan.outgoingSequence - client.deltaMessage;

			if(oldFrame.firstEntity <= nextSnapshotEntities - numSnapshotEntities){
				CLog.Info("%s Delta request from out of data entities.", client.name);
				oldFrame = null;
				lastFrame = 0;
			}
		}

		msg.WriteByte((byte)SVCCmd.SNAPSHOT);

		if(client.oldServerTime > 0){
			msg.WriteInt(time + client.oldServerTime);
		}else{
			msg.WriteInt(time);
		}

		msg.WriteByte((byte)lastFrame);

		snapFlags = (SnapFlags)snapFlagServerBit;
		if(client.rateDelayed){
			snapFlags |= SnapFlags.RATE_DELAYED;
		}
		if(client.state != ClientState.ACTIVE){
			snapFlags |= SnapFlags.NOT_ACTIVE;
		}

		msg.WriteByte((byte)snapFlags);

		// msg.WriteByte((byte)frame.ar)
		if(oldFrame != null){
			msg.WriteDeltaPlayerstate(oldFrame.playerState, frame.playerState);
		}else{
			msg.WriteDeltaPlayerstate(null, frame.playerState);
		}

		//增量编码entities
		EmitPacketEntities(oldFrame, frame, msg);

		//padding for rate debugging
		if(CConstVar.PadPackets > 0){
			for(i = 0; i < CConstVar.PadPackets; i++){
				msg.WriteByte((byte)SVCCmd.NOP);
			}
		}


	}

	private void EmitPacketEntities(SvClientSnapshot from, SvClientSnapshot to, MsgPacket msg){
		EntityState oldEnt = new EntityState(), newEnt = new EntityState();
		int oldIndex = 0, newIndex = 0;
		int oldNum, newNum;
		int fromNumEnts;

		if(from != null){
			fromNumEnts = 0;
		}else{
			fromNumEnts = from.numEntities;
		}

		while(newIndex < to.numEntities || oldIndex < fromNumEnts){
			if(newIndex >= to.numEntities){
				newNum = 9999;
			}else{
				oldEnt = snapshotEntities[(to.firstEntity + newIndex) % numSnapshotEntities];
				newNum = oldEnt.entityIndex;
			}

			if(oldIndex >= fromNumEnts){
				oldNum = 9999;
			}else{
				oldEnt = snapshotEntities[(from.firstEntity + oldIndex) % numSnapshotEntities];
				oldNum = oldEnt.entityIndex;
			}

			if(newNum == oldNum){
				msg.WriteDeltaEntity(ref oldEnt, ref newEnt, false);
			}
		}

	}

	private void SendClientMessages(){
		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			var c = clients[i];
			if(c.state == ClientState.FREE){
				continue;
			}

			//如果packetqueue仍然是满的，就丢掉这个snapshot，否则增量压缩就会出问题
			if(c.netChan.unsentFragments || !c.netChanQueue.IsEmpty){
				c.rateDelayed = true;
				continue;
			}

			if(!IPAddress.IsLoopback(c.netChan.remoteAddress.Address) || CConstVar.LanForceRate && CNetwork.IsLANAddress(c.netChan.remoteAddress.Address)){
				if(time - c.lastSnapshotTime < c.snapshotMsec * CConstVar.timeScale){
					continue;
				}
				if(RateMsec(c) > 0){
					c.rateDelayed = true;
					continue;
				} 
			}

			//生成并发送新的消息
			SendClientSnapshot(c);
			c.lastSnapshotTime = time;
			c.rateDelayed = true;
		}
	}

	private void NetChanFreeQueue(ClientNode client)
	{
		while(client.netChanQueue.IsEmpty){

		}
	}

	

	// private PlayerState GetClientPlayer(int index){
	// 	PlayerState ps = gameClients[index];
	// }
}

public class ClientNode
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

	public SharedEntity gEntity;

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

	public CircularBuffer<NetChanBuffer> netChanQueue;

	public int oldServerTime;

	public bool[] csUpdated;

	public PlayerState playerState;

	public ClientNode(){
		state = ClientState.FREE;
		netChan = new NetChan();
		netChanQueue = new CircularBuffer<NetChanBuffer>(10); 
	}
}

public class NetChanBuffer{
	public MsgPacket msg;

	public byte[] bytes;

	public NetChanBuffer()
	{
		msg = new MsgPacket();
		bytes = new byte[CConstVar.MAX_MSG_LEN];
	}
}

public enum ClientState
{
	FREE = 0,
	ZOMBIE,
	CONNECTED,
	PRIMED,
	ACTIVE,
}

public class SvClientSnapshot
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