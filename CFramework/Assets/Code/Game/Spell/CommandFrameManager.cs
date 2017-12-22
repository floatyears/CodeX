using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
丢包的处理：服务器会保存一个未模拟输入的缓冲区（收到了客户端消息还没处理），这个缓冲区尽量小，缓冲区如果空了，
服务器会根据最后一次输入去“猜测”，等到真正的输入达到时，又会试着“和解”，确保不会丢掉任何操作。一旦服务器意识到丢包，
就会通知客户端，服务器会使用之前的包来进行预测，然后一遍通知客户端，客户端会加快执行的速度（发送给服务器的帧越来越快），
服务器上的缓冲区也会跟着变大，这样来度过丢包的难关。一旦恢复了正常，客户端就会以更慢的速度发包，同时服务器减小缓冲区的尺寸。
如果持续发生丢包，就可能超过承受极限，这时通过输入冗余来使预测错误最小化。把最后一次被服务器确认的运动状态到现在的全部输入都
发送过去，比如确认的是第4帧，而模拟到了19帧，那么把第4帧到第19帧的所有输入打包发给服务器。这样即使丢包，下一个达到的包
依然有全部的输入操作。
 */
//这里需要处理服务器丢包的情况
public class CommandFrameManager : CModule {

	//记录了服务器返回的帧数据
	private Dictionary<int, List<EntityCommand>> commandFrames;

	private	List<EntityCommand> exeCommand;

	private static CommandFrameManager instance;

	//服务器的帧数
	private int serverFrame;

	public static CommandFrameManager Instance{
		get{
			return instance;
		}
	}

	public override void Init()
	{
		instance = this;

	}

	public void SimulateFrame(int frame, List<EntityCommand> commands)
	{

	}
	
}
