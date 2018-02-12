using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlatBuffers;
using System;

//玩家数据模型
public class CModelPlayer : CModelBase
{	
	public int roleID;

	public int roleName;

	public PMove pmove;

	private PlayerState predictedPlayerState;

	private bool validPPS = false;

	public override void Init()
	{
		validPPS = false;
	}

	private void Update()
	{

	}

	private void PredictPlayerState()
	{
		int cmdNum, current;
		PlayerState oldPlayerState;
		bool moved;
		UserCmd oldestCmd;
		UserCmd latestCmd;

		int stateIndex = 0, predictCmd = 0;
		int numPredicted = 0, numPlayedBack = 0;

		var gamestate = CDataModel.GameState;
		gamestate.hyperspace = false;

		if(!validPPS){
			validPPS = true;
			predictedPlayerState = gamestate.snap.playerState;
		}

		//如果是播回放，那么就复制移动，不做预测
		if(gamestate.demoPlayback || (gamestate.snap.playerState.pmFlags & PMoveFlags.FOLLOW) != PMoveFlags.NONE){
			InterpolatePlayerState(false);
			return;
		}

		//非预测的本地移动会抓取最近的视角
		if(CConstVar.NoPredict || CConstVar.SynchronousClients){
			InterpolatePlayerState(true);
			return;
		}

		pmove.playerState = predictedPlayerState;
		if(pmove.playerState.pmType == PMoveType.DEAD){
			// pmove.tracemask = 
		}else{
			// pmove.tracemask
		}

		// if(gamestate.snap.playerState.persistant[3] == )

		// pmove.noFootsteps = gamestate.dm

		oldPlayerState = predictedPlayerState;
		current = CDataModel.GameState.ClActive.cmdNum;

		//如果没有紧接着snapshot之后的comands，就不能精确预测当前的位置，所以就停在最后的正确位置上
		cmdNum = current - CConstVar.CMD_BACKUP + 1;
		CDataModel.GameState.GetUserCmd(cmdNum, out oldestCmd);
		if(oldestCmd.serverTime > gamestate.snap.playerState.commandTime && oldestCmd.serverTime < gamestate.time){
			if(CConstVar.ShowMiss > 0){
				CLog.Info("exceeded Packet_Backup on commands");
			}
			return;
		}

		CDataModel.GameState.GetUserCmd(current, out latestCmd);

		if(gamestate.nextSnap != null && !gamestate.nextFrameTeleport && !gamestate.thisFrameTeleport){
			predictedPlayerState = gamestate.nextSnap.playerState;
			gamestate.physicsTime = gamestate.nextSnap.serverTime;
		}else{
			predictedPlayerState = gamestate.snap.playerState;
			gamestate.physicsTime = gamestate.snap.serverTime;
		}

		if(CConstVar.PMoveMsec < 8){
			CConstVar.PMoveMsec = 8;
		}else if(CConstVar.PMoveMsec > 33){
			CConstVar.PMoveMsec = 33;
		}

		pmove.pmoveFixed = CConstVar.PMoveFixed;
		pmove.pmoveMsec = CConstVar.PMoveMsec;
		pmove.pmoveFloat = CConstVar.PMoveFloat;
		// pmove.pmv

		if(CConstVar.OptimizePrediction){
			if(gamestate.nextFrameTeleport || gamestate.thisFrameTeleport){
				gamestate.lastPredictedCommand = 0;
				gamestate.stateTail = gamestate.stateHead;
				predictCmd = current - CConstVar.CMD_BACKUP + 1;
			}else if(gamestate.time == gamestate.lastServerTime){
				predictCmd = gamestate.lastPredictedCommand + 1;
			}else{
				bool error = true;
				for(int i = gamestate.stateHead; i != gamestate.stateTail; i = (i+1)%CConstVar.NUM_SAVED_STATES){
					if(gamestate.savedPmoveState[i].commandTime == predictedPlayerState.commandTime){
						int errorcode = IsUnacceptableError(predictedPlayerState, gamestate.savedPmoveState[i]);

						if(errorcode > 0){
							if(CConstVar.ShowMiss > 0){
								CLog.Info("errorcode %d at %d", errorcode, gamestate.time);
							}
							break;
						}

						pmove.playerState = gamestate.savedPmoveState[i];

						gamestate.stateHead = (i + 1) % CConstVar.NUM_SAVED_STATES;

						predictCmd = gamestate.lastPredictedCommand + 1;

						error = false;
					break;
					}
				}

				if(error){
					gamestate.lastPredictedCommand = 0;
					gamestate.stateTail = gamestate.stateHead;
					predictCmd = current - CConstVar.CMD_BACKUP + 1;
				}
			}

			gamestate.lastServerTime = gamestate.physicsTime;
			stateIndex = gamestate.stateHead;
		}

		moved = false;
		for(cmdNum = current - CConstVar.CMD_BACKUP + 1; cmdNum <= current; cmdNum++){
			CDataModel.GameState.GetUserCmd(current, out pmove.cmd);
			
			if(pmove.pmoveFixed > 0){
				PMove.UpdateViewAngles(pmove.playerState, pmove.cmd);
			}

			if(pmove.cmd.serverTime <= predictedPlayerState.commandTime){
				continue;
			}
			if(pmove.cmd.serverTime > latestCmd.serverTime){
				continue;
			}

			if(predictedPlayerState.commandTime == oldPlayerState.commandTime){
				Vector3 delta;
				float len;

				if(gamestate.thisFrameTeleport){
					gamestate.predictedError = Vector3.zero;
					if(CConstVar.ShowMiss > 0){
						CLog.Info("PredictionTeleport");
					}
					gamestate.thisFrameTeleport = false;
				}else{
					Vector3 adjusted, new_angles;
					AdjustPositionForMover(predictedPlayerState.origin, predictedPlayerState.groundEntityNum, gamestate.physicsTime, gamestate.oldTime, out adjusted, predictedPlayerState.viewangles,out new_angles);
				
					if(CConstVar.ShowMiss > 0){
						if(oldPlayerState.origin != adjusted){
							CLog.Info("prediction error");
						}
					}

					delta = oldPlayerState.origin - adjusted;
					len = delta.magnitude;
					if(len > 0.1){
						if(CConstVar.ShowMiss > 0){
							CLog.Info("Prediction miss: %d", len);
						}
						if(CConstVar.ErrorDecay > 0){
							int t = gamestate.time - gamestate.predictedErrorTime;
							float f = (CConstVar.ErrorDecay - t) / CConstVar.ErrorDecay;
							if(f < 0f){
								f = 0f;
							}
							if(f > 0f && CConstVar.ShowMiss > 0){
								CLog.Info("Double prediction decay: %d", f);
							}
							gamestate.predictedError = gamestate.predictedError * f;
						}else{
							gamestate.predictedError = Vector3.zero;
						}

						gamestate.predictedError = delta + gamestate.predictedError;
						gamestate.predictedErrorTime = gamestate.oldTime;
					}
				}
			}

			pmove.gauntletHit = false;
			if(pmove.pmoveFixed > 0){
				pmove.cmd.serverTime = ((pmove.cmd.serverTime + CConstVar.PMoveMsec - 1) / CConstVar.PMoveMsec) * CConstVar.PMoveMsec;
			}

			if(CConstVar.OptimizePrediction){
				if(cmdNum >= predictCmd || (stateIndex + 1) % CConstVar.NUM_SAVED_STATES == gamestate.stateHead){
					pmove.Move();

					numPredicted++;

					gamestate.lastPredictedCommand = cmdNum;

					if((stateIndex + 1) % CConstVar.NUM_SAVED_STATES != gamestate.stateHead){
						gamestate.savedPmoveState[stateIndex] = pmove.playerState;
						stateIndex = (stateIndex + 1) % CConstVar.NUM_SAVED_STATES;
						gamestate.stateTail = stateIndex;
					}
				}else{
					numPlayedBack++;
					if(CConstVar.ShowMiss > 0 && gamestate.savedPmoveState[stateIndex].commandTime != pmove.cmd.serverTime){
						CLog.Info("saved state miss");
					}

					pmove.playerState = gamestate.savedPmoveState[stateIndex];
					stateIndex = (stateIndex + 1) % CConstVar.NUM_SAVED_STATES;
				}
			}else{
				pmove.Move();
				numPredicted++;
			}

			moved = true;
		}

		if(CConstVar.ShowMiss > 1){
			CLog.Info("[%d : %d] ", pmove.cmd.serverTime, gamestate.time);
		}
		if(!moved){
			if(CConstVar.ShowMiss > 0){
				CLog.Info("not moved");
			}
			return;
		}
		AdjustPositionForMover(predictedPlayerState.origin, predictedPlayerState.groundEntityNum, gamestate.physicsTime, gamestate.time, out predictedPlayerState.origin, predictedPlayerState.viewangles, out predictedPlayerState.viewangles);
		if(CConstVar.ShowMiss > 0){
			if(predictedPlayerState.eventSequence > oldPlayerState.eventSequence + CConstVar.MAX_PS_EVENTS){
				CLog.Info("dropped event");
			}
		}
	}

	

