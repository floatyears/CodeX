using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlatBuffers;

//玩家数据模型
public class CModelPlayer : IModel
{	
	public int roleID;

	public int roleName;

	public void Init()
	{
		
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

	public int damageEvent;

	public int damageCount;

	public int[] states;

	public int[] persistant;

	public int ping;

	public int pm_framecount;

	public int entityEventSequence;
}

public struct PMove{
	public PlayerState playerState;

	public UserCmd cmd;

	public int debugLevel;

	public int frameCount;

	public int numTouch;

	public int pmoveFixed;

	public int pmoveMsec;
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
