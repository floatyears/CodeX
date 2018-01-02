using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;

public class CSocketUDP {

	private IPAddress ip;
	
	private int port;

	private Socket recvSocket;

	private Socket sendSocket;

	private byte[] buffer;

	

	private int socketArgsLimit = 2;

	private MsgPacket[] packetBuffer;

	private int curPacket;

	private IPEndPoint remoteEP;

	private IPEndPoint destEP;

	private HuffmanMsg huffmanMsg;

	public void Init()
	{
		recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		
		buffer = new byte[CConstVar.BUFFER_LIMIT];
		remoteEP = new IPEndPoint(IPAddress.Any, 0); //可以接受任何ip和端口的消息
		recvSocket.Bind(remoteEP);

		// IPAddress.IsLoopback(remoteEP.Address);
		packetBuffer = new MsgPacket[2];
		curPacket = 0;

		InitHuffmanMsg();
	}

	public void Dispose()
	{

	}
	
	public void InitHuffmanMsg()
	{
		huffmanMsg = new HuffmanMsg();
		huffmanMsg.decompresser.loc = new HuffmanNode[CConstVar.HUFF_MAX+1];
		huffmanMsg.compresser.loc = new HuffmanNode[CConstVar.HUFF_MAX+1];

		huffmanMsg.decompresser.tree = huffmanMsg.decompresser.lhead = huffmanMsg.decompresser.ltail = 
			huffmanMsg.decompresser.loc[CConstVar.HUFF_MAX] = huffmanMsg.decompresser.nodeList[huffmanMsg.decompresser.blocNode++];

		huffmanMsg.decompresser.tree.symbol = CConstVar.HUFF_MAX;
		huffmanMsg.decompresser.tree.weight = 0;
		huffmanMsg.decompresser.lhead.next = huffmanMsg.decompresser.lhead.prev = null;
		huffmanMsg.decompresser.tree.parent = huffmanMsg.decompresser.tree.left = huffmanMsg.decompresser.tree.right = null;

		huffmanMsg.compresser.tree = huffmanMsg.compresser.lhead = 
			huffmanMsg.decompresser.loc[CConstVar.HUFF_MAX] = huffmanMsg.decompresser.nodeList[huffmanMsg.decompresser.blocNode++];

		huffmanMsg.compresser.tree.symbol = CConstVar.HUFF_MAX;
		huffmanMsg.decompresser.tree.weight = 0;
		huffmanMsg.decompresser.lhead.next = huffmanMsg.decompresser.lhead.prev = null;
		huffmanMsg.decompresser.tree.parent = huffmanMsg.decompresser.tree.left = huffmanMsg.decompresser.tree.right = null;

	}

	public void Connect(string ip, int port)
	{
		this.ip = IPAddress.Parse(ip);
		this.port = port;
		//socket.Connect(new IPEndPoint(this.ip, this.port));
		destEP = new IPEndPoint(this.ip, this.port);
		sendSocket.Bind(destEP);
	}

	public void BeginReceive()
	{
		var tmpEP = remoteEP as EndPoint;
		recvSocket.BeginReceiveFrom(buffer, 0, CConstVar.BUFFER_LIMIT, SocketFlags.None,ref tmpEP, ReceiveCallback, recvSocket);

		// if(!socket.ReceiveFromAsync(socketAsyncEventArgs[socketArgsIndex]))
		// {

		// }
	}