	private void TransitionPlayerState(PlayerState playerState, PlayerState oplayerState){
		var gamestate = CDataModel.GameState;
		if(playerState.clientIndex != oplayerState.clientIndex){
			gamestate.thisFrameTeleport = true;
			playerState.CopyTo(oplayerState);
		}

		if(playerState.damageEvent != oplayerState.damageEvent && playerState.damageCount > 0){

		}

		// if(playerState.persistant[])
		CheckPlayerstateEvents(playerState, oplayerState);

		if(playerState.viewHeight != oplayerState.viewHeight){
			// gamestate.
		}


	}

	private void CheckPlayerstateEvents(PlayerState playerState, PlayerState oplayerState){

	}

	public void AdjustPositionForMover(Vector3 inPos, int moverNum, int fromTime, int toTime, out Vector3 outPos, Vector3 anglesIn, out Vector3 anglesOut){
		ClientEntity clientEntity;
		Vector3 oldOrigin, origin, deltaOrigin;
		Vector3 oldAngles, angles, deltaAngles;

		if(moverNum <= 0 || moverNum >= CConstVar.ENTITYNUM_MAX_NORMAL){
			outPos = inPos;
			anglesOut = anglesIn;
			return;
		}

		clientEntity = CDataModel.GameState.clientEntities[moverNum];
		if(clientEntity.currentState.entityType != EntityType.MOVER){
			outPos = inPos;
			anglesOut = anglesIn;
			return;
		}

		CUtils.BG_EvaluateTrajectory(clientEntity.currentState.pos, fromTime, out oldOrigin);
		CUtils.BG_EvaluateTrajectory(clientEntity.currentState.apos, fromTime, out oldAngles);

		CUtils.BG_EvaluateTrajectory(clientEntity.currentState.pos, toTime, out origin);
		CUtils.BG_EvaluateTrajectory(clientEntity.currentState.pos, toTime, out angles);
		
		deltaOrigin = origin - oldOrigin;
		deltaAngles = angles - oldAngles;

		outPos = inPos + deltaOrigin;
		anglesOut = anglesIn + deltaAngles;
	}

