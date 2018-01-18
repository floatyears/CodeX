using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class HuffmanMsg{

	public static HuffmanTree compresser;

	public static HuffmanTree decompresser;

	public static int bloc = 0;

	private static bool inited = false;

	private static int[] msg_hData = new int[]{
		250315,			// 0
		41193,			// 1
		6292,			// 2
		7106,			// 3
		3730,			// 4
		3750,			// 5
		6110,			// 6
		23283,			// 7
		33317,			// 8
		6950,			// 9
		7838,			// 10
		9714,			// 11
		9257,			// 12
		17259,			// 13
		3949,			// 14
		1778,			// 15
		8288,			// 16
		1604,			// 17
		1590,			// 18
		1663,			// 19
		1100,			// 20
		1213,			// 21
		1238,			// 22
		1134,			// 23
		1749,			// 24
		1059,			// 25
		1246,			// 26
		1149,			// 27
		1273,			// 28
		4486,			// 29
		2805,			// 30
		3472,			// 31
		21819,			// 32
		1159,			// 33
		1670,			// 34
		1066,			// 35
		1043,			// 36
		1012,			// 37
		1053,			// 38
		1070,			// 39
		1726,			// 40
		888,			// 41
		1180,			// 42
		850,			// 43
		960,			// 44
		780,			// 45
		1752,			// 46
		3296,			// 47
		10630,			// 48
		4514,			// 49
		5881,			// 50
		2685,			// 51
		4650,			// 52
		3837,			// 53
		2093,			// 54
		1867,			// 55
		2584,			// 56
		1949,			// 57
		1972,			// 58
		940,			// 59
		1134,			// 60
		1788,			// 61
		1670,			// 62
		1206,			// 63
		5719,			// 64
		6128,			// 65
		7222,			// 66
		6654,			// 67
		3710,			// 68
		3795,			// 69
		1492,			// 70
		1524,			// 71
		2215,			// 72
		1140,			// 73
		1355,			// 74
		971,			// 75
		2180,			// 76
		1248,			// 77
		1328,			// 78
		1195,			// 79
		1770,			// 80
		1078,			// 81
		1264,			// 82
		1266,			// 83
		1168,			// 84
		965,			// 85
		1155,			// 86
		1186,			// 87
		1347,			// 88
		1228,			// 89
		1529,			// 90
		1600,			// 91
		2617,			// 92
		2048,			// 93
		2546,			// 94
		3275,			// 95
		2410,			// 96
		3585,			// 97
		2504,			// 98
		2800,			// 99
		2675,			// 100
		6146,			// 101
		3663,			// 102
		2840,			// 103
		14253,			// 104
		3164,			// 105
		2221,			// 106
		1687,			// 107
		3208,			// 108
		2739,			// 109
		3512,			// 110
		4796,			// 111
		4091,			// 112
		3515,			// 113
		5288,			// 114
		4016,			// 115
		7937,			// 116
		6031,			// 117
		5360,			// 118
		3924,			// 119
		4892,			// 120
		3743,			// 121
		4566,			// 122
		4807,			// 123
		5852,			// 124
		6400,			// 125
		6225,			// 126
		8291,			// 127
		23243,			// 128
		7838,			// 129
		7073,			// 130
		8935,			// 131
		5437,			// 132
		4483,			// 133
		3641,			// 134
		5256,			// 135
		5312,			// 136
		5328,			// 137
		5370,			// 138
		3492,			// 139
		2458,			// 140
		1694,			// 141
		1821,			// 142
		2121,			// 143
		1916,			// 144
		1149,			// 145
		1516,			// 146
		1367,			// 147
		1236,			// 148
		1029,			// 149
		1258,			// 150
		1104,			// 151
		1245,			// 152
		1006,			// 153
		1149,			// 154
		1025,			// 155
		1241,			// 156
		952,			// 157
		1287,			// 158
		997,			// 159
		1713,			// 160
		1009,			// 161
		1187,			// 162
		879,			// 163
		1099,			// 164
		929,			// 165
		1078,			// 166
		951,			// 167
		1656,			// 168
		930,			// 169
		1153,			// 170
		1030,			// 171
		1262,			// 172
		1062,			// 173
		1214,			// 174
		1060,			// 175
		1621,			// 176
		930,			// 177
		1106,			// 178
		912,			// 179
		1034,			// 180
		892,			// 181
		1158,			// 182
		990,			// 183
		1175,			// 184
		850,			// 185
		1121,			// 186
		903,			// 187
		1087,			// 188
		920,			// 189
		1144,			// 190
		1056,			// 191
		3462,			// 192
		2240,			// 193
		4397,			// 194
		12136,			// 195
		7758,			// 196
		1345,			// 197
		1307,			// 198
		3278,			// 199
		1950,			// 200
		886,			// 201
		1023,			// 202
		1112,			// 203
		1077,			// 204
		1042,			// 205
		1061,			// 206
		1071,			// 207
		1484,			// 208
		1001,			// 209
		1096,			// 210
		915,			// 211
		1052,			// 212
		995,			// 213
		1070,			// 214
		876,			// 215
		1111,			// 216
		851,			// 217
		1059,			// 218
		805,			// 219
		1112,			// 220
		923,			// 221
		1103,			// 222
		817,			// 223
		1899,			// 224
		1872,			// 225
		976,			// 226
		841,			// 227
		1127,			// 228
		956,			// 229
		1159,			// 230
		950,			// 231
		7791,			// 232
		954,			// 233
		1289,			// 234
		933,			// 235
		1127,			// 236
		3207,			// 237
		1020,			// 238
		927,			// 239
		1355,			// 240
		768,			// 241
		1040,			// 242
		745,			// 243
		952,			// 244
		805,			// 245
		1073,			// 246
		740,			// 247
		1013,			// 248
		805,			// 249
		1008,			// 250
		796,			// 251
		996,			// 252
		1057,			// 253
		11457,			// 254
		13504,			// 255
	};

	private static int m = 0;
	private static int n = 0;

	public static void Init()
	{
		if(inited) return;
		inited = true;
		// huffmanMsg = new HuffmanMsg();
		HuffmanMsg.decompresser = new HuffmanTree(false);
		HuffmanMsg.compresser = new HuffmanTree(true);
		
		for(int i = 0; i < 256; i++){
			for(int j = 0; j < msg_hData[i]; j++){
				// try{
					AddRef(HuffmanMsg.compresser, (byte)i);
				// }catch(System.Exception e){
				// 	CLog.Info("i:{0}, j:{1}", i, j);
				// 	break;
				// }
				// AddRef(HuffmanMsg.decompresser, (byte)i);
			}
		}
	}

	public static void Update(){
		for(int i = 0; i < 100; i++){
if(n == 256) return;
		if(m == msg_hData[n]) {n++; return;}
		AddRef(HuffmanMsg.compresser, (byte)n);
		m++;
		}
		
	}

	public static void Compress(MsgPacket packet, int offset)
	{

	}

	public static void Decompress(MsgPacket packet, int offset)
	{

	}

	private static void AddRef(HuffmanTree huff, byte ch){
		HuffNodePtr tnode;
		HuffNodePtr tnode2;
		if(huff.loc[ch] == null){
			tnode = new HuffNodePtr(huff.nodeList[huff.blocNode++]);
			tnode2 = new HuffNodePtr(huff.nodeList[huff.blocNode++]);

			tnode2.ptr.symbol = CConstVar.HUFF_INTERNAL_NODE;
			tnode2.ptr.weight = 1;
			tnode2.ptr.next = huff.lhead.ptr.next;
			if(huff.lhead.ptr.next != null){
				huff.lhead.ptr.next.ptr.prev = tnode2;
				if(huff.lhead.ptr.next.ptr.weight == 1){
					tnode2.ptr.head = huff.lhead.ptr.next.ptr.head;
				}else{
					CLog.Info("addref {0}",huff.blocNode);
					tnode2.ptr.head = GetPPNode(huff);
					// tnode2.ptr.head.ptr = tnode2;//.SetValue(tnode2) ;
					Marshal.StructureToPtr(tnode2, tnode2.ptr.head.ptr, false);
				}
			}else{
				CLog.Info("addref {0}", huff.blocNode);
				tnode2.ptr.head = GetPPNode(huff);
				// tnode2.ptr.head.ptr = tnode2;//.SetValue(tnode2);
				Marshal.StructureToPtr(tnode2, tnode2.ptr.head.ptr, false);
			}

			huff.lhead.ptr.next = tnode2;
			tnode2.ptr.prev = huff.lhead;

			tnode.ptr.symbol = ch;
			tnode.ptr.weight = 1;
			tnode.ptr.next = huff.lhead.ptr.next;
			if(huff.lhead.ptr.next != null){
				huff.lhead.ptr.next.ptr.prev = tnode;
				if(huff.lhead.ptr.next.ptr.weight == 1){
					tnode.ptr.head = huff.lhead.ptr.next.ptr.head;
				}else{
					CLog.Error("should never happen 1");
					tnode.ptr.head = GetPPNode(huff);
					// tnode.ptr.head.ptr = tnode2;// .SetValue(tnode2);
					Marshal.StructureToPtr(tnode2, tnode.ptr.head.ptr, false);
				}
			}else{
				CLog.Error("should never happen 2");
				tnode.ptr.head = GetPPNode(huff);
				// tnode.ptr.head.ptr = tnode2; //.SetValue(tnode);
				Marshal.StructureToPtr(tnode2, tnode.ptr.head.ptr, false);
			}

			huff.lhead.ptr.next = tnode;
			tnode.ptr.prev = huff.lhead;
			tnode.ptr.left = tnode.ptr.right = null;

			if(huff.lhead.ptr.parent != null){
				if(huff.lhead.ptr.parent.ptr.left == huff.lhead){
					huff.lhead.ptr.parent.ptr.left = tnode2;
				}else{
					huff.lhead.ptr.parent.ptr.right = tnode2;
				}
			}else{
				huff.tree = tnode2;
			}

			tnode2.ptr.right = tnode;
			tnode2.ptr.left = huff.lhead;

			tnode2.ptr.parent = huff.lhead.ptr.parent;
			huff.lhead.ptr.parent = tnode.ptr.parent = tnode2;

			huff.loc[ch] = tnode;

			Increment(huff, tnode2.ptr.parent);
		}else{
			Increment(huff,huff.loc[ch]);
		}
	}

	private static void Increment(HuffmanTree huff, HuffNodePtr node){
		HuffNodePtr lnode;
		if(node == null){
			return;
		}

		if(node.ptr.next != null && node.ptr.next.ptr.weight == node.ptr.weight){
			lnode = Marshal.PtrToStructure(node.ptr.head.ptr, typeof(HuffNodePtr));
			if(lnode != node.ptr.parent){
				Swap(huff, lnode, node);
			}
			Swaplist(lnode, node);
		}
		if(node.ptr.prev != null && node.ptr.prev.ptr.weight == node.ptr.weight){
			node.ptr.head.ptr = node.ptr.prev;
		}else{
			// node.ptr.head.SetValue(null) ;
			node.ptr.head.ptr = null;
			FreePPNode(huff, node.ptr.head);
		}

		node.ptr.weight++;
		if(node.ptr.next != null && node.ptr.next.ptr.weight == node.ptr.weight){
			node.ptr.head = node.ptr.next.ptr.head;
		}else{
			node.ptr.head = GetPPNode(huff);
			try{
				// node.ptr.head.SetValue(node);
			}catch(System.Exception e){
				CLog.Error("error:{0},{1}", e.Message,0);
			}
		}

		if(node.ptr.parent != null){
			Increment(huff, node.ptr.parent);
			if(node.ptr.prev == node.ptr.parent){
				Swaplist(node, node.ptr.parent);
				if(node.ptr.head.ptr == node){
					// node.ptr.head.SetValue(node.parent);
					node.ptr.head.ptr = node.ptr.parent;
				}
			}
		}
	}

	private static void Swap(HuffmanTree huff, HuffNodePtr node1, HuffNodePtr node2){
		HuffNodePtr par1, par2;

		par1 = node1.ptr.parent;
		par2 = node2.ptr.parent;

		if(par1 != null){
			if(par1.ptr.left == node1){
				par1.ptr.left = node2;
			}else{
				par1.ptr.right = node2;
			}
		}else{
			huff.tree = node2;
		}

		if(par2 != null){
			if(par2.ptr.left == node2){
				par2.ptr.left = node1;
			}else{
				par2.ptr.right = node1;
			}
		}else{
			huff.tree = node1;
		}

		node1.ptr.parent = par2;
		node1.ptr.parent = par1;
	}

	private static void Swaplist(HuffNodePtr node1, HuffNodePtr node2){
		HuffNodePtr par1;
		par1 = node1.ptr.next;
		node1.ptr.next = node2.ptr.next;
		node2.ptr.next = par1;

		par1 = node1.ptr.prev;
		node1.ptr.prev = node2.ptr.prev;
		node2.ptr.prev = par1;

		if(node1.ptr.next == node1){
			node1.ptr.next = node2;
		}
		if(node2.ptr.next == node2){
			node2.ptr.next = node1;
		}
		if(node1.ptr.next != null){
			node1.ptr.next.ptr.prev = node1;
		}
		if(node2.ptr.next != null){
			node2.ptr.next.ptr.prev = node2;
		}
		if(node1.ptr.prev != null){
			node1.ptr.prev.ptr.next = node1;
		}
		if(node2.ptr.prev != null){
			node2.ptr.prev.ptr.next = node2;
		}
	}

	private static void FreePPNode(HuffmanTree huff, HuffNodeDPointer ppnode)
	{
		ppnode.ptr = huff.freeList
		// CLog.Info("FreePPNode:ptr:{0}, node:{1}", huff.blocPtrs, huff.blocNode);
		
	}

	private static HuffNodeDPointer GetPPNode(HuffmanTree huff){
		if(huff.freeList == null){
			// try{
			return new HuffNodeDPointer(huff.nodePtrs[huff.blocPtrs++]);
			// }catch(System.Exception e){
			// 	CLog.Info("error:{0}", e.Message);
			// 	return null;
			// }
		}else{
			// CLog.Info("getppnode ex:ptr:{0}, node:{1}", huff.blocPtrs, huff.blocNode);
			
			// typedef struct nodetype {
			// 	struct	nodetype *left, *right, *parent; /* tree structure */ 
			// 	struct	nodetype *next, *prev; /* doubly-linked list */
			// 	struct	nodetype **head; /* highest ranked node in block */
			// 	int		weight;
			// 	int		symbol;
			// } node_t;
			// tppnode = huff->freelist;
			// huff->freelist = (node_t **)*tppnode;
			HuffNodeDPointer tppnode = huff.freeList;//new HuffNodePtr();
			huff.freeList = new HuffNodeDPointer(tppnode.ptr.ptr != null ? tppnode.ptr.ptr.left : null);

			return tppnode;
		}
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

	public static void OffsetReceive(HuffNodePtr node, ref int ch, byte[] fin, ref int offset){
		bloc = offset;
		while(node != null && node.ptr.symbol == CConstVar.HUFF_INTERNAL_NODE){
			if(GetBit(fin) != 0){
				node = node.ptr.right;
			}else{
				node = node.ptr.left;
			}
		}

		if(node == null){
			ch = 0;
			return;
		}
		ch = node.ptr.symbol;
		offset = bloc;
	}

	public static void OffsetTransmit(HuffmanTree huff, int ch, byte[] fout, ref int offset){
		bloc = offset;
		Send(huff.loc[ch], null, fout);
		offset = bloc;
	}

	private static void Send(HuffNodePtr node, HuffNodePtr child, byte[] fout){
		if(node.ptr.parent != null){
			Send(node.ptr.parent, node, fout);
		}
		if(child != null){
			if(node.ptr.right == child){
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

public class HuffmanTree
{
	public int blocNode;

	public int blocPtrs;

	public HuffNodePtr tree;

	public HuffNodePtr lhead;

	public HuffNodePtr ltail;

	public HuffNodePtr[] loc;

	public HuffNodeDPointer freeList;

	public HuffmanNode[] nodeList;

	public HuffNodePtr[] nodePtrs;

	public int ptrIndex;

	public HuffmanTree(bool isCompresser){
		loc = new HuffNodePtr[CConstVar.HUFF_MAX+1];
		// for(int i = 0; i < CConstVar.HUFF_MAX+1; i ++){
		// 	loc[i] = new HuffmanNode();
		// }

		nodeList = new HuffmanNode[768];
		for(int i = 0; i < 768; i ++){
			nodeList[i] = new HuffmanNode();
		}

		nodePtrs = new HuffNodePtr[768];
		for(int i = 0; i < 768; i++){
			nodePtrs[i] = new HuffNodePtr(null);
			// nodePtrs[i].id = i;
		}
		// huffmanMsg.compresser.loc = new HuffmanNode[768];

		if(isCompresser){
			tree = lhead = loc[CConstVar.HUFF_MAX] = new HuffNodePtr(nodeList[blocNode++]);
		}else{
			tree = lhead = ltail = loc[CConstVar.HUFF_MAX] = new HuffNodePtr(nodeList[blocNode++]);
		}

		freeList = new HuffNodeDPointer(null);

		tree.ptr.symbol = CConstVar.HUFF_MAX;
		tree.ptr.weight = 0;
		lhead.ptr.next = lhead.ptr.prev = null;
		tree.ptr.parent = tree.ptr.left = tree.ptr.right = null;

		if(isCompresser){
			loc[CConstVar.HUFF_MAX] = tree;
		}
	}

}


//定义为class可以指向引用地址
public class HuffmanNode
{


	public HuffNodePtr left;

	public HuffNodePtr right;

	public HuffNodePtr parent;
	
	public HuffNodePtr next;

	public HuffNodePtr prev;

	public HuffNodeDPointer head;

	public int weight;

	public int symbol;

	public HuffmanNode(){
		weight = 0;
		symbol = 0;
		head = new HuffNodeDPointer(null);
	}

}

public class HuffNodePtr{
	public HuffmanNode ptr;

	// public int idx = 0;

	// public int id;

	// public HuffmanNode[] array;


	// public void SetValue(HuffmanNode value){
	// 	array[idx] = value;
	// 	ptr = value;
	// }
	public HuffNodePtr(HuffmanNode ptr){
		this.ptr = ptr;
	}
}

public class HuffNodeDPointer{
	public IntPtr ptr;

	public HuffNodeDPointer(object ptr){
		Marshal.StructureToPtr(ptr, this.ptr, false);
	}
}