using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionActOnTargets : BaseAction {

	private BaseAction[] actions;

	private EntityTarget target;

	protected override bool ExeOnTarget(BaseEntity tar) 
	{
		int count = actions.Length;
		for(int j = 0; j < count; j++)
		{
			actions[j].SetTarget(target);
			actions[j].Execute();
		}
			
		return true;
	}	
	
}
