using System.Collections;
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

	public float jumpFactor = 8f;

	public float secJumpFactor = 5f;

	public float airMoveFactor = 1f;

	public float moveFactor = 1f;

	public float gravity = 13f;

	private Vector3 teleportDest;

	private List<TriggerTeleport> teleports;

	private TriggerTeleport lastTrigger; //最后一个触发的触发器

	private Transform interTrans;

	private Animator animContrller;

	private float scrollDelta;

	private Vector3 modelSize;

	private float yStayTime;

	private bool offGround = false;

	// Use this for initialization
	void Start () {
		cam = Camera.main;
		mouseDelta = cam.transform.position;
		z = cam.transform.position.z;

		gravity = -26f;
		// speed = new Vector3(0.1f,5f,3f);

		teleports = new List<TriggerTeleport>(5);
		agent = gameObject.GetComponent<Rigidbody>();
		character = transform;
		interTrans = character.GetChild(0);

		animContrller = GetComponentInChildren<Animator>();

		// modelSize = GetComponent<CapsuleCollider>().size;
	}
	
	// Update is called once per frame
	void Update () {
		if(Physics.gravity.y != gravity){
			Physics.gravity = new Vector3(0f,gravity,0f);
		}
		Vector3? dest = null;

		//传送过程中不能操作
		if(Input.GetKey(KeyCode.D)){ //右
			moveDir = -1; //右边
			interTrans.eulerAngles = new Vector3(0,270,0);
			dest = Vector3.left;
		}else if(Input.GetKey(KeyCode.W)){
			dest = Vector3.back;
		}else if(Input.GetKey(KeyCode.S)){
			dest = Vector3.forward;
		}else if(Input.GetKey(KeyCode.A)){ //左
			moveDir = 1;
			interTrans.eulerAngles = new Vector3(0,90,0);
			dest = Vector3.right;
		}
		if(Input.GetKeyDown(KeyCode.Space)){ //跳跃
			if(dest == null){
				dest = Vector3.up * jumpFactor;
			}else{
				dest = new Vector3(dest.Value.x, jumpFactor, dest.Value.z);
			}
		}

		if((moveState & MoveState.Jump) != MoveState.NONE){ //跳跃状态
			if(dest != null){
				if(jumpTime > 0.1f && dest.Value.y > 0){ //进入二级跳
					agent.AddForce(Vector3.up * secJumpFactor, ForceMode.Impulse);
					moveState |= MoveState.JumpSec;
					moveState &= ~MoveState.Jump;
				}else{
					dest *= airMoveFactor;
					agent.AddForce(new Vector3(dest.Value.x, 0f, dest.Value.z), ForceMode.VelocityChange); //空中的时候也可以移动
				}
			}
			jumpTime += Time.deltaTime;
		}else if((moveState & MoveState.JumpSec) != MoveState.NONE){ //二级跳跃状态
			if(dest != null){
				dest *= airMoveFactor;
				agent.AddForce(new Vector3(dest.Value.x, 0f, dest.Value.z), ForceMode.VelocityChange); //空中的时候也可以移动
			}
			jumpTime += Time.deltaTime;
		}else if((moveState & MoveState.Normal) != MoveState.NONE){ //一般的移动
			if(dest != null){
				var tmp = dest.Value * moveFactor;
				agent.velocity = new Vector3(tmp.x, agent.velocity.y, tmp.z);
				
				if(dest.Value.y > 0f){ //add force
					moveState |= MoveState.Jump;
					jumpTime = 0f;
					yStayTime = 0f;
					Debug.Log("jump start");
					agent.AddForce(new Vector3(0f, dest.Value.y, 0f), ForceMode.Impulse);
				}
			}
		}else{
			if(dest != null){
				if(dest.Value.y > 0f){ //add force
					moveState |= MoveState.Jump;
					jumpTime = 0f;
					yStayTime = 0f;
					Debug.Log("jump start");
					agent.AddForce(new Vector3(0f, dest.Value.y, 0f), ForceMode.Impulse);
				}else{
					if(dest.Value.x != 0f || dest.Value.z != 0f){
						moveState |= MoveState.Normal;
						var tmp = dest.Value * moveFactor;
						agent.velocity = new Vector3(tmp.x, agent.velocity.y, tmp.z);
					}
				}
			}
		}

		//处于跳跃的下落状态
		if(((moveState & MoveState.Jump) != MoveState.NONE || (moveState & MoveState.JumpSec) != MoveState.NONE) && yStayTime > 0.06f){
			// RaycastHit hit;
			// if(agent.SweepTest(Vector3.down, out hit, 0.1f)){ //跳跃状态结束
				// Debug.Log("hit : " + hit.collider.name);
				moveState &= ~MoveState.Jump; 
				moveState &= ~MoveState.JumpSec;
				// agent.MovePosition(hit.point);
				jumpTime = 0f;
				// agent.GetPointVelocity
				Debug.Log("jump end");
			// }
		}

		//速度很小，已经停下来了
		if((moveState & MoveState.Normal) != MoveState.NONE && Mathf.Abs(agent.velocity.x) < 0.1f && Mathf.Abs(agent.velocity.z) < 0.1f){
			moveState &= ~MoveState.Normal;
			agent.velocity = new Vector3(0f, agent.velocity.y, 0f);
		}
		if(Mathf.Abs(agent.velocity.x) > moveFactor){
			agent.velocity = new Vector3(moveDir * moveFactor, agent.velocity.y, agent.velocity.z);
		}

		//y轴速度
		if(agent.velocity.y == 0f){
			yStayTime += Time.deltaTime;
		}else{
			yStayTime = 0f;
		}

		//动作的处理部分
		if((moveState & MoveState.JumpSec) != MoveState.NONE){
			animContrller.SetInteger("aniState", (int)PlayerAnimationNames.JUMP2);
		}else if((moveState & MoveState.Jump) != MoveState.NONE){
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
		Debug.Log("trigger enter:" + other.name);
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
		Debug.Log("trigger exit:" + other.name);
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