	private int IsUnacceptableError(PlayerState playerState, PlayerState pplayerState){
		return 0;
	}

	private void InterpolatePlayerState(bool grabAngles)
	{
		float f;
		int i;
		var gamestate = CDataModel.GameState;
		PlayerState outP = predictedPlayerState;
		SnapShot prev = gamestate.snap;
		SnapShot next = gamestate.nextSnap;

		gamestate.snap.playerState.CopyTo(outP);

		var cl = CDataModel.GameState.ClActive;
		if(grabAngles){
			UserCmd cmd;
			int cmdNum = cl.cmdNum;

			CDataModel.GameState.GetUserCmd(cmdNum, out cmd);

			PMove.UpdateViewAngles(outP, cmd);
		}

		if(gamestate.nextFrameTeleport){
			return;
		}

		if(next == null || next.serverTime <= prev.serverTime){
			return;
		}

		f = (float)(gamestate.time - prev.serverTime) / (next.serverTime - prev.serverTime);
		i = next.playerState.bobCycle;
		if(i < prev.playerState.bobCycle){
			i += 256;
		}
		outP.bobCycle = prev.playerState.bobCycle + (int)(f * ( i - prev.playerState.bobCycle));

		for(i = 0; i < 3; i++){
			outP.origin[i] = prev.playerState.origin[i] * (int)(f * (next.playerState.origin[i] - prev.playerState.origin[i]));
			if(!grabAngles){
				outP.viewangles[i] = CUtils.LerpAngles(prev.playerState.viewangles[i], next.playerState.viewangles[i], f);
			}
			outP.velocity[i] = prev.playerState.velocity[i] + f * (next.playerState.velocity[i] - prev.playerState.velocity[i]);
		}
	}



