using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class CModelEntity : CModelBase {

	public override void Init()
	{

	}	
	
	public override void Dispose()
	{

	}

}

public struct ClientEntity{
	public EntityState currentState;

	public EntityState nextState;

	public bool interpolate; //true:下一帧可以插值

	public bool currentValid;

	public int previousEvent; //

	public int teleportFlag;

	public int trailTime; //missile可以处理丢包的情况

	public int miscTime;

	public int snapShotTime; //这个entity在帧中出现的最后时间

	public PlayerEntity playerEntity;

	public int errorTime; //从当前时间开始衰减出现的错误

	public Vector3 errorOrigin;

	public Vector3 errorAngle;

	public bool extrapolated; //false if origin/angles is an interpolation

	public Vector3 rawOrigin;

	public Vector3 rawAngles;

	public Vector3 beamEnd;

	//entity在这一帧的准确插值位置
	public Vector3 lerpOrigin;

	public Vector3 lerpAngles;

	public void Reset(){
		// if(snapShotTime < )
	}

	public void CopyTo(ClientEntity to){
		to.currentState = currentState;
		to.nextState = nextState;
	}
}

public class SvEntityState{

	public WorldSector worldSector;

	public SvEntityState nextEntityInWorldSector;
	public EntityState baseline;

	public int numClusters;

	public int clusterNums;

	public int snapshotCounter;
}

[StructLayout(LayoutKind.Sequential)]
unsafe public struct EntityState{
	public int entityID;

	public int entityIndex;

	//entityType超过EntityEventType.Event_Count之后，就表示单纯的事件，而不代表一个entity
	public int entityType; //EntityType

	public int entityFlags; //EntityFlags

	public Trajectory pos;

	public Trajectory apos; //用于计算角度

	public int time;

	public int time2;

	public Vector3 origin; //origin

	public Vector3 origin2; //target

	public Vector3 angles; //origin

	public Vector3 angles2; //target

	public int otherEntityIdx;

	public int otherEntityIdx2;

	public int sourceID;

	public int source2ID;

	public int clientNum; //范围是0-(MAX_CLIENT - 1)

	public int frame;

	public int solid;

	public int eventID; //EntityEventType

	public int eventParam;

	public int generic1;

	public void CopyTo(EntityState to){
		to.entityID = entityID;
		to.entityIndex = entityIndex;
		to.angles = angles;
	}

	public int CompareValue(ref EntityState to){
		int num = 0;
		if(entityID != to.entityID){
			num = 1;
		}
		if(entityIndex != to.entityIndex){
			num = 2;
		}
		if(entityType != to.entityType){
			num = 3;
		}
		if(entityFlags != to.entityFlags){
			num = 4;
		}
		num += pos.Equals(ref to.pos);
		num += apos.Equals(ref to.apos);

		if(time != to.time){
			num++;
		}
		if(time2 != to.time2){
			num++;
		}
		if(origin != to.origin){
			num++;
		}
		if(origin2 != to.origin2){
			num++;
		}
		if(angles != to.angles){
			num++;
		}
		if(angles2 != to.angles2){
			num++;
		}
		if(otherEntityIdx != to.otherEntityIdx){
			num++;
		}
		if(otherEntityIdx2 != to.otherEntityIdx2){
			num++;
		}
		if(sourceID != to.sourceID){
			num++;
		}
		if(source2ID != to.source2ID){
			num++;
		}
		if(clientNum != to.clientNum){
			num++;
		}
		if(frame != to.frame){
			num++;
		}
		if(solid != to.solid){
			num++;
		}
		if(eventID != to.eventID){
			num++;
		}
		if(eventParam != to.eventParam){
			num++;
		}
		if(generic1 != to.generic1){
			num++;
		}
		return num;
	}

	public void WriteDelta(MsgPacket msg, ref EntityState value){
		
	}

	

	public static NetField[] entityStateFields;

