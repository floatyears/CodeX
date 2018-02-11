using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class MsgPacket{

	static char[] readStr = new char[CConstVar.MAX_STRING_CHARS];

	static char[] bigStr = new char[CConstVar.BIG_INFO_STRING];
	bool allowOverflow;

	bool overflowed;

	bool oob; //out of band(带外数据)

	byte[] bytes;

	int curSize;

	int curPos;

	int bit;

	// int get;

	public System.Net.IPEndPoint remoteEP;

	public MsgPacket()
	{
		this.bytes = new byte[CConstVar.BUFFER_LIMIT];
		this.curPos = 0;
		// oob = true;
	}

	//把packet中的数据读出到data中
	public void ReadData(byte[] data, int start, int length)
	{
		if(start < 0) start = curSize;
		Array.Copy(bytes, start, data, 0, length);
		// curSize = start + length;
		curPos = start + length;
	}

	//把外面的数据写入到packet中
	public void WriteBufferData(byte[] data, int start, int length, int dataStart = 0)
	{
		if(start < 0) start = curSize;
		Array.Copy(data, dataStart, bytes, start, length);
		curSize = start + length;
		curPos = start + length;
		
	}

	public void WriteData(byte[] data, int length){
		for(int i = 0; i < length; i++){
			WriteByte(data[i]);
		}
	}

	public void BeginRead()
	{
		curPos = 0;
		bit = 0;
		oob = false;
	}

	public void BeginReadOOB()
	{
		curPos = 0;
		bit = 0;
		oob = true;
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
		}get{
			return bit;
		}
	}

	public bool Oob{
		set{
			oob = value;
		}
	}

	public bool AllowOverflow{
		set{
			allowOverflow = value;
		}
	}

	public bool Overflowed{
		set{
			overflowed = value;
		}
		get{
			return overflowed;
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


	public int ReadFirstInt(){
		// return System.BitConverter.ToInt32(Data, 0);
		return (bytes[3] << 24) + (bytes[2]<<16) + (bytes[1] << 8) + (int)bytes[0];
		
	}

	public int ReadInt()
	{
		int c = ReadBits(32);
		if(curPos > curSize){
			c = -1;
		}
		return c;
	}

	public int ReadByte()
	{
		int c = (char)ReadBits(8);
		if(curPos > curSize){
			return -1;
		}
		return c;
	}

	unsafe public int ReadBits(int bits)
	{
		int value = 0;
		int get = 0;
		int i, nbits;
		bool sgn;

		value = 0;
		if(bits < 0){
			bits = -bits;
			sgn = true;
		}else{
			sgn = false;
		}
		if(oob){
			if(bits == 8){
				value = bytes[curPos];
				curPos++;
				bit += 8;
			}else if(bits == 16){
				value = CopyLittleShort();
				curPos += 2;
				bit += 16;
			}else if(bits == 32){
				value = CopyLittleLong();
				curPos += 4;
				bit += 32;
			}else{
				CLog.Error("can't read %d bits", bits);
			}
		}else{
			nbits = 0;
			if((bits & 7) != 0){
				nbits = bits & 7;
				for(i = 0; i < nbits; i++){
					value |= (HuffmanMsg.GetBit(bytes, ref this.bit) << i);
				}
				bits = bits - nbits;
			}
			if(bits != 0){
				// HuffmanMsg.OffsetReceive(HuffmanMsg.decompresser.tree, ref get, bytes, ref bit);
				fixed(byte* f = &bytes[0]){
					fixed(int* b = &bit){
						for(i=0; i < bits; i+= 8){
							HuffmanMsg.HuffOffsetReceive(&get, f, b);
							value |= (get << (i + nbits));
						}
					}
				}
			}
			curPos = (this.bit >> 3) + 1;
		}

		if(sgn){
			if((value & ( 1 << (bits - 1))) != 0){
				value |= -1 ^ ((1 << bits) - 1);
			}
		}
		return value;
	}

	private int CopyLittleShort(){
		byte[] tmp = new byte[2];
		tmp[1] = bytes[curPos];
		tmp[0] = bytes[curPos+1];
		return (tmp[0] << 8) + (int)tmp[1];
	}

	private int CopyLittleLong(){
		byte[] tmp = new byte[4];
		tmp[0] = bytes[curPos+3];
		tmp[1] = bytes[curPos+2];
		tmp[2] = bytes[curPos+1];
		tmp[3] = bytes[curPos];
		return (tmp[0] << 24) + (tmp[1]<<16) + (tmp[2] << 8) + (int)tmp[3];
	}

	public int ReadPort(){
		// return System.BitConverter.ToInt16(bytes, 8);
		return ((bytes[8] << 8) + (int)bytes[9]);
	}

	public int ReadShort()
	{
		int c = ReadBits(16);
		if(curPos > curSize){
			c = -1;
		}
		return c;
	}

	//读取char
	public string ReadChars(int len,int start)
	{
		var chars = new char[len];
		for(int i = 0; i < len; i++){
			chars[i] = Convert.ToChar(bytes[i+start]);
		}
		return new string(chars);
	}

	public string ReadStringLine()
	{
		int l = 0,c;
		do{
			c = ReadByte();
			if(c == -1 || c == 0 || c == '\n'){
				break;
			}
			//翻译所有的格式化字符串
			if(c == '%'){
				c = '.';
			}
			if(c > 127){
				c = '.';
			}

			readStr[l] = (char)c;
			l++;
		}while(l < CConstVar.MAX_STRING_CHARS - 1);

		readStr[l] = (char)0;
		return new string(readStr,0,l);
	}

	public string ReadString()
	{
		int l = 0,c;
		do{
			c = ReadByte();
			if(c == -1 || c == 0){
				break;
			}
			//翻译所有的格式化字符串
			if(c == '%'){
				c = '.';
			}
			if(c > 127){
				c = '.';
			}

			readStr[l] = (char)c;
			l++;
		}while(l < CConstVar.MAX_STRING_CHARS - 1);

		readStr[l] = (char)0;
		return new string(readStr,0,l);
	}

	public string ReadBigString()
	{
		int l = 0,c;
		do{
			c = ReadByte();
			if(c == -1 || c == 0){
				break;
			}
			//翻译所有的格式化字符串
			if(c == '%'){
				c = '.';
			}
			if(c > 127){
				c = '.';
			}

			bigStr[l] = (char)c;
			l++;
		}while(l < CConstVar.BIG_INFO_STRING - 1);
		
		bigStr[l] = (char)0;
		return new string(bigStr, 0, l);
	}

	public int ReadDeltaKey(int key, int oldV, int bits){
		if(ReadBits(1) > 0){
			return ReadBits(bits) ^ (key & CConstVar.kbitmask[bits - 1]);
		}
		return oldV;
	}

	public void ReadDeltaUsercmdKey(int key, ref UserCmd from, ref UserCmd to){
		if(ReadBits(1) > 0){
			to.serverTime = from.serverTime + ReadBits(8);
		}else{
			to.serverTime = ReadBits(32);
		}

		if(ReadBits(1) > 0){
			key ^= to.serverTime;
			to.angles[0] = ReadDeltaKey(key, from.angles[0], 16);
			to.angles[1] = ReadDeltaKey(key, from.angles[1], 16);
			to.angles[2] = ReadDeltaKey(key, from.angles[2], 16);

			to.forwardmove = (sbyte)ReadDeltaKey(key, from.forwardmove, 8);
			if(to.forwardmove == -128){
				to.forwardmove = -127;
			}
			to.rightmove = (sbyte)ReadDeltaKey(key, from.rightmove, 8);
			if(to.rightmove == -128){
				to.rightmove = -127;
			}
			to.upmove = (sbyte)ReadDeltaKey(key, from.upmove, 8);
			if(to.upmove == -128){
				to.upmove = -127;
			}
			to.buttons = ReadDeltaKey(key, from.buttons, 16);
		}else{
			to.angles[0] = from.angles[0];
			to.angles[1] = from.angles[1];
			to.angles[2] = from.angles[2];
			to.forwardmove = from.forwardmove;
			to.upmove = from.upmove;
			to.rightmove = from.rightmove;
			to.buttons = from.buttons;
		}
	}

	//entity的索引值已经从消息中读取，这是from的state用来标识用的
	//如果delta移除了这个entity，entityState.entityIndex会被设置为MAX_GENTITIES - 1
	//可以从baseline中获取，或者从前面的packet_entity中获取
	unsafe public void ReadDeltaEntity(ref EntityState from, ref EntityState to, int number)
	{
		int i, lc;
		int numFields;
		NetField field;
		int* fromF = null;
		int* toF = null;
		int print;
		int trunc;
		int startBit, endBit;

		if(number < 0 || number >= CConstVar.MAX_GENTITIES){
			CLog.Error("Bad delta entity number: {0}", number);
		}

		if(bit == 0){
			startBit = curPos * 8 - CConstVar.GENTITYNUM_BITS;
		}else{
			startBit = (curPos - 1) * 8 + bit - CConstVar.GENTITYNUM_BITS;
		}

		//检查是否要移除
		if(ReadBits(1) == 1){
			// to = new EntityState();
			to.entityIndex = CConstVar.MAX_GENTITIES - 1;
			if(CConstVar.ShowNet != 0 && (CConstVar.ShowNet >= 2 || CConstVar.ShowNet == -1)){
				CLog.Info("remove entity: {0}", number);
			}
			return;
		}

		//检查是否无压缩
		if(ReadBits(1) == 0){
			from.CopyTo(to);
			to.entityIndex = number;
			return;
		}

		numFields = EntityState.entityStateFields.Length;
		lc = ReadByte();
		if(lc > numFields || lc < 0){
			CLog.Info("invalid entity state field count");
		}

		if(CConstVar.ShowNet != 0 && (CConstVar.ShowNet >= 2 || CConstVar.ShowNet == -1)){
			print = 1;
			CLog.Info("delta count {0}: #-{1}", curPos * 3, to.entityIndex);
		}else{
			print = 0;
		}

		to.entityIndex = number;

		EntityState tmpF = from; //TODO：这里每次都会创建一个struct，浪费内存，比较好的做法是把这部分逻辑写到dll中。
		EntityState tmpT = to;
		byte* startF = (byte *)&tmpF;
		byte* startT = (byte *)&tmpT;
		for(i = 0; i < lc; i++){
			field = EntityState.entityStateFields[i];
			fromF = (int *)(startF + field.offset);
			toF = (int *)(startT + field.offset);

			//最好的方式还是c++直接操作内存数据，利用反射效率比较低
			if(ReadBits(1) == 0){ //没有变化
				// field.name.SetValue(to, field.name.GetValue(from));
				*toF = *fromF;
				
			}else{
				if(field.bits == 0){
					if(ReadBits(1) == 0){
						// field.name.SetValue(to, 0.0f);
						try{
							*(float *)toF = 0.0f;
						}catch(Exception e){
							CLog.Error("error:{0}, {1}", e.Message, field.name);
						}
					}else{
						if(ReadBits(1) == 0){
							//积分浮点数
							trunc = ReadBits(CConstVar.FLOAT_INT_BITS);

							//偏移允许正的部分和负的部分是一样大小的
							trunc -= CConstVar.FLOAT_INT_BIAS;
							// field.name.SetValue(to, trunc);
							*(float *)toF = trunc;
							if(print > 0){
								CLog.Info("Read Delta Entity {0}:{1}", field.name, trunc);
							}
						}else{
							//完整的浮点值
							// field.name.SetValue(to, ReadBits(32));
							*toF = ReadBits(32);
							if(print > 0){
								CLog.Info("Read Delta Entity {0}:{1}", field.name, *(float *)toF);
							}
						}
					}
				}else{
					if(ReadBits(1) == 0){
						// field.name.SetValue(to, 0);
						*toF = 0;
					}else{
						// field.name.SetValue(to, ReadBits(field.bits));
						*toF = ReadBits(field.bits);
						if(print > 0){
							CLog.Info("Read Delta Entity {0}:{1}", field.name, *toF);
						}
					}
				}
			}
		}
		for(i = lc; i < numFields; i++){
			field = EntityState.entityStateFields[i];
			fromF = (int *)(startF + field.offset);
			toF = (int*)(startT + field.offset);
			
			//没有变化
			// field.name.SetValue(to, field.name.GetValue(from));
			*toF = *fromF;
		}

		if(print > 0){
			if(bit == 0){
				endBit = curPos * 8 - CConstVar.GENTITYNUM_BITS;
			}else{
				endBit = (curPos - 1) * 8 + bit - CConstVar.GENTITYNUM_BITS;
			}
			CLog.Info("Read Delta Entity Finished. ({0} bits)", endBit - startBit);
		}

		from = *(EntityState *)startF; //最后要进行一次复制struct，因为中间为了获取pointer，声明了一个新的struct
		to = *(EntityState *)startT;

	}

	public void ReadDeltaPlayerstate(PlayerState _from, PlayerState to)
	{
		int i,lc;

		PlayerState from = _from;
		if(from == null){
			from = new PlayerState();
		}

		int startBit, endBit;
		from.CopyTo(to);
		if(bit == 0){
			startBit = curPos * 8 - CConstVar.GENTITYNUM_BITS;
		}else{
			startBit = (curPos - 1) * 8 + bit - CConstVar.GENTITYNUM_BITS;
		}

		int print = 0;
		if(CConstVar.ShowNet > 0 && (CConstVar.ShowNet >= 2 || CConstVar.ShowNet == -2)){
			print = 1;
		}

		int numFields = PlayerState.PlayerStateFieldsInt.Length;
		lc = ReadByte();

		if(lc > numFields || lc < 0){
			CLog.Error("invalid playerstate field count");
			return;
		}

		int[] field;
		int trunc;
		for(i = 0; i < lc; i++){
			field = PlayerState.PlayerStateFieldsInt[i];

			if(ReadBits(1) == 0){//字段没有变化
				to[field[0]] = from[field[0]];
			}else{
				if(field[1] == 0){ //浮点数
					if(ReadBits(1) == 0){ //没有变化
						trunc = ReadBits(CConstVar.FLOAT_INT_BITS);
						trunc -= CConstVar.FLOAT_INT_BIAS;
						// float a = (float)trunc;
						//这里直接解析为float，不使用类型转换
						to[field[0]] = BitConverter.ToInt32(BitConverter.GetBytes(trunc), 0);
						if(print > 0){
							CLog.Info("read delta playerstate field float:{0}, value:{1}, bits:{2}",field[0], to[field[0]], field[1]);
						}
					}else{
						to[field[0]] = BitConverter.ToInt32(BitConverter.GetBytes(ReadBits(32)), 0); //完整的浮点数
						if(print > 0){
							CLog.Info("read delta playerstate field float:{0}, value:{1}, bits:{2}",field[0], to[field[0]], field[1]);
						}
					}
				}else{ //整数
					to[field[0]] = ReadBits(field[1]);
					if(print > 0){
						CLog.Info("read delta playerstate field:{0}, value:{1}, bits:{2}",field[0], to[field[0]], field[1]);
					}
				}
			}
		}

		for(i = lc; i < numFields; i++){
			field = PlayerState.PlayerStateFieldsInt[i];
			to[field[0]] = from[field[0]];
		}

		int bs = 0;
		if(ReadBits(1) > 0){
			//parse stats
			if(ReadBits(1) > 0){
				bs = ReadBits(CConstVar.MAX_STATS);
				for(i = 0; i < CConstVar.MAX_STATS; i++){
					if((bs & (1 << i)) > 0){
						to.states[i] = ReadShort();
					}
				}
			}

			//parse persistant stats
			if(ReadBits(1) > 0){
				bs = ReadBits(CConstVar.MAX_PERSISTANT);
				for(i = 0; i < CConstVar.MAX_PERSISTANT; i++){
					if((bs & (1<<i)) > 0){
						to.persistant[i] = ReadShort();
					}
				}
			}
		}

		if(print > 0){
			if(bit == 0){
				endBit = curPos * 8 - CConstVar.GENTITYNUM_BITS;
			}else{
				endBit = (curPos - 1) * 8 + bit - CConstVar.GENTITYNUM_BITS;
			}
			CLog.Info("delta player state end, len: {0} bits", endBit - startBit);
		}

	}

	public void WriteDeltaPlayerstate(PlayerState _from, PlayerState to)
	{
		PlayerState from = _from;
		PlayerState dummy = new PlayerState();
		if(from == null){
			from = dummy;
		}
		int[] field;
		int numFields = PlayerState.PlayerStateFieldsInt.Length;
		int lc = 0;
		for(int i = 0; i < numFields; i++){
			field = PlayerState.PlayerStateFieldsInt[i];
			if(from[field[0]] != to[field[0]]){
				if(field[0] == 6 || field[0] == 7 || field[0] == 8){
					CLog.Info("origin change");
				}
				lc = i + 1;
			}
		}

		WriteByte((byte)lc);
		float fullFloat;
		int trunc;
		for(int i = 0; i < lc; i++){
			field = PlayerState.PlayerStateFieldsInt[i];
			if(from[field[0]] == to[field[0]]){
				WriteBits(0, 1); //没有变化
				continue;
			}

			WriteBits(1,1);
			if(field[1] == 0){ //浮点
				fullFloat = BitConverter.ToSingle(BitConverter.GetBytes(to[field[0]]),0);
				trunc = (int)fullFloat;

				if(trunc == fullFloat && (trunc + CConstVar.FLOAT_INT_BIAS >= 0) &&
					(trunc + CConstVar.FLOAT_INT_BIAS) < (1 << CConstVar.FLOAT_INT_BITS)){
					//作为小的整数发送
					WriteBits(0,1);
					WriteBits(trunc + CConstVar.FLOAT_INT_BIAS, CConstVar.FLOAT_INT_BITS);
				}else{
					//发送完整的浮点值
					WriteBits(1,1);
					WriteBits(to[field[0]], 32);
				}
			}else{
				WriteBits(to[field[0]],field[1]);
			}
		}

		int statsbits = 0;
		for(int i = 0; i < CConstVar.MAX_STATS; i++){
			if(to.states[i] != from.states[i]){
				statsbits |= 1 << i;
			}
		}

		int persistantbits = 0;
		for(int i = 0; i < CConstVar.MAX_PERSISTANT; i++){
			if(to.persistant[i] != from.persistant[i]){
				persistantbits |= 1 << i;
			}
		}

		if(statsbits == 0 && persistantbits == 0){
			WriteBits(0,1);
			return;
		}
		if(statsbits > 0){
			WriteBits(1,1);
			WriteBits(statsbits, CConstVar.MAX_STATS);
			for(int i = 0; i < CConstVar.MAX_STATS; i++){
				if((statsbits & (1 << i)) > 0){
					WriteShort((short)to.states[i]);
				}
			}
		}else{
			WriteBits(0,1); //没有变化
		}

		if(persistantbits > 0 ){
			WriteBits(1,1);
			WriteBits(persistantbits, CConstVar.MAX_PERSISTANT);
			for(int i = 0; i < CConstVar.MAX_PERSISTANT; i++){
				if((persistantbits & (1 << i)) > 0){
					WriteShort((short)to.persistant[i]);
				}
			}
		}else{
			WriteBits(0,1);
		}
	}

	//写入消息的packet entities部分，包含entity number
	//可以从baseline或者前一个packet_entity更新，如果to是null，会发送移除消息
	//如果没有设置force，那么在entity是唯一的情况下，不会生成任何信息，为了保证按照顺序增量更新的代码会获得它。
	unsafe public void WriteDeltaEntity(EntityState? from, EntityState? to, bool force)
	{
		int numFields = EntityState.entityStateFields.Length;

		//所有的字段都必须是32位，避免任何编译器打包问题
		if(numFields + 1 == Marshal.SizeOf(from)/4){
			CLog.Error("Fatal Error:");
		}

		if(to == null || !to.HasValue){
			if(from == null || !from.HasValue){
				return;
			}
			WriteBits(from.Value.entityIndex, CConstVar.GENTITYNUM_BITS);
			WriteBits(1,1);
			return;
		}

		var fromEnt = from.Value;
		var toEnt = to.Value;
		if(toEnt.entityIndex < 0 || toEnt.entityIndex >= CConstVar.MAX_GENTITIES){
			CLog.Error("WriteDeltaEntity: Bad Entity idx: {0}", toEnt.entityIndex);
		}

		
		int lc = 0;
		int len = EntityState.entityStateFields.Length;

		int* fromF;
		int* toF;
		byte* startF = (byte *)&fromEnt;
		byte* startT = (byte *)&toEnt;
		int offset = 0;
		for(int i = 0; i < len; i++){
			// EntityState.entityStateFields[i].name.GetValue()
			offset = EntityState.entityStateFields[i].offset;
			fromF = (int *)(startF + offset);
			toF = (int *)(startT + offset);
			if(*fromF != *toF){
				lc = i + 1;
			}
		}

		if(lc == 0){ //没有改变任何属性
			if(!force){ //不写入任何东西
				return;
			}
			WriteBits(toEnt.entityIndex, CConstVar.GENTITYNUM_BITS);
			WriteBits(0,1); //没有被移除
			WriteBits(0,1); //没有增量更新
			return;
		}
		WriteBits(toEnt.entityIndex, CConstVar.GENTITYNUM_BITS);
		WriteBits(0, 1); //没有被移除
		WriteBits(1, 1); //没有增量更新

		if(lc >= 256) CLog.Error("Too many changed properties of EntityState!");
		WriteByte((byte)lc); //改变的属性

		int bits = 0;
		// int oldSize 
		float fullFloat = 0;
		int trunc = 0;
		for(int i = 0; i < lc; i++){
			offset = EntityState.entityStateFields[i].offset;
			bits = EntityState.entityStateFields[i].bits;
			fromF = (int *)(startF + offset);
			toF = (int *)(startT + offset);

			if(*fromF != *toF){
				WriteBits(0,1); //没有变化
				continue;
			}

			WriteBits(1,1); //变化了
			if(bits == 0){ //浮点数
				fullFloat = *(float *)toF;
				trunc = (int)fullFloat;

				if(fullFloat == 0.0f){
					WriteBits(0, 1);
				}else{
					WriteBits(1,1);
					if(trunc == fullFloat && trunc + CConstVar.FLOAT_INT_BIAS >= 0 && 
						trunc + CConstVar.FLOAT_INT_BIAS < (1 << CConstVar.FLOAT_INT_BITS)){
							//作为小的整数发送
							WriteBits(0,1);
							WriteBits(trunc + CConstVar.FLOAT_INT_BIAS, CConstVar.FLOAT_INT_BITS);
						}else{
							WriteBits(1,1);
							WriteBits(*toF, 32);
						}
				}
			}else{
				if(*toF == 0){
					WriteBits(0, 1);
				}else{
					WriteBits(1, 1);
					//整数
					WriteBits(*toF, bits);
				}
			}
		}
	}
	
	public void WriteFirstInt(int value){
		// var byts = BitConverter.GetBytes(value);
		// Array.Copy(byts, bytes, bytes.Length);
		bytes[0] = (byte)value;
		bytes[1] = (byte)(value << 8);
		bytes[2] = (byte)(value << 16);
		bytes[3] = (byte)(value << 24);
	}

	public void WriteInt(int value)
	{
		WriteBits(value, 32);
	}

	public void WriteShort(short value)
	{
		var tmp = 0x8000;
		if (value < ((short)tmp) || value > (short)0x7fff){
			CLog.Error("MSG_WriteShort: range error");
		}
		WriteBits(value, 16);
	}

	public void WriteByte(byte value)
	{
		WriteBits(value, 8);
	}

	public void WriteDeltaUserCmdKey(int key, ref UserCmd from, ref UserCmd to){
		if(to.serverTime - from.serverTime < 256){
			WriteBits(1,1);
			WriteBits(to.serverTime - from.serverTime, 8);
		}else{
			WriteBits(0, 1);
			WriteBits(to.serverTime, 32);
		}

		if(from.angles[0] == to.angles[0] &&
		   from.angles[1] == to.angles[1] &&
		   from.angles[2] == to.angles[2] &&
		   from.forwardmove == to.forwardmove &&
		   from.rightmove == to.rightmove &&
		   from.upmove == to.upmove &&
		   from.buttons == to.buttons){
			   WriteBits(0, 1);
			   return;
		}

		key ^= to.serverTime;
		WriteBits(1,1);
		WriteDeltaKey(key, from.angles[0], to.angles[0], 16);
		WriteDeltaKey(key, from.angles[1], to.angles[1], 16);
		WriteDeltaKey(key, from.angles[2], to.angles[2], 16);
		WriteDeltaKey(key, from.forwardmove, to.forwardmove, 8);
		WriteDeltaKey(key, from.rightmove, to.rightmove, 8);
		WriteDeltaKey(key, from.upmove, to.upmove, 8);
		WriteDeltaKey(key, from.buttons, to.buttons, 16);
	}

	public void WriteDeltaKey(int key, int oldV, int newV, int bits){
		if(oldV == newV){
			WriteBits(0, 1);
			return;
		}
		WriteBits(1, 1);
		WriteBits(newV ^ key, bits);
	}


	unsafe public void WriteBits(int value, int bits)
	{
		int i;
		//溢出检查
		if(CConstVar.BUFFER_LIMIT - curSize < 4){
			overflowed = true;
			return;
		}

		if(bits == 0 || bits < -31 || bits > 32){
			CLog.Error("Packet WriteBits: bad bits {0}", bits);
		}

		if(bits != 32){
			if(bits > 0){
				if((value > ((1 << bits) - 1)) || value < 0){
					// overflowes++;
				}
			}else{
				int r = 1 << (bits - 1);
				if((value > r - 1) || value < -r){
					// overflowes++;
				}
			}
		}
		if(bits < 0){
			bits = - bits;
		}
		if(oob){
			if(bits == 8){
				bytes[curSize] = (byte)value;
				curSize += 1;
				bit += 8;
			}else if(bits == 16){
				// value = CopyLittleShort();
				bytes[curSize+1] = (byte)(value >> 8);
				bytes[curSize] = (byte)value;

				curSize += 2;
				bit += 16;
			}else if(bits ==32){
				// value = CopyLittleLong();
				bytes[curSize+3] = (byte)(value >> 24);
				bytes[curSize+2] = (byte)(value >> 16);
				bytes[curSize+1] = (byte)(value >> 8);
				bytes[curSize] = (byte)value;

				// int re1 =  (int)bytes[curSize];
				// int result = (bytes[curSize+3] << 24) + (bytes[curSize+2] << 16) + (bytes[curSize +1] << 8) + (int)bytes[curSize];
				curSize += 4;
				bit += 32;
			}else{
				CLog.Error("Can't write {0} bits", bits);
			}
		}else{
			value &= (int)(0xffffffff >> (32 - bits));
			if((bits & 7) != 0){
				int nbits = bits & 7;
				for(i = 0; i < nbits; i++){
					// fixed(byte* tmp = &(bytes[0])){
					// 	fixed(int* b = &bit){
					// 		HuffmanMsg.Huff_putBit((value & 1), tmp, b);
					// 	}
					// }
					HuffmanMsg.PutBit((value & 1), bytes, ref bit);
					value = (value >> 1);
				}
				bits = bits - nbits;
			}
			if(bits != 0){
				for(i = 0; i < bits; i += 8){
					// HuffmanMsg.OffsetTransmit(HuffmanMsg.compresser, (value & 0xff), bytes, ref bit);
					fixed(byte* f = &bytes[0]){
						fixed(int* b = &bit){
							HuffmanMsg.HuffOffsetTransmit((value & 0xff), f, b);
						}
					}
					value = (value >> 8);
				}
			}
			curSize = (bit >> 3) + 1;
		}
	}

	public void WriteString(string value)
	{
		if(string.IsNullOrEmpty(value)){
			WriteByte(System.Text.Encoding.Default.GetBytes("")[0]);
		}else{
			if(value.Length > CConstVar.MAX_STRING_CHARS){
				WriteByte(System.Text.Encoding.Default.GetBytes("")[0]);
				CLog.Error("WriteString: MAX_STRING_CHARS overflow");
				return;
			}

			var byts = System.Text.Encoding.Default.GetBytes(value);
			for(int i = 0; i < byts.Length; i++){
				if(byts[i] > 127 || byts[i] == (byte)'%'){
					byts[i] = (byte)'.';
				}
			}

			// WriteBufferData(byts, -1, byts.Length);
			WriteData(byts, byts.Length);
		}
	}

	public void WriteString(char[] value)
	{
		if(value != null){
			// WriteByte(System.Text.Encoding.Default.GetBytes("")[0]);
			WriteData(new byte[1]{0},1);
		}else{
			if(value.Length > CConstVar.MAX_STRING_CHARS){
				// WriteByte(System.Text.Encoding.Default.GetBytes("")[0]);
				WriteData(new byte[1]{0},1);
				CLog.Error("WriteString: MAX_STRING_CHARS overflow");
				return;
			}

			int len = 0;
			// var byts = System.Text.Encoding.Default.GetBytes(value);
			byte[] byts = new byte[CConstVar.MAX_STRING_CHARS];
			for(int i = 0; i < value.Length; i++){
				if(value[i] > 127 || value[i] == (byte)'%'){
					value[i] = '.';
				}
				byts[i] = (byte)value[i];
				if(value[i] != '\0'){
					len = i;
				}
			}

			// WriteBufferData(byts, -1, byts.Length);
			WriteData(byts, len);
		}
	}

	public void Copy(MsgPacket dest, byte[] data, int length){
		if(length < curSize){
			CLog.Error("Msg Copy: can't copy into a smaller MsgPacket buffer");
		}

		Array.Copy(data, 0, dest.Data, 0, length);
		curSize = length;
		dest.AllowOverflow = allowOverflow;
		dest.Overflowed = overflowed;
		dest.Oob = oob;
		dest.Bit = bit;
	}

	public void Clear(){
		curSize = 0;
		overflowed = false;
		bit = 0;
	}

}