	public void CsAccountLogin()
	{
		CNetwork.Instance.SendMsg((fb)=>{
			fb.Finish(ScPlayerBasic.CreateScPlayerBasic(fb,10001,23,24,fb.CreateString("123124"), 26.0, 27f, 1).Value);
		});
		//CsPlayerBasic.StartCsPlayerBasic(fb);
	}

	public void OnScPlayerBasic(ByteBuffer buffer, ScPlayerBasic info)
	{
		ScPlayerBasic.GetRootAsScPlayerBasic(buffer);
	}

	public override void Dispose()
	{

	}

	public static void CheckPlayerStateEnvet(PlayerState playerState, PlayerState oplayerState)
	{
		// playerState.bobCycle
	}
}

public struct LerpFrame
{
	public int oldFrame;

	public int oldFrameTime;

	public int frame;

	public int frameTime;

	public float backLerp;

	public float yawAngle;

	public bool yawing;

	public float pitchAngle;

	public bool pitching;

	public int animatioinNumber;

	public int animationTime;

	public void Reset(){
		oldFrame = 0;
		oldFrameTime = 0;
		frame = 0;
		frameTime = 0;
		backLerp = 0;
		yawAngle = 0f;
		yawing = false;
		pitchAngle = 0f;
		pitching = false;
		animatioinNumber = 0;
		animationTime = 0;
	}
}

//PlayerEntity需要记录更多的信息
public struct PlayerEntity
{
	public LerpFrame torso; //躯干

	public LerpFrame legs; //腿

	public LerpFrame flag;

	public LerpFrame head;
	
	
	public int painTime; //收到伤害时间

	public int instantAttackTime; //立即攻击

	public int missileFireTime; //弹道攻击
}

public class PlayerState{
	public int commandTime; //最后执行的cmd的cmd.serverTime;

	public PMoveType pmType;

	public int bobCycle;

	public PMoveFlags pmFlags;

	public int pmTime;

	public Vector3 origin;

	public Vector3 velocity;

	public int gravity;

	public int speed;

	public int[] delta_angles; //

	public int groundEntityNum;

	public int entityID; //entityID

	public int movementDir; //摇杆操作的方向，范围是为0-180(int8)

	public int entityFlags; //EntityFlags

	public int eventSequence;

	public int[] events; //EntityEventType

	public int[] eventParams;

	public int externalEvent; //EntityEventType

	public int externalEventParam;

	public int clientIndex; //范围是0-MAX_CLIENT - 1

	public Vector3 viewangles;

	public int viewHeight;

	public int damageEvent;

	public int damageCount;

	public int ping;

	public int pm_framecount;

	public int entityEventSequence;

	public int[] states;

	// public int states[int idx]{
	// 	get{
	// 		return _states[idx];
	// 	}
	// 	set{
	// 		_states[idx] = value;
	// 	}
	// }

	public int[] persistant;

