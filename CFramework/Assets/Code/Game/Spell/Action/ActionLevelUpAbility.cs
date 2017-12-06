using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLevelUpAbility : BaseAction {

	private string abilityName;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		return true;
	}


}
