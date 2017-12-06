using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionStun : BaseAction {

	private int duration;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		return true;
	}
}
