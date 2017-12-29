using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class CConstVar {

	/*---------配置相关--------*/
	//丢包的概率
	public const int NET_DROP_SIM = 10;

	public const bool ShowPacket = false;

	/*-------------预定义数据-----------*/
	public const int MaxEntity = 10;

	public const int MAX_PS_EVENT = 2;

	public const int CMD_BACKUP = 64;

	public const int PACKET_BACKUP = 32;

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


}
