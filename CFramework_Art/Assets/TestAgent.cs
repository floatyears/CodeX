using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestAgent : MonoBehaviour {

	public float minX = -64f;

	public float maxX = 64f;

	public float minY = 4.3f;

	public float maxY = 11.6f;

	public float minZ = 0f;

	public float maxZ = 30;

	public Vector3 speed;

	private Camera cam;

	private float z;

	private Vector3 mouseDelta = Vector3.zero;

	private bool start;

	private NavMeshAgent agent;

	private MoveState moveState;

	private Transform character;

	private float jumpTime;

	public float jumpVel = 8f;

	public float gravity = 13f;

	private NavMeshPath curPath;

	private Vector3 lastPos; //上一帧的时间

	private float scrollDelta;

	// Use this for initialization
	void Start () {
		cam = Camera.main;
		mouseDelta = cam.transform.position;
		z = cam.transform.position.z;

		minX = -64f;
		maxX = 64f;
		minY = 4.3f;
		maxY = 11.6f;
		minZ = 50f;
		maxZ = 100f;
		jumpVel = 8f;
		gravity = 13f;
		speed = new Vector3(0.022f,-0.022f,2f);
		curPath = new NavMeshPath();

		agent = transform.Find("agent").gameObject.GetComponent<NavMeshAgent>();
		character = transform.Find("model");
	}
	
	// Update is called once per frame
	void Update () {
		// if(Input.GetMouseButtonDown(0)){
		// 	mouseDelta = Input.mousePosition;
		// }
		// if(Input.GetMouseButton(0)){
		// 	RaycastHit hit;
		// 	if(Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit)){
		// 		hit.collider.
		// 	}
		// }
			
		// 	if(cam != null){
		// 		var camPos = cam.transform.position;
		// 		camPos.x += temp.x * speed.x;
		// 		camPos.y += temp.y * speed.y;
		// 		if(camPos.x > maxX) camPos.x = maxX;
		// 		else if(camPos.x < minX) camPos.x = minX;
		// 		else if(camPos.y < minY) camPos.y = minY;
		// 		else if(camPos.y > maxY) camPos.y = maxY;
		// 		cam.transform.position = camPos;
		// 	}
		// }
		if(Input.GetKey(KeyCode.LeftArrow)){ //左
			// agent.SetDestination(agent.transform.position + Vector3.left * speed.z);// 
			agent.Move(Vector3.left * speed.z);
		}else if(Input.GetKey(KeyCode.UpArrow)){
			agent.Move(Vector3.back* speed.z);
			// agent.SetDestination(agent.transform.position + Vector3.back* speed.z);
		}else if(Input.GetKey(KeyCode.DownArrow)){
			agent.Move(Vector3.forward* speed.z);
			// agent.SetDestination(agent.transform.position + Vector3.forward* speed.z);
		}else if(Input.GetKey(KeyCode.RightArrow)){ //下
			agent.Move(Vector3.right* speed.z);  
			// agent.SetDestination(agent.transform.position + Vector3.right* speed.z);
		}
		else if(Input.GetKey(KeyCode.C)){
		}
		
		Vector3? dest = null;
		
		if(Input.GetKey(KeyCode.D)){ //左
			moveState |= MoveState.Normal;
			dest = agent.transform.position + Vector3.left * speed.z;// 
			// agent.Move(Vector3.left * speed.z);
		}else if(Input.GetKey(KeyCode.W)){
			moveState |= MoveState.Normal;
			// agent.Move(Vector3.back* speed.z);
			dest = agent.transform.position + Vector3.back* speed.z;
		}else if(Input.GetKey(KeyCode.S)){
			moveState |= MoveState.Normal;
			// agent.Move(Vector3.forward* speed.z);
			dest = agent.transform.position + Vector3.forward* speed.z;
		}else if(Input.GetKey(KeyCode.A)){ //下
			moveState |= MoveState.Normal;
			// agent.Move(Vector3.right* speed.z);  
			dest = agent.transform.position + Vector3.right* speed.z;
		}
		if(Input.GetKey(KeyCode.Space)){
			if(jumpTime == 0f){
				jumpTime = 0f;
			}
			moveState |= MoveState.Jump;
		}

		if(agent.isOnOffMeshLink){
			if((moveState & MoveState.Jump) != MoveState.NONE){ //开始跳跃
				moveState |= MoveState.OffMeshLink;
				Debug.Log("jump start");
				// moveState &= ~MoveState.Jump;
			}else if(dest != null){ //取消传送
				Debug.Log("jump end");
				moveState &= ~MoveState.OffMeshLink;
				agent.CompleteOffMeshLink();
				agent.ResetPath();
				agent.Warp(character.position);
				// agent.transform.position = character.position;
				// agent.ResetPath();
			}
		}else{
			moveState &= ~MoveState.OffMeshLink;
		}

		if((moveState & MoveState.Normal) != MoveState.NONE){
			if(dest != null){
				// agent.SetDestination(dest.Value);
				if(agent.CalculatePath(dest.Value, curPath)){
					agent.SetPath(curPath);
				}
			}
		}
		// NavMeshLink link;
		// link.
		
		if((moveState & MoveState.Jump) != MoveState.NONE){
			var y = jumpVel * jumpTime - gravity * jumpTime * jumpTime;
			jumpTime += Time.deltaTime;
			if(jumpTime > 0.1f && y < 0f){
				moveState &= ~MoveState.Jump;
				y = 0f;
				jumpTime = 0f;
			}
			character.GetChild(0).localPosition = new Vector3(0, y, 0);
		}

		if((moveState & MoveState.OffMeshLink) != MoveState.NONE){
			var endpos = agent.currentOffMeshLinkData.endPos;
			if((endpos - agent.transform.position).magnitude > 0.01f){
				agent.transform.position = Vector3.MoveTowards(agent.transform.position, endpos, agent.speed* Time.deltaTime);
			}else{
				agent.CompleteOffMeshLink();
				Debug.Log("jump complete");
				moveState &= ~MoveState.OffMeshLink;
			}
		}

		character.position = agent.transform.position;

		if(Input.mouseScrollDelta.y != 0f){
			scrollDelta += Input.mouseScrollDelta.y;
			z += Input.mouseScrollDelta.y;
			z = Mathf.Max(z, minZ);
			z = Mathf.Min(z, maxZ);
		}
		// scrollDelta -= 
		

		var temp = character.position;
		temp.z = z;
		
		// if(temp.x > maxX) temp.x = maxX;
		// else if(temp.x < minX) temp.x = minX;
		// else if(temp.y < minY) temp.y = minY;
		// else if(temp.y > maxY) temp.y = maxY;

		cam.transform.position = temp;
	}
}

public enum MoveState{
	NONE = 0,
	Normal = 0x1,
	Jump = 0x2,
	OffMeshLink = 0x4, //处于offmeshlink传输过程中（无法取消）
}
