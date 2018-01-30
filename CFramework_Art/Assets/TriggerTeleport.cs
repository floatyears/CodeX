using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TriggerTeleport : MonoBehaviour {


	public TeleportType type = TeleportType.HorizenRight;

	public bool IsAuto = false; //是否自动触发

	public BoxCollider trigger;

	public Vector3 destPos;

	public Vector3 worldDestPos;

	public Vector3 startPos;

	// public BoxCollider curCollider;

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Start()
	{
		if(trigger == null) trigger = GetComponent<BoxCollider>();
		startPos = transform.localToWorldMatrix.MultiplyPoint(trigger.center);
		worldDestPos = transform.localToWorldMatrix.MultiplyPoint(destPos);
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.black;
		Gizmos.DrawWireSphere(transform.localToWorldMatrix.MultiplyPoint(destPos),0.2f);
	}
}

public enum TeleportType{
	JumpLeft = TeleportType.Up | TeleportType.Down | TeleportType.UpLeft | TeleportType.DownLeft | TeleportType.HorizenLeft,

	JumpRight = TeleportType.Up | TeleportType.Down | TeleportType.UpRight | TeleportType.DownRight | TeleportType.HorizenRight ,

	None = 0,

	Down = 0x1,
	
	DownLeft = 0x2,

	DownRight = 0x4,

	Up = 0x8,

	UpLeft = 0x10,

	UpRight = 0x20,

	HorizenLeft = 0x40,

	HorizenRight = 0x80,
}
