using System.Collections;
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

	//物理攻击
	public int physicAttack;

	//物理防御
	public float physicArmor;

	//魔法攻击
	public int magicAttack;

	//魔法防御
	public int magicResistance;

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

	//
	public int missRate;

	//躲闪
	public int evasion;

	//攻击免疫
	public int avoidDamage;

	//法术免疫
	public int avoidSpell;
	

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
			case EntityAttributeType.PhyAttack:
				return physicAttack;
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

	public void TakeDamage(DamageType type, int value)
	{
		switch(type)
		{
			case DamageType.DAMAGE_TYPE_MAGICAL:
			break;
			case DamageType.DAMAGE_TYPE_PHYSICAL:
			break;
			case DamageType.DAMAGE_TYPE_PURE:
			break;
			case DamageType.DAMAGE_TYPE_REMOVAL:
			break;
		}
		
		health -= value;
	}

	//根据算法计算出是否击中
	public bool CheckHit(EntityAttribute target)
	{
		return false;
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
							perBonus *= (100 + val.GetVal<int>(_source.Level)) / 100;
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
			case EntityAttributeType.AttackSpeed:
				//Attack time = BAT / [(100 + IAS) × 0.01] = 1 / (attacks per second)
				floatVal = source.BaseAttribute.attackSpeed; //基础攻击速度
				_source = source as BaseNPC;
				if(_source != null)
				{
					//一点敏捷提高一点攻击速度
					intValue = EntityAttribute.CalcAndUpdateValue(source, EntityAttributeType.Agility);
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						//加成值
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_ATTACKSPEED_BASE_OVERRIDE, out val)) 
						{
							floatVal = val.GetVal<float>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT, out val))
						{
							intValue += val.GetVal<int>(_source.Level);
						}
					}

					intValue = Math.Min(-80,intValue);
					intValue = Math.Max(500,intValue);
					floatVal = floatVal/((100 + intValue)*0.01f);
					_source.Attribute.attackSpeed = floatVal; //更新后的数值
				}
				else{
					_source.Attribute.attackSpeed = floatVal;
				}
				
				
				break;
			case EntityAttributeType.PhysicArmor:	
				_source = source as BaseNPC;
				//main armor = base armor + (agility/6);
				//main armor表示
				if(_source != null)
				{
					//main armor
					floatVal = _source.BaseAttribute.physicArmor + EntityAttribute.CalcAndUpdateValue(source, EntityAttributeType.Agility)/6;
					if(_source != null)
					{
						var tmp = _source.Modifiers;
						int count = tmp.Count;
						for(int i = 0; i < count; i++)
						{
							var iter = tmp[i].Properties;
							AbilitySpecial val;
							float finalVal = -1f;
							//加成值
							if(finalVal < 0 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS, out val)) 
							{
								floatVal += val.GetVal<float>(_source.Level);
							}else if(finalVal < 0 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS_UNIQUE, out val))
							{
								floatVal += val.GetVal<float>(_source.Level);
							}else if(finalVal < 0 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS_UNIQUE_ACTIVE, out val))
							{
								floatVal += val.GetVal<float>(_source.Level);
							}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_IGNORE_PHYSICAL_ARMOR, out val))
							{
								finalVal = 0f;
								break;
							}else if(finalVal < 0 && iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_TOTAL_CONSTANT_BLOCK_UNAVOIDABLE_PRE_ARMOR, out val))
							{
								
							}
							
							if(finalVal < 0)
							{
								_source.Attribute.physicArmor = floatVal;
							}else
							{
								_source.Attribute.physicArmor = 0f;
							}
						}
					}
				}else
				{

				}
				break;
			case EntityAttributeType.Health:
				intValue = source.Attribute.health;
				break;
			case EntityAttributeType.MaxHealth:
				intValue = source.BaseAttribute.health + EntityAttribute.CalcAndUpdateValue(source, EntityAttributeType.Strength)  * 20;
				
				source.Attribute.maxHealth = intValue;
				break;
			case EntityAttributeType.HealthGegen:
				//Health Regeneration = (Base + Sum of Flat Bonuses) × (1 + strength × (5/700))
				//intValue = (int)((source.BaseAttribute.healthRegen + ) * (1 + EntityAttribute.CalcAndUpdateValue(source,EntityAttributeType.Strength) * 5 /700));
				source.Attribute.healthRegen = intValue;
				break;
			
			case EntityAttributeType.Strength:
				//intValue = 
				_source = source as BaseNPC;
				//基础的力量值
				intValue = source.Attribute.strength + source.Attribute.strengthGain * source.Level;
				if(_source != null)
				{
					var tmp = _source.Modifiers;
					int count = tmp.Count;
					for(int i = 0; i < count; i++)
					{
						var iter = tmp[i].Properties;
						AbilitySpecial val;
						if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_STATS_STRENGTH_BONUS,out val)) //直接增加力量值
						{
							intValue += val.GetVal<int>(_source.Level);
						}else if(iter.TryGetValue(Modifier_Property.MODIFIER_PROPERTY_EXTRA_STRENGTH_BONUS, out val)) //最外层的增加
						{
							intValue += val.GetVal<int>(_source.Level);
						}
					}
					//intValue = (int)(100 - (1 - EntityAttribute.CalcAndUpdateValue(_source,EntityAttributeType.Strength) * 0.0015f)*(1-_source.*100);
					
				}
				break;
		}
		return intValue;
	}
}

public enum EntityAttributeType{
	Health = 1,

	MaxHealth,

	HealthGegen,

	Strength,

	Agility,

	Intellect,

	StatusResistance,

	PhyAttack,

	PhysicArmor,

	MagicAttack,

	MagicResistance,

	Crit,

	AttackSpeed,

	CastRange,

	AtkRange,

	MoveSpeed,

	MissRate,

	Evasion,

	AvoidDamage,

}
