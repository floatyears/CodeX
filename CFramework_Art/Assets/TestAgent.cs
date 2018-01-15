using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAgent : MonoBehaviour {

	public float minX = -64f;

	public float maxX = 64f;

	public float minY = 4.3f;

	public float maxY = 11.6f;

	public Vector3 speed;

	private Camera cam;

	private float z;

	private Vector3 mouseDelta = Vector3.zero;

	private bool start;

	// Use this for initialization
	void Start () {
		cam = Camera.main;
		mouseDelta = cam.transform.position;
		z = cam.transform.position.z;

		minX = -64f;
		maxX = 64f;
		minY = 4.3f;
		maxY = 11.6f;
		speed = new Vector3(0.022f,-0.022f,0f);

	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)){
			mouseDelta = Input.mousePosition;
		}
		if(Input.GetMouseButton(0)){
			var temp = mouseDelta;
			mouseDelta = Input.mousePosition;
			temp = mouseDelta - temp;
			temp.z = 0;
			
			if(cam != null){
				var camPos = cam.transform.position;
				camPos.x += temp.x * speed.x;
				camPos.y += temp.y * speed.y;
				if(camPos.x > maxX) camPos.x = maxX;
				else if(camPos.x < minX) camPos.x = minX;
				else if(camPos.y < minY) camPos.y = minY;
				else if(camPos.y > maxY) camPos.y = maxY;
				cam.transform.position = camPos;
			}
		}
	}
}
