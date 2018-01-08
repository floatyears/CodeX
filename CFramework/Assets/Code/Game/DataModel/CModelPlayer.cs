using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlatBuffers;

//玩家数据模型
public class CModelPlayer : IModel
{	
	public int roleID;

	public int roleName;

	public PMove pmove;

	private PlayerState predictedPlayerState;

	private bool validPPS = false;

	public void Init()
	{
		validPPS = false;
	}

	public void Update()
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
		current = CDataModel.GameState.ClientActive.cmdNum;

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
				UpdateViewAngles(pmove.playerState, pmove.cmd);
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
					PMoveRun(pmove);

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
				PMoveRun(pmove);
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
			if(predictedPlayerState.eventSequence > oldPlayerState.eventSequence + CConstVar.MAX_PS_EVENT){
				CLog.Info("dropped event");
			}
		}


	}

	private void PMoveRun(PMove move){

	}

	private void TransitionPlayerState(PlayerState playerState, PlayerState oplayerState){
		var gamestate = CDataModel.GameState;
		if(playerState.clientNum != oplayerState.clientNum){
			gamestate.thisFrameTeleport = true;
			oplayerState = playerState;
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

		outP = gamestate.snap.playerState;

		var cl = CDataModel.GameState.ClientActive;
		if(grabAngles){
			UserCmd cmd;
			int cmdNum = cl.cmdNum;

			CDataModel.GameState.GetUserCmd(cmdNum, out cmd);

			UpdateViewAngles(outP, cmd);
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

	private void UpdateViewAngles(PlayerState ps, UserCmd cmd)
	{
		if(ps.pmType == PMoveType.INTERMISSION || ps.pmType == PMoveType.SPINGTERMISSION){
			return;
		}

		//TODO:state
		if(ps.pmType != PMoveType.SPECTATOR && ps.states[0] <= 0){
			return;
		}

		int temp;
		for(int i = 0; i < 3; i++){
			temp = cmd.angles[i] + ps.delta_angles[i];
			if(i == 0){
				if(temp > 16000){
					ps.delta_angles[i] = 16000 - cmd.angles[i];
					temp = 16000;
				}else if(temp < -16000){
					ps.delta_angles[i] = -16000 - cmd.angles[i];
					temp = -16000;
				}
			}
			ps.viewangles[i] = temp * 360 / 65536;
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

	public void Dispose()
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
	
}

//PlayerEntity需要记录更多的信息
public struct PlayerEntity
{
	public LerpFrame torso; //躯干

	public LerpFrame legs; //腿

	public LerpFrame flag;
	
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

	public EntityFlags entityFlags;

	public int eventSequence;

	public EntityEventType[] events;

	public int[] eventParams;

	public int externalEvent;

	public int externalEventParam;

	public int externalEventTime;

	public int clientNum; //范围是0-MAX_CLIENT - 1

	public Vector3 viewangles;

	public int viewHeight;

	public int damageEvent;

	public int damageCount;

	public int[] states;

	public int[] persistant;

	public int ping;

	public int pm_framecount;

	public int entityEventSequence;
}

public enum PlayerPersistant{
	SCORE = 0,
	LENGTH = 1,
}

public struct PMove{
	public PlayerState playerState;

	public UserCmd cmd;

	public int tracemask;

	public int debugLevel;

	public bool noFootsteps;

	public bool gauntletHit;

	public int frameCount;

	public int numTouch;

	public int pmoveFixed;

	public int pmoveMsec;

	public int pmoveFloat;

	
}

public enum PMoveType
{
	NORMAL = 1,

	NOCLIP = 2,

	SPECTATOR = 3, //

	DEAD = 4,

	FREEZE = 5,

	INTERMISSION = 6, //间歇期

	SPINGTERMISSION = 7,
}

//移动的标签
public enum PMoveFlags
{
	NONE = 0,

	DUCKED = 0x1,

	JUMP_HELD = 0x2,

	BACKWARDS_JUMP = 0x04,

	BACKWARDS_RUN = 0x8,

	TIME_LAND = 0x10,

	TIME_KOCKBACK = 0x20,

	TIME_WATERJUMP = 0x40,

	RESPAWNED = 0x80,

	USE_ITEM_HELD = 0x100,

	GRAPPLE_PULL = 0x200,

	FOLLOW = 0x400, //跟随其他玩家的视角

	SCOREBOARD = 0x800,

	INVULEXPAND = 0x1000,
}
