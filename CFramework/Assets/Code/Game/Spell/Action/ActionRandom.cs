using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionRandom : BaseAction {

	private int chance;

	private int pseudoRandom;

	private BaseAction[] OnSuccess;

	private BaseAction[] OnFailure;

	protected override bool ExeOnTarget(BaseEntity tar) 
	{
		int random = UnityEngine.Random.Range(0,100);
		if(random >= chance)
		{
			int count = OnSuccess.Length;
			for(int i = 0; i < count; i++)
			{
				OnSuccess[i].Execute();
			}
		}else{
			int count = OnFailure.Length;
			for(int i = 0; i < count; i++)
			{
				OnFailure[i].Execute();
			}
		}
		return true;
	}
}
