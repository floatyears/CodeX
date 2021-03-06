﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EntityAttribute {

	//主属性，不同的主属性会带来生命值或者智力的增长
	public MainAttribute mainAttr;

	//对控制状态的防御
	public float statusResistance;

	//生命值
	public int health;

	//最大生命值
	public int maxHealth;

	//生命回复
	public float healthRegen; 

	public int mana;

	public int maxMana;

	//魔法回复
	public int manaRegen;

	//攻击伤害
	public int attackDamage;

	//伤害的随机部分
	public int attackDamageRadom;

	//物理防御
	public float physicArmor;

	//魔法防御
	public float magicResistance;

	//物理格挡
	public int damageBlock;

	//格挡概率
	public int blockRate;

	//暴击率
	public int crit;
	
	//攻击范围
	public float attackSpeed;

	//释放范围
	public int castRange;

	//攻击范围
	public int attackRange;

	//移动速度
	public int moveSpeed;

	//最小速度
	public int minMoveSpeed;

	//未命中概率（攻击者计算）
	public int missRate;

	//躲闪（被攻击者计算）
	public int evasion;

	//攻击免疫
	public int avoidDamage;

	//技能伤害
	public float spellDamage;
	

	//每升一级获得的力量
	public int strengthGain; 

	//力量
	public int strength;

	//每升一级获得的敏捷
	public int agilityGain;
	//敏捷
	public int agility;

	//智力
	public int intellect;

	//每升一级获得的智力
	public int intellectGain;

	//复活时间
	public int reincarnation;


	public int GetInt(string name)
	{
		return 0;
	}

	public object GetValue(EntityAttributeType type)
	{
		switch(type)
		{
			case EntityAttributeType.Health:
				return health;
			case EntityAttributeType.MaxHealth:
				return maxHealth + strength * 20;
			case EntityAttributeType.HealthGegen:
				return healthRegen;
			case EntityAttributeType.StatusResistance: //返回的是实际的持续时间百分比
				return statusResistance;
			case EntityAttributeType.Strength:
				return strength;
			case EntityAttributeType.Agility:
				return agility;
			case EntityAttributeType.Intellect:
				return intellect;
			case EntityAttributeType.AttackDamage:
				return attackDamage;
			case EntityAttributeType.PhysicArmor:
				return physicArmor;
			case EntityAttributeType.MagicResistance:
				return 0;
			default:
				return 0;
		}

	}

	//修改属性值
	public void ModifyValue(EntityAttributeType type, int value)
	{
		switch (type)
		{
			case EntityAttributeType.Health:
				health += value;
				break;
			case EntityAttributeType.Strength:
				strength += value;
				break;
			case EntityAttributeType.Agility:
				agility += value;
				if(mainAttr == MainAttribute.ATTRIBUTE_AGILITY)
				{
					//phyArmor += 
				}
				break;
			default:
				return;
		}
	}

	public float GetFloat(string name)
	{
		return 0f;
	}

	//计算新的值并更新这个值
	public static int CalcAndUpdateValue(BaseEntity source, EntityAttributeType type)
	{
		//属性值的变换大致遵循这样的公式：(Base+∑Bonus)*(Percentage1 + 1)*(Percentage2 + 1)··· + ∑Bonus
		int intValue = 0;
		float floatVal = 0f;

		BaseNPC _source;
		switch (type)
		{
			case EntityAttributeType.MoveSpeed:
				_source = source as BaseNPC;
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					intValue = _source.BaseAttribute.moveSpeed; //基础值
					int maxVal = -1;
					int finalVal = -1;
					
					int baseBonus = 0;
					float perBonus = 1f; //百分比的增长值
					float perBonus1 = 1f; //百分比的增长值
					int extBonus = 0;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						//绝对值的优先级是最高的
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_ABSOLUTE, out val)) //速度的绝对值
						{
							finalVal = Math.Max(val.GetVal<int>(_source.Level), finalVal);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_ABSOLUTE_MIN,out val)) //最小速度的绝对值
						{
							finalVal = Math.Max(val.GetVal<int>(_source.Level), finalVal);
						}else if(finalVal < 0 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_BASE_OVERRIDE, out val))
						{
							intValue = val.GetVal<int>(_source.Level);
						}else if(finalVal < 0 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_BONUS_CONSTANT, out val)) //基础值的加成
						{
							baseBonus += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_BONUS_PERCENTAGE, out val))
						{
							perBonus *= (100 + val.GetVal<int>(_source.Level)) / 100;
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_BONUS_PERCENTAGE_UNIQUE, out val)) //unique表示不可叠加
						{
							perBonus1 *= (100 + val.GetVal<int>(_source.Level)) / 100;
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_BONUS_PERCENTAGE_UNIQUE_2, out val)) //unique表示不可叠加
						{

						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_BONUS_UNIQUE, out val))
						{
							baseBonus += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_BONUS_UNIQUE_2, out val)) //unique表示不可叠加
						{

						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_LIMIT, out val)) //最终值达到的上限
						{
							maxVal = Math.Min(val.GetVal<int>(_source.Level), maxVal);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MOVESPEED_MAX, out val))
						{
							maxVal = Math.Min(val.GetVal<int>(_source.Level), maxVal);
						}

						if(finalVal < 0)
						{
							intValue = maxVal > 0 ? Math.Min(finalVal, maxVal) : finalVal;
						}else{
							intValue = (int)((intValue + baseBonus) * perBonus + extBonus);
							intValue = maxVal > 0 ? Math.Min(intValue, maxVal) : intValue;
						}

						_source.Attribute.moveSpeed = intValue;
					}
					
				}
				break;
			case EntityAttributeType.StatusResistance:
				//Total status resistance = 1 − ((1 - Strength × 0.15% on strength heroes) × (1 − first resistance bonus) × (1 − second resistance bonus))
				_source = source as BaseNPC;
				if(_source != null)
				{
					//基础值
					floatVal = 1 - EntityAttribute.CalcAndUpdateValue(source,EntityAttributeType.Strength) * 0.0015f;
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						//加成值
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_STATUS_RESISTANCE_BONUS, out val)) 
						{
							floatVal *= (100 - val.GetVal<int>(_source.Level)) / 100;
						}
					}
					floatVal = 1 - floatVal;
					_source.Attribute.statusResistance = floatVal;
				}else
				{
					floatVal = (1 - (1 - EntityAttribute.CalcAndUpdateValue(source,EntityAttributeType.Strength) * 0.0015f));
					source.Attribute.statusResistance = floatVal;
				}

				break;
			case EntityAttributeType.MagicResistance:	
				_source = source as BaseNPC;
				//Total magic resistance = 1 − ((1 − natural resistance) × (1 - Intelligence × 0.15% on intelligence heroes) × (1 − first resistance bonus) × (1 − second resistance bonus) × (1 + first resistance reduction) × (1 + second resistance reduction))
				if(_source != null)
				{
					//main armor
					floatVal = 1f;
					if(_source != null)
					{
						var tmp = _source.Modifiers;
						int count = tmp.Count;
						int finalVal = 0;
						for(int i = 0; i < count; i++)
						{
							var iter = tmp[i].Properties;
							AbilitySpecial val;
							//加成值
							if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MAGICAL_RESISTANCE_BONUS, out val)) 
							{
								floatVal *= (1 - val.GetVal<int>(_source.Level)/100);
							}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MAGICAL_RESISTANCE_DECREPIFY_UNIQUE, out val))
							{
								finalVal = Math.Max(finalVal, val.GetVal<int>(_source.Level)) ;
							}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MAGICAL_RESISTANCE_DIRECT_MODIFICATION, out val)) //?直接修改变化值
							{
								//floatVal += val.GetVal<float>(_source.Level);
							}
						}

						_source.Attribute.magicResistance = (1 - _source.BaseAttribute.magicResistance)*(1 - EntityAttribute.CalcAndUpdateValue(_source, EntityAttributeType.Intellect) * 0.0015f) * floatVal * (1 - finalVal/100);
					}
				}else
				{
					floatVal = 1 - source.BaseAttribute.magicResistance;
				}
				break;
			case EntityAttributeType.PhysicArmor:	
				_source = source as BaseNPC;
				//main armor = base armor + (agility/6);
				//illusions能够从unit中继承值了main armor的属性
				if(_source != null)
				{
					//main armor
					floatVal = _source.BaseAttribute.physicArmor + EntityAttribute.CalcAndUpdateValue(source, EntityAttributeType.Agility)/6;
					if(_source != null)
					{
						var tmp = _source.Modifiers;
						int count = tmp.Count;
						float finalVal = 0f;
						float bonus = 0f;
						for(int i = 0; i < count; i++)
						{
							var iter = tmp[i].Properties;
							AbilitySpecial val;
							//加成值
							if(finalVal > -1 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS, out val)) 
							{
								bonus += val.GetVal<float>(_source.Level);
							}else if(finalVal > -1 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS_UNIQUE, out val))
							{
								finalVal = Math.Max(finalVal, val.GetVal<float>(_source.Level));
							}else if(finalVal > -1 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS_UNIQUE_ACTIVE, out val))
							{
								finalVal = Math.Max(finalVal, val.GetVal<float>(_source.Level));
							}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_IGNORE_PHYSICAL_ARMOR, out val)) //这个优先级最高
							{
								finalVal = -2f;
								break;
							}
						}
						if(finalVal > -1)
						{
							_source.Attribute.physicArmor = floatVal + Math.Max(bonus, finalVal);
						}else
						{
							_source.Attribute.physicArmor = 0f;
						}
					}
				}else
				{
					floatVal = source.BaseAttribute.physicArmor;
				}
				break;
			case EntityAttributeType.DamageBlock:
				//格挡的伤害比较特殊，需要根据每个装备或者buff单独计算，不会叠加，而且跟自身的格挡是无关的
				_source = source as BaseNPC;
				if(_source != null)
				{
					intValue = _source.BaseAttribute.damageBlock;
					if(_source != null)
					{
						var tmp = _source.Modifiers;
						int count = tmp.Count;
						for(int i = 0; i < count; i++)
						{
							var iter = tmp[i].Properties;
							AbilitySpecial val;
							//加成值
							if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_TOTAL_CONSTANT_BLOCK, out val)) 
							{
								intValue += val.GetVal<int>(_source.Level);
							}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_TOTAL_CONSTANT_BLOCK_UNAVOIDABLE_PRE_ARMOR, out val))
							{
								intValue += val.GetVal<int>(_source.Level);
							}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MAGICAL_CONSTANT_BLOCK, out val)) //
							{
								
							}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PHYSICAL_CONSTANT_BLOCK, out val))
							{
								
							}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PHYSICAL_CONSTANT_BLOCK_SPECIAL, out val))
							{
								
							}

							_source.Attribute.damageBlock = intValue; //格挡伤害值
						}
					}
				}else
				{

				}
				break;
				
			case EntityAttributeType.AttackSpeed:
				//Attack time = BAT / [(100 + IAS) × 0.01] = 1 / (attacks per second)
				floatVal = source.BaseAttribute.attackSpeed; //基础攻击速度
				_source = source as BaseNPC;
				if(_source != null)
				{
					//一点敏捷提高一点攻击速度
					intValue = EntityAttribute.CalcAndUpdateValue(_source, EntityAttributeType.Agility);
					var tmp = _source.Modifiers;
					float finalVal = -1f;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						//加成值
						if(finalVal < 0 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_ATTACKSPEED_BASE_OVERRIDE, out val)) 
						{
							floatVal = val.GetVal<float>(_source.Level);
						}else if(finalVal < 0 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT, out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_BASE_ATTACK_TIME_CONSTANT, out val))
						{
							finalVal = Math.Max(finalVal, val.GetVal<float>(_source.Level));
						}
					}

					if(finalVal < 0)
					{
						intValue = Math.Min(-80,intValue);
						intValue = Math.Max(500,intValue);
						floatVal = floatVal/((100 + intValue)*0.01f);
						_source.Attribute.attackSpeed = floatVal; //更新后的数值
					}else
					{
						_source.Attribute.attackSpeed = 1/finalVal;
					}
				}
				else{
					_source.Attribute.attackSpeed = floatVal;
				}
				break;
			case EntityAttributeType.CastRange:
				_source = source as BaseNPC;
				intValue = source.BaseAttribute.castRange;
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					int stackVal = 0;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_CAST_RANGE_BONUS,out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_CAST_RANGE_BONUS_STACKING,out val)) 
						{
							//stackVal += val.GetVal<int>(_source.Level);
						}
					}

					//更新值
					_source.Attribute.castRange = intValue;
				}
				break;
			case EntityAttributeType.AttackRange:
				_source = source as BaseNPC;
				intValue = source.BaseAttribute.attackRange;
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					int maxVal = 0;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_ATTACK_RANGE_BONUS,out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_ATTACK_RANGE_BONUS_UNIQUE,out val))
						{
							//intValue = val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MAX_ATTACK_RANGE,out val)) 
						{
							maxVal = Math.Max(maxVal, val.GetVal<int>(_source.Level));
						}

						intValue = maxVal > 0 ? Math.Min(intValue, maxVal) : intValue;
					}
				}
				break;
			case EntityAttributeType.Evasion:
				_source = source as BaseNPC;
				floatVal = 1 - source.BaseAttribute.evasion/100;
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_EVASION_CONSTANT, out val))
						{
							floatVal *= (1 - val.GetVal<int>(_source.Level)/100);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_NEGATIVE_EVASION_CONSTANT, out val)) 
						{
							floatVal *= (1 - val.GetVal<int>(_source.Level)/100);
						}
					}

					_source.Attribute.evasion = 100 - (int)(floatVal * 100); //
					return _source.Attribute.evasion;
				}else
				{

				}
				break;
			case EntityAttributeType.MissRate: //miss rate是相加
				_source = source as BaseNPC;
				intValue = source.BaseAttribute.missRate;
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MISS_PERCENTAGE, out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}
					}

					_source.Attribute.missRate = intValue;
					return intValue;
				}else
				{

				}
				break;
			case EntityAttributeType.Crit: //计算暴击率
				_source = source as BaseNPC;
				intValue = source.BaseAttribute.crit;
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PREATTACK_CRITICALSTRIKE, out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PREATTACK_TARGET_CRITICALSTRIKE, out val)) //目标的暴击率?
						{
							//intValue += val.GetVal<int>(_source.Level);
						}
					}

					_source.Attribute.crit = intValue;
				}else
				{

				}
				break;
			case EntityAttributeType.AttackDamage:
				//Damage = { [Main Damage × (1 +   Percentage Bonuses) + FlatBonuses] × Crit Multiplier - Blocked Damage } × Armor Multipliers × General Damage Multipliers
				//此处的计算只计算到Main Damage × (1 +   Percentage Bonuses) + FlatBonuses] × Crit Multiplier，后续的需要在确定目标之后才能计算
				//只是计算了单outgoing的伤害，
				_source = source as BaseNPC;
				intValue = source.BaseAttribute.attackDamage;
				if(_source != null)
				{
					//根据主属性计算主伤害
					switch(_source.Attribute.mainAttr)
					{
						case MainAttribute.ATTRIBUTE_AGILITY:
							intValue += EntityAttribute.CalcAndUpdateValue(_source, EntityAttributeType.Agility);
							break;
						case MainAttribute.ATTRIBUTE_INTELLECT:
							intValue += EntityAttribute.CalcAndUpdateValue(_source, EntityAttributeType.Intellect);
							break;
						case MainAttribute.ATTRIBUTE_STRENGTH:
							intValue += EntityAttribute.CalcAndUpdateValue(_source, EntityAttributeType.Strength);
							break;
					}
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					int perBonus = 0;
					int finalPerBonus = 0;
					int flatBonus = 0;
					int postCrit = 0;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE, out val))
						{
							flatBonus += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE_POST_CRIT, out val)) //在计算了暴击之后的攻击增加
						{
							postCrit += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_BASEATTACK_BONUSDAMAGE, out val)) //基础攻击增加
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_BASEDAMAGEOUTGOING_PERCENTAGE_UNIQUE, out val)) //?难道是在计算基础damage的时候引入
						{
							finalPerBonus = Math.Max(finalPerBonus,val.GetVal<int>(_source.Level));
						}
						else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_BASEDAMAGEOUTGOING_PERCENTAGE, out val))
						{
							perBonus += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PRE_ATTACK, out val)) //这个值增加在哪里，亦或就是基础值
						{

						}
						else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_OVERRIDE_ATTACK_MAGICAL, out val)) //?覆写魔法攻击，覆写的是基础值?
						{

						}
						/* 这部分触发攻击伤害，需要在modifier中特殊计算，不能单独计算
						else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE_PROC, out val))
						{

						}
						else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PROCATTACK_BONUS_DAMAGE_MAGICAL, out val))
						{

						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PROCATTACK_BONUS_DAMAGE_PHYSICAL, out val))
						{

						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PROCATTACK_BONUS_DAMAGE_PURE, out val))
						{

						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PROCATTACK_FEEDBACK, out val))
						{

						}
						 */
						
						 
					}

					intValue = (intValue *(1 + Math.Max(perBonus, finalPerBonus)/100) + flatBonus) * EntityAttribute.CalcAndUpdateValue(_source, EntityAttributeType.Crit);
					return intValue;
				}
				break;
			case EntityAttributeType.AttackIncomeModifier:
				//incomming相关计算，这部分在目标上
				_source = source as BaseNPC;
				intValue = 100; //默认没有伤害加深
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_INCOMING_DAMAGE_PERCENTAGE, out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_INCOMING_PHYSICAL_DAMAGE_PERCENTAGE, out val))
						{
							
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_INCOMING_SPELL_DAMAGE_CONSTANT, out val))
						{

						}
					}

					return intValue;
				}
				return intValue;
			case EntityAttributeType.AttackOutgoModifier:
				//incomming相关计算，这部分在目标上
				_source = source as BaseNPC;
				intValue = 100; //默认没有伤害加深
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_DAMAGEOUTGOING_PERCENTAGE, out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}
						else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MAGICDAMAGEOUTGOING_PERCENTAGE, out val)) //魔法攻击加成
						{

						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_DAMAGEOUTGOING_PERCENTAGE_ILLUSION, out val)) //幻影的加成
						{

						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_TOTALDAMAGEOUTGOING_PERCENTAGE, out val)) //?总的加成，好像跟前面的没有区别
						{
							intValue += val.GetVal<int>(_source.Level);
						}
					}
					return intValue;
				}
				return intValue;	
			case EntityAttributeType.IncomeSpellModifier:
				_source = source as BaseNPC;
				floatVal = 1f;
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_INCOMING_SPELL_DAMAGE_CONSTANT, out val))
						{
							floatVal = (val.GetVal<int>(_source.Level))/100;
						}
					}

					_source.Attribute.spellDamage = floatVal;
				}else
				{

				}
				break;
			case EntityAttributeType.SpellDamage:
				_source = source as BaseNPC;
				floatVal = source.BaseAttribute.spellDamage + EntityAttribute.CalcAndUpdateValue(source, EntityAttributeType.Intellect) / 1400;
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;

					float bonusVal = 1f;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_SPELL_AMPLITY_PERCENTAGE, out val))
						{
							bonusVal += val.GetVal<int>(_source.Level);
						}
					}

					_source.Attribute.spellDamage = floatVal * (1 + bonusVal/100);
				}else
				{

				}
				break;	
			case EntityAttributeType.Mana:
				intValue = source.Attribute.mana;
				break;
			case EntityAttributeType.MaxMana:
				intValue = source.BaseAttribute.maxMana + EntityAttribute.CalcAndUpdateValue(source, EntityAttributeType.Intellect)  * 14;
				_source = source as BaseNPC;
				
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					int finalVal = 0;
					int bonus = 0;
					floatVal = 1f;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_HEALTH_BONUS,out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MIN_HEALTH,out val))
						{
							finalVal = Math.Min(finalVal, val.GetVal<int>(_source.Level));
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_EXTRA_HEALTH_BONUS,out val)) //在percentage之后识别
						{
							bonus += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_EXTRA_HEALTH_PERCENTAGE,out val))
						{
							floatVal *= (100 + val.GetVal<int>(_source.Level))/100;
						}
					}

					source.Attribute.maxHealth = (int)((intValue + bonus)* floatVal);
				}
				else{

				}
				break;
			case EntityAttributeType.Health:
				intValue = source.Attribute.health;
				break;
			case EntityAttributeType.MaxHealth:
				intValue = source.BaseAttribute.maxHealth + EntityAttribute.CalcAndUpdateValue(source, EntityAttributeType.Strength)  * 20;
				_source = source as BaseNPC;
				
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					int finalVal = 0;
					int bonus = 0;
					floatVal = 1f;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_HEALTH_BONUS,out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_MIN_HEALTH,out val))
						{
							finalVal = Math.Min(finalVal, val.GetVal<int>(_source.Level));
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_EXTRA_HEALTH_BONUS,out val)) //在percentage之后识别
						{
							bonus += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_EXTRA_HEALTH_PERCENTAGE,out val))
						{
							floatVal *= (100 + val.GetVal<int>(_source.Level))/100;
						}
					}

					source.Attribute.maxHealth = (int)((intValue + bonus)* floatVal);
				}
				else{

				}
				break;
			case EntityAttributeType.HealthGegen:
				//Health Regeneration = (Base + Sum of Flat Bonuses) × (1 + strength × (5/700))
				//intValue = (int)((source.BaseAttribute.healthRegen + ) * (1 + EntityAttribute.CalcAndUpdateValue(source,EntityAttributeType.Strength) * 5 /700));
				
				_source = source as BaseNPC;
				
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					float bonus = 0;
					floatVal = _source.BaseAttribute.healthRegen;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_HEALTH_REGEN_CONSTANT,out val))
						{
							bonus += val.GetVal<float>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_HEALTH_REGEN_PERCENTAGE,out val))
						{
							floatVal *= (1 + val.GetVal<int>(_source.Level)/100);
						}
					}
					_source.Attribute.healthRegen = (_source.BaseAttribute.healthRegen + bonus) * floatVal;
				}
				else{

				}
				break;
			case EntityAttributeType.HealModifier:
				//Health Regeneration = (Base + Sum of Flat Bonuses) × (1 + strength × (5/700))
				//intValue = (int)((source.BaseAttribute.healthRegen + ) * (1 + EntityAttribute.CalcAndUpdateValue(source,EntityAttributeType.Strength) * 5 /700));
				
				_source = source as BaseNPC;
				
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					floatVal = 1f;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_HEAL_AMPLIFY_PERCENTAGE,out val))
						{
							floatVal *= (1 + val.GetVal<float>(_source.Level)/100);
						}
					}
					//return floatVal;
				}
				else{

				}
				break;
			case EntityAttributeType.Strength:
				//intValue = 
				_source = source as BaseNPC;
				//基础的力量值
				intValue = source.BaseAttribute.strength;// + source.BaseAttribute.strengthGain * source.Level; 升级的效果在外面处理
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_STATS_STRENGTH_BONUS,out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_EXTRA_STRENGTH_BONUS, out val)) //最外层的增加
						{
							intValue += val.GetVal<int>(_source.Level);
						}
					}
					_source.Attribute.strength = intValue;
				}
				break;
			case EntityAttributeType.Agility:
				//intValue = 
				_source = source as BaseNPC;
				//基础的力量值
				intValue = source.BaseAttribute.agility;// + source.BaseAttribute.strengthGain * source.Level; 升级的效果在外面处理
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_STATS_AGILITY_BONUS,out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}
					}
					_source.Attribute.strength = intValue;
				}
				break;
			case EntityAttributeType.Intellect:
				//intValue = 
				_source = source as BaseNPC;
				//基础的力量值
				intValue = source.BaseAttribute.intellect;// + source.BaseAttribute.strengthGain * source.Level; 升级的效果在外面处理
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_STATS_INTELLECT_BONUS,out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}
					}
					_source.Attribute.strength = intValue;
				}
				break;
		}
		return intValue;
	}
}

public enum EntityAttributeType{
	Mana = 1,

	MaxMana,
	Health,

	MaxHealth,

	HealthGegen,

	HealModifier,

	Strength,

	Agility,

	Intellect,

	StatusResistance,

	AttackDamage,

	//计算出去的伤害加深
	AttackOutgoModifier,

	//计算进入的伤害加深
	AttackIncomeModifier,

	AttackRange,
	
	AttackSpeed,

	AttackTime,

	PhysicArmor,

	MagicResistance,

	DamageBlock,

	Crit,

	CastRange,

	AtkRange,

	MoveSpeed,

	MissRate,

	Evasion,

	SpellDamage,

	IncomeSpellModifier,

}
