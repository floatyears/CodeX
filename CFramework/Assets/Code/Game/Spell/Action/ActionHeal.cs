using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionHeal : BaseAction {

	private int healAmount;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		var tmp = tar as BaseNPC;
		tmp.TakeHeal(healAmount);
		tmp.TriggerModifierEvent(ModifierEventType.OnHealthGained); //外部方式获得的血量
		tmp.TriggerModifierEvent(ModifierEventType.OnHealthReceived);
		
		return true;
	}

}
