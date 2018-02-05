﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovePhy : MonoBehaviour {

	public float minZ = 0f;

	public float maxZ = 30;

	public Vector3 speed;

	private Camera cam;

	private float z;

	private Vector3 mouseDelta = Vector3.zero;

	private bool start;

	private int moveDir; //1-左，-1-右

	private Rigidbody agent;

	private MoveState moveState;

	private Transform character;

	private float jumpTime;

	private float totalTeleportTime;

	private float curTeleportTime;
	
	private Vector3 teleportVel;

	private Vector3 teleportStartPos;

	public float jumpVel = 8f;

	public float gravity = 13f;

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
		gravity = 26f;
		speed = new Vector3(0.022f,-0.022f,1f);

		teleports = new List<TriggerTeleport>(5);
		agent = gameObject.GetComponent<Rigidbody>();
		character = transform;
		interTrans = character.GetChild(0);

		animContrller = GetComponentInChildren<Animator>();

		modelSize = GetComponent<BoxCollider>().size;

	}
	
	// Update is called once per frame
	void Update () {
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
					agent.useGravity = false;
					lastTrigger = teleports[idx];

					teleportStartPos = transform.position;
					teleportDest = lastTrigger.worldDestPos;
					teleportDest.x += modelSize.x/2 * moveDir;
					totalTeleportTime = Mathf.Sqrt(Mathf.Abs(teleportDest.y - transform.position.y)*2/gravity); //总时间
					curTeleportTime = 0f;
					// teleportVel = new Vector3(moveDir * agent.speed, 0f, 0f);// (teleportDest - teleportStartPos + 0.5f * new Vector3(0f, gravity, 0f)  * teleportTime * teleportTime) / teleportTime;
				}else{
					moveState |= MoveState.Normal;
					dest = Vector3.left * speed.z;// 
				}
			}else if(Input.GetKey(KeyCode.W)){
				moveState |= MoveState.Normal;
				dest = Vector3.back* speed.z;
			}else if(Input.GetKey(KeyCode.S)){
				moveState |= MoveState.Normal;
				dest = Vector3.forward* speed.z;
			}else if(Input.GetKey(KeyCode.A)){ //左
				interTrans.eulerAngles = new Vector3(0,90,0);
				moveDir = 1;
				int idx = -1;
				if((idx = CheckTrigger(false, TeleportType.DownLeft)) >= 0){ //传送到其他地方
					moveState |= MoveState.Teleport;
					moveState &= ~MoveState.Normal;
					agent.useGravity = false;
					lastTrigger = teleports[idx];

					teleportStartPos = transform.position;
					teleportDest = lastTrigger.worldDestPos;
					teleportDest.x += modelSize.x/2 * moveDir;
					totalTeleportTime = Mathf.Sqrt(Mathf.Abs(teleportDest.y - transform.position.y)*2/gravity); //总时间
					curTeleportTime = 0f;
					// teleportVel = new Vector3(moveDir * agent.speed, 0f, 0f);
				}else{
					moveState |= MoveState.Normal;
					dest = Vector3.right* speed.z;
				}
			}else{ //没有一般移动
				moveState &= ~MoveState.Normal;
			}

			//可以同时按下方向键和空格键
			if(Input.GetKey(KeyCode.Space)){ //跳跃
				// if(jumpTime == 0f){
				// 	jumpTime = 0f;
				// }
				// moveState |= MoveState.Jump;
				agent.AddForce(Vector3.up,ForceMode.Acceleration);
				
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
				// agent.Warp(tmp);
				agent.useGravity = true;
				moveState &= ~MoveState.Teleport;
				moveState &= ~MoveState.Jump;
				
				totalTeleportTime = 0f;
				curTeleportTime = -1f;
				lastTrigger = null;
				animContrller.SetInteger("aniState", (int)PlayerAnimationNames.IDLE1);
			}
		}else{
			if((moveState & MoveState.Normal) != MoveState.NONE){ //一般的移动
				if(dest != null){
					// agent.SetDestination(dest.Value);
					agent.AddForce(dest.Value, ForceMode.Impulse);
				}
			}else{
				// agent.Warp(transform.position); //停到当前的位置
			}

			if((moveState & MoveState.Jump) != MoveState.NONE){ //跳跃状态
				
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
		temp.y += 27;

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