using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController {

	private EntityCommand
	
	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	public void Update()
	{
#if UNITY_WIN || UNITY_EDITOR || UNITY_MAC //在PC平台上操作
		int tc = Input.touchCount;
		if(tc == 1)
		{

		}else if(tc == 2) //两点操作
		{

		}
#else	//在移动平台上操作
		
#endif
	}
}
