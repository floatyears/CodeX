﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLinearProjectile : BaseAction {

	
	private string effectName;

	private int moveSpeed;

	private int startRadius;

	private int endRadius;

	private int fixedDistance;

	private Vector3 startPosition;

	private UnitTargetTeam targetTeams;

	private UnitTargetFlags targetFlags;

	private UnitTargetType targetType;

	private int hasFrontalCone;

	private int providesVision;

	private int visionRadius;


	protected override bool ExeOnTarget(BaseEntity tar)
	{
		return true;
	}

}
