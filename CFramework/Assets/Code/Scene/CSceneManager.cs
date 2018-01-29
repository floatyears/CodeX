using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CSceneManager : CModule {

	private static CSceneManager instance;

	private CSceneState state;

	//当前场景
	private CSceneBase curScene;
	
	private CSceneBase prevScene;

	private Dictionary<int, CSceneBase> dicScene;

	//场景的加载栈，可以快速连续地切换场景。
	private Stack<int> loadSceneStack;

	public static CSceneManager Instance
	{
		get
		{
			return instance;
		}
	}

	public override void Init()
	{
		instance = this;
		SceneManager.sceneLoaded += OnSceneLoaded;
		dicScene = new Dictionary<int, CSceneBase>();
		loadSceneStack = new Stack<int>();
	}

	public override void Update()
	{
		//检测当前正在加载的栈
		if(loadSceneStack.Count > 0 && curScene.State != CSceneState.Loading)
		{
			var sceneID = loadSceneStack.Pop();
			loadSceneStack.Clear();
			ChangeScene(sceneID);
		}
		//if()
	}

	public override void Dispose()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		instance = null;
	}
	public void ChangeScene(int sceneID)
	{
		//当前有场景正在加载，那么就加入到loading栈
		if(curScene != null && curScene.State == CSceneState.Loading)
		{
			loadSceneStack.Push(sceneID);
		}else{
			// CGameCore
			var data = CDatabase.Instance.GetData<CTableScene>(sceneID);
			
			CAssetsManager.Instance.LoadAssetAsync(data.name, data.resPath, (asset)=>{

				if(curScene != null)
				{
					curScene.Dispose();
				}

				CSceneBase scene;
				if(!dicScene.TryGetValue(data.id, out scene))
				{
					scene = new CSceneBase();
					scene.Init();
					dicScene.Add(data.id, scene);
				}
				scene.State = CSceneState.Loading;
				curScene = scene;

				UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(data.name);

			});
		}
		
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if(scene.name == curScene.Data.name)
		{
			curScene.State = CSceneState.Normal;
			curScene.OnLoaded();
		}
	}

	public CSceneBase CurScene
	{
		get{
			return curScene;
		}
	}


}

public enum CSceneState
{
	None = 0,
	Loading = 1,
	Normal = 2,
	Disposed = 3,

}
