using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileItem {

	private int projectileID;

	private float expireTime;

	private ProjectileType projectileType;

	private BaseEntity source;

	private BaseAbility ability;

	private ProjectileItemParams infoParams;

	private EntityTarget target;

	private Vector3 position;

	private Vector3 velocity;

	private ParticleItem particleItem;


	public int ProjectileID
	{
		get{
			return projectileID;
		}
	}

	public ProjectileType ProjectileType{
		get{
			return projectileType;
		}
		set{
			projectileType = value;
		}
	}

	public BaseAbility Ability{
		get{
			return ability;
		}
		set{
			ability = value;
		}
	}

	public int MoveSpeed{
		set{
			infoParams.moveSpeed = value;
		}
	}

	public Vector3 Velocity
	{
		get{
			return velocity;
		}
	}

	
	public ProjectileItem(BaseEntity source, BaseAbility ability, ProjectileItemParams infoParams)
	{
		this.source = source;
		this.ability = ability;
		this.infoParams = infoParams;
		this.projectileID = ProjectileManager.Instance.GernerateProjectileID();
		this.velocity = infoParams.velocity;
		this.position = infoParams.spawnOrigin;
		this.expireTime = infoParams.expireTime;
		this.particleItem = new ParticleItem(this.infoParams.effectName, this.source);
	}

	public ProjectileItem(BaseEntity source, BaseAbility ability, ProjectileItemParams infoParams, EntityTarget target)
	{
		this.source = source;
		this.ability = ability;
		this.infoParams = infoParams;
		this.target = target;
		this.projectileID = ProjectileManager.Instance.GernerateProjectileID();
		this.velocity = infoParams.velocity;
		this.position = infoParams.spawnOrigin;
		this.expireTime = infoParams.expireTime;
		this.particleItem = new ParticleItem(this.infoParams.effectName, this.source);
	}

	public void Update(float deltaTime)
	{
		expireTime -= deltaTime;
		if(expireTime < 0)
		{
			ProjectileManager.Instance.DestroyLinearProjectile(this.projectileID);
		}else
		{
			if(projectileType == ProjectileType.Linear)
			{
				position += this.velocity * deltaTime;
			}else
			{
				velocity = Vector3.RotateTowards(velocity,target.GetFirstTarget().Position - position, 10f, 10f);
				position += this.velocity * deltaTime;
			}
			this.CheckHitTarget();
		}
	}
	
	public void CheckHitTarget()
	{
		var tars = target.GetTarget();
		int count = tars.Length;
		for(int i = 0; i < count; i++)
		{
			var tar = tars[i];
			if(Vector3.Distance(tars[i].Position, position) < 0.01)
			{
				var npc = tars[i] as BaseNPC;
				if(npc != null)
				{
					npc.TriggerAbilityEvent(AbilityEventType.OnProjectileHitUnit);
				}
			}
			
		}
	}

	public void Destroy()
	{
		particleItem.Destroy();
	}
}

public class ProjectileItemParams{
	public string effectName;

	public EffectAttachType sourceAttachment;

	public float startRadius;

	public float endRadius;

	public bool drawsOnMiniMap;

	public bool dodgeable;

	public bool isAttack;

	public bool visibleToEnemies;

	public bool replaceExisting; 

	public float expireTime;

	public bool provideVision;

	public int moveSpeed;
	
	public int visionRadius;

	public int visionTeamNum;

	public bool hasFrontalCone;

	public UnitTargetTeam unitTargetTeam;

	public UnitTargetFlags unitTargetFlags;

	public UnitTargetType unitTargetType;

	//释放的原点
	public Vector3 spawnOrigin;

	public Vector3 velocity;

	public Vector3 sourceLoc;

	public float distance;
}

public enum ProjectileType{
	Linear = 1,

	Tracking = 2,
}
