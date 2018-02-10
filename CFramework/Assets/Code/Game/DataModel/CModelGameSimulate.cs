using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//游戏模拟模块，只负责逻辑，不负责表现
public class CModelGameSimulate : CModelBase {

	private bool inited = false;

	private int frameNum;

	//限定了最大的客户端数量
	private GameClient[] clients;

	//entity的数量是比较多的，因为包含了所有游戏中出现的entity
	private GameEntity[] gEntities;

	private int time;

	private int prevTime;

	private int startTime;

	private int frameStartTime; //服务器帧实际开始的时间

	private int numEntities;

	private int numConnectedClients;

	private int numNonSpectatorClients;

	private int numPlayingClients;

	private int follow1; //自动观察的client

	private int follow2; //自动观察的client

	private int[] teamVoteTime;

	private int[] teamVoteYes;

	private int[] teamVoteNo;

	private int[] numTeamVotingClients;

	private bool spawning;

	private GameEntity[] bodyQue;

	private int bodyQueueIdx;

	private bool restarted = false;

	private bool mapRestart = true;

	private MapData map;

	private bool hadBots;

	public override void Init(){
		inited = true;
		
		// update = Update;
		//总是为最大的客户端数量留足空间
		numEntities = CConstVar.MAX_CLIENTS;
		gEntities = new GameEntity[CConstVar.MAX_GENTITIES];
		for(int i = 0; i <CConstVar.MAX_GENTITIES; i++){
			gEntities[i] = new GameEntity();
		}
		clients = new GameClient[CConstVar.MAX_CLIENTS];
		for(int i = 0; i < CConstVar.MAX_CLIENTS; i++){
			clients[i] = new GameClient();
			gEntities[i].client = clients[i];
			gEntities[i].classname = "clientslot";
		}
		bodyQue = new GameEntity[CConstVar.BodyQueueSize];

		time = startTime = (int)(Time.realtimeSinceStartup*1000);
		
		Server.Instance.LocateGameData(gEntities, numEntities, clients);

		//为死亡的玩家恢复尸体
		bodyQue = new GameEntity[CConstVar.BodyQueueSize];
		bodyQueueIdx = 0;
		for(int i = 0; i < CConstVar.BodyQueueSize; i++){
			var ent = Spawn();
			ent.classname = "bodyque";
			ent.neverFree = true;
			bodyQue[i] = ent;
		}

		SpawnEntityFromConfig();

		map = new MapData();
		map.Init();
	}

	private GameEntity Spawn(){
		GameEntity e = null;
		int i = 0;
		for(int force = 0; force < 2; force++){
			e = gEntities[CConstVar.MAX_CLIENTS];
			//遍历所有的entity，但是找不到一个可用的
			for(i = CConstVar.MAX_CLIENTS; i < numEntities; i++){
				e = gEntities[CConstVar.MAX_CLIENTS + i];
				if(e.inuse){
					continue;
				}

				if(force == 0 && (e.freeTime > startTime + 2000) && (time - e.freeTime < 1000)){
					continue;
				}

				InitGEntity(e);
				return e;
			}
			if( i != CConstVar.MAX_GENTITIES){
				break;
			}
		}
		if(i == CConstVar.ENTITYNUM_MAX_NORMAL){
			for( i = 0; i < CConstVar.MAX_GENTITIES; i++){
				CLog.Info("gEntity name: {0}", gEntities[i].classname);
			}
			CLog.Error("Spawn: no free entities");
		}
		numEntities++;
		Server.Instance.SetGEntsNum(numEntities);

		InitGEntity(e);
		return e;
	}

	private void SpawnEntityFromConfig(){

	}


	private void InitGEntity(GameEntity e){
		e.inuse = true;
		e.classname = "noclass";
		e.sEnt.s.entityIndex = System.Array.IndexOf(gEntities, e);
		e.sEnt.r.ownerNum = CConstVar.ENTITYNUM_NONE;

	}

	private bool EntitiesFree(){
		GameEntity e = gEntities[CConstVar.MAX_CLIENTS];
		for(int i = CConstVar.MAX_CLIENTS; i < numEntities; i++){
			if(e.inuse){
				continue;
			}
			return true;
		}
		return false;
	}

