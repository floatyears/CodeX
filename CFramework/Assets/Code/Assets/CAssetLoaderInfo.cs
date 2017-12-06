using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//获取资源得到的对象
public class CAssetLoaderInfo  {

	//资源所在的文件
	public string assetFileName;

	//资源名字
	public string assetName;

	//资源路径
	public string path;

	//资源类型
	public CAssetType assetType;

	//加载类型
	public CAssetLoadType loadType;

	public float progress = 0f;

	//是否加载完成
	public bool isDone = false;

	public AssetBundle assetBundle;

	public UnityEngine.Object mainObject;

	//引用计数
	public int refCount;

	//依赖资源
	public string[] depFiles;
	
	
}

public enum CAssetType
{
	Bundle = 1,

}

public enum CAssetLoadType
{
	AssetBundleFile = 1,
	AssetBundleMemory = 2,

	WWW = 3,
}
