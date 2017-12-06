using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAction {

	protected ActionType type;
	
	protected BaseEntity owner;

	protected EntityTarget target;
	
	public virtual void Init()
	{
		//target = new EntityTarget();
	}

	public void SetTarget(EntityTarget target)
	{
		this.target = target;
	}

	public bool Execute()
	{
		var tar = target.GetTarget();
		int count = tar.Length;
		for(int i = 0; i < count; i++)
		{
			ExeOnTarget(tar[i]);
		}
		return false;
	}

	protected virtual bool ExeOnTarget(BaseEntity tar)
	{
		return false;
	}
	
}

public enum ActionType
{
	None = 0,
	AddAbility,

	RemoveAbility,

	ActOnTargets,

	ApplyModifier,

	RemoveModifier,

	Blink,

	CleaveAttack,

	CreateThinker,

	CreateThinkerWall,

	CreateItem,

	Damage,

	DelayedAction,

	DestroyTrees,

	AttachEffect,

	FireEffect,

	FireSound,

	Heal,

	Knockback,

	LevelUpAbility,

	Lifesteal,

	LinearProjectile,

	TrackingProjectile,

	Random,

	RunScript,

	SpawnUnit,

	MoveUnit,

	RemoveUnit,

	Stun,

	ApplyMotionController,

	CreateBonusAttack,

	IsCastAlive,

	SpendMana,

}
