using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CModelGameState : IModel {

	private int clientFrame;

	private int clientNum;

	private bool demoPlayback;

	private bool levelShot;

	private int deferredPlayerLoading;

	private bool loading; //

	private bool intermissionStarted;

	private int latestSnapshotNum; //客户端收到的最近的snapshots的数据

	private int latestSnapshotTime; //收到latestSnapshotNum的snapshot的时间

	private SnapShot snap; //snap.serverTime = time;

	private SnapShot nextSnap; //snap.serverTime > time or NULL

	private SnapShot[] activeSnapshots; //

	private float frameInterpolation; //(time - snap.serverTime) / (nextSnap.serverTime - snap.serverTime)

	private bool thisFrameTeleport;

	private bool nextFrameTeleport;

	private int frameTime; //time - oldTime

	public int time; //这是客户端当前帧渲染的时间

	public int realTime; //忽略暂停

	private int oldTime; //这是上一帧的时间，用于missile trails 和 prediction checking

	private int physicsTime; //snap.time或者nextSnap.time

	private int timeLimitWarnings; //

	private int frameLimitWarnings;

	private bool mapRestart;

	private bool renderingThirdPerson;

	private bool hyperspace; //prediction state

	private PlayerState predictedPlayerState;

	private ClientEntity preditedPlayerEntity;

	private bool validPPS; //第一次调用PredictPlayerState之后，保证predictedPlayerState是一个可访问的状态

	private int predictedErrorTime;

	private Vector3 predictedError;

	private int eventSequence;

	private int[] predictableEvents;

	private float stepChange;

	private int stepTime;

	private float duckChange;

	private int duckTime;

	private float landChange;

	private int landTime;

	//attack player
	private int attackerTime;

	public int paused = 0;

	//是否记录
	public int journal = 1;

	private ClientActive clientActive;

	public ClientActive ClientActive{
		get{
			return clientActive;
		}
	}

	// Use this for initialization
	public void Init () {
		
	}
	
	
	public void Dispose () {
		
	}
}

public struct ClientActive
{
	public int timeoutCount;

	public ClientSnapshot snap;

	public int serverTime;

	public int oldServerTime; //防止时间倒退的情况

	public int oldFrameServerTime;

	public int serverTimeDelta; // serverTime = realTime + serverTimeDelta;这个时间会随着延迟而变化

	public bool extrapolateSnapshot; //在任何客户端帧被强制向外插值时设置

	public bool newSnapshots; //在解包了任何可用的消息时设置

	public int parseEntitiesIndex;

	public int[] mouseDx;

	public int[] mouseDy;

	public int mouseIndex;

	public int[] joystickAxis;

	public int userCmdValue;

	public float sensitivity;

	public UserCmd[] cmds;

	public int cmdNum; //每一帧都在增长，因为可能好几帧被打包到一起

	public OutPacket[] outPackets;

	public Vector3 viewAngles;

	public int serverID;

	public ClientSnapshot[] snapshots;

	public EntityState[] entityBaselines; //用于增量更新，如果没有在上一帧里面那么就用baseline内的来计算

	public EntityState[] parseEntities;


}

//客户端所用的snapshot，
public class ClientSnapshot
{
	public bool valid;

	public SnapFlags snapFlags;

	public int serverTime;

	public int messageNum;

	public int deltaNum;

	public int ping;

	public int cmdNum;

	public PlayerState playerState;

	public int numEntities; //所需要在这一帧显示的entity的数量

	public int parseEntitiesIndex; //指向循环列表内的索引值

	public int serverCommandNum; //执行这个指令之前的所有指令，使这一帧成为当前帧。
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

public struct OutPacket
{
	public int cmdNum;

	public int serverTime;

	public int realTime; //packet发送时的realTime
}