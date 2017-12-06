using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCreateThinker : BaseAction {

	private string modifierName;

	protected override bool ExeOnTarget(BaseEntity target)
	{
		//BaseModifier modifier = ModifierManager.GetModifier(modifierName);
		(target as BaseNPC).AddNewModifier(modifierName);
		return true;
	}
}
