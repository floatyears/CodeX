using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionKnockback : BaseAction {

	private Vector3 center;

	private float duration;

	private int distance;

	private int height;

	private int isFixedDistance;

	private int shouldStun;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		return true;
	}


}
