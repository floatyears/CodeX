using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUtils {

	public static float LerpAngles(float from, float to, float frac){
		float a;
		if(to - from > 180){
			to -= 360;
		}
		if(to - from < -180){
			to += 360;
		}
		a = from + frac * (to - from);
		return a;
	}

	public static bool BG_EvaluateTrajectory(Trajectory trajectory, int atTime, out Vector3 result){
		float deltaTime;
		float phase;

		Vector3 tmp1 = Vector3.zero;
		trajectory.GetTrBase(ref tmp1);
		Vector3 tmp2 = Vector3.zero;
		trajectory.GetTrDelta(ref tmp2);

		switch((TrajectoryType)trajectory.trType){
			case TrajectoryType.STATIONARY:
			case TrajectoryType.INTERPOLATE:
				result = Vector3.zero;
				trajectory.GetTrBase(ref result);
				return true;
			case TrajectoryType.LINEAR:
				deltaTime = (atTime - trajectory.trTime) * 0.001f;
				
				result = VectorMA(tmp1, deltaTime, tmp2);
				return true;
			case TrajectoryType.SINE:
				deltaTime = (atTime - trajectory.trTime) / (float) trajectory.trDuration;
				phase = Mathf.Sin(deltaTime * Mathf.PI * 2);
				
				result = VectorMA(tmp1, phase, tmp2);
				return true;
			case TrajectoryType.LINEAR_STOP:
				if(atTime > trajectory.trTime + trajectory.trDuration){
					atTime = trajectory.trTime + trajectory.trDuration;
				}
				deltaTime = (atTime - trajectory.trTime) * 0.001f;
				if(deltaTime < 0){
					deltaTime = 0f;
				}
				result = VectorMA(tmp1, deltaTime, tmp2);
				return true;
			case TrajectoryType.GRAVITY:
				deltaTime = (atTime - trajectory.trTime) * 0.001f;
				result = VectorMA(tmp1, deltaTime, tmp2);
				result[2] -= 0.5f * CConstVar.DEFAULT_GRAVITY * deltaTime * deltaTime;
				return true;
			default:
				CLog.Error("BG_EvaluateTrajectory: unknown trType: %d", trajectory.trTime);
				result = Vector3.zero;
				return false;
		}
	}

	public static Vector3 VectorMA(Vector3 a, float phase, Vector3 b){
		return a + b * phase;
	}

	public static void BG_PlayerStateToEntityStateExtraPolate(PlayerState playerState, ref EntityState entity, int time, bool snap){

	}

	public static void BG_AddPredictableEventToPlayerstate(int newEvent, int eventParams, PlayerState ps){
		ps.events[ps.eventSequence & (CConstVar.MAX_PS_EVENTS -1)] = newEvent;
		ps.eventParams[ps.eventSequence & (CConstVar.MAX_PS_EVENTS - 1)] = eventParams;
		ps.eventSequence++;
	}

	public static void PlayerStateToEntityState(PlayerState playerState, ref EntityState state, bool snap){
		if(playerState.pmType == PMoveType.INTERMISSION || playerState.pmType == PMoveType.SPECTATOR){
			state.entityType = EntityType.INVISIBLE;
		}else if(playerState.states[CConstVar.STAT_HEALTH] <= CConstVar.GIB_HEALTH){
			state.entityType = EntityType.INVISIBLE;
		}else{
			state.entityType = EntityType.PLAYER;
		}

		state.entityIndex = playerState.clientNum;
		state.pos.trType = (int)TrajectoryType.INTERPOLATE;

		state.pos.SetTrBase(playerState.origin);
		Vector3 tmp = Vector3.zero;
		if(snap){
			state.pos.GetTrBase(ref tmp);
			SnapVector(ref tmp);
			state.pos.SetTrBase(tmp);
		}
		state.pos.GetTrDelta(ref playerState.velocity);

		state.apos.trType = (int)TrajectoryType.INTERPOLATE;
		state.apos.SetTrBase(playerState.viewangles);

		if(snap){
			state.pos.GetTrBase(ref tmp);
			SnapVector(ref tmp);
			state.pos.SetTrBase(tmp);
		}
		state.angles2[CConstVar.YAW] = playerState.movementDir;
		state.clientNum = playerState.clientNum;

		state.entityFlags = playerState.entityFlags;
		if(playerState.states[CConstVar.STAT_HEALTH] <= 0){
			state.entityFlags |= EntityFlags.DEAD;
		}else{
			state.entityFlags &= ~EntityFlags.DEAD;
		}

		if(playerState.externalEvent > 0){
			state.eventID = playerState.externalEvent;
			state.eventParam = playerState.externalEventParam;
		}else if(playerState.entityEventSequence < playerState.eventSequence){
			if(playerState.entityEventSequence < playerState.eventSequence - CConstVar.MAX_PS_EVENTS){
				playerState.entityEventSequence = playerState.eventSequence - CConstVar.MAX_PS_EVENTS;
			}

			int seq = playerState.entityEventSequence & (CConstVar.MAX_PS_EVENTS - 1);
			state.eventID = playerState.events[seq] | ((playerState.entityEventSequence & 3) << 8);
			state.eventParam = playerState.eventParams[seq];
			playerState.entityEventSequence++;
		}

		// state.generic1 = playerState.groundEntityNum;

	}

	public static void SnapVector(ref Vector3 v) {
		v[0]=((int)(v[0]));v[1]=((int)(v[1]));v[2]=((int)(v[2]));
	}

	public static string GetValueForKey(string s, string key){
		string newkey = "\\" + key;
		int start = s.IndexOf(newkey);
		if(start < 0) return "";
		int end = s.IndexOf("\\", start+1);
		if(end < 0) end = s.Length;
		return s.Substring(start + newkey.Length + 1, end - start - newkey.Length - 1);
	}

	public static void SetValueForKey(ref string s, string key, string value){
		string newkey = "\\" + key;
		int start = s.IndexOf(newkey);
		if(start >= 0){
			int end = s.IndexOf("\\", start+1);
			if(end < 0){
				end = s.Length;
			}
			s.Replace(s.Substring(start, end), "\\" + key + "$" + value);
		}else{
			s += "\\" + key + "$" + value;
		}
	}

	public static int Random(){
		return 10;
	}

	public static int HashKey(char[] s, int maxLen){
		int hash = 0;
		for(int i = 0; i < maxLen && s[i] !='\0'; i++){
			if((s[i] & 0x80) > 0 || s[i] == '%'){
				hash += '.' * (119 + i);
			}else{
				hash += s[i] * (119 + i);
			}
		}
		hash = (hash ^ (hash >> 10) ^ (hash >> 20));
		return hash;
	}

	public static sbyte ClampChar(int val){
		if(val > 127){
			val = 127;
		}
		if(val < -128){
			val = -128;
		}
		return (sbyte)val;
	}

	static public void SetLayer (GameObject go, int layer)
	{
		go.layer = layer;

		Transform t = go.transform;
		
		for (int i = 0, imax = t.childCount; i < imax; ++i)
		{
			Transform child = t.GetChild(i);
			SetLayer(child.gameObject, layer);
		}
	}
	
}
