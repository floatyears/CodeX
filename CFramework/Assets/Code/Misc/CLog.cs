using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//日志信息
public class CLog {

	public static void Info(string msg)
	{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
		Debug.Log(msg);
#endif
	}

	public static void Info(string format, string msg)
	{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
		Debug.Log(string.Format(format, msg));
		TestConnections.AddLog(string.Format(format, msg));
#endif
	}

	public static void Info(string format, params object[] args)
	{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
		Debug.Log(string.Format(format, args));
		TestConnections.AddLog(string.Format(format, args));
#endif
	}

	public static void Warning(string msg)
	{
#if UNITY_EDITOR
		Debug.LogWarning(msg);
#endif
	}

	public static void Warning(string msg1, string msg2)
	{
#if UNITY_EDITOR
		Debug.LogWarning(msg1 + msg2);
#endif		
	}

	public static void Warning(string msg1, string msg2, string msg3)
	{
#if UNITY_EDITOR
		Debug.LogWarning(msg1 + msg2 + msg3);
#endif	
	}

	public static void Error(string msg)
	{
#if UNITY_EDITOR
		Debug.LogError(msg);
#endif	
	}

	public static void Error(string format, string msg)
	{
#if UNITY_EDITOR
		Debug.LogError(string.Format(format, msg));
#endif			
	}

	public static void Error(string format, params object[] args)
	{
#if UNITY_EDITOR
		Debug.LogError(string.Format(format, args));
#endif	
	}
}

public enum CLogType{

}
