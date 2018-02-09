using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameClient{
	public PlayerState playerState;

	public GameClientPersistant pers;

	public GameClientSession sess;

	public bool readyToExit;

	public bool noclip;

	public int lastCmdTime;

	public int buttons;

	public int oldButtons;

	public int latched_buttons;

	public Vector3 oldOrigin;

	public int lastkilled_client;

	public int lasthurt_client;

	public int lasthurt_mod;

	public int attackTime;

	public int respawnTime;

	public int inactivityTime; //踢掉玩家，如果time > this

	public bool inactivityWarning;

	public int lastKillTime;
	
	public int timeResidual;

	public bool isEliminated; //是否被杀

	public int vote;

	public int historyHead;

	public ClientHistory[] history;

	public ClientHistory saved;

	public int frameOffset;

	public int lastUpdateFrame; //从客户端得到的最后更新的帧

	//--上层实现
	public BaseEntity agent;

	public GameClient(){
		playerState = new PlayerState();
		pers = new GameClientPersistant();
		sess = new GameClientSession();
		history = new ClientHistory[CConstVar.NUM_CLIENT_HISTORY];
		for(int i = 0; i < CConstVar.NUM_CLIENT_HISTORY; i++){
			history[i] = new ClientHistory();
		}
		saved = new ClientHistory();

        agent = new BaseEntity();
		// agent.Init();
	}

	//初始化
	public void Init(){
		agent.Init();
	}

	public void Clear(){

	}

	public void StoreHistory(SharedEntity sEnt, int time){
		historyHead++;
		if(historyHead >= CConstVar.NUM_CLIENT_HISTORY){
			historyHead = 0;
		}

		int head = historyHead;

		history[head].mins = sEnt.r.mins;
		history[head].maxs = sEnt.r.maxs;
		history[head].currentOrigin = sEnt.r.currentOrigin;
		history[head].time = time;
	}

	public void SetPosition(Vector3 pos){
		playerState.origin = pos;
		agent.SetPosition(pos);
	}

	//如果不是正在使用，那么就先销毁掉
	public void Dispose(){
		agent.Dispose();
	}


}

public class GameClientPersistant{
	public ClientConnState connected;

	public UserCmd cmd;

	public bool localClient;

	public bool initialSpawn;

	public bool predictItemPickUp;

	public bool pmoveFixed;

	public string netname;

	public int maxHealth;

	public int enterTime;

	public int voteCount;

	public int teamVoteCount;

	public bool teamInfo;

//unlagged - client options
	//unlagged
	public int delag;

	// public int debugDelag;

	public int cmdTimeNudge;
//unlagged - lag simulation #2
	public int			latentSnaps;
	public int			latentCmds;
	public int			plOut;
	public UserCmd[]	cmdqueue;
	public int			cmdhead;
//unlagged - lag simulation #2

	public int realPing;

	public int[] pingSamples;

	public int sampleHead;

	public int killStreak;

	public int deathStreak;

	public GameClientPersistant(){
		pingSamples = new int[CConstVar.NUM_PING_SAMPLES];

		cmdqueue = new UserCmd[CConstVar.MAX_LATENT_CMDS];
		cmd = new UserCmd();
	}

}

public class GameClientSession {
	public TeamType sessionTeam;

	public int spectorNum;

	public SpectatorState spectatorState;

	public int spectatorClient;

	public int wins, losses;

	public bool teamLeader;
}

public class ClientHistory{
	public Vector3	mins;

	public Vector3 maxs;

	public Vector3 currentOrigin;

	public int time;



}