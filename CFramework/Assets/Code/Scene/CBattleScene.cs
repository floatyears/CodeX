using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBattleScene : CSceneBase {

	private List<BaseNPC> allNpc;

	private List<BaseNPC> blueTeam;

	private List<BaseNPC> redTeam;

	public override void Init()
	{
		allNpc = new List<BaseNPC>();
		blueTeam = new List<BaseNPC>();
		redTeam = new List<BaseNPC>();
	}

	public BaseNPC GetNpcByUID(int uid)
	{
		int count = allNpc.Count;
		for(int i = 0; i < count; i++)
		{
			var tmp = allNpc[i];
			if(tmp.GUID == uid)
			{
				return tmp;
			}
		}
		return null;
	}
}
