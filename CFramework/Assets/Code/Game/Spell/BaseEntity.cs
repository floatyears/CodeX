﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEntity {

	protected string name;

	protected int guid;

	protected Vector3 position;
	
	protected Vector3 velocity;

	protected Vector3 direction;

	//重力
	protected Vector3 gravity;

	//移动速度
	protected Vector3 speed;

	protected int teamID;

	protected int level;

	protected int maxLevel;

	protected EntityState entityData;

	protected BaseEntity owner;

	//npc自带的所有属性，升级所得以及各种永久性的改变，不包含各种外部加成导致的变换，以及属性联动带来的变化。
	protected EntityAttribute baseAttribute;

	//npc的当前属性，包括了各种道具，技能，buff带来的提升
	protected EntityAttribute attribute;

	protected EntityTarget target;

	protected BaseModel model;

	public BaseEntity Owner{
		get{
			return owner;
		}
	}

	public Vector3 Position{
		get{
			return position;
		}
	}

	public string Name{
		get{
			return name;
		}
	}

	public int Level{
		get{
			return level;
		}
	}

	public int GUID
	{
		get{
			return guid;
		}
	}

	public EntityAttribute BaseAttribute{
		get{
			return baseAttribute;
		}
	}

	public EntityAttribute Attribute{
		get{
			return attribute;
		}
	}

	public BaseModel Model{
		get{
			return model;
		}
		set{
			model = value;
		}
	}

	public virtual void Init()
	{
        model = new BaseModel();
        model.Init();
	}

	public virtual void Clear(){
		model.Dispose();
	}

	public virtual void Dispose(){
		model.Dispose();
	}
	

	public bool IsAlive()
	{
		return attribute.health <= 0;
	}

	public int GetIntVal(string name)
	{
		return attribute.GetInt(name);
	}

	public int GetIntVal(EntityAttributeType type)
	{
		return (int)attribute.GetValue(type);
	}

	public float GetFloatVal(string name)
	{
		return attribute.GetFloat(name);
	}

	public bool IsPlayer()
	{
		return false;
	}

	public void AttachData(EntityState ent){
		if(guid != ent.entityIndex){ //表示当前的entityidx跟之前的不一样了，需要执行特殊操作

		}else{
			
		}

		Vector3 tmp = Vector3.zero;
		ent.pos.GetTrBase(ref tmp);
		SetPosition(tmp);
	}

	public void AttachData(PlayerState ent){
		if(guid != ent.clientIndex){ //表示当前的entityidx跟之前的不一样了，需要执行特殊操作

		}else{
			
		}

		SetPosition(ent.origin);
		Camera.main.transform.parent.position = ent.origin;
	}

	public virtual void Update(float deltaTime)
	{
		
	}


	//-------移动相关接口--------//
	public void Rotate(Vector3 val)
	{
		
	}
	
	public void SetPosition(Vector3 pos){
		model.position = pos;
	}
	

	public void AddEffects()
	{

	}

	public void RemoveEffects()
	{

	}

	public void SetTeam(int teamID)
	{
		this.teamID = teamID;
	}

	

	public void AddEffects(string effectName, EffectAttachType attachType)
	{

	}
	

	public virtual void TakeDamage(DamageType damageType, int value)
	{
		//attribute.TakeDamage(damageType, value);
		
	}

	public void TakeHeal(int healAmount)
	{
	 	attribute.health += healAmount;
	}

	public void AttackTarget()
	{
        target.ForEachTarget((tar) => tar.TakeDamage(DamageType.DAMAGE_TYPE_PURE, attribute.attackDamage));
	}



}
