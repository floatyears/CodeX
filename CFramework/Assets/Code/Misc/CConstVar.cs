using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class CConstVar {

	/*---------配置相关--------*/
	//丢包的概率
	public const int NET_DROP_SIM = 10;

	public const int ShowPacket = 1;

	public const int ShowNet = 2;

	public static int Qport = 8001;

	public static int maxClient = 20;
	

	/*-------------预定义数据-----------*/
	public const int MaxEntity = 10;

	public const int MAX_PS_EVENT = 2;

	public const int CMD_BACKUP = 64;

	public const int PACKET_BACKUP = 32;

	public const int PACKET_MASK = PACKET_BACKUP - 1;

	public const int MAX_ENTITIES = 1<<10;

	public const int MAX_RELIABLE_COMMANDS = 64;

	//最大的消息的长度，可能会被
	public const int MAX_MSG_LEN = 16384;

	public const int HUFF_MAX = 256;

	public const int HUFF_LEN = 768;

	//MTU的大小是1400，一般默认设置是500，所以不能大于500
	public const int BUFFER_LIMIT = 1024;

	public const int PACKET_MAX_LEN = 1400;

	public const int FRAGMENT_SIZE = PACKET_MAX_LEN - 100;

	public const int FRAGMENT_BIT = 1 << 31;

	public const int MAX_SNAPSHOT_ENTITIES = 256;

	public const int MAX_PARSE_ENTITIES = PACKET_BACKUP * MAX_SNAPSHOT_ENTITIES;
	

	public const int MAX_PUSHED_EVENTS = 1024;

	public const int MAX_QUEUED_EVENTS = 256;

	public const int MASK_QUEUED_EVENTS = MAX_QUEUED_EVENTS - 1;

	public const int ComSpeeds = 1;

	public const int MAX_STRING_TOKENS = 1024;

	public const int BIG_INFO_STRING = 8192;

	public const int MAX_CONFIGSTRINGS = 1024;

	public const int MAX_CHALLENGES = 2048;

	public const int MAX_LOOPBACK = 16;

	public const int GENTITYNUM_BITS = 10;

	public const int MAX_GENTITIES = 1 << GENTITYNUM_BITS;

	public const int FLOAT_INT_BITS = 13;
	public const int FLOAT_INT_BIAS = 1 << (FLOAT_INT_BITS);

	public static NetField[] entityStateFields = new NetField[]{
		
	};
}
