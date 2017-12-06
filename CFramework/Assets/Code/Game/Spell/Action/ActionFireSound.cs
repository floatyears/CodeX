using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionFireSound : BaseAction {

	private string effectName;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		return true;
	}
}
