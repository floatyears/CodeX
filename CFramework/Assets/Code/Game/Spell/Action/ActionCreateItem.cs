using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCreateItem : BaseAction {

	private string itemName;

	private int itemCount;

	private int itemChargeCount;

	private float spawnRadius;

	private float launchHeight;

	private float launchDistance;

	private float launchDuration;

	protected override bool ExeOnTarget(BaseEntity target)
	{
		return true;
	}
}
