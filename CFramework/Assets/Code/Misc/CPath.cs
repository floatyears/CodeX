using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//封装不同平台下的path
public class CPath {

	//Android平台下，如果没有权限，访问这个路径会返回空字符串
	public static string persistentDataPath;

	//该文件夹是只读
	public static string streamingAssetsPath;

	//指向Assets目录
	public static string dataPath;

	//资源目录
	public static string assetsPath;

	public static string demoPath;
	
	public static void Init()
	{
#if UNITY_EDITOR
			persistentDataPath = Application.persistentDataPath;
			dataPath = Application.dataPath;
			streamingAssetsPath = Application.streamingAssetsPath;
			assetsPath = Application.dataPath + "!assets/";
			demoPath = Application.persistentDataPath + "/demo.play";
#elif UNITY_IOS
			persistentDataPath = Application.persistentDataPath;
			dataPath = Application.dataPath;
			streamingAssetsPath = Application.streamingAssetsPath;
			assetsPath = Application.streamingAssetsPath;
			demoPath = Application.persistentDataPath + "/demo.play";
			
#elif UNITY_ANDROID
			persistentDataPath = Application.persistentDataPath;
			dataPath = Application.dataPath;
			streamingAssetsPath = Application.streamingAssetsPath;
			demoPath = Application.persistentDataPath + "/demo.play";
#else
			persistentDataPath = Application.persistentDataPath;
			dataPath = Application.dataPath;
			streamingAssetsPath = Application.streamingAssetsPath;
			demoPath = Application.persistentDataPath + "/demo.play";
			
#endif
	}
}