	private void FreeEntity(GameEntity ent){
		Server.Instance.UnLinkEntity(ent);
		if(ent.neverFree){
			return;
		}

		ent.Reset();
		ent.classname = "freed";
		ent.freeTime = time;
		ent.inuse = false;
	}

	public void Update(int sv_time){
		if(restarted){
			return;
		}

		frameNum++;
		prevTime = time;
		time = sv_time;//(int)(Time.realtimeSinceStartup*1000);

		GameEntity ent = gEntities[0];
		for(int i = 0; i < numEntities; i++){
			if(!ent.inuse){
				continue;
			}

			if(time - ent.eventTime > CConstVar.EVENT_VALID_MSEC){
				if(ent.sEnt.s.eventID > 0){
					ent.sEnt.s.eventID = 0;
					if(ent.client != null){
						ent.client.playerState.externalEvent = 0;
					}
				}

				if(ent.freeAfterEvent){
					FreeEntity(ent);
					continue;
				}else if(ent.unlinkAfterEvent){
					ent.unlinkAfterEvent = false;
					Server.Instance.UnLinkEntity(ent);
				}
			}

			//临时的entities不会update
			if(ent.freeAfterEvent){
				continue;
			}

			var sType = ent.sEnt.s.entityType;
			if(!ent.sEnt.r.linked && ent.neverFree){
				continue;
			}

			if(sType == EntityType.MISSILE){
				RunMissile(ent);
				continue;
			}

			if(sType == EntityType.ITEM){
				RunItem(ent);
				continue;
			}

			
			if(i < CConstVar.MAX_CLIENTS){
				RunClient(ent);
				continue;
			}

			RunThink(ent);

			ent = gEntities[0];
			for(i = 0; i < CConstVar.MAX_CLIENTS; i++){
				ent = gEntities[i];
				if(ent.inuse){
					ClientEndFrame(ent);
				}
			}

			CheckExitRules();

			if(CConstVar.ListEntity){
				for(i = 0; i < CConstVar.MAX_GENTITIES; i++){
					CLog.Info("gEntity: {0}", gEntities[i].classname);
				}
			}
		}
	}

	private void RunMissile(GameEntity ent){

	}

	private void RunItem(GameEntity ent){

	}

	private void RunClient(GameEntity ent){
		if((ent.sEnt.r.svFlags & SVFlags.BOT) != SVFlags.NONE){
			return;
		}
		ent.client.pers.cmd.serverTime = time;

		ClientThink_real(ent);
	}

	//在每帧的末尾调用
	private void ClientEndFrame(GameEntity ent){
		int frames;

		GameClient cl = ent.client;
		if(cl.sess.sessionTeam == TeamType.TEAM_SPECTATOR || ent.client.isEliminated){
			SpectatorClientEndFrame(ent);
			return;
		}

		DamageFeedback(ent);
		// ent.client.playerState.states[CConstVar.STAT_HEALTH];

		CUtils.PlayerStateToEntityState(ent.client.playerState, ref ent.sEnt.s, true);

		SendPendingPredictableEvents(ent.client.playerState);

		ent.client.playerState.entityFlags &= ~EntityFlags.CONNECTION;
		frames = frameNum - ent.client.lastUpdateFrame - 1;

		if(frames > 2){
			frames = 2;

			ent.client.playerState.entityFlags |= EntityFlags.CONNECTION;
			ent.sEnt.s.entityFlags |= EntityFlags.CONNECTION;
		}

		if(frames > 0){
			PredictPlayerMove(ent,(float)frames/CConstVar.SV_FPS);

		}

		ent.client.StoreHistory(ent.sEnt, time);
	}

	private void MapRestart(){
		mapRestart = true;
		
		map.Init();
	}

	private void DamageFeedback(GameEntity ent){

	}

	private void SendPendingPredictableEvents(PlayerState ps){

	}

	private void PredictPlayerMove(GameEntity ent, float frameTime){

	}

