using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class CSceneBase {

	protected string name;

	protected Transform camTrans;

	private ClientActive clientActive;

	protected MapData map;

	//第一个entity表示当前帧的player state
	protected BaseEntity[] entsList;

	protected BaseEntity[] cachedEnts;

	protected int curCachePos;

	public string Name{
		get{
			return name;
		}
	}

	protected CSceneState state;

	public CSceneState State{
		get{
			return state;
		}
		set{
			state = value;
		}
	}

	public virtual void Init()
	{
		state = CSceneState.Inited;

		//排除模拟层
		Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Simulate"));
		camTrans = Camera.main.transform.parent;
		//包含了player state在里面
		entsList = new BaseEntity[CConstVar.MAX_ENTITIES_IN_SNAPSHOT+1];
		cachedEnts = new BaseEntity[CConstVar.MAX_ENTITIES_IN_SNAPSHOT+1];
		curCachePos = -1;
	}

	public virtual void OnLoaded()
	{
		clientActive = CDataModel.GameState.ClActive;
		map = new MapData();
		map.Init();
		//TODO：临时这样写
		var root = typeof(MapData).GetField("mapRoot", BindingFlags.NonPublic | BindingFlags.Instance);
		CUtils.SetLayer((root.GetValue(map) as GameObject), LayerMask.NameToLayer("Default"));
	}

	public virtual void Update()
	{
		if(entsList[0] == null){
			entsList[0] = new BaseEntity();
			entsList[0].Init();
			CUtils.SetLayer(typeof(BaseModel).GetField("_mainObj",BindingFlags.NonPublic | BindingFlags.Instance).GetValue(entsList[0].Model) as GameObject, LayerMask.NameToLayer("Default")); 
		}

		entsList[0].AttachData(CDataModel.GameState.ClActive.snap.playerState);
		entsList[0].Update(Time.deltaTime);

		int i = 0;
		for(; i < clientActive.snap.numEntities; i++){ //表示当前帧的entity
			int idx = clientActive.snap.parseEntitiesIndex + i;
			var ent = clientActive.parseEntities[idx & (CConstVar.MAX_PARSE_ENTITIES - 1)];
			
			if(entsList[i+1] == null){
				if(curCachePos >= 0){
					entsList[i+1] = cachedEnts[curCachePos];
					entsList[i+1].Init(); //重新初始化
					CUtils.SetLayer(typeof(BaseModel).GetField("_mainObj",BindingFlags.NonPublic | BindingFlags.Instance).GetValue(entsList[i+1].Model) as GameObject, LayerMask.NameToLayer("Default")); 
					curCachePos--;
				}else{
					entsList[i+1] = new BaseEntity();
					entsList[i+1].Init();
					CUtils.SetLayer(typeof(BaseModel).GetField("_mainObj",BindingFlags.NonPublic | BindingFlags.Instance).GetValue(entsList[i+1].Model) as GameObject, LayerMask.NameToLayer("Default")); 
				}
			}
			entsList[i+1].AttachData(ent);
			entsList[i+1].Update(Time.deltaTime);
			
		}

		//把没有用到的缓存起来
		for(; i < CConstVar.MAX_PARSE_ENTITIES; i++){
			if(entsList[i+1] == null){
				break;
			}
			cachedEnts[++curCachePos] = entsList[i+1];
			entsList[i+1].Clear();
			entsList[i+1] = null;
		}


		// clientActive.snap.parseEntitiesIndex
		// clientActive.parseEntities[clientActive.snap.parseEntitiesIndex + clientActive.snap.numEntities]
	}

	public virtual void Dispose()
	{
		//data = null;
		state = CSceneState.Deactive;
	}
}
