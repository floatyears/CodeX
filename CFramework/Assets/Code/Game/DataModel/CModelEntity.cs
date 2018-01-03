using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CModelEntity : IModel {

	public void Init()
	{

	}	
	
	public void Dispose()
	{

	}

}

public struct ClientEntity{
	EntityState currentState;

	EntityState nextState;

	bool interpolate; //true:下一帧可以插值

	bool currentValid;

	int previousEvent;

	int teleportFlag;

	int trailTime; //missile可以处理丢包的情况

	int miscTime;

	int snapShotTime; //这个entity在帧中出现的最后时间

	PlayerEntity playerEntity;

	int errorTime; //从当前时间开始衰减出现的错误

	Vector3 errorOrigin;

	Vector3 errorAngle;

	bool extrapolated; //false if origin/angles is an interpolation

	Vector3 rawOrigin;

	Vector3 rawAngles;

	Vector3 beamEnd;

	//entity在这一帧的准确插值位置
	Vector3 lerpOrigin;

	Vector3 lerpAngles;
}

public struct EntityState{
	public int entityID;

	public int entityIndex;

	public EntityEventType entityType;

	public EntityFlags entityFlags;

	public Trajectory pos;

	public Trajectory apos; //用于计算角度

	public int time;

	public int time2;

	public Vector3 origin; //origin

	public Vector3 origin2; //target

	public Vector3 angles; //origin

	public Vector3 angles2; //target

	public int otherEntityID;

	public int otherEntity2ID;

	public int sourceID;

	public int source2ID;

	public int clientNum; //范围是0-(MAX_CLIENT - 1)

	public int frame;

	public int solid;

	public EventType eventID;

	public int eventParam;

}

public struct EntityShared
{
	public EntityState unused;

	public bool linked;

	public int linkCount;

	public int svFlags;

	public int singleClinet;

	public bool bmodel;

	public Vector3 mins, maxs;

	public int contents;

	public Vector3 absmin, absmax;

	public Vector3 currentOrigin, currentAngles;

	public int ownerNum;
}

public struct SharedEntity
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
	int serverTime;

	int[] angles;

	int buttons;

	int skillID;
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
	TrajectoryType trType;

	int trTime;

	int trDuration; //如果不是0，trTime + trDuration = stop time

	Vector3 trBase;

	Vector3 trDelta; //velocity
}


public enum SnapFlags{

	RATE_DELAYED = 1,

	NOT_ACTIVE = 2,

	SERVER_COUNT = 3,
}


public enum EntityFlags{
	DEAD = 0x1,

	TELEPORT_BIT = 0x2, //只要origin变化过大就设置

	BOUNCE = 0x4,

	BOUNCE_HALF = 0x8,

	NODRAW = 0x10,

	FIRING = 0x20,

	MOVE_STOP = 0x40,

	AWARD_CAP = 0x80,

	VOTED = 0x100,

	AWARD_EXCELLENT = 0x200, //

	AWARD_IMPRESSIVE = 0x400,

	AWARD_DEFEND = 0x800,

	AWARD_ASSIST = 0x1000,

	AWARD_DENIED = 0x2000,
}

public enum EntityType
{
	GENERNAL = 1,

	PLAYER = 2,

	ITEM = 3,

	MISSILE = 4,

	MOVER = 5,

	BEAM = 6, //光束

	PUSH_TRIGGER = 7,

	TELEPORT
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