	public int this[int idx]{
		get{
			switch(idx){
				case 1:
					return commandTime;				
				case 2:
					return (int)pmType;
				case 3:
					return bobCycle;
				case 4:
					return (int)pmFlags;
				case 5:
					return pmTime;
				case 6:
					return BitConverter.ToInt32(BitConverter.GetBytes(origin.x), 0);
				case 7:
					return BitConverter.ToInt32(BitConverter.GetBytes(origin.y), 0);
				case 8:
					return BitConverter.ToInt32(BitConverter.GetBytes(origin.z), 0);
				case 9:
					return BitConverter.ToInt32(BitConverter.GetBytes(velocity.x), 0);
				case 10:
					return BitConverter.ToInt32(BitConverter.GetBytes(velocity.y), 0);
				case 11:
					return BitConverter.ToInt32(BitConverter.GetBytes(velocity.z), 0);
				case 12:
					return gravity;
				case 13:
					return speed;
				case 14:
					return delta_angles[0];
				case 15:
					return delta_angles[1];
				case 16:
					return delta_angles[2];
				case 17:
					return groundEntityNum;
				case 18:
					return entityID;
				case 19:
					return movementDir;
				case 20:
					return entityFlags;
				case 21:
					return eventSequence;
				case 22:
					return events[0];
				case 23:
					return events[1];
				case 24:
					return eventParams[0];
				case 25:
					return eventParams[1];
				case 26:
					return externalEvent;
				case 27:
					return externalEventParam;
				case 28:
					return clientIndex;
				case 29:
					return BitConverter.ToInt32(BitConverter.GetBytes(viewangles.x), 0);
				case 30:
					return BitConverter.ToInt32(BitConverter.GetBytes(viewangles.y), 0);
				case 31:
					return BitConverter.ToInt32(BitConverter.GetBytes(viewangles.z), 0);
				case 32:
					return viewHeight;
				case 33:
					return damageEvent;
				case 34:
					return damageCount;
				case 35:
					return ping;
				case 36:
					return pm_framecount;
				case 37:
					return entityEventSequence;
			}
			return 0;
		}
		set{
			switch(idx){
				case 1:
					commandTime = value;		
					break;		
				case 2:
					pmType = (PMoveType)value;
					break;
				case 3:
					bobCycle = value;
					break;
				case 4:
					pmFlags = (PMoveFlags)value;
					break;
				case 5:
					pmTime = value;
					break;
				case 6:
					origin.x = System.BitConverter.ToSingle(System.BitConverter.GetBytes(value), 0);
					break;
				case 7:
					origin.y = System.BitConverter.ToSingle(System.BitConverter.GetBytes(value), 0);
					break;
				case 8:
					origin.z = System.BitConverter.ToSingle(System.BitConverter.GetBytes(value), 0);
					break;
				case 9:
					velocity.x = System.BitConverter.ToSingle(System.BitConverter.GetBytes(value), 0);
					break;
				case 10:
					velocity.y = System.BitConverter.ToSingle(System.BitConverter.GetBytes(value), 0);
					break;
				case 11:
					velocity.z = System.BitConverter.ToSingle(System.BitConverter.GetBytes(value), 0);
					break;
				case 12:
					gravity = value;
					break;
				case 13:
					speed = value;
					break;
				case 14:
					delta_angles[0] = value;
					break;
				case 15:
					delta_angles[1] = value;
					break;
				case 16:
					delta_angles[2] = value;
					break;
				case 17:
					groundEntityNum = value;
					break;
				case 18:
					entityID = value;
					break;
				case 19:
					movementDir = value;
					break;
				case 20:
					entityFlags = value;
					break;
				case 21:
					eventSequence = value;
					break;
				case 22:
					events[0] = value;
					break;
				case 23:
					events[1] = value;
					break;
				case 24:
					eventParams[0] = value;
					break;
				case 25:
					eventParams[1] = value;
					break;
				case 26:
					externalEvent = value;
					break;
				case 27:
					externalEventParam = value;
					break;
				case 28:
					clientIndex = value;
					break;
				case 29:
					viewangles.x = value;
					break;
				case 30:
					viewangles.y = value;
					break;
				case 31:
					viewangles.z = value;
					break;
				case 32:
					viewHeight = value;
					break;
				case 33:
					damageEvent = value;
					break;
				case 34:
				 	damageCount = value;
					break;
				case 35:
					ping = value;
					break;
				case 36:
					pm_framecount = value;
					break;
				case 37:
					entityEventSequence = value;
					break;
			}
		}
	}

