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

	public const int ShowMiss = 1;

	public static int Qport = 8001;

	public static int LocalPort = 0;

	public static int maxClient = 20;

	public static int PMoveMsec = 10;

	public static int PMoveFixed = 10;

	public static int PMoveFloat = 10;

	public static int dmFlags = 10;

	public static int ErrorDecay = 10;

	public static int MaxPackets = 30;

	public static int PacketDUP = 3;

	public static int MAX_PLAYER_STORED = 32;

	public static int NoDelta = 0;

	public static int PureServer = 0;

	public static int PacketDelayClient = 10;

	public static int PacketDelayServer = 10;

	public static float timeScale = 1f;

	public static float AnglesSpeedKey = 1f;

	public static float YawSpeed = 1f;

	public static float PitchSpeed = 1f;

	public static int PadPackets = 1;

	public static int MAX_CLIENTS = 20;

	public static int BodyQueueSize = 8;

	public static int SV_FPS = 20;

	public static int TimeGame = 0;

	public static int minPing = 20;

	public static int maxPing = 2000;

	public static int reconnectLimit = 10;

	public static int SmoothClients = 1;

	public static int PrivateClients = 1;

	//监听的起始端口，考虑到端口被占用的情况
	public static int SERVER_PORT = 27960;

	//实际的端口
	public static int ServerPort = 0;

	public static int NUM_SERVER_PORTS = 4;
	
	public static int JUMP_VELOCITY = 270;

	public static string PrivatePwd = "server_passwd";

	public static string GameName = "moba";

	public static bool ListEntity = false;
	
	public static bool SV_PAUSE = false;

	public static bool NoPredict = false;

	public static bool SynchronousClients = false;

	public static bool OptimizePrediction = true;

	public static bool IsLocalNetwork = true;

	public static bool TruePing;

	public static int Speed = 2;

	public static int GIB_HEALTH = -40;

	public static int STEPSIZE = 18;

	public static int TimeNudge = 30;

	public const bool LanForcePackets = false;

	public const bool LanForceRate = false;

	public const string	 ServerAddress = "";

	public static int serverID = 100023;

	public const int MAX_SERVER_NUM = 32;

	

	/*-------------预定义数据-----------*/
	public const int MaxEntity = 10;

	public const int MAX_PS_EVENTS = 2;

	public const int MAX_PREDICTED_EVENTS = 16;

	public const int CMD_BACKUP = 64;

	public const int NUM_SAVED_STATES = CMD_BACKUP + 2;

	public const int CMD_MASK = CMD_BACKUP - 1;

	public const int PACKET_BACKUP = 32;

	public const int PACKET_MASK = PACKET_BACKUP - 1;

	public const int MAX_ENTITIES = 1<<10;

	public const int MAX_ENTITIES_IN_SNAPSHOT = 256;

	public const int ENTITYNUM_MAX_NORMAL = MAX_ENTITIES - 2;

	public const int ENTITYNUM_NONE = CConstVar.MAX_GENTITIES - 1;

	public const int MAX_RELIABLE_COMMANDS = 64;

	//最大的消息的长度，可能会被
	public const int MAX_MSG_LEN = 16384;

	public const int HUFF_MAX = 256;

	public const int HUFF_INTERNAL_NODE = HUFF_MAX + 1;

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

	public const int MAX_STRING_CHARS = 1024;

	public const int BIG_INFO_STRING = 8192;

	public const int MAX_CONFIGSTRINGS = 1024;

	public const int MAX_CHALLENGES = 2048;

	public const int MAX_LOOPBACK = 16;

	public const int GENTITYNUM_BITS = 10;

	public const int MAX_GENTITIES = 1 << GENTITYNUM_BITS;

	public const int MAX_JOYSTICK_AXIS = 16;

	public const int FLOAT_INT_BITS = 13;

	public const int DEFAULT_GRAVITY = 800;

	public const int FLOAT_INT_BIAS = 1 << (FLOAT_INT_BITS);

	public const int MAX_PACKET_USERCMDS = 32;

	public const int PITCH = 0;

	public const int YAW = 1;

	public const int ROLL = 2;

	public const int SV_MAX_RATE = 3000;

	public const int SV_MIN_RATE = 2000;

	public const int UDPIP_HEADER_SIZE = 28;

	public const int UDPIP6_HEADER_SIZE = 48;

	public const int Protocol = 8002;

	public const int SV_TimeOut = 10000;

	public const int SV_ZombieTime = 10000;

	public const int MAX_OTHER_SERVERS = 10;

	public const int MAX_PING_REQUESTS = 32;

	public const int EVENT_VALID_MSEC = 300;

	public const int EVENT_BITS = 0x100 | 0x200;

	public const int MAX_STATS = 16;

	public const int STAT_MAX_HEALTH = 6;

	public const int STAT_HEALTH = 0;

	public const int MAX_PERSISTANT = 16;

	public const int PERS_SPAWN_COUNT = 4;

	public const int PERS_TEAM = 3;

	public const float M_PI = 3.14159265358979323846f;


	public const int NUM_CLIENT_HISTORY = 17;

	public const int CONTENTS_BODY = 0x2000000;

	public const int CONTENTS_SOLID = 1;

	public const int SOLID_BMODEL = 0xffffff;

	public const int NUM_PING_SAMPLES = 64;

	public const int MAX_LATENT_CMDS = 64;

	public const int RESET_TIME = 500;

	public const int PS_PMOVEFRAMECOUNTBITS = 6;

	public static int[] kbitmask = new int[]{
		0x00000001, 0x00000003, 0x00000007, 0x0000000F,
		0x0000001F,	0x0000003F,	0x0000007F,	0x000000FF,
		0x000001FF,	0x000003FF,	0x000007FF,	0x00000FFF,
		0x00001FFF,	0x00003FFF,	0x00007FFF,	0x0000FFFF,
		0x0001FFFF,	0x0003FFFF,	0x0007FFFF,	0x000FFFFF,
		0x001FFFFf,	0x003FFFFF,	0x007FFFFF,	0x00FFFFFF,
		0x01FFFFFF,	0x03FFFFFF,	0x07FFFFFF,	0x0FFFFFFF,
		0x1FFFFFFF,	0x3FFFFFFF,	0x7FFFFFFF,	unchecked((int)0xFFFFFFFF),
	};
}