	private void SpectatorClientEndFrame(GameEntity ent){

	}

	private void SpectatorThink(GameEntity ent, UserCmd cmd){

	}

	private void RunThink(GameEntity ent){

	}

	public void ClientThink( int clientNum ) {
		GameEntity ent;

		ent = gEntities[clientNum];
		Server.Instance.GetUserCmd(clientNum, ref ent.client.pers.cmd);

		//Unlagged: commented out
		// mark the time we got info, so we can display the
		// phone jack if they don't get any for a while
		//ent->client->lastCmdTime = level.time;

		if ( (ent.sEnt.r.svFlags & SVFlags.BOT) != SVFlags.NONE ) {
			ClientThink_real( ent );
		}
	}

	private void ClientThink_real(GameEntity ent){
		GameClient cl = ent.client;

		if(cl.pers.connected != ClientConnState.CONNECTED){
			return;
		}

		UserCmd ucmd = ent.client.pers.cmd;
		if(ucmd.forwardmove != 0 || ucmd.rightmove != 0){
			CLog.Info("cmd move {0}, {1}", ucmd.forwardmove, ucmd.rightmove);
		}
		if(ucmd.serverTime > time + 200){
			ucmd.serverTime = time + 200;
		}

		if(ucmd.serverTime < time - 1000){
			ucmd.serverTime = time - 1000;
		}

//unlagged - backward reconciliation #4
		//frameOffset应该是一帧内收到命令数据包的偏移毫秒数，依赖于服务器运行RunFrame的速度
		cl.frameOffset = CDataModel.InputEvent.Milliseconds() - frameStartTime;
//unlagged - backward reconciliation #4

//unlagged - lag simulation #3
		if(cl.pers.plOut > 0){
			float thresh = cl.pers.plOut / 100f;
			if(CUtils.Random() < thresh){
				//这是丢失了的命令，不做任何事情
				return;
			}
		}

//unlagged - lag simulation #3

//unlagged - true ping
		cl.pers.pingSamples[cl.pers.sampleHead] = prevTime + cl.frameOffset - ucmd.serverTime;
		cl.pers.sampleHead++;
		if(cl.pers.sampleHead >= CConstVar.NUM_PING_SAMPLES){
			cl.pers.sampleHead -= CConstVar.NUM_PING_SAMPLES;
		}

		if(CConstVar.TruePing){
			int sum = 0;
			for(int i = 0; i < CConstVar.NUM_PING_SAMPLES;i++){
				sum += cl.pers.pingSamples[i];
			}
			cl.pers.realPing = sum / CConstVar.NUM_PING_SAMPLES;
		}else{
			cl.pers.realPing = cl.playerState.ping;
		}
		//unlagged - true ping

//unlagged - lag simulation #2
	// 	cl.pers.cmdqueue[cl.pers.cmdhead] = cl.pers.cmd;
	// 	cl.pers.cmdhead++;
	// 	if(cl.pers.cmdhead >= CConstVar.MAX_LATENT_CMDS){
	// 		cl.pers.cmdhead -= CConstVar.MAX_LATENT_CMDS;
	// 	}

	// 	if(cl.pers.latentCmds > 0){
	// 		int time = ucmd.serverTime;

	// 		int cmdindex = cl.pers.cmdhead - cl.pers.latentCmds - 1;
	// 		while(cmdindex < 0){
	// 			cmdindex += CConstVar.MAX_LATENT_CMDS;
	// 		}
	// 		cl.pers.cmd = cl.pers.cmdqueue[cmdindex];
	// 		cl.pers.realPing += time - ucmd.serverTime;
	// 	}
//unlagged - lag simulation #2

		cl.attackTime = ucmd.serverTime;

		cl.lastUpdateFrame = frameNum;

//unlagged - lag simulation #1
		// if(cl.pers.latentSnaps > 0){
		// 	cl.pers.realPing += cl.pers.latentSnaps * (1000 / CConstVar.SV_FPS);

		// 	cl.attackTime -= cl.pers.latentSnaps * (1000 / CConstVar.SV_FPS);
		// }
//unlagged - lag simulation #1

//unlagged - true ping

		if(cl.pers.realPing < 0){
			cl.pers.realPing = 0;
		}
//unlagged - true ping

		int msec = ucmd.serverTime - cl.playerState.commandTime;
		if(msec > 200){
			msec = 200;
		}

		if(CConstVar.PMoveMsec < 8){
			CConstVar.PMoveMsec = 8;
		}

		if(CConstVar.PMoveMsec > 33){
			CConstVar.PMoveMsec = 33;
		}

		if(CConstVar.PMoveFixed > 0 || cl.pers.pmoveFixed){
			ucmd.serverTime = ((ucmd.serverTime + CConstVar.PMoveMsec - 1) / CConstVar.PMoveMsec) * CConstVar.PMoveMsec;
		}

		if(cl.sess.sessionTeam == TeamType.TEAM_SPECTATOR || cl.isEliminated){
			if(cl.sess.spectatorState == SpectatorState.SCOREBOARD){
				return;
			}
			SpectatorThink(ent, ucmd);
			return;
		}

		if(cl.noclip){
			cl.playerState.pmType = PMoveType.NOCLIP;
		}else if(cl.playerState.states[CConstVar.STAT_HEALTH] <= 0){
			cl.playerState.pmType = PMoveType.DEAD;
		}else{
			cl.playerState.pmType = PMoveType.NORMAL;
		}

		// cl.playerState.gravity = 
		cl.playerState.speed = CConstVar.Speed;

		int oldEventSeq = cl.playerState.eventSequence;
		PMove pm = new PMove();

		pm.playerState = cl.playerState;
		pm.agent = cl.agent.Model;
		ucmd.CopyTo(pm.cmd);
		if(pm.playerState.pmType == PMoveType.DEAD){
			// pm.tracemask
		}else if((ent.sEnt.r.svFlags & SVFlags.BOT) != SVFlags.NONE){
			// pm.
		}else{

		}
		pm.pmoveFixed = CConstVar.PMoveFixed | (cl.pers.pmoveFixed ? 1 : 0);
		pm.pmoveMsec = CConstVar.PMoveMsec;
		pm.pmoveFloat = CConstVar.PMoveFloat;
		pm.pmvoveFlags = CConstVar.dmFlags;

		cl.oldOrigin = cl.playerState.origin;

		pm.Move();

		if(ent.client.playerState.eventSequence != oldEventSeq){
			ent.eventTime = time;
		}

		if(CConstVar.SmoothClients > 1){
			CUtils.BG_PlayerStateToEntityStateExtraPolate(ent.client.playerState, ref ent.sEnt.s, ent.client.playerState.commandTime, true);
		}else{
			CUtils.PlayerStateToEntityState(ent.client.playerState, ref ent.sEnt.s, true);
		}

		SendingPredictableEvents(ent.client.playerState);
		if((ent.client.playerState.entityFlags & EntityFlags.FIRING) == 0){
			// cl.fireHeld = false;
		}
		ent.sEnt.s.pos.GetTrBase(ref ent.sEnt.r.currentOrigin);
		// ent.sEnt.r.mins = pm.mins;

		//执行客户端事件
		ClientEvents(ent, oldEventSeq);

		Server.Instance.LinkEntity(ent);
		if(!ent.client.noclip){
			TouchTrigger(ent);
		}

		ent.sEnt.r.currentOrigin = ent.client.playerState.origin;

		ClientImpacts(ent, ref pm);

		//保存触发器和客户端事件
		if(ent.client.playerState.eventSequence != oldEventSeq){
			ent.eventTime = time;
		}

		cl.oldButtons = cl.buttons;
		cl.buttons = ucmd.buttons;
		cl.latched_buttons |= cl.buttons & ~cl.oldButtons;

		if(cl.playerState.states[CConstVar.STAT_HEALTH] <= 0){
			if((time > cl.respawnTime)){
				ClientRespawn(ent);
			}
			return;
		}

	}

