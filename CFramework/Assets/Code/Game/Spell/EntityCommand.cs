using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCommand  {

	public EntityCommandType commandType;

	public EntityCommandState commandState;


}

public enum EntityCommandType
{
	Move = 1,

	Cast = 2,

	Channel = 4,
}

public enum EntityCommandState
{
	None = 0,

	Unstart,

	Exceuting,

	End,
}
