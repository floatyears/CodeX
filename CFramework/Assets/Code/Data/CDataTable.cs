using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CDataTable {

	public delegate void ForeachFunc(int index, CTableBase data);
	//
	public int id;

	//
	public string name;


	//用来对数据进行随机读取
	private Dictionary<int, CTableBase> dicData;

	//主要用来对数据进行遍历
	private List<CTableBase> listData;
	//public List<KeyValuePair> values;

	public void Init()
	{
		dicData = new Dictionary<int, CTableBase>();
		listData = new List<CTableBase>();
		
	}

	public T GetData<T>(int key) where T : CTableBase
	{
		CTableBase data;
		if(dicData.TryGetValue(key, out data))
		{
			return data as T;
		}
		return null;
	}

	private void Foreach(ForeachFunc func)
	{
		
	}
}

public class CTableBase
{
	public int id;

	public virtual void Init()
	{

	}
}

public class CTableScene : CTableBase
{
	

	//
	public string name;
	
	//资源所在的路径
	public string resPath;

	public override void Init()
	{
		
	}
}

public class CTableUI : CTableBase 
{
	public string name;

	public string resPath;

	public string uiScript;
}
