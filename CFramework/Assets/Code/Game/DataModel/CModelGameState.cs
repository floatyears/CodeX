using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CModelGameState : IModel {

	// Use this for initialization
	public void Init () {
		
	}
	
	// Update is called once per frame
	public void Dispose () {
		
	}
}

//每一帧的数据，给定时间的服务器呈现。
//服务器会在固定时间产生，不过如果客户端的帧率太高了，有可能不会发送给客户端，或者网络传输过程中被丢弃了。
public struct SnapShot{
	SnapFlags snapFlags;

	int ping;

	int serverTime; //服务器时间（毫秒）

	PlayerState playerState; //玩家当前的所有信息

	int numEntities;

	EntityState[] entities; //所有需要展示的entity

	int numServerCommands;

	int serverCommandSequence;

}

public struct PlayerState{
	int commandTime; //最后执行的cmd的cmd.serverTime;

	PMoveType pmType;

	int bobCycle;

	PMoveFlags pmFlags;

	int pmTime;

	Vector3 origin;

	Vector3 velocity;

	int gravity;

	int speed;

	int[] delta_angles; //




}

public struct EntityState{
	int index;

	// int 
}

public struct PMove{
	PlayerState playerState;

	UserCmd cmd;

	int debugLevel;

	int frameCount;

	int numTouch;

	int pmoveFixed;

	int pmoveMsec;
}

public struct UserCmd
{

}

public enum SnapFlags{

	RATE_DELAYED = 1,

	NOT_ACTIVE = 2,

	SERVER_COUNT = 3,
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

public enum PMoveFlags
{
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