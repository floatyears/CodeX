using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CDatabase : CModule {

	//数据模块需要全局访问，这里设计成单例
	private static CDatabase instance;

	private Dictionary<Type, CDataTable> tables;

	
	public static CDatabase Instance{
		get{
			if(instance == null) throw new UnityException("CDatabase is not inited!");
			return instance;
		}
	}

	//初始化
	public override void Init()
	{
		if(instance == null) 
		{
			instance = this;
		}else
		{
			throw new UnityException("CDatabase is not inited!");
		}
		tables = new Dictionary<Type, CDataTable>();
		needUpdate = false;
	}

	public T GetData<T>(int key) where T : CTableBase
	{
		return tables[typeof(T)].GetData<T>(key);
	}

	public void AddData<T>(CDataTable data) where T : CTableBase
	{
		if(tables.ContainsKey(typeof(T))){
			tables[typeof(T)] = data;
		}else{
			tables.Add(typeof(T), data);
		}
	}

	public override void Dispose()
	{
		instance = null;
	}
	
}

