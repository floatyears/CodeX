using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionRemoveAbility : BaseAction {

	private string abilityName;

	protected override bool ExeOnTarget(BaseEntity tar) 
	{
		(tar as BaseNPC).RemoveAbility(abilityName);
		return true;
	}
}
