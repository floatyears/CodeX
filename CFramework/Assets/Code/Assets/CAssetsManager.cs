using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

//namespace CFramework{
	
	//比较扁平的资源管理器
	public class CAssetsManager : CModule {

		private static CAssetsManager instance;

		//资源加载的根路径
		private string assetRootPath;

		//manifest文件路径
		private string manifestPath;

		//等待加载的资源loader
		private Stack<CAssetLoaderInfo> loaderInfoQueue;

		//已经加载完并缓存的loader
		private Dictionary<string,CAssetLoaderInfo> cachedLoaderInfos;

		//正在异步加载的quest
		private List<CAssetAsyncLoader> asyncQuests;

		private Dictionary<string, List<AssetCallback>> callBacks;
		
		//manifest文件信息
		private AssetBundleManifest manifestInfo;

		//被谁使用的信息
		private Dictionary<string, List<string>> usedByInfos;

		//资源加载完成的回调函数
		public delegate void AssetCallback(CAssetLoaderInfo loader);
		
		//此处没有设计成单例模式，而是把实例化交给了模块，模块本身是可以不进行初始化的
		public static CAssetsManager Instance
		{
			get{
				return instance;
			}
		}

		public override void Init()
		{
			loaderInfoQueue = new Stack<CAssetLoaderInfo>();
			assetRootPath = CPath.streamingAssetsPath;
			manifestPath = CPath.streamingAssetsPath + "Assets";
			callBacks = new Dictionary<string, List<AssetCallback>>();
			cachedLoaderInfos = new Dictionary<string, CAssetLoaderInfo>();
			asyncQuests = new List<CAssetAsyncLoader>();
			usedByInfos = new Dictionary<string, List<string>>();
			instance = this;
			LoadManifest();
		}

		private void LoadManifest()
		{
			if(File.Exists(manifestPath))
			{
				manifestInfo = AssetBundle.LoadFromFile(manifestPath).LoadAsset("AssetBundleManifest") as AssetBundleManifest;
			}
		}

		public override void Update()
		{
			if(loaderInfoQueue.Count > 0)
			{
				CAssetLoaderInfo firstItem = loaderInfoQueue.Peek();
				if(LoadAssetImpl(firstItem)){ //加载之后再从队列中移除
					loaderInfoQueue.Pop();
				}
			}

			//遍历
			int len = asyncQuests.Count;
			for(int i = len - 1; i >= 0; i--)
			{
				if(asyncQuests[i].isDone())
				{
					var info = asyncQuests[i].LoaderInfo;
					asyncQuests.RemoveAt(i);
					
					cachedLoaderInfos.Add(info.assetFileName, info);
					var calls = callBacks[info.assetFileName];
					int count = calls.Count;
					for(int k = count - 1; k >= 0; k--)
					{
						info.refCount++; //这里根据每个回调来增加引用计数，感觉是有问题的
						calls[k](info);
					}
					callBacks.Remove(info.assetFileName);
				}
			}
		}

		//基于现有的经验，大部分情况下，异步加载资源的表现更加优异，所以直接使用异步接口，不管是否是同步还是异步
		public void LoadAssetAsync(string assetName, string assetFileName, AssetCallback callBack)
		{
			if(!IsAssetsLoaded(assetFileName))
			{
				var info = new CAssetLoaderInfo();
				info.assetName = assetName;
				info.path = assetRootPath + assetFileName;
				info.loadType = CAssetLoadType.AssetBundleFile;
				
				loaderInfoQueue.Push(info);
			}
			
			if(callBack != null)
			{
				if(!callBacks.ContainsKey(assetFileName))
				{
					callBacks.Add(assetFileName, new List<AssetCallback>());
				}
				callBacks[assetFileName].Add(callBack);
			}
			
			//loader.callBack = callBack;
			
		}


		//释放资源，在手动调用之后才会释放
		public void ReleaseAsset(CAssetLoaderInfo info)
		{
			if(info.isDone)
			{
				info.refCount = info.refCount > 0 ? info.refCount-1 : 0;
			
				//TODO: 一般引用计数等于0之后，不立即释放，后续需要优化。
				if(info.refCount == 0)
				{
					bool hasDep = false;
					//除了引用计数，还需要考虑资源的互相依赖，只有被依赖的资源已经释放了，才能释放。
					var relations = usedByInfos[info.assetFileName];
					for(int i = relations.Count - 1; i >= 0; i--)
					{
						var fName = relations[i];
						if(cachedLoaderInfos.ContainsKey(fName))
						{
							hasDep = true;
						}else
						{
							relations.RemoveAt(i);
						}
					}
					
					if(!hasDep){
						cachedLoaderInfos.Remove(info.assetFileName);
						info.assetBundle.Unload(true);
					}
				}
			}
			
		}

		//返回值表明是否加载了所需的资源
		private bool LoadAssetImpl(CAssetLoaderInfo loader)
		{
			switch(loader.loadType)
			{
				case CAssetLoadType.AssetBundleFile:
				case CAssetLoadType.AssetBundleMemory: //从内存中载入，这个时候也需要考虑异步载入的情况，可以另开一个线程来加载
					if(File.Exists(loader.path))
					{
						if(!LoadDependencyAssets(loader)) //依赖项已经载入
						{
							asyncQuests.Add(new CAssetAsyncLoader(loader));
							return true;
							// AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes());
							// asyncQuests.Add(AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(loader.path)),loader);
						}
					}
					break;	
				case CAssetLoadType.WWW: //不准备用www的方式直接从服务器读取数据

					break;
			}
			return true;
		}

		//加载依赖资源，如果没有依赖项需要被加载或者所有依赖项都被载入，那么返回true
		private bool LoadDependencyAssets(CAssetLoaderInfo asset)
		{
			string[] dAssets = manifestInfo.GetAllDependencies(asset.assetFileName);
			asset.depFiles = dAssets; //这里设置资源的依赖资源
			int len = dAssets.Length;
			bool hasLoader = false;
			for(int i = 0; i < len; i++)
			{
				string tmpName = dAssets[i];
				if(!usedByInfos.ContainsKey(tmpName))
				{
					usedByInfos.Add(tmpName, new List<string>());
				}
				if(!usedByInfos[tmpName].Contains(asset.assetFileName))
				{
					usedByInfos[tmpName].Add(asset.assetFileName);
				}

				//如果依赖资源没有加载
				if(!IsAssetsLoaded(asset.assetFileName))
				{
					LoadAssetAsync("", tmpName, null);
					hasLoader = true;
				}
			}

			return hasLoader;
		}

		//该资源是否正在加载或者已经加载过
		private bool IsAssetsLoaded(string assetFileName)
		{
			if(cachedLoaderInfos.ContainsKey(assetFileName)) return true;
			int len = this.asyncQuests.Count;
			for(int i = 0; i < len; i++)
			{
				if(this.asyncQuests[i].LoaderInfo.assetFileName == assetFileName)
					return true;
			}

			return false;
		}
	}
//}