	public static void InitNetField(){

		
		entityStateFields = new NetField[46];
		//把经常改变的量放到前面
		entityStateFields[0] = new NetField("id", 0, 32);
		entityStateFields[1] = new NetField("idx",sizeof(int), 32);
		entityStateFields[2] = new NetField("eType",sizeof(int)*2, 8);
		entityStateFields[3] = new NetField("eFlag",sizeof(int)*3, 32);

		entityStateFields[4] = new NetField("pos.trType",sizeof(int)*4, 8); 
		entityStateFields[5] = new NetField("pos.trTime",sizeof(int)*5, 32); //0
		entityStateFields[6] = new NetField("pos.trDur",sizeof(int)*6, 32); 
		entityStateFields[7] = new NetField("pos.trBase.x",sizeof(int)*7, 0); //1
		entityStateFields[8] = new NetField("pos.trBase.y",sizeof(int)*8, 0); //2
		entityStateFields[9] = new NetField("pos.trBase.z",sizeof(int)*9, 0);  //5
		entityStateFields[10] = new NetField("pos.trDelta.x",sizeof(int)*10, 0); //3
		entityStateFields[11] = new NetField("pos.trDelta.y",sizeof(int)*11, 0); //4
		entityStateFields[12] = new NetField("pos.trDelta.z",sizeof(int)*12, 0); //7

		entityStateFields[13] = new NetField("apos.trType",sizeof(int)*13, 8);
		entityStateFields[14] = new NetField("apos.trTime",sizeof(int)*14, 32);
		entityStateFields[15] = new NetField("apos.trDur",sizeof(int)*15, 32);
		entityStateFields[16] = new NetField("apos.trBase.x",sizeof(int)*16, 0); //8
		entityStateFields[17] = new NetField("apos.trBase.y",sizeof(int)*17, 0);  //6
		entityStateFields[18] = new NetField("apos.trBase.z",sizeof(int)*18, 0);
		entityStateFields[19] = new NetField("apos.trDelta.x",sizeof(int)*19, 0);
		entityStateFields[20] = new NetField("apos.trDelta.y",sizeof(int)*20, 0);
		entityStateFields[21] = new NetField("apos.trDelta.z",sizeof(int)*21, 0);

		entityStateFields[22] = new NetField("time",sizeof(int)*22, 32);
		entityStateFields[23] = new NetField("time2",sizeof(int)*23, 32);

		entityStateFields[24] = new NetField("origin.x",sizeof(int)*24, 0);
		entityStateFields[25] = new NetField("origin.y",sizeof(int)*25, 0);
		entityStateFields[26] = new NetField("origin.z",sizeof(int)*26, 0);

		entityStateFields[27] = new NetField("origin2.x",sizeof(int)*27, 0);
		entityStateFields[28] = new NetField("origin2.y",sizeof(int)*28, 0);
		entityStateFields[29] = new NetField("origin2.z",sizeof(int)*29, 0);

		entityStateFields[30] = new NetField("angles.x",sizeof(int)*30, 0);
		entityStateFields[31] = new NetField("angles.y",sizeof(int)*31, 0);
		entityStateFields[32] = new NetField("angles.z",sizeof(int)*32, 0);

		entityStateFields[33] = new NetField("angles2.x",sizeof(int)*33, 0);
		entityStateFields[34] = new NetField("angles2.y",sizeof(int)*34, 0);
		entityStateFields[35] = new NetField("angles2.z",sizeof(int)*35, 0);

		entityStateFields[36] = new NetField("oeIdx",sizeof(int)*36, CConstVar.GENTITYNUM_BITS);
		entityStateFields[37] = new NetField("oeIdx2",sizeof(int)*37, CConstVar.GENTITYNUM_BITS);

		entityStateFields[38] = new NetField("sourceID",sizeof(int)*38, 32);
		entityStateFields[39] = new NetField("source2ID",sizeof(int)*39, 32);
		entityStateFields[40] = new NetField("clientNum",sizeof(int)*40, 8);
		entityStateFields[41] = new NetField("frame",sizeof(int)*41, 32);
		entityStateFields[42] = new NetField("solid",sizeof(int)*42, 32);
		entityStateFields[43] = new NetField("eventID",sizeof(int)*43, 32);
		entityStateFields[44] = new NetField("eventParam",sizeof(int)*44, 32);
		entityStateFields[45] = new NetField("generic1",sizeof(int)*45, 8);

		var tmps = new string[]{
			"pos.trTime",
			"pos.trBase.x",
			"pos.trBase.y",
			"pos.trDelta.x",
			"pos.trDelta.y",
			"pos.trBase.z",
			"apos.trBase.y",
			"pos.trDelta.z",
			"apos.trBase.x",
			"eventID",
			"angles2.y",
			"eType",
			"eventParam",
			"pos.trType",
			"eFlag",
			"oeIdx",
			"clientNum",
			"angles.y",
			"pos.trDur",
			"apos.trType",
			"origin.x",
			"origin.y",
			"origin.z",
			"solid",
			"sourceID",
			"oeIdx2",
			"generic1",
			"origin2.z",
			"origin2.x",
			"origin2.y",
			"source2ID",
			"angles.x",
			"time",
			"apos.trTime",
			"apos.trDur",
			"apos.trBase.z",
			"apos.trDelta.x",
			"apos.trDelta.y",
			"apos.trDelta.z",
			"time2",
			"angles.z",
			"angles2.x",
			"angles2.z",
			"frame",
			"id",
			"idx",
			};

		Array.Sort(entityStateFields, (x1,x2)=>{
			var i1 = Array.IndexOf(tmps, x1.name);
			var i2 = Array.IndexOf(tmps, x2.name);
			if(i1 < 0 || i2 < 0){
				CLog.Error("entityStateFields sort failed");
				return 0;
			}
			return i1 - i2;
		});

	}

}

