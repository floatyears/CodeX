using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//基础模型
public class BaseModel {

	private GameObject _mainObj;

    private Transform _mainTrans;

    private Rigidbody agent;

    private BoxCollider collider;

    private int state;

    public BaseModel()
    {
        state = 0;
    }

    public void Init(int sourceID = 0)
    {
        state = 1;
        _mainObj = new GameObject("Model");
        _mainTrans = _mainObj.transform;

        //物理碰撞
        agent = _mainObj.AddComponent<Rigidbody>();
        collider = _mainObj.AddComponent<BoxCollider>();

        //刷新模型资源
        if (sourceID > 0){
            RefreshModel(sourceID);
        }
        var obj = GameObject.Instantiate(Resources.Load("character")) as GameObject;
        obj.transform.parent = _mainTrans;
    }

    //刷新模型
    public void RefreshModel(int sourceID, params object[] args)
    {
        state = 2;

    }

    public Vector3 position{
        set{
            _mainTrans.position = value;
        }
        get{
            return _mainTrans.position;
        }
    }

    public void Move(Vector3 pos){
        agent.MovePosition(agent.position + pos);
    }

    public void Dispose()
    {
        agent = null;
        GameObject.Destroy(_mainObj);
    }
	
}
