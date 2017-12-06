using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : CModule {

	public List<ParticleItem> particlesList;

	private static ParticleManager instance;

	public static ParticleManager Instance{
		get{
			return instance;
		}
	}

	public override void Init()
	{
		particlesList = new List<ParticleItem>();
		instance = this;
	}

	public int CreateParticle(string particleName, EffectAttachType particleAttach, BaseEntity owningEntity)
	{
		ParticleItem item = new ParticleItem(particleName, owningEntity);
		particlesList.Add(item);
		return item.particleID;
	}

	public int CreateParticleForPlayer(string particleName, EffectAttachType particleAttach, BaseEntity owningEntity, BaseEntity owningPlayer)
	{
		ParticleItem item = new ParticleItem(particleName, owningEntity);
		particlesList.Add(item);
		return item.particleID;
	}

	public int CreateParticleForTeam(string particleName)
	{
		return 0;
	}

	public void DestroyParticle(int particleID, bool immediately)
	{
		bool hasParticle = false;
		int count = particlesList.Count;
		for(int i = count - 1; i >= 0; i--)
		{
			ParticleItem item = particlesList[i];
			if(item.particleID == particleID)
			{
				if(immediately){
					particlesList.Remove(item);
				}
				else
				{
					item.toBeRemoved = true;
				}
				hasParticle = true;
			}
		}
		
		if(!hasParticle)
		{
			CLog.Info("Particle is not in the list");
		}
	}

	public string GetParticleReplacement(string particleName, BaseEntity entity)
	{
		return "";
	}

	public void ReleaseParticleIndex(int particleID)
	{
		
	}

	public void SetParticleAlwaysSimulate(int particleID)
	{

	}

	public void SetParticleControl(int particleID, int controlIndex, Vector3 controlData)
	{

	}

	public void SetParticleControlEnt(int int_1, int int_2, BaseEntity entity, int int_4, string string_5, Vector3 Vector_6, bool bool_7)
	{

	}

	public void SetParticleControlForward(int index, int point, Vector3 forward)
	{

	}

	public void SetParticleControlOrientation(int index, int point, Vector3 forward, Vector3 right, Vector3 up)
	{

	}

	public override void Update()
	{
		int count = particlesList.Count;
		for(int i = count - 1; i >= 0; i--)
		{
			if(particlesList[i].toBeRemoved)
			{
				particlesList.RemoveAt(i);
			}
			else
			{
				particlesList[i].Update();
			}
		}
	}

}