	private void ClientRespawn(GameEntity ent){

	}

	private void ClientEvents(GameEntity ent, int oldEventSeq){

	}

	private void TouchTrigger(GameEntity ent){

	}

	private void ClientImpacts(GameEntity ent, ref PMove pm){

	}

	private void SendingPredictableEvents(PlayerState playerState){
		if(playerState.entityEventSequence < playerState.eventSequence){
			int seq = playerState.entityEventSequence & (CConstVar.MAX_PS_EVENTS - 1);
			int evt = playerState.events[seq] | ((playerState.entityEventSequence & 3) << 8);
			int extEvent = playerState.externalEvent;
			playerState.externalEvent = 0;
			GameEntity t = TempEntity(playerState.origin, evt);
			int idx = t.sEnt.s.entityIndex;
			CUtils.PlayerStateToEntityState(playerState, ref t.sEnt.s, true);
			t.sEnt.s.entityIndex = idx;
			t.sEnt.s.entityType = EntityType.EVENTS_COUNT + evt;
			t.sEnt.s.entityFlags |= EntityType.PLAYER;
			t.sEnt.s.otherEntityIdx = playerState.clientNum;
			t.sEnt.r.svFlags |= SVFlags.NOTSINGLE_CLIENT;
			playerState.externalEvent = extEvent;
			// int t = 
		}
	}

