using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSpendMana : BaseAction {

	private int mana;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		return true;
	}
}
