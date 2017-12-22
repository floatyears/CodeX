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