	private GameEntity TempEntity(Vector3 origin, int evt)
	{
		var e = Spawn();
		e.sEnt.s.entityType = EntityType.EVENTS_COUNT + evt;
		e.classname = "tempEntity";
		e.eventTime = time;
		e.freeAfterEvent = true;

		SetOrigin(e, origin);
		Server.Instance.LinkEntity(e);
		return e;
	}

	private void CheckExitRules(){

	}

	public void ClientBegin(int clientIdx){
		string userinfo = Server.Instance.GetUserInfo(clientIdx);
		GameEntity ent = gEntities[clientIdx];

		GameClient cl = clients[clientIdx];

		if(ent.sEnt.r.linked){
			Server.Instance.UnLinkEntity(ent);
		}
		InitGEntity(ent);
		ent.client = cl;
		cl.Init(); //只有在用到的时候进行初始化

		cl.pers.connected = ClientConnState.CONNECTED;
		cl.pers.enterTime = time;

		if((ent.sEnt.r.svFlags & SVFlags.BOT) != SVFlags.NONE){
			if(!hadBots){
				hadBots = true;
			}
		}

		int countFree = TeamCount(-1, TeamType.TEAM_FREE);
		int countRed = TeamCount(-1, TeamType.TEAM_RED);
		int countBlue = TeamCount(-1, TeamType.TEAM_BLUE);

		//保存entityFlags，因为改变队伍会引起这个值变化，需要保证teleport bit正确设置。
		int flags = cl.playerState.entityFlags;
		cl.playerState.Reset();

		if(cl.sess.sessionTeam != TeamType.TEAM_SPECTATOR){
			PlayerRestore(CUtils.GetValueForKey(userinfo, "cl_guid"), ref cl.playerState);
		}

		cl.playerState.entityFlags = flags;
		ClientSpawn(ent);

		// if(cl.sess.sessionTeam != TeamType.TEAM_SPECTATOR && ())

	}

	public void ClientDisconnect(int clientIdx){
		CDataModel.GameBot.RemoveQueueBotBegine(clientIdx);

		GameEntity tent;
		GameEntity ent = gEntities[clientIdx];
		if(ent.client == null){
			return;
		}

		//停掉任何跟随的客户端
		var level = CDataModel.GameSimulate;
		for(int i = 0; i < CConstVar.maxClient; i++){
			if(level.clients[i].sess.sessionTeam == TeamType.TEAM_SPECTATOR &&
			level.clients[i].sess.spectatorState == SpectatorState.FOLLOW &&
			level.clients[i].sess.spectatorClient == clientIdx){
				StopFollowing(gEntities[clientIdx]);
			}
		}

		if(ent.client.pers.connected == ClientConnState.CONNECTED
			&& ent.client.sess.sessionTeam != TeamType.TEAM_SPECTATOR){
			tent = TempEntity(ent.client.playerState.origin, (int)EntityEventType.PLAYER_TELEPORT_OUT);
			tent.sEnt.s.clientNum = ent.sEnt.s.clientNum;

		}

		Server.Instance.UnLinkEntity(ent);
		ent.sEnt.s.sourceID = 0;
		ent.inuse = false;
		ent.classname = "disconnected";
		ent.client.pers.connected = ClientConnState.DISCONNECTED;
		ent.client.playerState.persistant[CConstVar.PERS_TEAM] = (int)TeamType.TEAM_FREE;
		ent.client.sess.sessionTeam = TeamType.TEAM_FREE;

		if((ent.sEnt.r.svFlags & SVFlags.BOT ) != SVFlags.NONE){
			CDataModel.GameBot.BotAIShutdownClient(clientIdx, false);
		}
	}

