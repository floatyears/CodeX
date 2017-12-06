using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleItem {

	private bool gameInit = false;

	private GameObject gameObject;

	private CAssetLoaderInfo loaderInfo;

	public bool toBeRemoved = false;

	public int particleID;

	private BaseEntity owningEntity; 

	private string particleName;

	public ParticleItem(string particleName, BaseEntity owningEntity )
	{
		this.particleName = particleName;
		this.owningEntity = owningEntity;
		gameInit = false;

		CAssetsManager.Instance.LoadAssetAsync(particleName,"",(info)=>{
			loaderInfo = info;
			gameObject = info.assetBundle.LoadAsset<GameObject>(particleName);
			gameInit = true;
		});
	}

	public void Update()
	{
		if(gameInit)
		{
			
		}
		else
		{

		}
	}

	public void Destroy()
	{
		if(gameObject != null) GameObject.Destroy(gameObject);
		CAssetsManager.Instance.ReleaseAsset(loaderInfo);
	}
}
