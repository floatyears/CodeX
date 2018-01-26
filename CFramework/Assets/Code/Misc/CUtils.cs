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

	public static void PlayerStateToEntityState(PlayerState playerState, ref EntityState state, bool snap){
		
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
	
	public static void PMoveRun(PMove move){
		int finalTime = move.cmd.serverTime;

		if(finalTime < move.playerState.commandTime){
			return;
		}

		if(finalTime > move.playerState.commandTime + 1000){
			move.playerState.commandTime = finalTime - 1000;
		}

		move.playerState.pm_framecount = (move.playerState.pm_framecount + 1) & ((1<<CConstVar.PS_PMOVEFRAMECOUNTBITS) - 1);

		while(move.playerState.commandTime != finalTime){
			int msec = finalTime - move.playerState.commandTime;
			if(move.pmoveFixed > 0){
				if(msec > move.pmoveMsec){
					msec = move.pmoveMsec;
				}
			}else{
				if(msec > 66){
					msec = 66;
				}
			}

			move.cmd.serverTime = move.playerState.commandTime + msec;
			PMoveSingle(move);

			if((move.playerState.pmFlags & PMoveFlags.JUMP_HELD) != PMoveFlags.NONE){
				move.cmd.upmove = 20;
			}
		}
	}

	private static void PMoveSingle(PMove pMove){
		pMove.numTouch = 0;
		if(pMove.playerState.states[CConstVar.STAT_HEALTH] <= 0){
			// 
		}

		if(Mathf.Abs(pMove.cmd.forwardmove) > 64 || Mathf.Abs(pMove.cmd.rightmove) > 64){
			pMove.cmd.buttons &= ~ButtonsDef.BUTTON_WALKING;
		}

		if(pMove.playerState.states[CConstVar.STAT_HEALTH] > 0 && (pMove.cmd.buttons & ButtonsDef.BUTTON_ATTACK) == 0){
			pMove.playerState.pmFlags &= ~PMoveFlags.RESPAWNED;
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
	
}
