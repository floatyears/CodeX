using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionMoveUnit : BaseAction {

	private BaseEntity moveToTarget;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		if(tar is BaseNPC)
		{
			(tar as BaseNPC).MoveTo(moveToTarget.Position);
		}
		return true;
	}


}
