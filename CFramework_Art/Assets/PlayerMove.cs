using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour {

	public float minZ = 0f;

	public float maxZ = 30;

	public Vector3 speed;

	private Camera cam;

	private float z;

	private Vector3 mouseDelta = Vector3.zero;

	private bool start;

	private int moveDir; //1-左，-1-右

	private NavMeshAgent agent;

	private MoveState moveState;

	private Transform character;

	private float jumpTime;

	private float totalTeleportTime;

	private float curTeleportTime;
	
	private Vector3 teleportVel;

	private Vector3 teleportStartPos;

	public float jumpVel = 8f;

	public float gravity = 13f;

	private NavMeshPath curPath;

	private Vector3 teleportDest;

	private List<TriggerTeleport> teleports;

	private TriggerTeleport lastTrigger; //最后一个触发的触发器

	private Transform interTrans;

	private Animator animContrller;

	private float scrollDelta;

	private Vector3 modelSize;

	// Use this for initialization
	void Start () {
		cam = Camera.main;
		mouseDelta = cam.transform.position;
		z = cam.transform.position.z;

		jumpVel = 8f;
		gravity = 13f;
		speed = new Vector3(0.022f,-0.022f,1f);
		curPath = new NavMeshPath();

		teleports = new List<TriggerTeleport>(5);
		agent = gameObject.GetComponent<NavMeshAgent>();
		character = transform;
		interTrans = character.GetChild(0);

		animContrller = GetComponentInChildren<Animator>();

		modelSize = GetComponent<BoxCollider>().size;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.LeftArrow)){ //左
			agent.Move(Vector3.left * speed.z);
		}else if(Input.GetKey(KeyCode.UpArrow)){
			agent.Move(Vector3.back* speed.z);
		}else if(Input.GetKey(KeyCode.DownArrow)){
			agent.Move(Vector3.forward* speed.z);
		}else if(Input.GetKey(KeyCode.RightArrow)){ //下
			agent.Move(Vector3.right* speed.z);  
		}
		else if(Input.GetKey(KeyCode.C)){
		}
		
		Vector3? dest = null;

		//传送过程中不能操作
		if((moveState & MoveState.Teleport) == MoveState.NONE){
			if(Input.GetKey(KeyCode.D)){ //右
				moveDir = -1; //右边
				interTrans.eulerAngles = new Vector3(0,270,0);
				int idx = -1;
				if((idx = CheckTrigger(false, TeleportType.DownRight)) >= 0){ //传送到其他地方
					moveState |= MoveState.Teleport;
					moveState &= ~MoveState.Normal;
					agent.updatePosition = false;
					lastTrigger = teleports[idx];

					teleportStartPos = transform.position;
					teleportDest = lastTrigger.worldDestPos;
					teleportDest.x += modelSize.x/2 * moveDir;
					totalTeleportTime = Mathf.Sqrt(Mathf.Abs(teleportDest.y - transform.position.y)*2/gravity); //总时间
					curTeleportTime = 0f;
					teleportVel = new Vector3(moveDir * agent.speed, 0f, 0f);// (teleportDest - teleportStartPos + 0.5f * new Vector3(0f, gravity, 0f)  * teleportTime * teleportTime) / teleportTime;
				}else{
					moveState |= MoveState.Normal;
					dest = agent.transform.position + Vector3.left * speed.z;// 
				}
			}else if(Input.GetKey(KeyCode.W)){
				moveState |= MoveState.Normal;
				dest = agent.transform.position + Vector3.back* speed.z;
			}else if(Input.GetKey(KeyCode.S)){
				moveState |= MoveState.Normal;
				dest = agent.transform.position + Vector3.forward* speed.z;
			}else if(Input.GetKey(KeyCode.A)){ //左
				interTrans.eulerAngles = new Vector3(0,90,0);
				moveDir = 1;
				int idx = -1;
				if((idx = CheckTrigger(false, TeleportType.DownLeft)) >= 0){ //传送到其他地方
					moveState |= MoveState.Teleport;
					moveState &= ~MoveState.Normal;
					agent.updatePosition = false;
					lastTrigger = teleports[idx];

					teleportStartPos = transform.position;
					teleportDest = lastTrigger.worldDestPos;
					teleportDest.x += modelSize.x/2 * moveDir;
					totalTeleportTime = Mathf.Sqrt(Mathf.Abs(teleportDest.y - transform.position.y)*2/gravity); //总时间
					curTeleportTime = 0f;
					teleportVel = new Vector3(moveDir * agent.speed, 0f, 0f);
				}else{
					moveState |= MoveState.Normal;
					dest = agent.transform.position + Vector3.right* speed.z;
				}
			}else{ //没有一般移动
				moveState &= ~MoveState.Normal;
			}

			//可以同时按下方向键和空格键
			if(Input.GetKey(KeyCode.Space)){ //跳跃
				if(jumpTime == 0f){
					jumpTime = 0f;
				}
				moveState |= MoveState.Jump;

				int idx = -1;
				if((idx = CheckTrigger(false, moveDir == -1 ? TeleportType.JumpRight : TeleportType.JumpLeft)) >= 0){ //传送到其他地方
					moveState |= MoveState.Teleport;
					moveState &= ~MoveState.Normal;
					agent.updatePosition = false;
					lastTrigger = teleports[idx];

					teleportStartPos = transform.position;
					teleportDest = lastTrigger.worldDestPos;
					float y = 0f;
					if(lastTrigger.type == TeleportType.UpLeft || lastTrigger.type == TeleportType.UpRight){ //向上跳需要更大的加速度
						y = teleportDest.y - transform.position.y;// + 1f;
						teleportVel = new Vector3(agent.speed * moveDir, gravity * Mathf.Sqrt(2*y/gravity), 0f);
						totalTeleportTime = Mathf.Sqrt(2*y/gravity);// + Mathf.Sqrt(2/gravity);
					}else{
						teleportVel = new Vector3(agent.speed * moveDir, jumpVel, 0f);
						y = transform.position.y - teleportDest.y;
						totalTeleportTime = jumpVel/gravity + Mathf.Sqrt(2*( 0.5f * jumpVel * jumpVel / gravity + transform.position.y - teleportDest.y)/gravity);
					}
					curTeleportTime = 0f;
				}
			}
		}

		if((moveState & MoveState.Teleport) != MoveState.NONE){ //传送状态
			if(curTeleportTime < totalTeleportTime){
				curTeleportTime += Time.deltaTime;
				transform.position = CalcTeleportPos(curTeleportTime);
				interTrans.localPosition = Vector3.zero;
			}else{
				var tmp = transform.position;
				tmp.y = teleportDest.y;
				agent.Warp(tmp);
				agent.updatePosition = true;
				moveState &= ~MoveState.Teleport;
				moveState &= ~MoveState.Jump;
				
				totalTeleportTime = 0f;
				curTeleportTime = -1f;
				lastTrigger = null;
			}
		}else{
			if((moveState & MoveState.Normal) != MoveState.NONE){ //一般的移动
				if(dest != null){
					agent.SetDestination(dest.Value);
				}
			}else{
				agent.Warp(transform.position); //停到当前的位置
			}

			if((moveState & MoveState.Jump) != MoveState.NONE){ //跳跃状态
				var y = jumpVel * jumpTime - 0.5f * gravity * jumpTime * jumpTime;
				jumpTime += Time.deltaTime;
				if(jumpTime > 0.1f && y < 0f){
					moveState &= ~MoveState.Jump;
					y = 0f;
					jumpTime = 0f;
				}
				interTrans.localPosition = new Vector3(0, y, 0);
			}	
		}

		//动作的处理部分
		if((moveState & MoveState.Jump) != MoveState.NONE){
			animContrller.SetInteger("aniState", (int)PlayerAnimationNames.JUMP);
		}else if((moveState & MoveState.Normal) != MoveState.NONE){
			animContrller.SetInteger("aniState", (int)PlayerAnimationNames.RUN);
		}else{
			animContrller.SetInteger("aniState", (int)PlayerAnimationNames.IDLE1);
		}

		if(Input.mouseScrollDelta.y != 0f){
			scrollDelta += Input.mouseScrollDelta.y;
			z += Input.mouseScrollDelta.y;
			z = Mathf.Max(z, minZ);
			z = Mathf.Min(z, maxZ);
		}
		

		var temp = character.position;
		temp.z = z;

		cam.transform.position = temp;
	}

	/// <summary>
	/// OnTriggerEnter is called when the Collider other enters the trigger.
	/// </summary>
	/// <param name="other">The other Collider involved in this collision.</param>
	void OnTriggerEnter(Collider other)
	{
		var tmp = other.GetComponent<TriggerTeleport>();
		if(tmp != null){
			teleports.Add(tmp);
			// teleports.Sort((x1,x2)=>{return x1.});
		}		
	}

	/// <summary>
	/// OnTriggerExit is called when the Collider other has stopped touching the trigger.
	/// </summary>
	/// <param name="other">The other Collider involved in this collision.</param>
	void OnTriggerExit(Collider other)
	{
		var tmp = other.GetComponent<TriggerTeleport>();
		if(tmp != null && teleports.Contains(tmp)){
			teleports.Remove(tmp);
		}
	}

	private int CheckTrigger(bool isAuto, TeleportType type){
		int len = teleports.Count;
		for(int i = 0; i < len; i++){
			if(teleports[i].IsAuto == isAuto && (teleports[i].type & type) != TeleportType.None ){
				return i;
			}
		}
		return -1;
	}

	private Vector3 CalcTeleportPos(float time){
		return teleportStartPos + teleportVel * time + 0.5f * new Vector3(0f, -gravity, 0f) * time * time;
	}
}

public enum MoveState{
	NONE = 0,

	STOP = 0x1,

	Normal = 0x2, //正常移动，和Jump可以共存，不能跟Teleport共存

	Jump = 0x4,

	Teleport = 0x8, //处于offmeshlink传输过程中（无法取消）

	JumpSec = 0x10, //二级跳
}

internal enum PlayerAnimationNames
{
    WAIT = 0,
    HURT,
    DIE,
    RUN,
    SKILL01,
    SKILL01_START,
    SKILL01_LOOP,
    SKILL01_END,
    SKILL02,
    SKILL02_START,
    SKILL02_LOOP,
    SKILL02_END,
    SKILL03,
    SKILL03_START,
    SKILL03_LOOP,
    SKILL03_END,
    SKILL04,
    SKILL04_START,
    SKILL04_LOOP,
    SKILL04_END,
    STUN,
    RIDING,
    CAPTURE,
    DEFENSE,
    WIN,
	ATTACK01,
    IDLE,
    R_IDLE,
	DISMOUNT,
    IDLE1,
    JUMP,
    JUMP2,
    COUNT,
}
