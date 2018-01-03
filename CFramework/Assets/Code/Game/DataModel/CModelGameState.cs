using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class CModelGameState : IModel {

	private int clientFrame;

	private int clientNum;

	public bool demoPlayback;

	private bool levelShot;

	private int deferredPlayerLoading;

	private bool loading; //

	private bool intermissionStarted;

	private int latestSnapshotNum; //客户端收到的最近的snapshots的数据

	private int latestSnapshotTime; //收到latestSnapshotNum的snapshot的时间

	public SnapShot snap; //snap.serverTime = time;

	public SnapShot nextSnap; //snap.serverTime > time or NULL

	public SnapShot[] activeSnapshots; //

	public float frameInterpolation; //(time - snap.serverTime) / (nextSnap.serverTime - snap.serverTime)

	public bool thisFrameTeleport;

	public bool nextFrameTeleport;

	/*------非延迟-------*/
	public int lastPredictedCommand;

	public int lastServerTime;

	public PlayerState[] savedPmoveState;

	public int stateHead, stateTail;


	private int frameTime; //time - oldTime

	public int time; //这是客户端当前帧渲染的时间

	public int realTime; //忽略暂停

	public int oldTime; //这是上一帧的时间，用于missile trails 和 prediction checking

	public int physicsTime; //snap.time或者nextSnap.time

	private int timeLimitWarnings; //

	private int frameLimitWarnings;

	private bool mapRestart;

	private bool renderingThirdPerson;

	public bool hyperspace; //prediction state

	public PlayerState predictedPlayerState;

	private ClientEntity preditedPlayerEntity;

	public bool validPPS; //第一次调用PredictPlayerState之后，保证predictedPlayerState是一个可访问的状态

	public int predictedErrorTime;

	public Vector3 predictedError;

	private int eventSequence;

	private int[] predictableEvents;

	private float stepChange;

	private int stepTime;

	private float duckChange;

	private int duckTime;

	private float landChange;

	private int landTime;

	//attack player
	private int attackerTime;

	public int paused = 0;

	//是否记录
	public int journal = 1;

	public ClientEntity[] clientEntities;

	private ClientActive clientActive;

	public ClientActive ClientActive{
		get{
			return clientActive;
		}
	}

	// Use this for initialization
	public void Init () {
		clientEntities = new ClientEntity[CConstVar.MAX_GENTITIES];
		ClearState();
	}
	
	public void ClearState(){
		clientActive.cmdNum = 0;
		clientActive.cmds = new UserCmd[CConstVar.CMD_BACKUP];
		clientActive.entityBaselines = new EntityState[CConstVar.MAX_GENTITIES];
		clientActive.extrapolateSnapshot = false;
		clientActive.joystickAxis = new int[CConstVar.MAX_JOYSTICK_AXIS];
		clientActive.mouseDx = new int[2];
		clientActive.mouseDy = new int[2];
		clientActive.mouseIndex = 0;
		clientActive.newSnapshots = false;
		clientActive.oldFrameServerTime = 0;
		clientActive.oldServerTime = 0;
		clientActive.outPackets = new OutPacket[CConstVar.PACKET_BACKUP];
		clientActive.parseEntities = new EntityState[CConstVar.MAX_PARSE_ENTITIES];
		clientActive.parseEntitiesIndex = 0;
		clientActive.sensitivity = 0f;
		clientActive.serverID = 0;
		clientActive.serverTime = 0;
		clientActive.serverTimeDelta = 0;
		clientActive.snap = null;
		clientActive.snapshots = new ClientSnapshot[CConstVar.PACKET_BACKUP];
		clientActive.timeoutCount = 0;
		clientActive.userCmdValue = 0;
		clientActive.viewAngles = Vector3.zero;
		
	}

	public bool GetUserCmd(int cmdNum, out UserCmd cmd)
	{
		if(cmdNum > clientActive.cmdNum){
			CLog.Error("GetUserCmd: %d >= %d", cmdNum, clientActive.cmdNum);
		}
		cmd = new UserCmd();
		if(cmdNum <= clientActive.cmdNum - CConstVar.CMD_BACKUP){
			return false;
		}

		cmd = clientActive.cmds[cmdNum & CConstVar.CMD_MASK];
		return true;
	}

	//如果snapshot解析正确，它会被复制到snap中，并被存储在snapshots[]
	//如果snapshot因为任何原因不合适，不会任何改变任何state
	public void ParseSnapshot(MsgPacket packet){
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

		this.paused = 0;

		newSnap.messageNum = connection.serverMessageSequence;

		deltaNum = packet.ReadByte();
		if(deltaNum == 0){
			newSnap.deltaNum = -1;
		}else{
			newSnap.deltaNum = newSnap.messageNum - deltaNum;
		}
		newSnap.snapFlags = (SnapFlags)packet.ReadByte();

		// var clientActive = CDataModel.GameState.ClientActive;
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
				clientActive.snap.ping = this.realTime - clientActive.outPackets[packetNum].realTime;
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

	public void ParseGamestate(MsgPacket packet)
	{
		int i;
		EntityState entityState;
		int newNum;
		EntityState nullState;
		int cmd;
		string s = null;

		var connection = CDataModel.Connection;
		connection.connectPacketCount = 0;

		//清除本地客户端状态
		CDataModel.GameState.ClearState();

		//gamestate总会标记一个服务器command sequence
		connection.serverCommandSequence = packet.ReadInt();

		// CDataModel.GameState.clientActive.
		while(true){
			cmd = packet.ReadByte();
			if(cmd == (int)SVCCmd.EOF){
				break;
			}
			if(cmd == (int)SVCCmd.CONFIG_STRING){
				int len;

				i = packet.ReadShot();
				if(i < 0 || i > CConstVar.MAX_CONFIGSTRINGS){
					CLog.Error(" configstring > MAX_CONFIGSTRING");
				}
				s = packet.ReadBigString();
				len = s.Length;


			}else if(cmd == (int)SVCCmd.BASELINE){
				newNum = packet.ReadBits(CConstVar.GENTITYNUM_BITS);
				if(newNum < 0 || newNum >= CConstVar.MAX_GENTITIES){
					CLog.Error("Baseline number out of range: %d", newNum);
				}

				nullState = new EntityState();
				entityState = clientActive.entityBaselines[newNum];
				ReadDeltaEntity(packet, ref nullState, ref entityState, newNum);
			}else{
				CLog.Error("ParseGameState: bad command type");
			}
		}

		connection.clientNum = packet.ReadInt();

		connection.checksumFeed = packet.ReadInt();
		

		ParseServerInfo(s);

		//处理server id和其他信息
		SystemInfoChanged(s);


	}

	private void ParseServerInfo(string systemInfo)
	{

	}

	private void SystemInfoChanged(string systemInfo)
	{

	}

	//TODO：这里有问题，这个函数做了一个假设，可以新增entity，但是不能删除entity，因为删除了，index就会出现错乱
	//不过其实问题不大，因为是用的index，所以entity变换成为另外一个entity也不是问题（删除的话先不管，如果新增，就补位到那个entity的位置）。
	private void ParseEntities(MsgPacket packet, ClientSnapshot oldframe, ClientSnapshot newframe)
	{
		int newIndex;
		EntityState oldState = new EntityState();
		int oldIndex, lastIndex;

		newframe.parseEntitiesIndex = clientActive.parseEntitiesIndex;
		newframe.numEntities = 0;

		//
		oldIndex = 0;
		// oldState = 
		if(oldframe == null){
			lastIndex = 99999;
		}else
		{
			if(oldIndex >= oldframe.numEntities){
				lastIndex = 99999;
			}else{
				oldState = clientActive.parseEntities[(oldframe.parseEntitiesIndex + oldIndex) & (CConstVar.MAX_PARSE_ENTITIES - 1)];
				lastIndex = oldState.entityIndex;
			}
		}

		while(true)
		{
			//读取entity的索引值
			//服务器会根据number从小到大排序，保证小的总在前面
			newIndex = packet.ReadBits(CConstVar.GENTITYNUM_BITS);
			if(newIndex == (CConstVar.MAX_GENTITIES - 1)){
				break;
			}

			if(packet.CurPos > packet.CurSize){
				CLog.Error("ParseEntities: end of message");
				break;
			}

			while(lastIndex < newIndex)
			{
				//来自旧数据包中的一个或者多个entity没有发生变化
				if(CConstVar.ShowNet == 3){
					CLog.Info("entity unchanged: %d", lastIndex);
				}
				DeltaEntity(packet, newframe, lastIndex, ref oldState, true);

				oldIndex++;

				if(oldIndex >= oldframe.numEntities){
					lastIndex = 99999;
				}else{
					oldState = clientActive.parseEntities[(oldframe.parseEntitiesIndex + oldIndex) & (CConstVar.MAX_PARSE_ENTITIES - 1)];
					lastIndex = oldState.entityIndex;
				}
			}

			if(lastIndex == newIndex)
			{
				//增量来自于前一帧
				if(CConstVar.ShowNet == 3){
					CLog.Info("delta entity: %d",newIndex);
				}
				DeltaEntity(packet, newframe,newIndex, ref oldState, false);

				oldIndex++;

				if(oldIndex >= oldframe.numEntities){
					lastIndex = 99999;
				}else{
					oldState = clientActive.parseEntities[(oldframe.parseEntitiesIndex+oldIndex) & (CConstVar.MAX_PARSE_ENTITIES - 1)];
					lastIndex = oldState.entityIndex;
				}
				continue;
			}

			//上一帧中没有对应的数据
			if(lastIndex > newIndex){
				if(CConstVar.ShowNet == 3){
					CLog.Info("base line entity: %d", newIndex);
				}
				DeltaEntity(packet, newframe, newIndex, ref clientActive.entityBaselines[newIndex], false);
				continue;
			}
		}

		//oldframe内剩下的entities都直接复制过去
		//只有客户端新增了entity时，lastIndex == 99999
		while(lastIndex != 99999)
		{
			if(CConstVar.ShowNet == 3){
				CLog.Info("unchanged entity: %d", lastIndex);
			}
			DeltaEntity(packet, newframe, lastIndex, ref oldState, true);
			
			oldIndex++;

			if(oldIndex >= oldframe.numEntities){
				lastIndex = 99999;
			}else{
				oldState = clientActive.parseEntities[(oldframe.parseEntitiesIndex + oldIndex) & (CConstVar.MAX_PARSE_ENTITIES - 1)];
				lastIndex = oldState.entityIndex;
			}
		}
	}

	private void DeltaEntity(MsgPacket msg, ClientSnapshot frame, int newNum, ref EntityState old, bool unchanged)
	{
		EntityState state = clientActive.parseEntities[clientActive.parseEntitiesIndex & (CConstVar.MAX_PARSE_ENTITIES - 1)];
		if(unchanged){
			state = old;
		}else{
			ReadDeltaEntity(msg, ref old, ref state, newNum);
		}
		if(state.entityIndex == (CConstVar.MAX_GENTITIES - 1)){
			return;
		}
		clientActive.parseEntitiesIndex++;
		frame.numEntities++;
	}

	//entity的索引值已经从消息中读取，这是from的state用来标识用的
	//如果delta移除了这个entity，entityState.entityIndex会被设置为MAX_GENTITIES - 1
	//可以从baseline中获取，或者从前面的packet_entity中获取
	private void ReadDeltaEntity(MsgPacket msg, ref EntityState from, ref EntityState to, int number)
	{
		int i, lc;
		int numFields;
		NetField field;
		int fromF, toF;
		int print;
		int trunc;
		int startBit, endBit;

		if(number < 0 || number >= CConstVar.MAX_GENTITIES){
			CLog.Error("Bad delta entity number: %d", number);
		}

		if(msg.Bit == 0){
			startBit = msg.CurPos * 8 - CConstVar.GENTITYNUM_BITS;
		}else{
			startBit = (msg.CurPos - 1) * 8 + msg.Bit - CConstVar.GENTITYNUM_BITS;
		}

		//检查是否要移除
		if(msg.ReadBits(1) == 1){
			to = new EntityState();
			to.entityIndex = CConstVar.MAX_GENTITIES - 1;
			if(CConstVar.ShowNet != 0 && (CConstVar.ShowNet >= 2 || CConstVar.ShowNet == -1)){
				CLog.Info("remove entity: %d", number);
			}
			return;
		}

		//检查是否无压缩
		if(msg.ReadBits(1) == 0){
			to = from;
			to.entityIndex = number;
			return;
		}

		numFields = CConstVar.entityStateFields.Length;
		lc = msg.ReadByte();
		if(lc > numFields || lc < 0){
			CLog.Info("invalid entity state field count");
		}

		if(CConstVar.ShowNet != 0 && (CConstVar.ShowNet >= 2 || CConstVar.ShowNet == -1)){
			print = 1;
			CLog.Info("delta count %d: #-%d", msg.CurPos*3, to.entityIndex);
		}else{
			print = 0;
		}

		to.entityIndex = number;

		for(i = 0; i < lc; i++){
			field = CConstVar.entityStateFields[i];
			// fromF = from + field.offset;
			// toF = to 

			//最好的方式还是c++直接操作内存数据，利用反射效率比较低
			if(msg.ReadBits(1) == 0){ //没有变化
				field.name.SetValue(to, field.name.GetValue(from));
			}else{
				if(field.bits == 0){
					if(msg.ReadBits(1) == 0){
						field.name.SetValue(to, 0.0f);
					}else{
						if(msg.ReadBits(1) == 0){
							//积分浮点数
							trunc = msg.ReadBits(CConstVar.FLOAT_INT_BITS);

							//偏移允许正的部分和负的部分是一样大小的
							trunc -= CConstVar.FLOAT_INT_BIAS;
							field.name.SetValue(to, trunc);

							if(print > 0){
								CLog.Info("Read Delta Entity %s:%d", field.name, trunc);
							}
						}else{
							//完整的浮点值
							field.name.SetValue(to, msg.ReadBits(32));
							if(print > 0){
								CLog.Info("Read Delta Entity %s:%d", field.name, field.name.GetValue(to));
							}
						}
					}
				}else{
					if(msg.ReadBits(1) == 0){
						field.name.SetValue(to, 0);
					}else{
						field.name.SetValue(to, msg.ReadBits(field.bits));
						if(print > 0){
							CLog.Info("Read Delta Entity %s:%d", field.name, field.name.GetValue(to));
						}
					}
				}
			}
		}
		for(i = lc; i < numFields; i++){
			field = CConstVar.entityStateFields[i];
			//没有变化
			field.name.SetValue(to, field.name.GetValue(from));
		}

		if(print > 0){
			if(msg.Bit == 0){
				endBit = msg.CurPos * 8 - CConstVar.GENTITYNUM_BITS;
			}else{
				endBit = (msg.CurPos - 1) * 8 + msg.Bit - CConstVar.GENTITYNUM_BITS;
			}
			CLog.Info("Read Delta Entity Finished. (%d bits)", endBit - startBit);
		}

	}

	private void ReadDeltaPlayerstate(MsgPacket packet, PlayerState from, PlayerState to)
	{
		int i,lc;
		int bits;

	}
	
	public void Dispose () {
		
	}
}

public struct ClientActive
{
	public int timeoutCount;

	public ClientSnapshot snap;

	public int serverTime;

	public int oldServerTime; //防止时间倒退的情况

	public int oldFrameServerTime;

	public int serverTimeDelta; // serverTime = realTime + serverTimeDelta;这个时间会随着延迟而变化

	public bool extrapolateSnapshot; //在任何客户端帧被强制向外插值时设置

	public bool newSnapshots; //在解包了任何可用的消息时设置

	public int parseEntitiesIndex;

	public int[] mouseDx;

	public int[] mouseDy;

	public int mouseIndex;

	public int[] joystickAxis;

	public int userCmdValue;

	public float sensitivity;

	public UserCmd[] cmds;

	public int cmdNum; //每一帧都在增长，因为可能好几帧被打包到一起

	public OutPacket[] outPackets;

	public Vector3 viewAngles;

	public int serverID;

	public ClientSnapshot[] snapshots;

	//用于增量更新，如果没有在上一帧里面那么就用baseline内的来计算
	public EntityState[] entityBaselines; 

	//parseEntities保存了每一帧的所有entityState，
	//每一帧起始的索引值是ClientSnapshot.parseEntitiesIndex
	//然后ClientSnapshot.numEntities表示了这一帧所有的entity数量
	public EntityState[] parseEntities;

	public void Init()
	{
		entityBaselines = new EntityState[CConstVar.MAX_SNAPSHOT_ENTITIES * CConstVar.PACKET_BACKUP];
	}
}

//客户端所用的snapshot，
public class ClientSnapshot
{
	public bool valid;

	public SnapFlags snapFlags;

	public int serverTime;

	public int messageNum;

	public int deltaNum;

	public int ping;

	public int cmdNum;

	public PlayerState playerState;

	public int numEntities; //所需要在这一帧显示的entity的数量

	public int parseEntitiesIndex; //指向循环列表内的索引值

	public int serverCommandNum; //执行这个指令之前的所有指令，使这一帧成为当前帧。
}

//每一帧的数据，给定时间的服务器呈现。
//服务器会在固定时间产生，不过如果客户端的帧率太高了，有可能不会发送给客户端，或者网络传输过程中被丢弃了。
public class SnapShot{
	public SnapFlags snapFlags;

	public int ping;

	public int serverTime; //服务器时间（毫秒）

	public PlayerState playerState; //玩家当前的所有信息

	public int numEntities;

	public EntityState[] entities; //所有需要展示的entity

	public int numServerCommands;

	public int serverCommandSequence;

}

public struct OutPacket
{
	public int cmdNum;

	public int serverTime;

	public int realTime; //packet发送时的realTime
}

public struct NetField{
	//根据反射来获取值
	public FieldInfo name;

	public int offset;

	public int bits; //0表示浮点数
}