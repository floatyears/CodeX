using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionRemoveModifier : BaseAction {

	private string modifierName;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		(tar as BaseNPC).RemoveModifier(modifierName);
		return true;
	}
}
