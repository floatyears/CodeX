using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDamage : BaseAction {

	private DamageType damageType;

	private int minDamage;

	private int maxDamage;

	private int damage;

	private float curHealthPercentBasedDamage;

	private float maxHealthPercentBasedDamage;

	protected override bool ExeOnTarget(BaseEntity target)
	{
		target.TakeDamage(damageType, damage);
		return true;
	}
}
