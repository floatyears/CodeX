using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLifesteal : BaseAction {

	private int lifestealPercent;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		int val = tar.GetIntVal(EntityAttributeType.MaxHealth) * lifestealPercent;
		tar.TakeDamage(DamageType.DAMAGE_TYPE_MAGICAL, val);
		return true;
	}


}
