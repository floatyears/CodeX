using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionApplyModifier : BaseAction {

	private string modifierName;

	private float duration;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		(tar as BaseNPC).AddNewModifier(modifierName);
		return true;
	}

	

}
