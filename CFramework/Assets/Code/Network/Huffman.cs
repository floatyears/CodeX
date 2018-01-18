using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

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

	#if !UNITY_EDITOR && UNITY_IPHONE
        const string HuffmanDLL = "__Internal";
	#else
		const string HuffmanDLL = "HuffmanInit";
	#endif  

	[DllImport(HuffmanDLL, CallingConvention = CallingConvention.Cdecl)]
	public extern static IntPtr InitHuffman(IntPtr compressor, IntPtr decompresser);

	public static void Init()
	{
		if(inited) return;
		inited = true;
		// huffmanMsg = new HuffmanMsg();
		HuffmanMsg.decompresser = new HuffmanTree(false);
		HuffmanMsg.compresser = new HuffmanTree(true);
		
		for(int i = 0; i < 256; i++){
			for(int j = 0; j < msg_hData[i]; j++){
				AddRef(HuffmanMsg.compresser, (byte)i);
				AddRef(HuffmanMsg.decompresser, (byte)i);
			}
		}
		// IntPtr compPtr = Marshal.AllocHGlobal(Marshal.SizeOf(HuffmanTree));
		// IntPtr decompPtr = new IntPtr();
		// Marshal.StructureToPtr(HuffmanMsg.decompresser, compPtr, false);
		// Marshal.StructureToPtr(HuffmanMsg.decompresser, decompPtr, false);
		// InitHuffman(HuffmanMsg.compresser, HuffmanMsg.decompresser);
		int a = 1;
	}

	public static void Compress(MsgPacket packet, int offset)
	{

	}

	public static void Decompress(MsgPacket packet, int offset)
	{

	}

	unsafe private static void AddRef(HuffmanTree huff, byte ch){
		if(huff.loc[ch] == null){
			fixed(HuffmanNode* tnode = &huff.nodeList[huff.blocNode++])
			{
				fixed(HuffmanNode* tnode2 = &huff.nodeList[huff.blocNode++])
				{
					tnode2->symbol = CConstVar.HUFF_INTERNAL_NODE;
					tnode2->weight = 1;
					tnode2->next = huff.lhead->next;
					if(huff.lhead->next != null){
						huff.lhead->next->prev = tnode2;
						if(huff.lhead->next->weight == 1){
							tnode2->head = huff.lhead->next->head;
						}else{
							tnode2->head = GetPPNode(huff);
							*tnode2->head = tnode2;
						}
					}else{
						tnode2->head = GetPPNode(huff);
						// tnode2.head.ptr = tnode2;
						// Marshal.StructureToPtr(tnode2, tnode2.head.ptr, false) ;
						*tnode2->head = tnode2;
					}

					huff.lhead->next = tnode2;
					tnode2->prev = huff.lhead;

					tnode->symbol = ch;
					tnode->weight = 1;
					tnode->next = huff.lhead->next;
					if(huff.lhead->next != null){
						huff.lhead->next->prev = tnode;
						// CLog.Info("weight:{0}", huff.lhead->next->weight);
						
						if(huff.lhead->next->weight == 1){
							tnode->head = huff.lhead->next->head;
						}else{
							CLog.Error("should never happen 1");
							tnode->head = GetPPNode(huff);
							// tnode.head.ptr = tnode2;
							// Marshal.StructureToPtr(tnode2, tnode->head.ptr, false) ;
							*tnode->head = tnode2;
						}
					}else{
						CLog.Error("should never happen 2");
						tnode->head = GetPPNode(huff);
						// tnode.head.ptr = tnode;
						// Marshal.StructureToPtr(tnode, tnode.head.ptr, false) ;
						*tnode->head = tnode;
					}

					huff.lhead->next = tnode;
					tnode->prev = huff.lhead;
					tnode->left = tnode->right = null;

					if(huff.lhead->parent != null){
						if(huff.lhead->parent->left == huff.lhead){
							huff.lhead->parent->left = tnode2;
						}else{
							huff.lhead->parent->right = tnode2;
						}
					}else{
						huff.tree = tnode2;
					}

					tnode2->right = tnode;
					tnode2->left = huff.lhead;

					tnode2->parent = huff.lhead->parent;
					huff.lhead->parent = tnode->parent = tnode2;

					huff.loc[ch] = tnode;

					Increment(huff,tnode2->parent);
				}
			}
		}else{
			Increment(huff,huff.loc[ch]);
		}
	}

	unsafe private static void Increment(HuffmanTree huff, HuffmanNode* node){
		HuffmanNode* lnode;
		if(node == null){
			return;
		}

		if(node->next != null && node->next->weight == node->weight){
			// lnode = (HuffmanNode)Marshal.PtrToStructure(node->head, typeof(HuffmanNode)) ;
			lnode = *node->head;
			if(lnode != node->parent){
				Swap(huff, lnode, node);
			}
			Swaplist(lnode, node);
		}
		if(node->prev != null && node->prev->weight == node->weight){
			// Marshal.StructureToPtr(node->prev, node->head.ptr, false) ;
			*node->head = node->prev;
		}else{
			*node->head = null;
			FreePPNode(huff, node->head);
		}

		node->weight++;
		if(node->next != null && node->next->weight == node->weight){
			node->head = node->next->head;
		}else{
			node->head = GetPPNode(huff);
			// node.head.ptr = node;
			// Marshal.StructureToPtr(node, node.head.ptr, false) ;
			*node->head = node;
		}

		if(node->parent != null){
			Increment(huff, node->parent);
			if(node->prev == node->parent){
				Swaplist(node, node->parent);
				// var ptr = new IntPtr();
				// Marshal.StructureToPtr(node,ptr,false);
				if(*node->head == node){
					// Marshal.StructureToPtr(node.parent,node.head.ptr,false) ;
					*node->head = node->parent;
				}
			}
		}
	}

	unsafe private static void Swap(HuffmanTree huff, HuffmanNode* node1, HuffmanNode* node2){
		HuffmanNode* par1;
		HuffmanNode* par2;

		par1 = node1->parent;
		par2 = node2->parent;

		if(par1 != null){
			if(par1->left == node1){
				par1->left = node2;
			}else{
				par1->right = node2;
			}
		}else{
			huff.tree = node2;
		}

		if(par2 != null){
			if(par2->left == node2){
				par2->left = node1;
			}else{
				par2->right = node1;
			}
		}else{
			huff.tree = node1;
		}

		node1->parent = par2;
		node1->parent = par1;
	}

	unsafe private static void Swaplist(HuffmanNode* node1, HuffmanNode* node2){
		HuffmanNode* par1;
		par1 = node1->next;
		node1->next = node2->next;
		node2->next = par1;

		par1 = node1->prev;
		node1->prev = node2->prev;
		node2->prev = par1;

		if(node1->next == node1){
			node1->next = node2;
		}
		if(node2->next == node2){
			node2->next = node1;
		}
		if(node1->next != null){
			node1->next->prev = node1;
		}
		if(node2->next != null){
			node2->next->prev = node2;
		}
		if(node1->prev != null){
			node1->prev->next = node1;
		}
		if(node2->prev != null){
			node2->prev->next = node2;
		}
	}

	unsafe private static void FreePPNode(HuffmanTree huff, HuffmanNode** ppnode){
		// var hptr = Marshal.PtrToStructure(huff.freeList.ptr, typeof(HuffNodePtr)); //看看是否指向的
		// Marshal.StructureToPtr(, ppnode.ptr, false);
		*ppnode = (HuffmanNode *)huff.freeList;
		huff.freeList = ppnode;
		// CLog.Info("FreePPNode:ptr:{0}, node:{1}", huff.blocPtrs, huff.blocNode);
		
	}

	unsafe private static HuffmanNode** GetPPNode(HuffmanTree huff){
		// CLog.Info("get ppnode",huff.freeList.ptr);
		HuffmanNode** tppnode;
		if(huff.freeList == null){
			// CLog.Info("getppnode:ptr:{0}, node:{1}", huff.blocPtrs, huff.blocNode);
			
			// try{
			fixed(HuffmanNode** retVal = &huff.nodePtrs[huff.blocPtrs++]){
				return retVal;
			}
			// }catch(System.Exception e){
				// CLog.Info("error:", e.Message);
				// return null;
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
			tppnode = huff.freeList;//new HuffNodePtr();
			huff.freeList = (HuffmanNode **) *tppnode;

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

	unsafe public static void OffsetReceive(HuffmanNode* node, ref int ch, byte[] fin, ref int offset){
		bloc = offset;
		while(node!= null && node->symbol == CConstVar.HUFF_INTERNAL_NODE){
			if(GetBit(fin) != 0){
				node = node->right;
			}else{
				node = node->left;
			}
		}

		if(node == null){
			ch = 0;
			return;
		}
		ch = node->symbol;
		offset = bloc;
	}

	unsafe public static void OffsetTransmit(HuffmanTree huff, int ch, byte[] fout, ref int offset){
		bloc = offset;
		Send(huff.loc[ch], null, fout);
		offset = bloc;
	}

	unsafe private static void Send(HuffmanNode* node, HuffmanNode* child, byte[] fout){
		if(node->parent != null){
			Send(node->parent, node, fout);
		}
		if(child != null){
			if(node->right == child){
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

[StructLayout(LayoutKind.Explicit)]
unsafe public class HuffmanTree
{
	[FieldOffset(0)]public int blocNode;

	[FieldOffset(sizeof(int))]public int blocPtrs;


	[FieldOffset(sizeof(int) * 2)]public HuffmanNode* tree;

	[FieldOffset(sizeof(int) * 4)]public HuffmanNode* lhead;

	[FieldOffset(sizeof(int) * 6)]public HuffmanNode* ltail;

	[FieldOffset(sizeof(int) * 8)]public HuffmanNode** freeList;

	[FieldOffset(sizeof(int) * 10)]public HuffmanNode*[] loc;

	[FieldOffset(sizeof(int) * (10 + 769*2))]public HuffmanNode[] nodeList;

	[FieldOffset(sizeof(int) * (10 + 1537*2))]public HuffmanNode*[] nodePtrs;

	unsafe public HuffmanTree(bool isCompresser){
		loc = new HuffmanNode*[CConstVar.HUFF_MAX+1];
		// for(int i = 0; i < CConstVar.HUFF_MAX+1; i ++){
		// 	loc[i] = new HuffmanNode();
		// }

		nodeList = new HuffmanNode[768];
		
		for(int i = 0; i < 768; i ++){
			nodeList[i] = new HuffmanNode();
		}

		nodePtrs = new HuffmanNode*[768];
		// for(int i = 0; i < 768; i++){
		// 	nodePtrs[i] = new HuffNodePtr();
		// }
		// huffmanMsg.compresser.loc = new HuffmanNode[768];
		fixed(HuffmanNode* tmp = &nodeList[blocNode++]){
			if(isCompresser){
				tree = lhead = loc[CConstVar.HUFF_MAX] = tmp;
			}else{
				tree = lhead = ltail = loc[CConstVar.HUFF_MAX] = tmp;
			}
		}
		

		// freeList = new HuffNodePtr();

		tree->symbol = CConstVar.HUFF_MAX;
		tree->weight = 0;
		lhead->next = lhead->prev = null;
		tree->parent = tree->left = tree->right = null;

		if(isCompresser){
			loc[CConstVar.HUFF_MAX] = tree;
		}
	}

}

//定义为class可以指向引用地址
[StructLayout(LayoutKind.Explicit)]
unsafe public struct HuffmanNode
{
	[FieldOffset(0)]public HuffmanNode* left;

	[FieldOffset(sizeof(int) * 2)]public HuffmanNode* right;

	[FieldOffset(sizeof(int) * 4)]public HuffmanNode* parent;

	[FieldOffset(sizeof(int) * 6)]public HuffmanNode* next;

	[FieldOffset(sizeof(int) * 8)]public HuffmanNode* prev;

	[FieldOffset(sizeof(int) * 10)]public HuffmanNode** head;

	[FieldOffset(sizeof(int) * 12)]public int weight;

	[FieldOffset(sizeof(int) * 13)]public int symbol;

	// public HuffmanNode(){
	// 	weight = 0;
	// 	symbol = 0;
	// 	// head = new HuffNodePtr();
	// }
}

// public class HuffNodePtr{
// 	public IntPtr ptr;
// }