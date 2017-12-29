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

		packetBuffer = new MsgPacket[2];
		curPacket = 0;

		InitHuffmanMsg();
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
			if(CDataModel.Connection.state < ConnectionState.CONNECTED)
			{
				return; //网络还没连上，不处理消息
			}
			//随机丢包处理
			if(CConstVar.NET_DROP_SIM > 0 && CConstVar.NET_DROP_SIM < 100 && UnityEngine.Random.Range(1,100) < CConstVar.NET_DROP_SIM)
			{
				//已经丢掉的包，不处理
			}else
			{
				if(remoteEP.Address != CDataModel.Connection.NetChan.remoteAddress)
				{
					CLog.Error(string.Format("%s:sequence packet without connection",remoteEP.Address));
				}else{
					var packet = packetBuffer[~curPacket];
					packet.GetData(buffer, count);
					if(CDataModel.Connection.ServerRunning){
						SV_PacketPrcess(packet, remoteEP);
					}else{
						PacketProcess(packet, remoteEP);
					}
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

	private bool PacketProcess(MsgPacket packet, IPEndPoint remote)
	{
		bool fragmented = false;
		int fragmentStart = 0;
		int fragmentLength = 0;
		packet.BeginRead();
		int sequence = packet.ReadInt();

		//check for fragment infomation
		if((sequence & CConstVar.FRAGMENT_BIT) != 0 )
		{
			sequence &= ~CConstVar.FRAGMENT_BIT;
			fragmented = true;
		}else{
			fragmented = false;
		}

		var netChan = CDataModel.Connection.NetChan;

		//read qport if we are a server
		if(netChan.src == NetSrc.SERVER)
		{
			packet.ReadShot(); //
		}

		int checkSum = packet.ReadInt();

		//UDP spoofing protection
		if(CheckSum(netChan.challenge, checkSum) != checkSum)
			return false;

		//read the fragment infomation
		if(fragmented)
		{
			fragmentStart = packet.ReadShot();
			fragmentLength = packet.ReadShot();
		}else
		{
			fragmentStart = 0;
			fragmentLength = 0;
		}

		if(CConstVar.ShowPacket)
		{
			if(fragmented)
			{
				CLog.Info("%s recv %d : s=%d fragment=%d,%d", netChan.src, packet.CurSize, sequence, fragmentStart, fragmentLength);

			}else
			{
				CLog.Info("%s recv %d : s=%d", netChan.remoteAddress, packet.CurSize, sequence);
			}
		}
		
		//丢掉乱序或者重复的packets
		if(sequence <= netChan.incomingSequence)
		{
			if(CConstVar.ShowPacket)
			{
				CLog.Error("%s:Out of order packet %d at %d", netChan.remoteAddress, netChan.dropped, sequence);
			}
		}

		//如果当前是可靠消息的最后一个fragment
		//就取得incomming_reliable_sequence
		if(fragmented)
		{
			//确保以正确的顺序添加fragment，可能有packet被丢弃，或者过早地收到了这个packet
			//不会重新构造这个fragment，会等这个packet再次达到
			if(sequence != netChan.fragementSequence){
				netChan.fragementSequence = sequence;
				netChan.fragmentLength = 0;
			}

			//如果有fragment丢失，打印出日志
			if(fragmentStart != netChan.fragmentLength)
			{
				if(CConstVar.ShowPacket)
				{
					CLog.Info("%s:Dropped a message fragment", netChan.remoteAddress);
				}
				return false;
			}

			//复制fragment到fragment buffer
			if(fragmentLength < 0 || packet.CurPos + fragmentLength > packet.CurSize || netChan.fragmentLength + fragmentLength > netChan.fragmentBuffer.Length)
			{
				if(CConstVar.ShowPacket)
				{
					CLog.Info("%s:illegal fragment length", netChan.remoteAddress);
				}
				return false;
			}
			netChan.WriteFragment(packet.Data, packet.CurPos, fragmentLength);

			//如果这不是最后一个fragment，就不处理任何事情(处于中间的fragment的长度是FRAGMENT_SIZE)
			if(fragmentLength == CConstVar.FRAGMENT_SIZE)
			{
				return false;
			}

			//
			if(netChan.fragmentLength > packet.Data.Length)
			{
				if(CConstVar.ShowPacket)
				{
					CLog.Info("$s:fragmentLength %d > packet.Data Length", netChan.remoteAddress, netChan.fragmentLength);
				}
				return false;
			}

			//保证前面的还是sequence
			packet.WriteInt(sequence, 0);
			packet.WriteData(netChan.fragmentBuffer, 4, netChan.fragmentLength);

			packet.CurSize = netChan.fragmentLength + 4;

		}
	
		return true;
	}

	private void SV_PacketPrcess(MsgPacket packet, IPEndPoint remote)
	{

	}

	public void Send()
	{
		// socket.SendTo();
	}

	public static int CheckSum(int challenge, int sequence)
	{
		return challenge ^(sequence * challenge);
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

	public byte[] Data{
		get{
			return bytes;
		}
	}

	public int CurPos{
		get{
			return curPos;
		}
	}

	public long ReadLong()
	{
		return 0L;
	}

	public int ReadInt()
	{
		return 0;
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

public class HuffmanMsg{

	public HuffmanTree compresser;

	public HuffmanTree decompresser;

}

public struct HuffmanTree
{
	public int blocNode;

	public int blocPtrs;

	public HuffmanNode tree;

	public HuffmanNode lhead;

	public HuffmanNode ltail;

	public HuffmanNode[] loc;

	public HuffmanNode freeList;

	public HuffmanNode[] nodeList;

}

//定义为class可以指向引用地址
public class HuffmanNode
{
	public int weight;

	public int symbol;

	public HuffmanNode left;

	public HuffmanNode right;

	public HuffmanNode parent;
	
	public HuffmanNode next;

	public HuffmanNode prev;

	public HuffmanNode head;

}