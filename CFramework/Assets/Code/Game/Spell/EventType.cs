using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityEventType {
	OnSpellStart = 1,

	OnToggleOn, //切换技能开启

	OnToggleOff, //切换技能关闭

	OnChannelFinish,//持续施法完成

	OnChannelInterrupted,//持续施法中断，中断的同时会触发OnChannelFinish

	OnChannelSucceeded,//持续施法成功，等同于OnChannelFinish，但是中断的同时不会触发

	OnOwnerSpawned, //技能持有者重生或者出生(创建）

	OnProjectileHitUnit,

	OnProjectileFinish,

	OnEquip,

	OnUnequip,

	OnUpgrade,

	OnAbilityPhaseStart, //这表示了一段在真正释放技能（OnSpellStart）和单位被告知释放技能的时间间隔
	
}

public enum ModifierEventType
{
	OnCreated,

	OnDestroy,

	OnIntervalThink, //ThinkInterval间隔触发一次

	OnAttack, //带有此modifier的unit完成了一次attack

	OnAttacked, //带有此modifier的unit被攻击了，在攻击的结束触发

	OnAttackStart,

	OnAttackLanded,

	OnAttackFailed,

	OnDealDamage,

	OnTakeDamage,

	OnDeath,

	OnKill,

	Orb, //法球效果，每次攻击就会触发（如果带有法球效果）

	OnOrbFire, //法球的OnAttackStart事件

	OnOrbImpact, //法球的OnAttackLanded事件

	OnAbilityExecuted, //带有这个modifier的unit释放了任何ability（包括item）

	OnAbilityStart, //带有这个modifier的unit释放一个ability，和OnSpellStart一样

	OnAbilityEndChannel,//任意持续性技能停止施法

	OnHealthReceived, //任何方式获得生命值，甚至是在满血的时候

	OnHealthGained, //通过外部方式获得生命值

	OnManaGained,

	OnSpentMana, //消耗法力值时触发

	OnOrder,  //发送命令的时候触发，例如移动、施法、停止、

	OnUnitMoved,

	OnProjectileDodge,  //躲闪的时候触发

	OnStateChanged, //
}