	private void StopFollowing(GameEntity ent){
		ent.client.playerState.persistant[CConstVar.PERS_TEAM] = (int)TeamType.TEAM_SPECTATOR;
		ent.client.sess.sessionTeam = TeamType.TEAM_SPECTATOR;
		ent.client.sess.spectatorState = SpectatorState.FREE;
		ent.client.playerState.pmFlags &= ~PMoveFlags.FOLLOW;
		ent.sEnt.r.svFlags &= ~ SVFlags.BOT;
		ent.client.playerState.clientNum = System.Array.IndexOf(gEntities, ent); 
	}

	private void ClientSpawn(GameEntity ent){
		int index = System.Array.IndexOf(gEntities, ent);
		var cl = ent.client;
		Vector3 spawnPoint;
		Vector3 spawnOrigin = Vector3.zero;
		Vector3 spawnAngles = Vector3.zero;

		cl.sess.spectatorState = SpectatorState.NOT;
		cl.playerState.pmType = PMoveType.NORMAL;

		if(cl.sess.sessionTeam == TeamType.TEAM_SPECTATOR || cl.playerState.pmType == PMoveType.SPECTATOR){
			// spawnPoint = SelectSpectatorSpawnPoint();
			spawnPoint = map.SpectatorSpawnPoint(spawnOrigin, spawnAngles);
		}else{
			if((ent.sEnt.r.svFlags & SVFlags.BOT) != SVFlags.NONE){
				
			}
			if(!cl.pers.initialSpawn && cl.pers.localClient){
				cl.pers.initialSpawn = true;
				// spawnPoint = SelectInitialSpawnPoint(spawnOrigin, spawnAngles);
				spawnPoint = map.InitialSpawnPoint(spawnOrigin, spawnAngles);
			}else
			{
				// spawnPoint = SelectSpawnPoint(cl.playerState.origin, spawnOrigin, spawnAngles);
				spawnPoint = map.SpawnPoint(spawnOrigin, spawnAngles);
			}
		}

		// ent.sEnt.s.entityFlags &= ~
		int flags = ent.client.playerState.entityFlags & (EntityFlags.TELEPORT_BIT | EntityFlags.VOTED);
		flags ^= EntityFlags.TELEPORT_BIT;

		ResetHistory(ent);
		ent.client.saved.time = 0;

		GameClientPersistant saved = cl.pers;
		GameClientSession savedSess = cl.sess;
		int savedPing = cl.playerState.ping;
		int vote = cl.vote;

		cl.lastkilled_client = -1;
		// cl.playerState.persistant
		cl.playerState.persistant[CConstVar.PERS_SPAWN_COUNT]++;
		cl.playerState.persistant[CConstVar.PERS_TEAM] = (int)cl.sess.sessionTeam;

		var userinfo = Server.Instance.GetUserInfo(index);
		var hp = CUtils.GetValueForKey(userinfo, "handicap");
		if(!string.IsNullOrEmpty(hp)){
			cl.pers.maxHealth = System.Convert.ToInt32(hp);
		}else{
			cl.pers.maxHealth = 100;
		}

		cl.playerState.states[CConstVar.STAT_MAX_HEALTH] = cl.pers.maxHealth;
		cl.playerState.states[CConstVar.STAT_HEALTH] = cl.pers.maxHealth;
		cl.playerState.entityFlags = flags;

		ent.client = clients[index];
		ent.inuse = true;
		ent.classname = "player";
		ent.sEnt.r.contents = CConstVar.CONTENTS_BODY;

		ent.flags = 0;
		cl.playerState.clientNum = index;

		cl.playerState.origin = spawnOrigin;

		cl.playerState.pmFlags |= PMoveFlags.RESPAWNED;
		Server.Instance.LinkEntity(ent);
		cl.playerState.pmFlags |= PMoveFlags.TIME_KOCKBACK;
		cl.playerState.pmTime = 100;
		cl.respawnTime = time;
		cl.inactivityTime = time + 100000000;
		cl.latched_buttons = 0;

		if(ent.client.sess.sessionTeam != TeamType.TEAM_SPECTATOR){
			// UseTargets(ent, spawnPoint);
			ent.SetPosition(spawnPoint);
		}

		cl.playerState.commandTime = time - 100;
		ent.client.pers.cmd.serverTime = time;
		ClientThink(index);

		if(ent.client.sess.spectatorState != SpectatorState.FOLLOW){
			ClientEndFrame(ent);
		}

		CUtils.PlayerStateToEntityState(cl.playerState, ref ent.sEnt.s, true);

	}

