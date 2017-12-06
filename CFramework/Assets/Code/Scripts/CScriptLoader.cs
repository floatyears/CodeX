using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System.IO;

public class CScriptLoader : LuaFileUtils {

	private bool allScriptsInOneBundle = true;

	private const string SCRIPT_DIR = "Scripts";

	private bool useJIT;

	public CScriptLoader()
	{
		instance = this;
	}

	public override byte[] ReadFile(string fileName)
	{
		var suffix = ".lua";
		if(!fileName.EndsWith(".lua"))
		{
			fileName += ".lua";
		}
		
		if(useJIT)
		{
			suffix = ".out";
		}
		else
		{
			suffix = "";
		}

		byte[] buffer = null;
		//首先读取更新目录下的内容，如果有单个文件的更新，那么就优先读取
		string path = CPath.persistentDataPath + "Update/" + fileName + suffix;
		if(File.Exists(path))
		{
			return File.ReadAllBytes(path);
		}

		//如果更新目录下没有文件，那么读取streaming assets目录
		path = CPath.streamingAssetsPath + fileName + suffix;
		if(File.Exists(path))
		{
			return File.ReadAllBytes(path);
		}

		//如果没有这个文件，那么从bundle中读取
		string bundleName = "scripts.data";
		string finalName = fileName;
		if(allScriptsInOneBundle)
		{
			finalName = "scripts_" + fileName.Replace('/', '_');
		}else
		{
			int pos = fileName.LastIndexOf('/');
			if(pos > 0)
			{
				bundleName = fileName.Substring(0,pos);
				bundleName = "scripts_" + bundleName.Replace('/','_').ToLower() + ".data";
				finalName = fileName.Substring(pos + 1);
			}
		}
		
		AssetBundle bundle = null;
		zipMap.TryGetValue(fileName, out bundle);
		if(bundle == null)
		{
			string bundlePath = CPath.assetsPath + bundleName;
			if(File.Exists(bundlePath))
			{
				bundle = AssetBundle.LoadFromFile(bundlePath);
			}
			
			if(bundle != null)
			{
				zipMap.Add(bundleName, bundle);
			}
		}
		
		if(bundle != null)
		{
#if UNITY_5
                TextAsset luaCode = bundle.LoadAsset<TextAsset>(fileName);
#else
                TextAsset luaCode = bundle.LoadAsset(fileName, typeof(TextAsset)) as TextAsset;
#endif

                if (luaCode != null)
                {
                    buffer = luaCode.bytes;
                    Resources.UnloadAsset(luaCode);
                }
		}

		return null;
	}
}
