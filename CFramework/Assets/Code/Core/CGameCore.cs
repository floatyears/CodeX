﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//游戏核心类
public class CGameCore {

	//游戏内的各个模块
	private List<CModule> modules;

	private List<CModule> fixedModules;

	private int updateNum = 0;

	private List<CModule> updateModules;

	private int fixedUpdateNum = 0;

	//游戏初始化
	public void Init()
	{
		CPath.Init();

		fixedModules = new List<CModule>(3);
		fixedModules.Add(new Server());
		fixedModules[0].Init();

		updateModules = new List<CModule>(20);
		modules = new List<CModule>(10);
		AddModule("Network", new CNetwork());
		AddModule("NetworkSrv", new CNetwork_Server());
		AddModule("AssetManager", new CAssetsManager());
		AddModule("DataBase",  new CDatabase());
		AddModule("ScriptManager", new CScriptManager());

		AddModule("DataModel", new CDataModel());
		AddModule("UIManager", new CUIManager());
		AddModule("SceneManager", new CSceneManager());
		
		
		fixedUpdateNum = fixedModules.Count;
		updateNum = updateModules.Count;
		
		// .ServerRunning = true;
		
		//fixedModules.Add();
		//CDataModel.Player.CsAccountLogin();
	}

	

	public void AddModule(string name, CModule module)
	{
		modules.Add(module);
		module.Init();

		if(module.needUpdate && !updateModules.Contains(module)){
			updateModules.Add(module);
		}
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
		for(int i = 0; i < updateNum; i++)
		{
			updateModules[i].Update();
		}

		for(int i = 0; i < fixedUpdateNum; i++)
		{
			fixedModules[i].Update();
		}
	}

	//fixed更新，比如帧同步这一块儿全是在FixedUpdate中更新
	public void FixedUpdate() {
		
	}
}
