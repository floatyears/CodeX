using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAddAbility : BaseAction {

	private string abilityName;

	protected override bool ExeOnTarget(BaseEntity target)
	{
		(target as BaseNPC).AddAbility(AbilityParser.Parse(abilityName));
		return true;
	}
}