public struct EntityShared
{
	public EntityState unused;

	public bool linked;

	public int linkCount;

	public SVFlags svFlags;

	public int singleClinet;

	public bool bmodel;

	public Vector3 mins, maxs;

	public int contents;

	public Vector3 absmin, absmax;

	public Vector3 currentOrigin, currentAngles;

	public int ownerNum;
}

public class SharedEntity
{
	public EntityState s;
	public EntityShared r;
}

public enum EntityEventType
{
	NONE = 0,
	FOOTSTEP,
	FOOTWADE,
	SWIM,

	STEP_4,
	STEP_8,
	STEP_12,
	STEP_16,

	FALL_SHORT,
	FALL_MEDIUM,
	FALL_FAR,

	JUMP_PAD,
	JUMP,

	ITEM_PICKUP,
	GLOBAL_ITEM_PICKUP,

	CAST_SKILL_0,
	CAST_SKILL_1,
	CAST_SKILL_2,
	CAST_SKILL_3,
	CAST_SKILL_4,
	CAST_SKILL_5,
	CAST_SKILL_6,
	CAST_SKILL_7,

	USE_ITEM_0,
	USE_ITEM_1,
	USE_ITEM_2,
	USE_ITEM_3,
	USE_ITEM_4,
	USE_ITEM_5,
	USE_ITEM_6,
	USE_ITEM_7,
	USE_ITEM_8,
	USE_ITEM_9,

	ITEM_RESPAWN,
	ITEM_POP,

	PLAYER_TELEPORT_IN,
	PLAYER_TELEPORT_OUT,

	GENERAL_SOUND,
	GLOBAL_SOUND,
	GLOBAL_ITEM_SOUND,

	MISSILE_HIT,
	MISSILE_MISS,

	HEALTH_REGEN,
	MANA_REGEN,

	DEBUG_LINE,

}

public struct UserCmd
{
	public int serverTime;

	public int[] angles;

	public int buttons;

	public int skillID;

	public sbyte forwardmove, rightmove, upmove;

	public UserCmd(int _serverTime = 0)
	{
		serverTime = _serverTime;
		angles = new int[3];
		buttons = 0;
		skillID = 0;
		forwardmove = 0;
		rightmove = 0;
		upmove = 0;
	}

	public void Reset(){
		serverTime = 0;
		if(angles == null){
			angles = new int[3];
		}else{
			angles[0] = angles[1] = angles[2] = 0;
		}
		buttons = 0;
		skillID = 0;
		forwardmove = 0;
		rightmove = 0;
		upmove = 0;
	}
}

public enum CmdButton
{
	BUTTON_ATTACK = 0x1,
	BUTTON_USE_HOLDABLE = 0x2,

	BUTTON_GESTURE = 0x4,

	BUTTON_WALKING = 0x8,

	BUTTON_MOVE_RUN = 0x10,

	BUTTON_ANY = 0x20,
}

[StructLayout(LayoutKind.Sequential)]
//弹道
public struct Trajectory{
	public int trType; //TrajectoryType

	public int trTime;

	public int trDuration; //如果不是0，trTime + trDuration = stop time

	public unsafe fixed float trBase[3];

	public unsafe fixed float trDelta[3];

	unsafe public int Equals(ref Trajectory value){
		int num = 0;
		if(trType != value.trType){
			num = 1;
		}
		if(trTime != value.trTime){
			num = 2;
		}
		if(trDuration != value.trDuration){
			num = 3;
		}
		
		fixed(float* a = trBase){
			fixed(float* b = value.trBase){
				for(int i = 0; i < 3; i++){
					if(*(a + i) != *(b + i)){
						num = 4 + i;
					}	
				}
			}
		}

		fixed(float* a = trDelta){
			fixed(float* b = value.trDelta){
				for(int i = 0; i < 3; i++){
					if(*(a + i) != *(b + i)){
						num = 7 + i;
					}	
				}
			}
		}
		
		return num;
	}

