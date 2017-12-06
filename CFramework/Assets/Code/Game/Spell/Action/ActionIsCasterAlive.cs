using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionIsCasterAlive : BaseAction {

	private BaseAction onSuccess;

	private BaseAction onFailure;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		if(tar.IsAlive())
		{
			onSuccess.Execute();
		}
		else
		{
			onFailure.Execute();
		}
		return true;
	}
}