	public PlayerState(){
		delta_angles = new int[3];
		events = new int[CConstVar.MAX_PS_EVENTS];
		eventParams = new int[CConstVar.MAX_PS_EVENTS];
		states = new int[CConstVar.MAX_STATS];
		persistant = new int[CConstVar.MAX_PERSISTANT];
	}

	public void CopyTo(PlayerState ps){
		for(int i = 1; i < 38; i++){
			ps[i] = this[i];
		}
	}

	public void Reset(){

	}


	private static int idx = 0;
	private static int GetFieldSize(){
		return sizeof(int)*idx++;
	}

	public static int[][] PlayerStateFieldsInt = new int[37][]
	{
		new int[2]{1,32},
		new int[2]{35,16},//ping
		new int[2]{36,32},//
		new int[2]{6,0},
		new int[2]{7,0},
		new int[2]{3,8},
		new int[2]{9,0},
		new int[2]{10,0},
		new int[2]{30,0},
		new int[2]{29,0},
		new int[2]{8,0},
		new int[2]{11,0},
		new int[2]{5,-16},
		new int[2]{21,16},
		new int[2]{19,4},
		new int[2]{22,4},
		new int[2]{23,8},
		new int[2]{4,8},
		new int[2]{17,CConstVar.GENTITYNUM_BITS}, //groundEntityNum
		new int[2]{20,16}, //entityFlags
		new int[2]{26,32},//externalEvent
		new int[2]{12,16},//gravity
		new int[2]{13,16},//speed
		new int[2]{15,16},//delta_angles[2]
		new int[2]{27,8},//externalEventParam
		new int[2]{32,-8},//viewHeight
		new int[2]{33,8},//damageEvent
		new int[2]{34,8},//damageCount
		new int[2]{2,8},//pmType
		new int[2]{14,16},//delta_angles[0]
		new int[2]{16,16},//delta_angles[2]
		new int[2]{24,8},//eventParams[0]
		new int[2]{25,8},//eventParams[1]
		new int[2]{28,8},//clientNum
		new int[2]{31,0},//viewangles
		new int[2]{37,16},//entityEventSequence
		new int[2]{18,32},//
	};

	public static NetField[] PlayerStateFields;

