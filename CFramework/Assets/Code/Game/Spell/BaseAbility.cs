using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAbility : BaseEntity {

	
	protected float m_castPoint = -2f;

	protected BaseNPC caster;

	protected bool isChannel;

	//是否是开关技能
	protected bool isToggle;

	protected bool isAOE;

	protected float m_channelTime;

	protected float m_channelInterval;

	protected float coolDown;

	protected float duration;

	protected AbilityParams abilityParams;

	protected Dictionary<string, AbilitySpecial> abilitySpecials;

	protected Dictionary<AbilityEventType, List<BaseAction>> eventActions;

	protected AbilityEvent.OnUpgrade OnUpgrade;

	protected AbilityEvent.OnAbilityPhaseInterrupted OnAbilityPhaseInterrupted;

	public AbilityEvent.OnAbilityPhaseStart OnAbilityPhaseStart;

	public AbilityEvent.OnAbilityPinged OnAbilityPinged;

	public AbilityEvent.OnChannelFinish OnChannelFinish;

	public AbilityEvent.OnChannelThink OnChannelThink;

	public AbilityEvent.OnHeroCalculateStatBonus OnHeroCalculateStatBonus;

	public AbilityEvent.OnHeroLevelUp OnHeroLevelUp;

	public AbilityEvent.OnOwnerDied OnOwnerDied;

	public AbilityEvent.OnOwnerSpawned OnOwnerSpawned;

	public AbilityEvent.OnSpellStart OnSpellStart;

	public AbilityEvent.OnToggle OnToggle;

	public override void Init()
	{
		base.Init();
		abilitySpecials = new Dictionary<string, AbilitySpecial>();
		eventActions = new Dictionary<AbilityEventType, List<BaseAction>>();
		
		isChannel = (abilityParams.behavior & AbilityBehavior.ABILITY_BEHAVIOR_CHANNELLED) > 0;
		isToggle = (abilityParams.behavior & AbilityBehavior.ABILITY_BEHAVIOR_TOGGLE) > 0;
		isAOE = (abilityParams.behavior & AbilityBehavior.ABILITY_BEHAVIOR_AOE) > 0;
	}

	public override void Update(float deltaTime)
	{
		if(m_castPoint < -1f) //达到了castPoint之后
		{
			if(isChannel)
			{
				if(m_channelTime < -1f)
				{
					
				}
				else if(m_channelTime < 0f)
				{
					m_channelTime = -2f;
					caster.TriggerAbilityEvent(AbilityEventType.OnChannelFinish);
					caster.TriggerAbilityEvent(AbilityEventType.OnChannelSucceeded);

					//引导技能完毕之后
					EndChannel();
				}
				else //引导中
				{
					m_channelTime -= deltaTime;
					m_channelInterval -= deltaTime;
					if(m_channelInterval < 0f)
					{
						m_channelInterval = abilityParams.channelThink;
						OnChannelThink();
					}
				}
			}
			
			
		}else if(m_castPoint < 0f) //开始释放技能之后，m_castPoint之前
		{
			m_castPoint = -2f;
			caster.TriggerAbilityEvent(AbilityEventType.OnSpellStart);
			caster.TriggerModifierEvent(ModifierEventType.OnAbilityStart);
			if(isChannel)
			{
				m_channelTime = abilityParams.channelTime;
			}
		}else
		{
			m_castPoint -= deltaTime;
		}

		if(duration < -1f) //没有在释放技能的过程中
		{
			if(coolDown < -1f) //没有在冷却中
			{

			}else if(coolDown < 0f) //刚刚冷却完成
			{
				coolDown = -2f;
			}
			//开始冷却
			else
			{
				coolDown -= deltaTime;
			}
		}else if(duration < 0f) //技能刚刚释放完毕
		{
			duration = -2f;
			coolDown = abilityParams.coolDown;
		}
		//开始冷却
		else
		{
			duration -= deltaTime;
		}
		
	}

	public virtual void CastAbility()
	{
		if(CheckCastCondition())
		{
			m_castPoint = abilityParams.castPoint;
			duration = abilityParams.duration;
			caster.TriggerAbilityEvent(AbilityEventType.OnAbilityPhaseStart);
			
		}
	}

	public void StopAbility()
	{
		if(m_castPoint > 0f)
		{
			m_castPoint = -2f;
		}
		duration = -2f;
		coolDown = abilityParams.coolDown;
	}

	protected virtual bool CheckCastCondition()
	{
		if(coolDown > 0f || duration > 0f)
		{
			return false;
		}
		return false;
	}

	public float GetCoolDown()
	{
		return abilityParams.coolDown;
	}

	public AbilityType GetAbilityType()
	{
		return abilityParams.abilityType;
	}

	public int GetAbilityBehavior()
	{
		return abilityParams.behavior;
	}

	public string GetAbilityName()
	{
		return name;
	}
	

	public DamageType GetAbilityDamage()
	{
		return abilityParams.damageType;
	}

	public DamageType GetAbilityDamageType()
	{
		return abilityParams.damageType;
	}

	public UnitTargetTeam GetAbilityTargetTeam()
	{
		return target.TargetTeam;
	}

	public UnitTargetType GetAbilityTargetType()
	{
		return target.TargetType;
	}

	public UnitTargetFlags GetAbilityTargetFlags()
	{
		return target.TargetFlags;
	}

	public bool IsChanneling()
	{
		return m_channelTime > 0f;
	}

	public bool IsCooldownReady()
	{
		return coolDown < -1f;
	}

	public bool IsPassive()
	{
		return (abilityParams.behavior & AbilityBehavior.ABILITY_BEHAVIOR_PASSIVE) > 0f;
	}

	public bool IsToggle()
	{
		return isToggle;
	}

	public T GetAbilitySpecialValueFor<T>(string name, int level)
	{
		AbilitySpecial special;
		if(abilitySpecials.TryGetValue(name, out special))
		{
			return special.GetVal<T>(level);
		}
		return default(T);
	}

	public void TriggerEvent(AbilityEventType eType)
	{
		List<BaseAction> actions;
		if(eventActions.TryGetValue(eType, out actions))
		{
			switch(eType)
			{
				case AbilityEventType.OnAbilityPhaseStart:
					OnAbilityPhaseStart();
					break;
				case AbilityEventType.OnUpgrade:
					OnUpgrade();
					break;
			}
			int count = actions.Count;
			for(int i = 0; i < count; i++)
			{
				actions[i].Execute();
			}
		}
	}

	public void RegisterEvent(AbilityEventType eType, BaseAction action)
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

	public void RegisterEvent(AbilityEventType eType, List<BaseAction> actions)
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

	public void ToggleAbility()
	{
		if(isToggle)
		{
			abilityParams.toggleState = abilityParams.toggleState == 0 ? 1 : 0;
			if(abilityParams.toggleState == 1)
			{
				TriggerEvent(AbilityEventType.OnToggleOn);
			}else
			{
				TriggerEvent(AbilityEventType.OnToggleOff);
			}
		}
	}

	public void StartCoolDown(float cooldDown = -1f)
	{
		if(cooldDown > 0f)
		{
			abilityParams.coolDown = cooldDown;
		}
		this.coolDown = cooldDown;
		this.duration = -2f;
	}

	public void EndCoolDown()
	{
		coolDown = -2f;
	}

	public void EndChannel(bool interrupted = false)
	{
		m_channelTime = -2f;
		StartCoolDown();
	}

	public void UpgrageAbility(bool supressSpeech)
	{
		TriggerEvent(AbilityEventType.OnUpgrade);
	}

}


public class AbilityEvent{

	public delegate void OnUpgrade();

	public delegate void OnAbilityPhaseInterrupted();

	public delegate void OnAbilityPhaseStart();

	public delegate void OnAbilityPinged();

	public delegate void OnChannelFinish();

	public delegate void OnChannelThink();

	public delegate void OnHeroCalculateStatBonus();

	public delegate void OnHeroLevelUp();

	public delegate void OnOwnerDied();

	public delegate void OnOwnerSpawned();

	public delegate void OnSpellStart();

	public delegate void OnToggle();
}
