using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCommand  {

	public EntityCommandType commandType;

	public EntityCommandState commandState;

	//在目标上面执行的UID
	public int targetUID;

	public int val1;

	public int val2;

	public EntityCommand(int tuid, EntityCommandType cType, int val1, int val2)
	{
		targetUID = tuid;
		
		commandType = cType;
		this.val1 = val1;
		this.val2 = val2;
	}

	public void Execute()
	{
		var tmpScene = CSceneManager.Instance.CurScene as CBattleScene;
		if(tmpScene != null)
		{
			BaseNPC target = tmpScene.GetNpcByUID(targetUID);
			//CSceneManager.Instance.
			switch(commandType)
			{
				case EntityCommandType.Move:
					if(this.val1 == 90)
					{
						target.MoveTo(target.Position + new Vector3(1,0,0));
					}
					else if(this.val1 == 270)
					{
						target.MoveTo(target.Position - new Vector3(1,0,0));
					}
					break;
				case EntityCommandType.Cast:

					break;
				case EntityCommandType.Use:

					break;
				case EntityCommandType.Target:

					break;
				default:
					break;
			}
		}
		
	}


}

public enum EntityCommandType
{
	Move = 1,

	Cast = 2,

	Target = 4,

	Use = 8,
}

public enum EntityCommandState
{
	None = 0,

	Unstart,

	Exceuting,

	End,
}
