using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CModelCMD : CModelBase {

	private int cmdArgCount;

	/*-----------服务器的指令-----------*/
	private string[] cmdArgv;

	private char[] cmdTokenized;

	private char[] cmdCmd;

	
	/*------------用户的指令------------*/
	private UserCmd[] cmds;

	private int cmdIndex;

	public override void Init()
	{
		cmdArgv = new string[CConstVar.MAX_STRING_TOKENS];
		cmdTokenized = new char[CConstVar.BIG_INFO_STRING + CConstVar.MAX_STRING_TOKENS];
		cmdCmd = new char[CConstVar.BIG_INFO_STRING];
		cmds = new UserCmd[CConstVar.CMD_BACKUP];

		update = Update;
	}

	private void Update()
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

	public override void Dispose()
	{

	}

	/*-------------服务器指令------------*/
	//解析给定的字符串到command line tokens
	//复制到单独的buffer中，argv会指向这个临时的buffer
	public void TokenizeString(string textIn, bool ignoreQuotes)
	{
		//清理之前的args
		cmdArgCount = 0;

		if(textIn.Length > CConstVar.BIG_INFO_STRING){
			CLog.Error("cmd token string bigger than max");
			return;
		}
		Array.Copy(cmdCmd, textIn.ToCharArray(), textIn.Length);
		var zeroChar = '\0';
		for(int i = textIn.Length; i < CConstVar.BIG_INFO_STRING; i++){
			cmdCmd[i] = zeroChar;
		}
		int textPos = 0;
		int outPos = 0;
		while(true){
			if(cmdArgCount == CConstVar.MAX_STRING_TOKENS){
				return;
			}

			while(true){
				//跳过空格
				while(cmdCmd[textPos] != 0 && cmdCmd[textPos] < ' '){
					textPos++;
				}

				if(cmdCmd[textPos] == 0){ //解析了所有的tokens
					return;
				}

				//注释 // 注释
				if(cmdCmd[textPos] == '/' && cmdCmd[textPos+1] == '/'){
					return; //解析了所有的tokens
				}

				//跳过/* */注释
				if(cmdCmd[textPos] == '/' && cmdCmd[textPos] == '*'){
					while(cmdCmd[textPos] != 0 && (cmdCmd[textPos] != '*' || cmdCmd[textPos+1] != '/')){
						textPos++;
					}
					if(cmdCmd[textPos] == 0){
						return;//解析了所有的tokens
					}
					textPos += 2;
				}else{
					break; //准备好解析token
				}
			}

			int outStart = 0;
			int outLength = 0;

			//处理引号
			if(!ignoreQuotes && cmdCmd[textPos] == '"'){
				outStart = outPos;

				// cmdArgv[cmdArgCount] == 
				textPos++;
				while(cmdCmd[textPos] != 0 && cmdCmd[textPos] != '"'){
					cmdTokenized[outPos++] = cmdCmd[textPos++]; //复制双引号内的内容
					outLength++;
				}
				cmdTokenized[outPos++] = zeroChar;
				cmdArgv[cmdArgCount] = new string(cmdTokenized,outStart,outLength);
				cmdArgCount++;
				if(cmdCmd[textPos] != 0){
					return;
				}
				textPos++;
				continue;
			}


			outStart = outPos;
			outLength = 0;
			//常规的token
			while(cmdCmd[textPos] > ' '){
				if(!ignoreQuotes && cmdCmd[textPos] == '"'){
					break;
				}
				//跳过注释
				if(cmdCmd[textPos] == '/' && cmdCmd[textPos+1] == '/'){
					break;
				}
				if(cmdCmd[textPos] == '/' && cmdCmd[textPos+1] == '*'){
					break;
				}
				outLength++;
				cmdTokenized[outPos++] = cmdCmd[textPos++];
				
			}
			cmdArgv[cmdArgCount] = new string(cmdTokenized, outStart, outLength);
			cmdArgCount++;

			cmdCmd[textPos++] = zeroChar;

			
		}
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
