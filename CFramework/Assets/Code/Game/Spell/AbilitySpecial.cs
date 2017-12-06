using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilitySpecial {

	private int[] valInt;

	private float[] valFloat;

	private string name;

	private AbilitySpecialVarType varType;

	//为了统一，所有的值都可以用AbilitySpecial来表示，固定不变的也可以
	private bool constValue;
	
	public AbilitySpecial(int[] value)
	{
		this.varType = AbilitySpecialVarType.INT;
		constValue = value.Length == 1;
		valInt = value;
	}

	public AbilitySpecial(float[] value)
	{
		this.varType = AbilitySpecialVarType.FLOAT;
		constValue = value.Length == 1;
		valFloat = value;
	}

	public T GetVal<T>(int level)
	{
		if(constValue) level = 1;
		switch(varType)
		{
			case AbilitySpecialVarType.FLOAT:
				return (T)Convert.ChangeType(valFloat[level], typeof(T)) ;
			case AbilitySpecialVarType.INT:
				return (T)Convert.ChangeType(valInt[level], typeof(T));
			default:
				return default(T);
		}
	}



	// public T GetVal<T>(int level) where T : struct
	// {
	// 	if(typeof(T) is int)
	// 	return (T)valInt[level];
	// }

}

public enum AbilitySpecialVarType
{
	NONE = 0,
	FLOAT,
	INT,
	STRING,
}
