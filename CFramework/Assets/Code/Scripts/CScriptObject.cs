using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//把lua脚本与gameObject进行关联的类
public class CScriptObject {
	
	private LuaInterface.LuaTable luaTable;

	private GameObject gameObject;

	private Transform transform;

	private LuaGameObjectEventDispatcher eventDispatcher;

	//缓存调用的cache，避免对Transform的重复调用
	private Dictionary<string, GameObject> cacheObjs;

	//缓存调用的cache，避免对GameObject的重复调用, cache的内容也会随着当前gameObject被销毁而销毁
	private Dictionary<string, Transform> cacheTrans;
	

	public void Link(GameObject obj, LuaInterface.LuaTable table)
	{
		this.gameObject = obj;
		this.eventDispatcher = this.gameObject.GetComponent<LuaGameObjectEventDispatcher>();
		if(this.eventDispatcher == null)
		{
			this.eventDispatcher = this.gameObject.AddComponent<LuaGameObjectEventDispatcher>();
		}
		this.transform = this.gameObject.transform;
		luaTable = table;
		this.eventDispatcher.AddEvent(OnEvent);

		SetLuaValue("CSharpObject",this); //lua可以通过CSharpObject直接访问到当前的对象
	}

	public void UnLink(CScriptObject scriptObj)
	{
		transform = null;
		luaTable = null;	
		cacheObjs.Clear();
		cacheObjs = null;
		cacheTrans.Clear();
		cacheTrans = null;
		this.eventDispatcher.RemoveEvent(OnEvent);
		UnityEngine.Object.Destroy(this.eventDispatcher);
	}

	public void Dispose()
	{
		transform = null;
		luaTable = null;	
		cacheObjs.Clear();
		cacheObjs = null;
		cacheTrans.Clear();
		cacheTrans = null;

		this.eventDispatcher.RemoveEvent(OnEvent);
		
		GameObject.Destroy(gameObject);
	}

	private void OnEvent(int eventType)
	{
		switch(eventType)
		{
			case 1:
				CallMethod("Start");
				break;
			case 2:
				CallMethod("Awake");
				break;
			case 3:
				CallMethod("OnEnable");
				break;
			case 4:
				CallMethod("OnDisable");
				break;
			case 5:
				CallMethod("OnDestroy");
				break;
		}
	}

	/*----------- lua调用C#的接口 ----------*/
	public Component getComponent(string objPath, string typeName)
	{
		var trans = transform.Find(objPath);
		if(trans == null)
		{
			CLog.Info("transform is null");
			return null;
		}
		return trans.GetComponent(typeName);
	}

	//常用的赋值操作进行缓存
	// public void SetTransformAttr(int id, int attrType,  float x, float y, float z)
	// {
	// 	switch(attrType)
	// 	{
	// 		case 1: //position

	// 			break;
	// 		case 2: //local position

	// 			break;
	// 		case 3: //欧拉角旋转
				
	// 			break;
	// 		case 4: //欧拉角旋转（local）

	// 			break;
	// 		case 5://缩放
				
	// 			break;
	// 	}
	// }

	// //
	// public void GetTransformAttr(int id, int attrType, out float x, out float y, out float z)
	// {
	// 	x = 0; y = 0; z = 0;
	// 	switch(attrType)
	// 	{
	// 		case 1: //position

	// 			break;
	// 		case 2: //欧拉角旋转

	// 			break;
	// 		case 3:
				
	// 			break;
	// 	}
	// }

