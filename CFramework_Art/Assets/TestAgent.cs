﻿using System.Collections;
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
		speed = new Vector3(0.022f,-0.022f,0.1f);

		agent = GetComponent<NavMeshAgent>();
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

		if(Input.GetKey(KeyCode.D)){ //左
			agent.SetDestination(agent.transform.position + Vector3.left * speed.z);// .Move(Vector3.left * speed.z);
		}else if(Input.GetKey(KeyCode.W)){
			// agent.Move(Vector3.back* speed.wz);
			agent.SetDestination(agent.transform.position + Vector3.back* speed.z);
		}else if(Input.GetKey(KeyCode.S)){
			// agent.Move(Vector3.forward* speed.z);
			agent.SetDestination(agent.transform.position + Vector3.forward* speed.z);
		}else if(Input.GetKey(KeyCode.A)){ //下
			// agent.Move(Vector3.right* speed.z);
			agent.SetDestination(agent.transform.position + Vector3.right* speed.z);
		}else if(Input.GetKey(KeyCode.Space)){ //跳跃
			agent.SetDestination(new Vector3(38f, 4.6f, 0f));
			if(agent.isOnOffMeshLink){
				var endpos = agent.currentOffMeshLinkData.endPos;
				if(endpos != agent.transform.position){
					agent.transform.position = Vector3.MoveTowards(agent.transform.position, endpos, agent.speed* Time.deltaTime);
				}
			}
		}

		if(Input.mouseScrollDelta.y != 0f){
			scrollDelta += Input.mouseScrollDelta.y;
			z += Input.mouseScrollDelta.y;
			z = Mathf.Max(z, minZ);
			z = Mathf.Min(z, maxZ);
		}
		// scrollDelta -= 
		

		var temp = transform.position;
		temp.z = z;
		
		// if(temp.x > maxX) temp.x = maxX;
		// else if(temp.x < minX) temp.x = minX;
		// else if(temp.y < minY) temp.y = minY;
		// else if(temp.y > maxY) temp.y = maxY;

		cam.transform.position = temp;
	}
}