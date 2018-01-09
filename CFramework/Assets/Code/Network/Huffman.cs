using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuffmanMsg{

	public static HuffmanTree compresser;

	public static HuffmanTree decompresser;

	public static int bloc = 0;

	private static bool inited = false;

	public static void Init()
	{
		if(inited) return;
		inited = true;
		// huffmanMsg = new HuffmanMsg();
		var a = HuffmanMsg.decompresser.loc = new HuffmanNode[CConstVar.HUFF_MAX+1];
		var b = HuffmanMsg.compresser.loc = new HuffmanNode[CConstVar.HUFF_MAX+1];
		for(int i = 0; i < CConstVar.HUFF_MAX+1; i ++){
			a[i] = new HuffmanNode();
			b[i] = new HuffmanNode();
		}

		a = HuffmanMsg.decompresser.nodeList = new HuffmanNode[768];
		b = HuffmanMsg.compresser.nodeList = new HuffmanNode[768];
		for(int i = 0; i < 768; i ++){
			a[i] = new HuffmanNode();
			b[i] = new HuffmanNode();
		}
		
		// huffmanMsg.compresser.loc = new HuffmanNode[768];

		HuffmanMsg.decompresser.tree = HuffmanMsg.decompresser.lhead = HuffmanMsg.decompresser.ltail = 
			HuffmanMsg.decompresser.loc[CConstVar.HUFF_MAX] = HuffmanMsg.decompresser.nodeList[HuffmanMsg.decompresser.blocNode++];

		HuffmanMsg.decompresser.tree.symbol = CConstVar.HUFF_MAX;
		HuffmanMsg.decompresser.tree.weight = 0;
		HuffmanMsg.decompresser.lhead.next = HuffmanMsg.decompresser.lhead.prev = null;
		HuffmanMsg.decompresser.tree.parent = HuffmanMsg.decompresser.tree.left = HuffmanMsg.decompresser.tree.right = null;

		HuffmanMsg.compresser.tree = HuffmanMsg.compresser.lhead = 
			HuffmanMsg.decompresser.loc[CConstVar.HUFF_MAX] = HuffmanMsg.decompresser.nodeList[HuffmanMsg.decompresser.blocNode++];

		HuffmanMsg.compresser.tree.symbol = CConstVar.HUFF_MAX;
		HuffmanMsg.decompresser.tree.weight = 0;
		HuffmanMsg.decompresser.lhead.next = HuffmanMsg.decompresser.lhead.prev = null;
		HuffmanMsg.decompresser.tree.parent = HuffmanMsg.decompresser.tree.left = HuffmanMsg.decompresser.tree.right = null;

	}

	public static void Compress(MsgPacket packet, int offset)
	{

	}

	public static void Decompress(MsgPacket packet, int offset)
	{

	}

	public static int GetBit(byte[] fin, ref int offset){
		int t;
		bloc = offset;
		t = (fin[bloc >> 3] >> (bloc & 7)) & 0x1;
		bloc ++;
		offset = bloc;
		return t;
	}

	public static void PutBit(int bit, byte[] fout, ref int offset){
		bloc = offset;
		if((bloc & 7) == 0){
			fout[(bloc >> 3)] = 0;
		}
		fout[(bloc >> 3)] |= (byte)(bit << (bloc & 7));
		bloc ++;
		offset = bloc;
	}

	private static int GetBit(byte[] fin){
		int t = (fin[bloc >> 3] >> (bloc & 7)) & 0x1;
		bloc++;
		return t;
	}

	public static void OffsetReceive(HuffmanNode node, ref int ch, byte[] fin, ref int offset){
		bloc = offset;
		while(node != null && node.symbol == CConstVar.HUFF_INTERNAL_NODE){
			if(GetBit(fin) != 0){
				node = node.right;
			}else{
				node = node.left;
			}
		}

		if(node == null){
			ch = 0;
			return;
		}
		ch = node.symbol;
		offset = bloc;
	}

	public static void OffsetTransmit(HuffmanTree huff, int ch, byte[] fout, int offset){
		bloc = offset;
		Send(huff.loc[ch], null, fout);
		offset = bloc;
	}

	private static void Send(HuffmanNode node, HuffmanNode child, byte[] fout){
		if(node.parent != null){
			Send(node.parent, node, fout);
		}
		if(child != null){
			if(node.right == child){
				AddBit((char)1, fout);
			}else{
				AddBit((char)0, fout);
			}
		}
	}

	private static void AddBit(char bit, byte[] fout){
		if((bloc & 7) == 0){
			fout[bloc>>3] = 0;
		}
		fout[bloc>>3] |= (byte)(bit << (bloc & 7));
		bloc++;
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