	private int TeamCount(int ignoreClientNum, TeamType ttype){
		int		i;
		int		count = 0;

		for ( i = 0 ; i < CConstVar.MAX_CLIENTS ; i++ ) {
			if ( i == ignoreClientNum ) {
				continue;
			}
			if ( clients[i].pers.connected == ClientConnState.DISCONNECTED ) {
				continue;
			}

			if ( clients[i].pers.connected == ClientConnState.CONNECTING) {
				continue;
			}

			if ( clients[i].sess.sessionTeam == ttype ) {
				count++;
			}
		}

		return count;
	}

	private void PlayerRestore(string guid, ref PlayerState ps){
		int i;
		if(guid.Length < 32){
			CLog.Error("PlayerRestore: Failed to restore player. Invalid guid: {0}", guid);
			return;
		}
		for(i = 0; i < CConstVar.MAX_PLAYER_STORED; i++){

		}
	}

	private void ResetHistory(GameEntity ent){
		int t = time;
		for(int i = 0; i < CConstVar.NUM_CLIENT_HISTORY; i++){
			t -= 50;
			ent.client.history[i].mins = ent.sEnt.r.mins;
			ent.client.history[i].maxs = ent.sEnt.r.maxs;
			ent.client.history[i].currentOrigin = ent.sEnt.r.currentOrigin;
			ent.client.history[i].time = t;
		}
	}


	private void SetOrigin(GameEntity entity, Vector3 origin){
		entity.sEnt.s.pos.SetTrBase(origin);
		entity.sEnt.s.pos.trTime = 0;
		entity.sEnt.s.pos.trDuration = 0;
		entity.sEnt.s.pos.SetTrDelta(Vector3.zero);
		entity.sEnt.r.currentOrigin = origin;
	}

	public override void Dispose(){
		inited = false;
	}
}


public class GameEntity{

	public SharedEntity sEnt;

	public GameClient client;

	public bool inuse;

	public string classname;

	public int spawnFlags;

	public bool neverFree; //设置为true时，FreeEntity只会unlink bodyque

	public int flags;

	public int sourceID;

	public int sourceID2;

	public int freeTime;  //对象被free的时间

	public int eventTime;

	public bool freeAfterEvent;

	public bool unlinkAfterEvent;

	// public bool physicss

	public GameEntity(){
		sEnt = new SharedEntity();
	}

	public void Reset(){

	}

	public void SetPosition(Vector3 pos){
		sEnt.r.currentOrigin = pos;
		client.SetPosition(pos);
	}

	public void SetTarget(){

	}

}



public enum SpectatorState{
	NOT,
	FREE,

	FOLLOW,

	SCOREBOARD,
}

public enum ClientConnState{
	DISCONNECTED,

	CONNECTING,

	CONNECTED,
}

