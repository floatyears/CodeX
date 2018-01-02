using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CModelCMD : IModel {

	private int cmdArgCount;

	private string[] cmdArgv;

	private char[] cmdTokenized;

	private char[] cmdCmd;

	public void Init()
	{
		cmdArgv = new string[CConstVar.MAX_STRING_TOKENS];
		cmdTokenized = new char[CConstVar.MAX_STRING_TOKENS];
		cmdCmd = new char[CConstVar.BIG_INFO_STRING];
	}

	public void Dispose()
	{

	}

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

}
