using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;


public class CMessageGen : Editor{

	[MenuItem("Tools/Network/GenerateMessages")]
	public static void GenerateMessages()
	{
		string codePath = Application.dataPath + "/Code/Network/CMessageID.cs";
		string filePath = Application.dataPath.Replace("CFramework/Assets","") + "Tools/Message/flatc";
		string definePath = Application.dataPath.Replace("CFramework/Assets","") + "Tools/Message/Define/";
		string toolPath = Application.dataPath.Replace("CFramework/Assets","") + "Tools/Message";
		var di = new DirectoryInfo(definePath);

		var sb = StringBuilderCache.Acquire(2);
		var msgs = StringBuilderCache.Acquire(2);
		msgs.Append(@"
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
		
public class CMessageID{

	private static Dictionary<int, Type> dicMsgID;
	private static Dictionary<Type, int> dicMsgType;

	public static Type GetMsgType(int msgID)
	{
		return dicMsgID[msgID];
	}

	public static int GetMsgID(Type type)
	{
		return dicMsgType[type];
	}
	
	public CMessageID(){
		dicMsgID = new Dictionary<int, Type>();
		dicMsgType = new Dictionary<Type, int>();
");
		
		sb.Append(" "); 
		foreach(var fi in di.GetFiles()) 
		{
			sb.Append(fi.FullName).Append(" ");
			var strs = File.ReadAllLines(fi.FullName);
			int count = strs.Length;
			for(int i = 0; i < count; i++)
			{
				var line = strs[i];
				if(line.StartsWith("table Cs") || line.StartsWith("table Sc"))
				{
					var nextLine = strs[i+1];
					var strID = nextLine.Substring(nextLine.LastIndexOf("=")+1,5);
					var strType = line.Replace("table","").Replace(" ", "").Replace("{","");
					msgs.Append("		dicMsgID.Add(").Append(strID).Append(", typeof(").Append(strType).Append("));\n");
					msgs.Append("		dicMsgType.Add(typeof(").Append(strType).Append("), ").Append(strID).Append(");\n");
				}
			}
		}
		
		msgs.Append("	}\n}");
		if(File.Exists(codePath))
		{
			File.Delete(codePath);
		}
		var fs = File.CreateText(codePath);
		fs.WriteLine(msgs.ToString());
		fs.Close();

		#if UNITY_EDITOR_OSX
			//RunCommand("/bin/sh", "cd " + toolPath + "\n flatc -n -o " + Application.dataPath + "Code/Network/Messages/" + sb.ToString());
			RunCommand("/bin/sh", toolPath + "/MsgGen.sh " + Application.dataPath + "/Code/Network/Messages/" + sb.ToString());

		#elif UNITY_EDITOR_WIN
			//RunCommand("powershell", "flatc -n" + filePath)
			RunCommand("powershell", "flatc -n -o" + Applicationd.dataPath + "/Code/Network/Messages/" + sb.ToString());
		#endif

		AssetDatabase.Refresh();
	}

	private static void RunCommand(string command, string arg)
	{
		ProcessStartInfo info = new ProcessStartInfo(command);
		info.Arguments = arg;
		info.CreateNoWindow = false;
		info.ErrorDialog = true;
		info.UseShellExecute = false;

		if(info.UseShellExecute)
		{
			info.RedirectStandardError = false;
			info.RedirectStandardInput = false;
			info.RedirectStandardOutput = false;
		}else{
			info.RedirectStandardError = true;
			info.RedirectStandardInput = true;
			info.RedirectStandardOutput = true;

			info.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
			info.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
		}

		Process p = Process.Start(info);

		if(!info.UseShellExecute)
		{
			CLog.Info(p.StandardOutput.ReadToEnd());
			CLog.Info(p.StandardError.ReadToEnd());
		}

		p.WaitForExit();
		p.Close();
	}
}
