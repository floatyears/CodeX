using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CModelCMD : IModel {

	private int cmdArgCount;

	/*-----------服务器的指令-----------*/
	private string[] cmdArgv;

	private char[] cmdTokenized;

	private char[] cmdCmd;

	
	/*------------用户的指令------------*/
	private UserCmd[] cmds;

	private int cmdIndex;

	public void Init()
	{
		cmdArgv = new string[CConstVar.MAX_STRING_TOKENS];
		cmdTokenized = new char[CConstVar.MAX_STRING_TOKENS];
		cmdCmd = new char[CConstVar.BIG_INFO_STRING];

		cmds = new UserCmd[CConstVar.CMD_BACKUP];
	}

	public void Update()
	{
		if(CDataModel.Connection.state < ConnectionState.CONNECTED){
			return;
		}

		if(CDataModel.Connection.ServerRunning && CDataModel.GameState.paused > 0){
			return;
		}

		//创建一个新的指令，即使是播回放
		// CreateNewUserCommands();

		// if()
	}

	public void Dispose()
	{

	}

	/*-------------服务器指令------------*/
	public void TokenizeString(string textIn, bool ignoreQuotes)
	{

	}
	

	public string Argv(int arg)
	{
		if(arg >= cmdArgCount)
		{
			return "";
		}
		return cmdArgv[arg];
	}

	/*-------------用户指令-------------*/
	

}
