using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseModifier {

	
	//当前正tick的时间
	private float curTickTime;

	private float intervalTime;

	private float duration;

	//堆叠数量
	private int stackCount;

	private bool isOrb;

	private bool isAura;

	private float auraRadius;

	private EntityTarget auraTarget;

	private OrbPriority orbPriority;

	private bool orbCastAttack;

	private BaseNPC caster;

	//buff所应用到的对象
	private BaseNPC parent;

	private BaseAbility ability;

	private ModifierParams modifierParams;

	private Dictionary<Modifier_State,Modifier_State_Value> states;

	private Dictionary<Modifier_Property, AbilitySpecial> properties;

	private Dictionary<ModifierEventType, List<BaseAction>> eventActions;

	public Dictionary<Modifier_Property, AbilitySpecial> Properties
	{
		get
		{
			return properties;
		}
	}

	public string Name{
		get{
			return modifierParams.name;
		}
	}

	public int Attributes
	{
		get{
			return modifierParams.attributes;
		}
	}

	public BaseNPC Parent{
		get{
			return parent;
		}
	}

	public int StackCount
	{
		set{
			stackCount = value;
		}
		get{
			return stackCount;
		}
	}

	public float Duration{
		get{
			return modifierParams.duration;
		}
	}

	public BaseNPC Caster{
		get{
			return caster;
		}
	}

	public bool IsBuff{
		get{
			return modifierParams.isBuff;
		}
	}

	public bool IsDebuff{
		get{
			return modifierParams.isDebuff;
		}
	}

	public bool IsPurgable
	{
		get{
			return modifierParams.isPurgable;
		}
	}

	public bool IsPurgeException{
		get{
			return modifierParams.isPurgeException;
		}
	}

	public bool IsStunned{
		get{
			Modifier_State_Value stateVal;
			if(states.TryGetValue(Modifier_State.MODIFIER_STATE_STUNNED, out stateVal))
			{
				return stateVal == Modifier_State_Value.MODIFIER_STATE_VALUE_ENABLED;
			}
			return false;
		}
	}

	public BaseModifier(string modifierName)
	{
		eventActions = new Dictionary<ModifierEventType, List<BaseAction>>();
		properties = new Dictionary<Modifier_Property, AbilitySpecial>();
		modifierParams = ModifierManager.GetModifier(modifierName);
		isAura = modifierParams.isAura;
		isOrb = eventActions.ContainsKey(ModifierEventType.Orb);
		if(isAura)
		{
			auraRadius = modifierParams.auraRadius;
			auraTarget = new EntityTarget(modifierParams.auraTargetTeam, modifierParams.auraTargetType, modifierParams.auraTargetFlags);
		}
		//name = modifierName;
	}

	public void ApplyOnTarget(BaseNPC target)
	{
		this.parent = target;
		this.stackCount = 1;
		if(modifierParams.isAura && modifierParams.auraApplyToCaster)
		{
			caster.AddNewModifier(modifierParams.auraModifier);
		}
		TriggerEvent(ModifierEventType.OnCreated);
		
	}


	public void Update(float deltaTime)
	{
		duration -= deltaTime;
		if(intervalTime > 0f)
		{
			curTickTime -= deltaTime;
			if(curTickTime < 0f)
			{
				curTickTime = intervalTime;
				TriggerEvent(ModifierEventType.OnIntervalThink);
			}
		}
		if(isAura) //光环效果
		{
			auraTarget.ForEachTarget(AuraTargetExe);
		}

		//持续时间结束
		if(duration < 0f && modifierParams.destroyOnExpire)
		{
			Destroy();
		}
	}

	private void AuraTargetExe(BaseEntity target)
	{
		var tar = target as BaseNPC;
		if(tar != null)
		{
			//在范围内就添加一个光环效果
			if(Vector3.Distance(caster.Position, target.Position) < auraRadius)
			{
				tar.AddNewModifier(modifierParams.auraModifier);
			}else //超过范围就移除
			{
				tar.RemoveModifier(modifierParams.auraModifier, caster);
			}
		}
		
	}

	public void TriggerEvent(ModifierEventType eType)
	{
		List<BaseAction> actions;
		if(isOrb)
		{
			switch(eType)
			{
				case ModifierEventType.OnAttack:
					eType = ModifierEventType.Orb;
					break;
				case ModifierEventType.OnAttackStart:
					eType = ModifierEventType.OnOrbFire;
					break;
				case ModifierEventType.OnAttackLanded:
					eType = ModifierEventType.OnOrbImpact;
					break;
			}
		} 
		if(eventActions.TryGetValue(eType, out actions))
		{
			int count = actions.Count;
			for(int i = 0; i < count; i++)
			{
				actions[i].Execute();
			}
		}
	}

	public void RegisterEvent(ModifierEventType eType, BaseAction action)
	{
		if(!eventActions.ContainsKey(eType))
		{
			eventActions.Add(eType, new List<BaseAction>());
		}
		if(!eventActions[eType].Contains(action))
		{
			eventActions[eType].Add(action);
		}else
		{
			CLog.Info("There is same action with the event type: ", action.ToString());
		}
	}

	public void RegisterEvent(ModifierEventType eType, List<BaseAction> actions)
	{
		if(!eventActions.ContainsKey(eType))
		{
			eventActions.Add(eType, new List<BaseAction>());
		}
		int count = actions.Count;
		for(int i = 0; i < count; i++)
		{
			if(!eventActions[eType].Contains(actions[i]))
			{
				eventActions[eType].Add(actions[i]);
			}else
			{
				CLog.Info("There is same action with the event type: ", actions[i].ToString());
			}
		}
	}

	public void ApplyModifierStates()
	{
		parent.ApplyModifierStates(states);
	}
	
	public void AddParticle(int index, bool destroyImmediately, bool statusEffect, int priority, bool heroEffect, bool overheadEffect)
	{

	}

	public void IncrementStackCount()
	{
		stackCount++;
	}

	public void DecrementStackCount()
	{
		stackCount--;
	}

	public void StartIntervalThink(float interval)
	{
		this.intervalTime = interval;
	}
	//刷新这个buff
	public void ForceRefresh()
	{
		stackCount = 1;
		duration = modifierParams.duration;
		//TriggerEvent(ModifierEventType.);
	}

	public void Destroy()
	{
		TriggerEvent(ModifierEventType.OnDestroy);
		states.Clear();
		properties.Clear();
		eventActions.Clear();
		caster = null;
		parent = null;
		ability = null;
	}
}

public struct ModifierParams{
	public string name;

	//持续时间
	public float duration;

	//是否是被动
	public int passive;

	//是否是debuff
	public bool isDebuff;

	//是否是buff
	public bool isBuff;

	//是否可以被清除
	public bool isPurgable;

	//是否可以被强力清除
	public bool isPurgeException;

	public bool isHidden;

	//是否是光环
	public bool isAura;

	public bool auraApplyToCaster;

	public string auraModifier;

	public int auraRadius;

	public string effectName;

	//间隔时间
	public float intervalTime;

	public int priority;

	public bool destroyOnExpire;

	public bool allowIllusionDuplicate;

	public EffectAttachType attachType;

	public int attributes;

	public UnitTargetTeam auraTargetTeam;

	public UnitTargetType auraTargetType;

	public UnitTargetFlags auraTargetFlags;
	
}

