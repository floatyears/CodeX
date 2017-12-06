using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BaseNPC : BaseEntity {

	//------攻击相关属性-------//
	protected int attackCabilities;

	protected int attackDamageMin;

	protected int attackDamageMax;

	protected DamageType attackDamageType;

	//攻击速度
	protected float attackRate;

	protected float attackAnimationPoint;

	//目标可以被捕获到的范围
	protected int attackAquisitionRange;

	//目标可以被攻击的范围
	protected int attackRange;

	//目标可以移动但是不取消攻击的额外的范围
	protected int attackRangeBuffer;

	protected string projectileModel;

	//飞弹的速度
	protected int projectileSpeed;

	//--------属性相关---------//
	protected MainAttribute attributePrimary;

	//protected int attributeBaseStrength; -->attribute.baseStrength;

	//protected int attributeStrengthGain; -->attribute.strengthGain;

	//protected int attributeBaseAgility; -->attribute.baseAgility;

	//protected int attributeAgilityGain; -->attribute.agilityGain;

	//protected int attributeBaseIntelligence; -->attribute.baseintellect;

	//protected int attributeIntelligence; -->attribute.intellectGain;

	//----------属性--------//

	//protected int armorPhysical; -->attribute.phyArmor;

	//protected int magicResistance; -->attribute.magicResistance;

	//---------移动相关---------//
	protected UnitMoveCapability movementCapabilities;

	protected int movementSpeed;

	protected int movementTurnRate;

	//---------生命值和魔法---------//
	protected int statusHealth;

	protected int statusHealthRegen;

	protected int statusMana;

	protected int statusManaRegen;

	//--------单位类型---------//
	protected bool isAncient;

	protected bool isNeutralUnitType;

	
	protected HullSizeType hullSizeType;

	protected int hullRadius;


	//
	protected float timer;

	protected Dictionary<Modifier_State,Modifier_State_Value> modifierStates;

	protected Dictionary<Modifier_Property, AbilitySpecial> modifierProperties;

	protected List<BaseAbility> abilities;

	protected List<BaseModifier> modifiers;

	protected Stack<EntityCommand> commandStack;

	public List<BaseModifier> Modifiers
	{
		get{
			return modifiers;
		}
	}

	public List<BaseAbility> Abilities
	{
		get{
			return abilities;
		}
	}

	public override void Init()
	{
		abilities = new List<BaseAbility>();
		modifierProperties = new Dictionary<Modifier_Property, AbilitySpecial>();
	}

	public override void Update(float deltaTime)
	{
		int count = abilities.Count;
		for(int i = 0; i < count; i++)
		{
			abilities[i].Update(deltaTime);
		}

		count = modifiers.Count;
		for(int i = 0; i< count; i++)
		{
			modifiers[i].Update(deltaTime);
		}

		this.timer += deltaTime;
		if(this.timer > 5f) //5秒回复
		{
			this.timer = 0f;
			attribute.health += attribute.health;
			attribute.mana += attribute.manaRegen;

			if(attribute.health > attribute.maxHealth) attribute.health = attribute.maxHealth;
			if(attribute.mana > attribute.maxMana) attribute.mana = attribute.maxMana;
		}
	}

	//--------游戏操作逻辑---------//
	public void CastSpell()
	{
		
	}

	public void MoveTo(Vector3 pos)
	{

	}

	public void MoveToNPC(BaseNPC npc)
	{

	}

	public void MoveToTargetToAttack(BaseNPC target)
	{

	}


	//打断当前正在执行的任何命令
	public void Interrupt()
	{

	}

	//停止当前的命令
	public void Stop()
	{

	}

	//保持当前的位置
	public void Hold()
	{

	}

	public void RespawnUnit()
	{
		
	}

	//proc-格挡攻击
	public void PerformAttack(BaseNPC target, bool useCastAttackOrb, bool prcoessProcs, bool skipCooldown, bool ignoreInvis = false, bool useProjectile = false, bool fakeAttack = false, bool neverMiss = false)
	{
		if(!fakeAttack)
		{
			TriggerModifierEvent(ModifierEventType.OnAttack);
		}
	}

	public void Purge(bool removePositiveBuffs, bool removeDebuffs, bool buffsCreatedThisFrameOnly, bool removeStuns, bool removeExceptions)
	{

	}

	//---------技能系统逻辑----------//
	public void TriggerAbilityEvent(AbilityEventType eType)
	{
		int count = abilities.Count;
		for(int i = 0; i < count; i++)
		{
			abilities[i].TriggerEvent(eType);
		}
	}

	public void TriggerModifierEvent(ModifierEventType eType)
	{
		int count = modifiers.Count;
		for(int i = 0; i < count; i++)
		{
			modifiers[i].TriggerEvent(eType);
		}
	}

	

	public void AddNewModifier(string modifierName)
	{
		var modifier = ModifierManager.GetModifier(modifierName);
		//免疫的状态下要判断是否能加上buff
		if(!GetModifierState(Modifier_State.MODIFIER_STATE_INVULNERABLE) || (modifier.attributes & ModifierAttribute.MODIFIER_ATTRIBUTE_IGNORE_INVULNERABLE) > 0)
		{
			//判断buff是否能叠加
			if((modifier.attributes & ModifierAttribute.MODIFIER_ATTRIBUTE_MULTIPLE) > 0)
			{
				modifiers.Add(new BaseModifier(modifierName));
			}else //不能叠加，就刷新buff
			{
				bool hasModifier = false;
				int count = modifiers.Count;
				for(int i = 0; i < count; i++)
				{
					if(modifiers[i].Name == modifier.name)
					{
						modifiers[i].ForceRefresh();
						hasModifier = true;
						break;
					}
				}
				if(!hasModifier)
				{
					modifiers.Add(new BaseModifier(modifierName));
				}
			}
		}
		
		//TriggerModifierEvent(ModifierEventType)
	}

	public void RemoveModifier(string modifierName)
	{
		int count = modifiers.Count;
		for(int i = count - 1; i > -1; i--)
		{
			if(modifiers[i].Name == modifierName)
			{
				modifiers.RemoveAt(i);
			}
		}
	}

	public void RemoveModifier(string modifierName, BaseNPC caster)
	{
		int count = modifiers.Count;
		for(int i = count - 1; i > -1; i--)
		{
			if(modifiers[i].Name == modifierName && modifiers[i].Caster == caster)
			{
				modifiers.RemoveAt(i);
			}
		}
	}
	

	public void AddAbility(BaseAbility ability)
	{
		abilities.Add(ability);
	}

	public void AddAbility(string name)
	{
		abilities.Add(AbilityParser.Parse(name));
	}

	public void RemoveAbility(string name)
	{
		int count = abilities.Count;
		for(int i = 0; i < count; i++)
		{
			if(abilities[i].Name == name)
			{
				abilities.RemoveAt(i);
				return;
			}
		}
	}

	public BaseAbility GetAbilityByIndex(int index)
	{
		int count = abilities.Count;
		if(index < count)
		{
			return abilities[index];
		}else{
			CLog.Error("Ability Index is out of range!");
			return null;
		}
	}

	public override void TakeDamage(DamageType damageType, int value)
	{
		attribute.TakeDamage(damageType, value);
		if(!IsAlive())
		{
			TriggerModifierEvent(ModifierEventType.OnDeath);
		}else
		{
			TriggerModifierEvent(ModifierEventType.OnAttacked);
		}
	}


	//-----------Modifier 状态相关----------//
	public void ApplyModifierStates(Dictionary<Modifier_State,Modifier_State_Value> states)
	{
		var iterator = modifierStates.GetEnumerator();
		while(iterator.MoveNext())
		{
			Modifier_State_Value value;
			if(this.modifierStates.TryGetValue(iterator.Current.Key, out value))
			{
				this.modifierStates[iterator.Current.Key] = value;
			}else
			{
				this.modifierStates.Add(iterator.Current.Key, value);
			}
		}
	}

	public bool IsStunned()
	{
		Modifier_State_Value stateVal;
		if(modifierStates.TryGetValue(Modifier_State.MODIFIER_STATE_STUNNED, out stateVal))
		{
			return stateVal == Modifier_State_Value.MODIFIER_STATE_VALUE_ENABLED;
		}
		return false;
	}

	public bool IsSilenced()
	{
		Modifier_State_Value stateVal;
		if(modifierStates.TryGetValue(Modifier_State.MODIFIER_STATE_SILENCED, out stateVal))
		{
			return stateVal == Modifier_State_Value.MODIFIER_STATE_VALUE_ENABLED;
		}
		return false;
	}

	public bool IsMuted()
	{
		Modifier_State_Value stateVal;
		if(modifierStates.TryGetValue(Modifier_State.MODIFIER_STATE_MUTED, out stateVal))
		{
			return stateVal == Modifier_State_Value.MODIFIER_STATE_VALUE_ENABLED;
		}
		return false;
	}

	public bool GetModifierState(Modifier_State state)
	{
		Modifier_State_Value stateVal;
		if(modifierStates.TryGetValue(state, out stateVal))
		{
			return stateVal == Modifier_State_Value.MODIFIER_STATE_VALUE_ENABLED;
		}
		return false;
	}

}
