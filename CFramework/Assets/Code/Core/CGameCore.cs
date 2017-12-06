using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//游戏核心类
public class CGameCore {

	//游戏内的各个模块
	private List<CModule> modules;

	private int updateNum = 0;

	//游戏初始化
	public void Init()
	{
		modules = new List<CModule>(10);
		AddModule("Network", new CNetwork());
		AddModule("AssetManager", new CAssetsManager());
		AddModule("DataBase",  new CDatabase());
		AddModule("ScriptManager", new CScriptManager());

		AddModule("DataModel", new CDataModel());
		AddModule("UIManager", new CUIManager());
		AddModule("SceneManager", new CSceneManager());

		CDataModel.Player.CsAccountLogin();
	}

	

	public void AddModule(string name, CModule module)
	{
		modules.Add(module);
		module.Init();
	}

	public void DiposeModule(string name)
	{
		
	}

	public void ActivateModule()
	{

	}

	public void DeactiveModule()
	{

	}

	// Update is called once per frame
	public void Update () 
	{
		for(int i = 0; i <= updateNum; i++)
		{
			modules[i].Update();
		}
	}
}
