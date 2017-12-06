using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSpawnUnit : BaseAction {

	private string unitName;

	private int unitCount;

	private int unitLimit;

	private int spawnRadius;

	private int duration;

	private EntityTarget target;

	private int grantsGold;

	private int grantsXP;

	private BaseAction[] OnSpawn;

	protected override bool ExeOnTarget(BaseEntity tar) 
	{
		int count = OnSpawn.Length;
		for(int i = 0; i < count; i++)
		{
			OnSpawn[i].Execute();
		}
		return true;
	}
}
