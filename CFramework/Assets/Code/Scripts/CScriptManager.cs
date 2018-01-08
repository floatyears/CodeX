using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

public class CScriptManager : CModule {

	private static CScriptManager instance;

	public static CScriptManager Instance{
		get{
			return instance;
		}
	}

	private LuaClient luaClient;

	private GameObject luaObject;
	// private void 

	//作为全局的CScriptObject管理，保存所有创建的CScriptObject
	private Dictionary<GameObject,CScriptObject> dicObjScript;

	public override void Init()
	{
		if(luaObject == null)
		{
			luaObject = new GameObject("LuaClientObject");
			luaClient = luaObject.AddComponent<LuaClient>();
			GameObject.DontDestroyOnLoad(luaObject);
			dicObjScript = new Dictionary<GameObject, CScriptObject>();
			luaClient.Init();

		}

		instance = this;
	}

	// Update is called once per frame
	public override void Update () {
		
	}

	public override void Dispose()
	{
		GameObject.Destroy(luaObject);
	}
}