	unsafe public void GetTrBase(ref Vector3 val){
		fixed(float* a = trBase){
			val.x = *a;
			val.y = *(a+1);
			val.z = *(a+2);
		}
	}

	unsafe public void GetTrDelta(ref Vector3 val){
		fixed(float* a = trDelta){
			val.x = *a;
			val.y = *(a+1);
			val.z = *(a+2);
		}
	}

	unsafe public void SetTrBase(Vector3 val){
		fixed(float* a = trBase){
			*a = val.x;
			*(a+1) = val.y;
			*(a+2) = val.z;
		}
	}

	unsafe public void SetTrDelta(Vector3 val){
		fixed(float* a = trDelta){
			*a = val.x;
			*(a+1) = val.y;
			*(a+2) = val.z;
		}
	}
}


public enum SnapFlags{

	NONE = 0,

	RATE_DELAYED = 1,

	NOT_ACTIVE = 2,

	SERVER_COUNT = 3,
}


public static class EntityFlags{

	public static int NONE = 0;

	public static int DEAD = 0x1;

	public static int TICKING = 0x2;

	public static int TELEPORT_BIT = 0x4; //只要origin变化过大就设置

	public static int AWARD_EXCELLENT = 0x8; //

	public static int PLAYER_EVENT = 0x10;

	public static int BOUNCE = 0x20;

	public static int BOUNCE_HALF = 0x40;

	public static int AWARD_GAUNTLET = 0x80;

	public static int NODRAW = 0x100;

	public static int FIRING = 0x200;

	public static int MOVE_STOP = 0x400;

	public static int AWARD_CAP = 0x800;

	public static int VOTED = 0x1000;

	public static int AWARD_IMPRESSIVE = 0x4000;

	public static int AWARD_DEFEND = 0x8000;

	public static int AWARD_ASSIST = 0x10000;

	public static int AWARD_DENIED = 0x20000;

	public static int CONNECTION = 0x40000;
}

//entityType超过EntityEventType.Event_Count之后，就表示单纯的事件，而不代表一个entity
public static class EntityType
{
	public static int GENERNAL = 1;

	public static int PLAYER = 2;

	public static int ITEM = 3;

	public static int MISSILE = 4;

	public static int MOVER = 5;

	public static int BEAM = 6; //光束

	public static int PUSH_TRIGGER = 7;

	public static int TELEPORT = 8;

	// any of the EV_* events can be added freestanding
	// by setting eType to ET_EVENTS + eventNum
	// this avoids having to set eFlags and eventNum
	// 任何EntityEventType都可以独立地添加，只要设置eType为EVENTS_COUNT + eventNum，这避免了设置eFlags和eventNum
	public static int EVENTS_COUNT = 9;

	public static int EVENT_ENT_1 = 10;
	public static int EVENT_ENT_2 = 11;
	public static int EVENT_ENT_3 = 12;
	public static int EVENT_ENT_4 = 13;
	public static int EVENT_ENT_5 = 14;
	public static int EVENT_ENT_6 = 15;
	public static int EVENT_ENT_7 = 16;
	public static int EVENT_ENT_8 = 17;
	public static int EVENT_ENT_9 = 18;
	public static int EVENT_ENT_10 = 19;
	public static int EVENT_ENT_11 = 20;
	public static int EVENT_ENT_12 = 21;
	public static int EVENT_ENT_13 = 22;
	public static int EVENT_ENT_14 = 23;
	public static int EVENT_ENT_15 = 24;
	public static int EVENT_ENT_16 = 25;
	public static int EVENT_ENT_17 = 26;
	public static int EVENT_ENT_18 = 27;

}

public enum TrajectoryType
{
	STATIONARY = 1,

	INTERPOLATE = 2,

	LINEAR = 3,

	LINEAR_STOP = 4,

	SINE = 5, //value = base + sin(time / duration) * delta

	GRAVITY = 6,
}

public enum SVFlags{
	NONE = 0,

	NO_CLIENT = 0x1,

	CLIENT_MASK = 0x2,

	BOT = 0x8,

	BROADCAST = 0x20,

	PORTAL = 0x40,

	USE_CURRENT_ORIGIN = 0x80,
	
	SINGLE_CLIENT = 0x100,

	NOSERVERINFO = 0x200,

	CAPSULE = 0x400,

	NOTSINGLE_CLIENT = 0x800,
}