	public static void InitNetFiled(){
		PlayerStateFields = new NetField[30];

		PlayerStateFields[idx] = new NetField("commandTime", GetFieldSize(), 32);
		PlayerStateFields[idx] = new NetField("pmType",GetFieldSize(), 8);
		PlayerStateFields[idx] = new NetField("bobCycle",GetFieldSize(), 8);
		PlayerStateFields[idx] = new NetField("pmFlags",GetFieldSize(), 16);

		PlayerStateFields[idx] = new NetField("pmTime",GetFieldSize(), -16); 
		PlayerStateFields[idx] = new NetField("origin.x",GetFieldSize(), 32); //0
		PlayerStateFields[idx] = new NetField("origin.y",GetFieldSize(), 32); 
		PlayerStateFields[idx] = new NetField("origin.z",GetFieldSize(), 32); //1
		PlayerStateFields[idx] = new NetField("velocity.x",GetFieldSize(), 32); //2
		PlayerStateFields[idx] = new NetField("velocity.y",GetFieldSize(), 32);  //5
		PlayerStateFields[idx] = new NetField("velocity.z",GetFieldSize(), 32); //3
		PlayerStateFields[idx] = new NetField("gravity",GetFieldSize(), 16); //4
		PlayerStateFields[idx] = new NetField("speed",GetFieldSize(), 16); //7

		PlayerStateFields[idx] = new NetField("deltaAngles.x",GetFieldSize(), 32);
		PlayerStateFields[idx] = new NetField("deltaAngles.y",GetFieldSize(), 32);
		PlayerStateFields[idx] = new NetField("deltaAngles.z",GetFieldSize(), 32);
		PlayerStateFields[idx] = new NetField("groundEntityNum",GetFieldSize(),  CConstVar.GENTITYNUM_BITS); //8
		PlayerStateFields[idx] = new NetField("entityID",GetFieldSize(), 0);  //6
		PlayerStateFields[idx] = new NetField("movementDir",GetFieldSize(), 4);
		PlayerStateFields[idx] = new NetField("entityFlags",GetFieldSize(), 16);
		PlayerStateFields[idx] = new NetField("eventSequence",GetFieldSize(), 16);

		PlayerStateFields[idx] = new NetField("events.0",GetFieldSize(), 8);
		PlayerStateFields[idx] = new NetField("events.1",GetFieldSize(), 8);

		PlayerStateFields[idx] = new NetField("eventParams.0",GetFieldSize(), 8);
		PlayerStateFields[idx] = new NetField("eventParams.1",GetFieldSize(), 8);

		PlayerStateFields[idx] = new NetField("externalEvent",GetFieldSize(), 8);
		PlayerStateFields[idx] = new NetField("externalEventParam",GetFieldSize(), 8);
		PlayerStateFields[idx] = new NetField("externalEventTime",GetFieldSize(), -16);
		PlayerStateFields[idx] = new NetField("clientNum",GetFieldSize(), 8);

		PlayerStateFields[idx] = new NetField("viewangles.x",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("viewangles.y",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("viewangles.z",GetFieldSize(), 0);

		PlayerStateFields[idx] = new NetField("viewHeight",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("damageEvent",GetFieldSize(), 8);
		PlayerStateFields[idx] = new NetField("damageCount",GetFieldSize(), 8);

		PlayerStateFields[idx] = new NetField("states.0",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.1",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.2",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.3",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.4",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.5",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.6",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.7",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.8",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.9",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.10",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.11",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.12",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.13",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.14",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("states.15",GetFieldSize(), 0);

		PlayerStateFields[idx] = new NetField("persistant.0",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.1",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.2",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.3",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.4",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.5",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.6",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.7",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.8",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.9",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.10",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.11",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.12",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.13",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.14",GetFieldSize(), 0);
		PlayerStateFields[idx] = new NetField("persistant.15",GetFieldSize(), 0);

		PlayerStateFields[idx] = new NetField("ping",GetFieldSize(), 0);

		PlayerStateFields[idx] = new NetField("pm_framecount",GetFieldSize(),0);
		PlayerStateFields[idx] = new NetField("entityEventSequence",GetFieldSize(), 0);

		var tmps = new string[]{
			"commandTime",
			"origin.x",
			"origin.y",
			"bobCycle",
			"velocity.x",
			"velocity.y",
			"viewangles.y",
			"viewangles.x",
			"origin.z",
			"velocity.z",
			"pmTime",
			"eventSequence",
			"movementDir",
			"events[0]",
			"pmFlags",
			"groundEntityNum",
			"entityFlags",
			"externalEvent",
			"gravity",
			"speed",
			"deltaAngles.y",
			"externalEventParam",
			"viewHeight",
			"damageEvent",
			"damageCount",
			"pmType",
			"deltaAngles.x",
			"deltaAngles.z",
			"eventParams.0",
			"eventParams.1",
			"clientNum",
			"viewangles.z",
			};

		System.Array.Sort(PlayerStateFields, (x1,x2)=>{
			var i1 = System.Array.IndexOf(tmps, x1.name);
			var i2 = System.Array.IndexOf(tmps, x2.name);
			if(i1 < 0 && i2 < 0){
				// CLog.Error("playerStateFields sort failed");
				return 0;
			}else if(i1 < 0){
				return 1;
			}else if(i2 < 0){
				return -1;
			}
			return i1 - i2;
		});
	}
}

