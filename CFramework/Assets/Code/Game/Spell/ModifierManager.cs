using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ModifierManager {

	private static Dictionary<string, ModifierParams> dicModifiers;

	private List<BaseModifier> updateModifiers;

	private int updateCount;

	public void Init()
	{
		dicModifiers = new Dictionary<string, ModifierParams>();
		updateModifiers = new List<BaseModifier>();
	}

	public static ModifierParams GetModifier(string name)
	{
		ModifierParams modifier;
		if(dicModifiers.TryGetValue(name, out modifier))
		{
			return modifier;
		}
		return default(ModifierParams);
	}

	public void AddThinkerModifier(BaseModifier modifier)
	{
		if(!updateModifiers.Contains(modifier))
		{
			updateModifiers.Add(modifier);
		}else
		{
			CLog.Info("There is a same thinker modifier: %s",modifier.ToString());
		}
	}

	public void UnRegisterModifier(BaseModifier modifier)
	{
		if(updateModifiers.Contains(modifier))
		{
			updateModifiers.Add(modifier);
		}else
		{
			CLog.Info("There is not the thinker modifier: %s",modifier.ToString());
		}
	}

	public void Update(float deltaTime)
	{
		updateCount = updateModifiers.Count;
		for(int i = 0; i < updateCount; i++)
		{
			updateModifiers[i].Update(deltaTime);
		}
	}



}