	public void SetTransformAttr(string objPath, int attrType,  float x, float y, float z)
	{
		Transform trans;
		bool isAdd = false;
		if(!cacheTrans.TryGetValue(objPath, out trans))
		{
			trans = transform.Find(objPath);
			isAdd = true;
		}
		if(trans != null)
		{
			if(isAdd)
			{
				cacheTrans.Add(objPath, trans);
			}

			switch(attrType)
			{
				case 1: //position
					trans.position = new Vector3(x,y,z);
					break;
				case 2: //local position
					trans.localPosition = new Vector3(x,y,z);
					break;
				case 3: //欧拉角旋转
					trans.eulerAngles = new Vector3(x,y,z);
					break;
				case 4: //欧拉角旋转（local）
					trans.localEulerAngles = new Vector3(x,y,z);
					break;
				case 5://缩放
					trans.localScale = new Vector3(x,y,z);
					break;
			}
		}
		
	}

	//
	public void GetTransformAttr(string objPath, int attrType, out float x, out float y, out float z)
	{
		x = 0f; y = 0f; z = 0f;
		Transform trans;
		bool isAdd = false;
		if(!cacheTrans.TryGetValue(objPath, out trans))
		{
			trans = transform.Find(objPath);
			isAdd = true;
		}
		if(trans != null)
		{
			if(isAdd)
			{
				cacheTrans.Add(objPath, trans);
			}
			

			switch(attrType)
			{
				case 1: //position
					var pos = trans.position;
					x = pos.x;
					y = pos.y;
					z = pos.z;
					break;
				case 2: //local position
					var pos1 = trans.localPosition;
					x = pos1.x;
					y = pos1.y;
					z = pos1.z;
					break;
				case 3: //欧拉角旋转
					var angle = trans.eulerAngles;
					x = angle.x;
					y = angle.y;
					z = angle.z;
					break;
				case 4: //欧拉角旋转（local）
					var angle1 = trans.localEulerAngles;
					x = angle1.x;
					y = angle1.y;
					z = angle1.z;
					break;
				case 5://缩放
					var scale = trans.localScale;
					x = scale.x;
					y = scale.y;
					z = scale.z;
					break;
			}
		}
		
	}

	/*----------- C#调用lua的接口 ----------*/
	public object[] CallMethod(string methodName, params object[] args)
	{
		if(luaTable != null)
		{
			var func = luaTable.GetLuaFunction(methodName);
			if(func != null)
			{
				return func.Call(args);
			}
		}
		return null;
	}

	public object GetLuaValue(string name)
	{
		if(luaTable != null)
		{
			return luaTable[name];
		}
		return null;
	}

	public void SetLuaValue(string name, object val)
	{
		if(luaTable != null)
		{
			luaTable[name] = val;
		}
	}

	public LuaInterface.LuaTable GetLuaTable()
	{
		return luaTable;
	}

	public LuaInterface.LuaFunction GetLuaFunction(string funcName)
	{
		if(luaTable != null)
		{
			return luaTable.GetLuaFunction(funcName);
		}
		return null;
	}



	
}

public enum TransformAttrType
{
	POSITION = 1,
	POSITION_LOCAL,
	ROTATION_EULER,
	ROTATION_EULER_LOCAL,
	SCALE_LOCAL,

}

//不提供update的事件，Update直接在Lua中定义
public class LuaGameObjectEventDispatcher : MonoBehaviour
{
	public delegate void EventDispatch(int eventType);
	
	private EventDispatch dispatcher;

	public void AddEvent(EventDispatch callback)
	{
		if(dispatcher == null)
		{
			dispatcher = new EventDispatch(callback);
		}
		dispatcher += callback;
	}

	public void RemoveEvent(EventDispatch callback)
	{
		if(dispatcher != null)
		{
			dispatcher -= callback;
		}
	}

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Start()
	{
		dispatcher(1);
	}

	void Awake()
	{
		dispatcher(2);
	}

	/// <summary>
	/// This function is called when the object becomes enabled and active.
	/// </summary>
	void OnEnable()
	{
		dispatcher(3);
	}

	/// <summary>
	/// This function is called when the behaviour becomes disabled or inactive.
	/// </summary>
	void OnDisable()
	{
		dispatcher(4);
	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	void OnDestroy()
	{
		dispatcher(5);
	}


}