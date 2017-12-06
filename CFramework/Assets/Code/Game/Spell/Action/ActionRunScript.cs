using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionRunScript : BaseAction {

	private string scriptFile;

	private string function;

	private Dictionary<string, object> extraParamters;

	protected override bool ExeOnTarget(BaseEntity tar) 
	{
		var state = LuaClient.GetMainState();
		state.DoFile(scriptFile);
		var func = state.GetFunction(function);
		var params1 = state.GetTable(function + "_Params");
		var iterator = extraParamters.GetEnumerator();
		while(iterator.MoveNext())
		{
			params1[iterator.Current.Key] = iterator.Current.Value;
		}
		func.BeginPCall();
		func.Push(params1);
		func.PCall();
		func.EndPCall();
		return true;
	}
}
