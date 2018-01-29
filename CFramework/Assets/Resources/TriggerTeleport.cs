using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTeleport : MonoBehaviour {

	private bool isInTigger = false;

	[SerializeField]
	public BoxCollider start;

	[SerializeField]
	public BoxCollider end;

	public Vector3 startPos;

	public Vector3 endPos;

	public BoxCollider curCollider;

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Start()
	{
		startPos = transform.localToWorldMatrix.MultiplyPoint(start.center);
		endPos = transform.localToWorldMatrix.MultiplyPoint(end.center);
	}
}
