using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDelayedAction : BaseAction {

	private float delay;

	private BaseAction action;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		CTimer.Instance.AddTimer(()=>{
			action.Execute();
		}, delay);
		return true;
	}

}
