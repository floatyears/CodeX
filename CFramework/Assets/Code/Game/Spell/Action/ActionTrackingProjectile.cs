using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTrackingProjectile : BaseAction {

	private string effectName;

	private int dodgeable;

	private int provideVision;

	private int visionRadius;

	private int moveSpeed;

	private int sourceAttachment;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		return true;
	}
}
