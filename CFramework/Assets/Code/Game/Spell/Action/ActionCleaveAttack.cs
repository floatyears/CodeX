using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCleaveAttack : BaseAction {

	private int CleavePercent;

	private int CleaveRadius;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		return false;
	}
}
