using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PMove {
	public PlayerState playerState;

	public UserCmd cmd;

	public int tracemask;

	public int debugLevel;

	public bool noFootsteps;

	public bool gauntletHit;

	public int frameCount;

	public int numTouch;

	public int pmoveFixed;

	public int pmoveMsec;

	public int pmoveFloat;

	public int pmvoveFlags;

	private static PMoveImp impl;

// movement parameters
	private const float	pm_stopspeed = 100.0f;
	private const float	pm_duckScale = 0.25f;
	private const float	pm_swimScale = 0.50f;

	private const float	pm_accelerate = 10.0f;
	private const float	pm_airaccelerate = 1.0f;
	private const float	pm_wateraccelerate = 4.0f;
	private const float	pm_flyaccelerate = 8.0f;

	private const float	pm_friction = 6.0f;
	private const float	pm_waterfriction = 1.0f;
	private const float	pm_flightfriction = 3.0f;
	private const float	pm_spectatorfriction = 5.0f;

	private static int DEFAULT_VIEWHEIGHT = 26;

	public PMove(){
		playerState = new PlayerState();
		cmd = new UserCmd();

	}

	// Use this for initialization
	public void Move(){
		int finalTime = cmd.serverTime;

		if(finalTime < playerState.commandTime){
			return;
		}

		if(finalTime > playerState.commandTime + 1000){
			playerState.commandTime = finalTime - 1000;
		}

		playerState.pm_framecount = (playerState.pm_framecount + 1) & ((1<<CConstVar.PS_PMOVEFRAMECOUNTBITS) - 1);

		while(playerState.commandTime != finalTime){
			int msec = finalTime - playerState.commandTime;
			if(pmoveFixed > 0){
				if(msec > pmoveMsec){
					msec = pmoveMsec;
				}
			}else{
				if(msec > 66){
					msec = 66;
				}
			}

			cmd.serverTime = playerState.commandTime + msec;
			PMoveSingle(this);

			if((playerState.pmFlags & PMoveFlags.JUMP_HELD) != PMoveFlags.NONE){
				cmd.upmove = 20;
			}
		}
	}

	private void PMoveSingle(PMove pMove){
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

		if(impl == null){
			impl = new PMoveImp();
		}else{
			impl.Reset();
		}

		impl.msec = pMove.cmd.serverTime - pMove.playerState.commandTime;
		if(impl.msec < 1){
			impl.msec = 1;
		}else if(impl.msec > 200){
			impl.msec = 200;
		}

		pMove.playerState.commandTime = pMove.cmd.serverTime;
		pMove.playerState.origin = impl.prevOrigin;
		pMove.playerState.velocity = impl.prevVelocity;

		impl.frameTime = impl.msec * 0.001f;

		UpdateViewAngles(pMove.playerState, pMove.cmd);

		AngleVectors(pMove.playerState.viewangles, ref impl.forward, ref impl.right, ref impl.up);

		if(pMove.cmd.upmove < 10){
			pMove.playerState.pmFlags &= ~PMoveFlags.JUMP_HELD;
		}

		if(pMove.cmd.forwardmove < 0){
			pMove.playerState.pmFlags |= PMoveFlags.BACKWARDS_RUN;
		}else if(pMove.cmd.forwardmove > 0 || (pMove.cmd.forwardmove == 0 && pMove.cmd.rightmove > 0)){
			pMove.playerState.pmFlags &= ~PMoveFlags.BACKWARDS_RUN;
		}

		if(pMove.playerState.pmType >= PMoveType.DEAD){
			pMove.cmd.forwardmove = 0;
			pMove.cmd.rightmove = 0;
			pMove.cmd.upmove = 0;
		}

		if(pMove.playerState.pmType == PMoveType.SPECTATOR){
			pMove.CheckDuck();
			// FlyMove();
			// DropTimers();
			return;
		}

		if(pMove.playerState.pmType == PMoveType.NOCLIP){
			pMove.NoClipMove();
			// DropTimers();
			return;
		}

		if(pMove.playerState.pmType == PMoveType.FREEZE){
			return;
		}

		if(pMove.playerState.pmType == PMoveType.INTERMISSION || pMove.playerState.pmType == PMoveType.SPINGTERMISSION){
			return;
		}

		pMove.CheckDuck();
		
		if(pMove.playerState.pmType == PMoveType.DEAD){
			pMove.DeadMove();
		}

		if(impl.walking){
			WalkMove();
		}else{
			AirMove();
		}
		
	}

	private void WalkMove()
	{
		if(CheckJump()){
			//空中
			AirMove();
		}
		return;
	}



	private void AirMove()
	{

	}

	private void Friction()
	{
		var vel = playerState.velocity;
		Vector3 vec = vel;
		if(impl.walking){
			vec[2] = 0f;
		}
		float speed = vec.magnitude;
		if(speed < 1){
			vel[0] = 0;
			vel[1] = 0;
			return;
		}
		float drop = 0;
		if(playerState.pmType == PMoveType.SPECTATOR){
			drop += speed * pm_spectatorfriction * impl.frameTime;
		}

		float newspeed = speed - drop;
		if(newspeed < 0){
			newspeed = 0;
		}
		newspeed /= speed;
		vel[0] = vel[0] * newspeed;
		vel[1] = vel[1] * newspeed;
		vel[2] = vel[2] * newspeed;
	}

	private bool CheckJump()
	{
		if((playerState.pmFlags & PMoveFlags.RESPAWNED) != PMoveFlags.RESPAWNED){
			return false;
		}

		if(cmd.upmove < 10){
			return false;
		}

		if((playerState.pmFlags & PMoveFlags.JUMP_HELD ) != PMoveFlags.NONE){
			cmd.upmove = 0;
			return false;
		}

		impl.groundPlane = false;
		impl.walking = false;
		playerState.pmFlags |= PMoveFlags.JUMP_HELD;

		playerState.groundEntityNum = CConstVar.ENTITYNUM_NONE;
		playerState.velocity[2] = CConstVar.JUMP_VELOCITY;

		CUtils.BG_AddPredictableEventToPlayerstate((int)EntityEventType.JUMP,0,playerState);
		if(cmd.forwardmove >= 0){
			playerState.pmFlags &= ~PMoveFlags.BACKWARDS_JUMP;
		}else{
			playerState.pmFlags |= PMoveFlags.BACKWARDS_JUMP;
		}
		return true;

	}

	private void NoClipMove()
	{
		playerState.viewHeight = DEFAULT_VIEWHEIGHT;

		float drop = 0f;
		float friction = 0f;
		float control = 0f;
		float newspeed = 0f;
		float wishspeed = 0f;
		float scale = 1f;
		float fmove, smove;
		Vector3 wishdir;
		Vector3 wishel = Vector3.zero;
		Vector3 vorigin;
		float speed = playerState.velocity.sqrMagnitude;
		if(speed < 1){
			vorigin = playerState.velocity;
		}else{
			drop = 0f;
			friction = pm_friction * 1.5f;
			control = speed < pm_stopspeed ? pm_stopspeed : speed;
			drop += control * friction * impl.frameTime;

			newspeed = speed - drop;
			if(newspeed < 0){
				newspeed = 0;
			}
			newspeed /= speed;

			playerState.velocity *= newspeed;
		}

		scale = CMDScale(cmd);

		fmove = cmd.forwardmove;
		smove = cmd.rightmove;

		for(int i = 0; i < 3; i++){
			wishel[i] = impl.forward[i]*fmove + impl.right[1] * smove;
		}
		wishel[2] = cmd.upmove;

		wishdir = wishel;
		wishspeed = wishdir.magnitude;
		wishspeed *= scale;

		wishdir = Vector3.Normalize(wishdir);
		Accelerate(wishdir, wishspeed, pm_accelerate);

		playerState.origin = playerState.origin + playerState.velocity * impl.frameTime;
	}

	private void CheckDuck()
	{

	}

	private void DeadMove()
	{
		if(!impl.walking){
			return;
		}

		float forward = playerState.velocity.magnitude;
		forward -= 20;
		if(forward <= 20){
			playerState.velocity = Vector3.zero;
		}else{
			playerState.velocity = Vector3.Normalize(playerState.velocity);
			playerState.velocity *= forward;
		}
	}

	public static void UpdateViewAngles(PlayerState ps, UserCmd cmd)
	{
		if(ps.pmType == PMoveType.INTERMISSION || ps.pmType == PMoveType.SPINGTERMISSION){
			return;
		}

		//TODO:state
		if(ps.pmType != PMoveType.SPECTATOR && ps.states[0] <= 0){
			return;
		}

		int temp;
		for(int i = 0; i < 3; i++){
			temp = cmd.angles[i] + ps.delta_angles[i];
			if(i == 0){
				if(temp > 16000){
					ps.delta_angles[i] = 16000 - cmd.angles[i];
					temp = 16000;
				}else if(temp < -16000){
					ps.delta_angles[i] = -16000 - cmd.angles[i];
					temp = -16000;
				}
			}
			ps.viewangles[i] = temp * 360 / 65536;
		}
	}

	//处理玩家的加速情况
	private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
	{
		float currentspeed = Vector3.Dot(playerState.velocity, wishdir);
		float addspeed = wishspeed - currentspeed;
		if(addspeed <= 0){
			return;
		}
		float accelspeed = accel * impl.frameTime * wishspeed;
		if(accelspeed > addspeed){
			accelspeed = addspeed;
		}

		for(int i = 0; i < 3; i++){
			playerState.velocity[i] += accelspeed * wishdir[i];
		}
	}

	private float CMDScale(UserCmd cmd){
		int max = Mathf.Abs(cmd.forwardmove);
		if(Mathf.Abs(cmd.rightmove) > max){
			max = Mathf.Abs(cmd.rightmove);
		}
		if(Mathf.Abs(cmd.upmove) > max){
			max = Mathf.Abs(cmd.upmove);
		}

		if(max == 0){
			return 0;
		}

		float total = Mathf.Sqrt(cmd.forwardmove * cmd.forwardmove + cmd.rightmove * cmd.rightmove + cmd.upmove * cmd.upmove);
		float scale = (float)playerState.speed * max / (127f * total);
		return scale;
	}

	public void AngleVectors(Vector3 angles, ref Vector3 forward, ref Vector3 right, ref Vector3 up) 
	{
		float		angle;
		float		sr, sp, sy, cr, cp, cy;
		// static to help MS compiler fp bugs

		angle = angles[CConstVar.YAW] * (CConstVar.M_PI*2 / 360);
		sy = Mathf.Sin(angle);
		cy = Mathf.Cos(angle);
		angle = angles[CConstVar.PITCH] * (CConstVar.M_PI*2 / 360);
		sp = Mathf.Sin(angle);
		cp = Mathf.Cos(angle);
		angle = angles[CConstVar.ROLL] * (CConstVar.M_PI*2 / 360);
		sr = Mathf.Sin(angle);
		cr = Mathf.Cos(angle);

		// if (forward)
		// {
			forward[0] = cp*cy;
			forward[1] = cp*sy;
			forward[2] = -sp;
		// }
		// if (right)
		// {
			right[0] = (-1*sr*sp*cy+-1*cr*-sy);
			right[1] = (-1*sr*sp*sy+-1*cr*cy);
			right[2] = -1*sr*cp;
		// }
		// if (up)
		{
			up[0] = (cr*sp*cy+-sr*-sy);
			up[1] = (cr*sp*sy+-sr*cy);
			up[2] = cr*cp;
		}
	}
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
	NONE = 0,

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

public class PMoveImp{
	public Vector3 forward;
	public Vector3 right;
	public Vector3 up;

	public float frameTime;

	public int msec;

	public bool walking;

	public bool groundPlane;

	public float impactSpeed;

	public Vector3 prevOrigin;

	public Vector3 prevVelocity;

	public void Reset(){
		forward = right = up = Vector3.zero;
		msec = 0;
		walking = false;
		groundPlane = false;
		impactSpeed = 0f;
		prevOrigin = prevVelocity = Vector3.zero;
	}

}
