using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class CAssetAsyncLoader {

	private CAssetLoaderInfo info;
	private AssetBundleCreateRequest quest;

	public CAssetAsyncLoader(CAssetLoaderInfo info)
	{
		this.info = info;
		Start();
	}

	private void Start()
	{
		ThreadPool.QueueUserWorkItem(AsyncLoad);
	}

	private void AsyncLoad(object state)
	{
		switch(info.loadType)
		{
			case CAssetLoadType.AssetBundleFile:
				var bytes = File.ReadAllBytes(info.path);
				quest = AssetBundle.LoadFromMemoryAsync(bytes);
				break;
			case CAssetLoadType.AssetBundleMemory:
				quest = AssetBundle.LoadFromFileAsync(info.path);
				break;
		}
		
	}

	public bool isDone()
	{
		if(quest != null && quest.isDone)
		{
			info.assetBundle = quest.assetBundle;
			return true;
		}
		return false;
	}

	public CAssetLoaderInfo LoaderInfo
	{
		get{
			return this.info;
		}
	}
}
