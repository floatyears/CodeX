using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using FlatBuffers;

//封装对收到的字节的处理，把最终处理得到的协议缓冲队列返回到CNetwork中。
public class CSocket {

	private IPAddress ip;

	private int port;

	private Socket socket;

	//用于缓存的数据
	private byte[] buffer;

	private AsyncCallback callback;

	//限制字节的长度，防止过大的字节数
	private int bufferLimit = 1024;

	private int streamLimit = 102400;

	//双缓冲：这里做了一个基本假设，单个协议长度不会超过
	private byte[][] readStreams;

	//当前使用的流索引
	private int curMemIndex;

	//当前的流的位置
	private long curMemPos;

	//缓存的协议数量，环形缓冲
	private CircularBuffer<ByteBuffer> msgCache;

	//缓冲的当前位置
	private int cachePos;
	
	//专门读取长度的字节
	private byte[] lenByts;

	
	public void Init()
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		buffer = new byte[bufferLimit];

		readStreams = new byte[][]{new byte[streamLimit],new byte[streamLimit]};
		curMemIndex = 0;
		curMemPos = 0;
		msgCache = new CircularBuffer<ByteBuffer>(30);
		lenByts = new byte[4];
		callback = ReceiveCallback;
	}

	public void Connect(string ip, int port)
	{
		this.ip = IPAddress.Parse(ip);
		this.port = port;
		socket.Connect(ip, port);
	}

	public void BeginReceive()
	{
		socket.BeginReceive(buffer, 0, bufferLimit, SocketFlags.None, callback, null);
	}

	private void ReceiveCallback(IAsyncResult ar)
	{
		//接收到的数据长度
		int count = socket.EndReceive(ar);

		if(count > 0 )
		{
			//在可操作的范围内
			if(curMemPos + count < streamLimit)
			{
				Array.Copy(buffer, 0, readStreams[curMemIndex], curMemPos, count);
			}
			else
			{
				Array.Copy(buffer, 0, readStreams[curMemIndex], curMemPos, streamLimit - curMemPos);
				//如果第一个流被用完，那么切换到下一个流
				int tmpIdx = curMemIndex == 0 ? 1 : 0;
				Array.Copy(buffer, streamLimit - curMemPos, readStreams[tmpIdx], curMemPos, count - (streamLimit - curMemPos));
				
			}
			ProcessData();

			//继续接收消息
			BeginReceive();
		}else
		{
			CLog.Error("socket receive length is zero, close socket");
			socket.Close();
		}

	}

	private void ProcessData()
	{
		Array.Copy(readStreams[curMemIndex], curMemPos, lenByts, 0, 4);
		curMemPos = curMemPos + 4;
		//读出的协议的长度，
		long msgLen = BitConverter.ToInt64(lenByts, 0);
		
		var tmp = new byte[msgLen];
		//当前流包含了完整的协议信息
		
		//TODO:一下代码做了一个假设，当在一个缓冲中没有读取完的消息，在下一个缓冲中一定能读取成功，可能在某些极限情况下会出现bug
		if(msgLen + curMemPos < streamLimit)
		{
			Array.Copy(readStreams[curMemIndex], curMemPos, tmp,0, msgLen);
			//设置当前的字节位置
			curMemPos = curMemPos + msgLen;

			//TODO:如果单次处理的协议过多，可能会导致环形缓冲溢出，这里也需要注意
			msgCache.Enqueue(new ByteBuffer(tmp,4));

		}
		else
		{
			//复制当前缓冲中剩余的字节
			Array.Copy(readStreams[curMemIndex], curMemPos, tmp, 0, streamLimit - curMemPos);
			//切换到另一个字节数组
			curMemIndex = curMemIndex == 0 ? 1 : 0;
			msgLen = msgLen - (streamLimit - curMemPos);
			Array.Copy(readStreams[curMemIndex], 0, tmp, streamLimit - curMemPos, msgLen);
			curMemPos = msgLen;

			msgCache.Enqueue(new ByteBuffer(tmp,4));
			
		}

	}

	//发送请求
	public void SendMsg(byte[] bytes, Action callback)
	{
		if(socket.Connected)
		{
			socket.BeginSend(bytes,0,bytes.Length,SocketFlags.None,(ar)=>{
				int write = this.socket.EndSend(ar);

				if(write != 0 && callback != null)
				{
					callback();
				}
			},null);
		}else
		{
			CLog.Error("socket断开，数据发送失败");
		}
		
	}

	//获取一条待处理的消息
	public ByteBuffer GetMsg()
	{
		return msgCache.Dequeue();
	}

	public bool HasCacheMsg
	{
		get
		{
			return !msgCache.IsEmpty;
		}
	}

	public bool IsConnected
	{
		get{
			return socket.Connected;
		}
	}


	public void Reconnect()
	{
		if(ip != null && !IsConnected)
		{
			socket.Connect(ip, port);
		}
	}


	public void Disconnect()
	{
		socket.Disconnect(true);
	}

	public void Dipose()
	{
		var tmp = socket;
		socket = null;
		tmp.Close();
	}
}
