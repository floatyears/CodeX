using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuffmanMsg{

	public HuffmanTree compresser;

	public HuffmanTree decompresser;

	public static void OffsetReceive(HuffmanNode node, int ch, byte fin, int offset)
	{

	}

	public static void Compress(MsgPacket packet, int offset)
	{

	}

	public static void Decompress(MsgPacket packet, int offset)
	{

	}
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

	// public HuffmanTree(int size){

	// }

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

	public HuffmanNode(){
		weight = 0;
		symbol = 0;
	}

}