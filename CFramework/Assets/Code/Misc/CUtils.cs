﻿using System.Collections;
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

		switch((TrajectoryType)trajectory.trType){
			case TrajectoryType.STATIONARY:
			case TrajectoryType.INTERPOLATE:
				result = trajectory.trBase;
				return true;
			case TrajectoryType.LINEAR:
				deltaTime = (atTime - trajectory.trTime) * 0.001f;
				result = VectorMA(trajectory.trBase, deltaTime, trajectory.trDelta);
				return true;
			case TrajectoryType.SINE:
				deltaTime = (atTime - trajectory.trTime) / (float) trajectory.trDuration;
				phase = Mathf.Sin(deltaTime * Mathf.PI * 2);
				result = VectorMA(trajectory.trBase, phase, trajectory.trDelta);
				return true;
			case TrajectoryType.LINEAR_STOP:
				if(atTime > trajectory.trTime + trajectory.trDuration){
					atTime = trajectory.trTime + trajectory.trDuration;
				}
				deltaTime = (atTime - trajectory.trTime) * 0.001f;
				if(deltaTime < 0){
					deltaTime = 0f;
				}
				result = VectorMA(trajectory.trBase, deltaTime, trajectory.trDelta);
				return true;
			case TrajectoryType.GRAVITY:
				deltaTime = (atTime - trajectory.trTime) * 0.001f;
				result = VectorMA(trajectory.trBase, deltaTime, trajectory.trDelta);
				result[2] -= 0.5f * CConstVar.DEFAULT_GRAVITY * deltaTime * deltaTime;
				return true;
			default:
				CLog.Error("BG_EvaluateTrajectory: unknown trType: %d", trajectory.trTime);
				result = Vector3.zero;
				return false;
		}

		result = Vector3.zero;
		return false;
	}

	public static Vector3 VectorMA(Vector3 a, float phase, Vector3 b){
		return a + b * phase;
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
	

	public static int Random(){
		return 10;
	}

	
}
