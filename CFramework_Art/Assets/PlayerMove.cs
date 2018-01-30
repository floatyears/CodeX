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

	private int moveDir; //0-左，1-右

	private NavMeshAgent agent;

	private MoveState moveState;

	private Transform character;

	private float jumpTime;

	private float teleportTime;

	public float jumpVel = 8f;

	public float gravity = 13f;

	private NavMeshPath curPath;

	private Vector3 teleportPos;

	private List<TriggerTeleport> teleports;

	private TriggerTeleport lastTrigger; //最后一个触发的触发器

	private Transform interTrans;

	private float scrollDelta;

	// Use this for initialization
	void Start () {
		cam = Camera.main;
		mouseDelta = cam.transform.position;
		z = cam.transform.position.z;

		jumpVel = 8f;
		gravity = 13f;
		speed = new Vector3(0.022f,-0.022f,2f);
		curPath = new NavMeshPath();

		teleports = new List<TriggerTeleport>(5);
		agent = gameObject.GetComponent<NavMeshAgent>();
		character = transform;
		interTrans = character.GetChild(0);
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
		
		if(Input.GetKey(KeyCode.D)){ //右
			moveDir = 1; //右边
			interTrans.eulerAngles = new Vector3(0,0,0);
			int idx = -1;
			if((idx = CheckTrigger(false, TeleportType.DownRight)) >= 0){ //传送到其他地方
				moveState |= MoveState.Teleport;
				moveState &= ~MoveState.Normal;
				agent.updatePosition = false;
				lastTrigger = teleports[idx];
				teleportPos = lastTrigger.worldDestPos;
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
			interTrans.eulerAngles = new Vector3(0,180,0);
			moveDir = 0;
			int idx = -1;
			if((idx = CheckTrigger(false, TeleportType.DownLeft)) >= 0){ //传送到其他地方
				moveState |= MoveState.Teleport;
				moveState &= ~MoveState.Normal;
				agent.updatePosition = false;
				lastTrigger = teleports[idx];
				teleportPos = lastTrigger.worldDestPos;
			}else{
				moveState |= MoveState.Normal;
				dest = agent.transform.position + Vector3.right* speed.z;
			}
			
		}
		if(Input.GetKey(KeyCode.Space)){ //跳跃
			if(jumpTime == 0f){
				jumpTime = 0f;
			}
			moveState |= MoveState.Jump;

			int idx = -1;
			if((idx = CheckTrigger(false, moveDir == 1 ? TeleportType.JumpRight : TeleportType.JumpLeft)) >= 0){ //传送到其他地方
				moveState |= MoveState.Teleport;
				moveState &= ~MoveState.Normal;
				agent.updatePosition = false;
				lastTrigger = teleports[idx];
				teleportPos = lastTrigger.worldDestPos;
			}
		}

		if((moveState & MoveState.Normal) != MoveState.NONE){
			if(dest != null){
				agent.SetDestination(dest.Value);
			}
		}
		if((moveState & MoveState.Teleport) != MoveState.NONE){
			if(transform.position != teleportPos){
				teleportTime += Time.deltaTime;
				float speed = 0f;
				if(lastTrigger.type == TeleportType.HorizenLeft || lastTrigger.type == TeleportType.HorizenRight){ //减速
					speed = (10 - agent.speed * teleportTime) * Time.deltaTime;
				}else if(lastTrigger.type == TeleportType.UpLeft || lastTrigger.type == TeleportType.UpRight){
					speed = agent.speed * Time.deltaTime;
				}else if(lastTrigger.type == TeleportType.DownLeft || lastTrigger.type == TeleportType.DownRight){//加速度
					speed = agent.speed * teleportTime * Time.deltaTime;
				}
				transform.position = Vector3.MoveTowards(transform.position, teleportPos, speed);
			}else{
				agent.Warp(transform.position);
				agent.updatePosition = true;
				moveState |= MoveState.Normal;
				moveState &= ~MoveState.Teleport;
				teleportTime = 0f;
				lastTrigger = null;
			}
		}
		
		if((moveState & MoveState.Jump) != MoveState.NONE){
			var y = jumpVel * jumpTime - gravity * jumpTime * jumpTime;
			jumpTime += Time.deltaTime;
			if(jumpTime > 0.1f && y < 0f){
				moveState &= ~MoveState.Jump;
				y = 0f;
				jumpTime = 0f;
			}
			interTrans.localPosition = new Vector3(0, y, 0);
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
}

public enum MoveState{
	NONE = 0,
	Normal = 0x1,
	Jump = 0x2,
	Teleport = 0x4, //处于offmeshlink传输过程中（无法取消）
}
