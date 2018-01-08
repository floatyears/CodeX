using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

	public EntityEventType previousEvent;

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

	public SvEntityState nextEntityInWorldSector;
	public EntityState baseline;

	public int numClusters;

	public int clusterNums;

	public int snapshotCounter;
}

public struct EntityState{
	public int entityID;

	public int entityIndex;

	//entityType超过EntityEventType.Event_Count之后，就表示单纯的事件，而不代表一个entity
	public EntityType entityType;

	public EntityFlags entityFlags;

	public Trajectory pos;

	public Trajectory apos; //用于计算角度

	public int time;

	public int time2;

	public Vector3 origin; //origin

	public Vector3 origin2; //target

	public Vector3 angles; //origin

	public Vector3 angles2; //target

	public int otherEntityIdx;

	public int otherEntity2ID;

	public int sourceID;

	public int source2ID;

	public int clientNum; //范围是0-(MAX_CLIENT - 1)

	public int frame;

	public int solid;

	public EntityEventType eventID;

	public int eventParam;

	public int generic1;

	public void CopyTo(EntityState to){
		to.entityID = entityID;
		to.entityIndex = entityIndex;
		to.angles =angles;
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

//弹道
public struct Trajectory{
	public TrajectoryType trType;

	public int trTime;

	public int trDuration; //如果不是0，trTime + trDuration = stop time

	public Vector3 trBase;

	public Vector3 trDelta; //velocity
}


public enum SnapFlags{

	NONE = 0,

	RATE_DELAYED = 1,

	NOT_ACTIVE = 2,

	SERVER_COUNT = 3,
}


public enum EntityFlags{

	NONE = 0,

	DEAD = 0x1,

	TICKING = 0x2,

	TELEPORT_BIT = 0x4, //只要origin变化过大就设置

	AWARD_EXCELLENT = 0x8, //

	PLAYER_EVENT = 0x10,

	BOUNCE = 0x20,

	BOUNCE_HALF = 0x40,

	AWARD_GAUNTLET = 0x80,

	NODRAW = 0x100,

	FIRING = 0x200,

	MOVE_STOP = 0x400,

	AWARD_CAP = 0x800,

	VOTED = 0x1000,

	AWARD_IMPRESSIVE = 0x4000,

	AWARD_DEFEND = 0x8000,

	AWARD_ASSIST = 0x10000,

	AWARD_DENIED = 0x20000,
}

//entityType超过EntityEventType.Event_Count之后，就表示单纯的事件，而不代表一个entity
public enum EntityType
{
	GENERNAL = 1,

	PLAYER = 2,

	ITEM = 3,

	MISSILE = 4,

	MOVER = 5,

	BEAM = 6, //光束

	PUSH_TRIGGER = 7,

	TELEPORT,

	// any of the EV_* events can be added freestanding
	// by setting eType to ET_EVENTS + eventNum
	// this avoids having to set eFlags and eventNum
	// 任何EntityEventType都可以独立地添加，只要设置eType为EVENTS_COUNT + eventNum，这避免了设置eFlags和eventNum
	EVENTS_COUNT,

	EVENT_ENT_1,
	EVENT_ENT_2,
	EVENT_ENT_3,
	EVENT_ENT_4,
	EVENT_ENT_5,
	EVENT_ENT_6,
	EVENT_ENT_7,
	EVENT_ENT_8,
	EVENT_ENT_9,
	EVENT_ENT_10,
	EVENT_ENT_11,
	EVENT_ENT_12,
	EVENT_ENT_13,
	EVENT_ENT_14,
	EVENT_ENT_15,
	EVENT_ENT_16,
	EVENT_ENT_17,
	EVENT_ENT_18,

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
