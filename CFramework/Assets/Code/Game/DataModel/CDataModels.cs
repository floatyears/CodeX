using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class CDataModel : CModule {

	//场景模块
	private static CModelScene scene;

	public static CModelScene Scene
	{
		get{
			return scene;
		}
	}

	private static CModelPlayer player;

	public static CModelPlayer Player
	{
		get{
			return player;
		}
	}

	private static CModelConnection connection;

	public static CModelConnection Connection
	{
		get{
			return connection;
		}
	}

	private static CModelGameState gameState;

	public static CModelGameState GameState
	{
		get{
			return gameState;
		}
	} 

	private static CModelInputEvent inputEvent;

	public static CModelInputEvent InputEvent
	{
		get{
			return inputEvent;
		}
	}

	private static CModelCMD cmdBuffer;

	public static CModelCMD CmdBuffer
	{
		get{
			return cmdBuffer;
		}
	}

	private static CDataModel instance;

	public static CDataModel Instance
	{
		get{
			return instance;
		}
	}


	private Dictionary<Type, List<Callback>> msgRegister;

	private List<CModelBase> models;

	private List<CModelBase.UpdateHandler> updateList;


	public override void Init()
	{
		instance = this;
		models = new List<CModelBase>();
		updateList = new List<CModelBase.UpdateHandler>();
		msgRegister = new Dictionary<Type, List<Callback>>();

		AddModel<CModelScene>(out scene);
		AddModel<CModelPlayer>(out player);
		AddModel<CModelConnection>(out connection);
		AddModel<CModelGameState>(out gameState);
		AddModel<CModelInputEvent>(out inputEvent);
		AddModel<CModelCMD>(out cmdBuffer);
	}
	
	private void AddModel<T>(out T instance) where T : CModelBase, new()
	{
		instance = new T();
		models.Add(instance);

		//自动注册所有的消息监听函数
		foreach(var method in typeof(T).GetMethods())
		{
			if(method.Name.StartsWith("OnSc"))
			{
				//获取到泛型
				var args = method.GetParameters()[1].ParameterType;

				List<Callback> calls;
				if(!msgRegister.TryGetValue(args, out calls))
				{
					calls = new List<Callback>();
					msgRegister.Add(args, calls);
				}
				calls.Add(new Callback(method, instance));
			}
		}
		instance.Init();

		if(instance.update != null && !updateList.Contains(instance.update)) updateList.Add(instance.update);
	}

	public void DispatchMessage(FlatBuffers.ByteBuffer buffer)
	{
		var type = CMessageID.GetMsgType(GetMsgID(buffer));
		List<Callback> calls;
		if(msgRegister.TryGetValue(type, out calls))
		{
			int len = calls.Count;
			for(int i = 0; i < len; i++)
			{
				calls[i].method.Invoke(calls[i].instance, new object[]{ buffer });
			}
		}
		
	}

	private int GetMsgID(FlatBuffers.ByteBuffer bb)
	{
		int bb_pos = bb.GetInt(0);
		int vtable = bb_pos - bb.GetInt(bb_pos);
		var o = 4 < bb.GetShort(vtable) ? (int)bb.GetShort(vtable + 4) : 0;
		return bb.GetInt(o + bb_pos);
	}

	public override void Update(){
		int len = updateList.Count;
		for(int i = 0; i < len; i++){
			updateList[i].Invoke();
		}

		CNetwork.Instance.FlushPacketQueue();
	}
	
	

	public override void Dispose()
	{
		int len = models.Count;
		for(int i = 0; i < len; i++)
		{
			models[i].Dispose();
		}

		var iterator = msgRegister.GetEnumerator();
		while(iterator.MoveNext())
		{
			iterator.Current.Value.Clear();
		}
		msgRegister.Clear();
		updateList.Clear();
		instance = null;
	}
}

public class Callback{
	public MethodInfo method;
	public object instance;

	public Callback(MethodInfo method, object instance)
	{
		this.method = method;
		this.instance = instance;
	}
}

