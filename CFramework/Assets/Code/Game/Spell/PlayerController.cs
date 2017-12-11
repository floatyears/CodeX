using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController {

	/* 玩家输入处理 */

	private Vector3 touchPos; 

	private Touch touch1;

	private Touch touch2;

	private Vector3 startPos;

	private Vector3 stationPos;

	//主要用于在touch下模拟点击
	private int elapseFrame = 0;

	/* */
	//服务器的帧
	private int serverFrame;

	//客户端的帧
	private int clientFrame;

	//这是模拟的输入，有部分来自服务器，同时包含了客户端的预测，当预测出错时，需要用来自服务器的最后一帧到玩家当前帧的所有输入重播一遍来进行缓和
	private CircularBuffer<EntityCommand> simulateCommands;

	//玩家输入的commands，跟播放的commands分开是
	private CircularBuffer<EntityCommand> inputCommands;

	public void Init()
	{
		//保存20帧的命令
		inputCommands = new CircularBuffer<EntityCommand>(20);
		simulateCommands = new CircularBuffer<EntityCommand>(20);
	}
	
	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	public void Update()
	{
#if UNITY_STANDALONE || UNITY_EDITOR //在PC平台上操作
		
		// if(Input.GetMouseButtonUp(0)) //UI暂时还没定用uGUI还是NGUI，这里要判断防止点击穿透UI：UICamera.GetMouse(0).pressedCam == self.camera
		// {
		// 	touchPos = Input.mousePosition;
		// }else if(Input.GetMouseButton(0)) //这里是按下了左键还没释放
		// {
			
		// }
		//用键盘操作方向，用鼠标操作点击
		if(Input.anyKey)
		{
			if(Input.GetKeyUp(KeyCode.W))
			{
				inputCommands.Enqueue(new EntityCommand(1, EntityCommandType.Move, 1, 0));
			}else if(Input.GetKeyUp(KeyCode.D))
			{
				inputCommands.Enqueue(new EntityCommand(1, EntityCommandType.Move, 90, 0));
			}else if(Input.GetKeyUp(KeyCode.A))
			{
				inputCommands.Enqueue(new EntityCommand(1, EntityCommandType.Move, 180, 0));
			}else if(Input.GetKeyUp(KeyCode.S))
			{
				inputCommands.Enqueue(new EntityCommand(1, EntityCommandType.Move, 270, 0));
			}
		}
		

		
#elif UNITY_ANDROID || UNITY_IOS	//在移动平台上操作
		int tc = Input.touchCount;
		if(tc == 1)
		{
			touch1 = Input.GetTouch(0);
			if(touch1.phase == TouchPhase.Began)
			{
				startPos = touch1.position;
				elapseFrame = 1;
			}else
			if(touch1.phase == TouchPhase.Stationary || touch1.phase == TouchPhase.Moved)
			{
				elapseFrame++;
				stationPos = touch1.position;

				//if(stationPos - startPos)
				
			}else if(touch1.phase == TouchPhase.Ended)
			{
				if(elapseFrame < 3) //点击时间太短，视为点击
				{

				}
				elapseFrame = 0;
			}
		}else if(tc == 2) //两点操作
		{
			
		}
#endif
	}
}
