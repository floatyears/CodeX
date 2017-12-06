using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCreateBonusAttack : BaseAction {

	
	protected override bool ExeOnTarget(BaseEntity tar)
    {
        tar.AttackTarget();
        return true;
    }
}