	private void ReceiveCallback(IAsyncResult ar)
	{
		int count = recvSocket.EndReceive(ar);
		if(count > 0)
		{
			var connection = CDataModel.Connection;
			if(connection.state < ConnectionState.CONNECTED)
			{
				return; //网络还没连上，不处理消息
			}
			//随机丢包处理
			if(CConstVar.NET_DROP_SIM > 0 && CConstVar.NET_DROP_SIM < 100 && UnityEngine.Random.Range(1,100) < CConstVar.NET_DROP_SIM)
			{
				//已经丢掉的包，不处理
			}else
			{
				if(remoteEP.Address != connection.NetChan.remoteAddress)
				{
					CLog.Error(string.Format("%s:sequence packet without connection",remoteEP.Address));
				}else{
					var packet = packetBuffer[~curPacket];
					packet.GetData(buffer, count);

					var network = CNetwork.Instance;
					if(network == null) return;
					network.PacketEvent(packet, remoteEP);
				}
			}
			
			if(remoteEP == null) {
				remoteEP = new IPEndPoint(IPAddress.Any, 0);
			}
			else{
				remoteEP.Address = IPAddress.Any;
				remoteEP.Port = 0;
			}
			var ep = remoteEP as EndPoint;
			recvSocket.BeginReceiveFrom(buffer, 0, CConstVar.BUFFER_LIMIT, SocketFlags.None, ref ep, ReceiveCallback, recvSocket);
		}else{
			CLog.Error("udp socket has recevied no byte");
		}
	}

	
}

public class MsgPacket{
	bool allowOverflow;

	bool overflowed;

	bool oob; //out of band(带外数据)

	byte[] bytes;

	int curSize;

	int curPos;

	int bit;

	public MsgPacket()
	{
		this.bytes = new byte[CConstVar.BUFFER_LIMIT];
		this.curPos = 0;
	}

	public void GetData(byte[] data, int length)
	{
		Array.Copy(data, 0, bytes, 0, length);
		curSize = length;
	}

	public void WriteData(byte[] data, int start, int length)
	{
		Array.Copy(bytes, start, data, 0, length);
	}

	public void BeginRead()
	{
		curPos = 0;
		bit = 0;
		oob = false;
	}

	public int CurSize
	{
		get{
			return curSize;
		}
		set{
			curSize = value;
		}
	}

	public int Bit{
		set{
			bit = value;
		}
	}

	public bool Oob{
		set{
			oob = value;
		}
	}

	public byte[] Data{
		get{
			return bytes;
		}
	}

	public int CurPos{
		get{
			return curPos;
		}
		set{
			curPos = value;
		}
	}

	public long ReadLong()
	{
		return 0L;
	}

	public int ReadInt(int start = -1)
	{
		if(start < 0)
		{
			start = curPos;
		}else{

		}
		return 0;
	}

	public int ReadByte()
	{
		return 0;
	}

	public int ReadBits(int bits)
	{
		int value, get, i, nbits;
		bool sgn;

		value = 0;
		if(bits < 0)
		{
			bits = -bits;
			sgn = true;
		}else{
			sgn = false;
		}
		if(oob)
		{
			if(bits == 8)
			{
				value = bytes[curPos];
				curPos++;
				bit += 8;
			}else if(bits == 16)
			{
				short temp;

			}else if(bits == 32)
			{

			}else{
				CLog.Error("can't read %d bits", bits);
			}
		}else{
			nbits = 0;
			if((bits & 7) != 0)
			{
				nbits = bits & 7;
				for(i = 0; i < nbits; i++)
				{
					// value |= (Huff)
				}
				bits = bits - nbits;
			}
			if(bits != 0)
			{
				for(i=0; i < bits; i+= 8)
				{
					// value |= (get << (i + nbits));
				}
			}
		}

		if(sgn)
		{
			if((value & ( 1 << (bits - 1))) != 0){
				value |= -1 ^ ((1 - bits) - 1);
			}
		}
		return value;
	}

	public short ReadShot()
	{
		return 0;
	}

	public void WriteInt(int value, int pos = -1)
	{
		if(pos < 0)
		{
			pos = curPos;
		}

		
	}

}

public enum SVCCmd
{
	BAD = 0,
	NOP,
	GAME_STATE,
	CONFIG_STRING,
	BASELINE,
	SERVER_COMMAND,
	DOWNLOAD,
	SNAPSHOT,
	EOF,
}

