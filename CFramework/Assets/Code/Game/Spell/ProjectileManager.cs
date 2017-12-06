using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : CModule {

	private int projectileID = 0;

	private float curTime = 0f;

	private static ProjectileManager instance;

	public static ProjectileManager Instance 
	{
		get{
			return instance;
		}
	}

	private List<ProjectileItem> projectiles;

	public override void Init()
	{
		projectiles = new List<ProjectileItem>();
	}

	//更新函数
	public override void Update()
	{
		float deltaTime = Time.realtimeSinceStartup - curTime;
		curTime = Time.realtimeSinceStartup;
		int count = projectiles.Count;
		for(int i = 0; i < count; i++)
		{
			projectiles[i].Update(deltaTime);
		}
	}

	public void ChangeTrackingProjectileSpeed(BaseAbility ability, int speed)
	{
		bool hasAbility = false;
		int count = projectiles.Count;
		for(int i = 0; i < count; i++)
		{
			if(projectiles[i].Ability == ability) 
			{
				projectiles[i].MoveSpeed = speed;
				hasAbility = true;
			}
		}
		if(!hasAbility){
			CLog.Info("there is not a projectile associated with the ability");
		}
	}

	public void CreateLinearProjectile(BaseEntity source, BaseAbility ability, ProjectileItemParams param )
	{
		var projectile = new ProjectileItem(source, ability, param);
		projectile.ProjectileType = ProjectileType.Linear;
		projectiles.Add(projectile);
	}

	public void CreateTrackingProjectile(BaseEntity source, BaseAbility ability, ProjectileItemParams param )
	{
		var projectile = new ProjectileItem(source, ability, param);
		projectile.ProjectileType = ProjectileType.Tracking;
		projectiles.Add(projectile);
	}

	public void DestroyLinearProjectile(int projectileID)
	{
		int count = projectiles.Count;
		for(int i = 0; i < count; i++)
		{
			var projectile = projectiles[i];
			if(projectile.ProjectileID == projectileID && projectile.ProjectileType == ProjectileType.Linear) 
			{
				projectile.Destroy();
				projectiles.RemoveAt(i);
			}
		}
	}

	public Vector3 GetLinearProjectileVelocity(int projectileID)
	{
		int count = projectiles.Count;
		for(int i = 0; i < count; i++)
		{
			if(projectiles[i].ProjectileID == projectileID) 
			{
				return projectiles[i].Velocity;
			}
		}
		return Vector3.zero;
	}

	public void ProjectileDodge()
	{

	}

	public int GernerateProjectileID()
	{
		return ++projectileID;
	}
}
