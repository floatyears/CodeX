using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionRotate : BaseAction {

	private Vector3 pitchYawRoll;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		tar.Rotate(pitchYawRoll);
		return true;
	}
}
