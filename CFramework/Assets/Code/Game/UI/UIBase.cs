using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUIBase {

	private CUIState state;

	public CUIState State{
		get{
			return state;
		}
	}

	private int uiKey;

	private GameObject uiObject;

	private CTableUI data;

	private CAssetLoaderInfo loaderInfo;

	public CUIBase(int uikey)
	{
		this.uiKey = uikey;

		data = CDatabase.Instance.GetData<CTableUI>(this.uiKey);
		// CAssetsManager.Instance.LoadAssetAsync(data.name,data.resPath,(info)=>{
		// 	loaderInfo = info;
		// 	uiObject = loaderInfo.assetBundle.LoadAsset<GameObject>(data.name);
		// 	state = CUIState.Inited;
		// });
	}

	public virtual void Init()
	{
		if(uiObject != null)
		{
			CAssetsManager.Instance.LoadAssetAsync(data.name,data.resPath,(info)=>{
				loaderInfo = info;
				uiObject = loaderInfo.assetBundle.LoadAsset<GameObject>(data.name);
				state = CUIState.Inited;
			});
		}
	}

	public virtual void Show()
	{
		
	}

	public virtual void Refresh()
	{
		
	}

	public virtual void Close()
	{

	}

	public virtual void Dispose()
	{
		CAssetsManager.Instance.ReleaseAsset(loaderInfo);
		GameObject.Destroy(uiObject);
		state = CUIState.None;
	} 
}

public enum CUIState
{
	None = 0,

	Inited = 0x1,

	Show = 0x3,
}