using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathologicalGames;

//这个类用于连接PoolManager和CAssetManager，上层实现只需要拿到实例化的Object，而不需要关心资源的创建与释放
public class CObjectsPool : CModule  {

	public delegate void InstatiateCallback(UnityEngine.Object obj, int uid);

	private static CObjectsPool instance;

	public static CObjectsPool Instance
	{
		get{
			return instance;
		}
	}

	private SpawnPool modelPool;

	private SpawnPool effectPool;


	public override void Init()
	{
		instance = this;

		var _modelPool = new GameObject();
		var _effPool = new GameObject();
		GameObject.DontDestroyOnLoad(_modelPool);
		GameObject.DontDestroyOnLoad(_effPool);
		modelPool = PoolManager.Pools.Create("Model", _modelPool);
		effectPool = PoolManager.Pools.Create("Effect", _effPool);
		

		assetsInfos = new Dictionary<string,CAssetLoaderInfo>();
		needUpdate = false;
	}

	private Dictionary<string, CAssetLoaderInfo> assetsInfos;

	private int instanceUID = 1;

	//返回的uid用于标识每一次的调用，因为异步的关系，同一个实例的同一个函数可能会多次调用本函数，这样无法得知回调函数是哪一次发起的，所以每次调用用一个单独的uid来标识
	//
	public void Instantiate(string assetFileName, string assetName, InstatiateCallback callback, ObjectType oType)
	{
		var fullName = assetFileName + "-" + assetName; //资源的唯一标识

		SpawnPool tmpPool = null;
		switch(oType)
		{
			case ObjectType.Model:
				tmpPool = modelPool;
				break;
			case ObjectType.Effect:
				tmpPool = effectPool;
				break;
		}
		CAssetLoaderInfo _info;
		int uid = instanceUID++;
		if(assetsInfos.TryGetValue(fullName, out _info))
		{
			callback(tmpPool.Spawn(_info.assetName), uid) ;
		}else{
			CAssetsManager.Instance.LoadAssetAsync(assetName, assetFileName,(CAssetLoaderInfo info)=>
			{
				//因为异步的关系，可能同时发起多个加载，第一次的加载返回之后，后面的加载调用已经不需要再次从AB包里面load了。
				if(assetsInfos.TryGetValue(fullName, out _info)) //表明已经加载过这个资源，池里面已经有了
				{
					CAssetsManager.Instance.ReleaseAsset(info); //最终只保留一份资源的引用
					callback(tmpPool.Spawn(_info.assetName), uid) ;
				}else{
					var obj = info.assetBundle.LoadAsset<GameObject>(info.assetName);
					if(obj == null)
					{
						CAssetsManager.Instance.ReleaseAsset(info); //加载出错即释放资源
						CLog.Error("Instantiate Failed, no such asset: %s",info.assetName);
					}else
					{
						assetsInfos.Add(fullName, info);
						callback(tmpPool.Spawn(obj.transform), uid);
					}
				}
			});
		}
	}

	public void Destroy(Transform obj, ObjectType oType)
	{
		SpawnPool tmpPool = null;
		switch(oType)
		{
			case ObjectType.Model:
				tmpPool = modelPool;
				break;
			case ObjectType.Effect:
				tmpPool = effectPool;
				break;
		}
		tmpPool.Despawn(obj);
	}


	
}

public enum ObjectType
{
	Model = 1,
	Effect = 2,
}