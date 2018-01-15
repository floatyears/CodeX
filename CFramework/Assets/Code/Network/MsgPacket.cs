using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
	public void WriteData(byte[] data, int start, int length, int dataStart = 0)
	{
		if(start < 0) start = curSize;
		Array.Copy(data, dataStart, bytes, start, length);
		curSize = start + length;
		curPos = start + length;
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
		return System.BitConverter.ToInt32(Data, 0);
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

	public int ReadBits(int bits)
	{
		int value, get = 0, i, nbits;
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
					value |= (HuffmanMsg.GetBit(bytes, ref bit) << i);
				}
				bits = bits - nbits;
			}
			if(bits != 0){
				for(i=0; i < bits; i+= 8){
					HuffmanMsg.OffsetReceive(HuffmanMsg.decompresser.tree, ref get, bytes, ref bit);
					value |= (get << (i + nbits));
				}
			}
			curPos = (bit >> 3) + 1;
		}

		if(sgn){
			if((value & ( 1 << (bits - 1))) != 0){
				value |= -1 ^ ((1 << bits) - 1);
			}
		}
		return value;
	}

	private short CopyLittleShort(){
		byte[] tmp = new byte[2];
		tmp[1] = bytes[curPos];
		tmp[0] = bytes[curPos+1];
		return System.BitConverter.ToInt16(tmp, 0);
	}

	private int CopyLittleLong(){
		byte[] tmp = new byte[4];
		tmp[0] = bytes[curPos+3];
		tmp[1] = bytes[curPos+2];
		tmp[2] = bytes[curPos+1];
		tmp[3] = bytes[curPos];
		return System.BitConverter.ToInt32(tmp, 0);
	}

	public int ReadPort(){
		return System.BitConverter.ToInt16(bytes, 8);
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
		
		readStr[l] = (char)0;
		return new string(readStr, 0, l);
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
	public void ReadDeltaEntity(ref EntityState from, ref EntityState to, int number)
	{
		int i, lc;
		int numFields;
		NetField field;
		int fromF, toF;
		int print;
		int trunc;
		int startBit, endBit;

		if(number < 0 || number >= CConstVar.MAX_GENTITIES){
			CLog.Error("Bad delta entity number: %d", number);
		}

		if(bit == 0){
			startBit = curPos * 8 - CConstVar.GENTITYNUM_BITS;
		}else{
			startBit = (curPos - 1) * 8 + bit - CConstVar.GENTITYNUM_BITS;
		}

		//检查是否要移除
		if(ReadBits(1) == 1){
			to = new EntityState();
			to.entityIndex = CConstVar.MAX_GENTITIES - 1;
			if(CConstVar.ShowNet != 0 && (CConstVar.ShowNet >= 2 || CConstVar.ShowNet == -1)){
				CLog.Info("remove entity: {0}", number);
			}
			return;
		}

		//检查是否无压缩
		if(ReadBits(1) == 0){
			to = from;
			to.entityIndex = number;
			return;
		}

		numFields = CConstVar.entityStateFields.Length;
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

		for(i = 0; i < lc; i++){
			field = CConstVar.entityStateFields[i];
			// fromF = from + field.offset;
			// toF = to 

			//最好的方式还是c++直接操作内存数据，利用反射效率比较低
			if(ReadBits(1) == 0){ //没有变化
				field.name.SetValue(to, field.name.GetValue(from));
			}else{
				if(field.bits == 0){
					if(ReadBits(1) == 0){
						field.name.SetValue(to, 0.0f);
					}else{
						if(ReadBits(1) == 0){
							//积分浮点数
							trunc = ReadBits(CConstVar.FLOAT_INT_BITS);

							//偏移允许正的部分和负的部分是一样大小的
							trunc -= CConstVar.FLOAT_INT_BIAS;
							field.name.SetValue(to, trunc);

							if(print > 0){
								CLog.Info("Read Delta Entity {0}:{1}", field.name, trunc);
							}
						}else{
							//完整的浮点值
							field.name.SetValue(to, ReadBits(32));
							if(print > 0){
								CLog.Info("Read Delta Entity {0}:{1}", field.name, field.name.GetValue(to));
							}
						}
					}
				}else{
					if(ReadBits(1) == 0){
						field.name.SetValue(to, 0);
					}else{
						field.name.SetValue(to, ReadBits(field.bits));
						if(print > 0){
							CLog.Info("Read Delta Entity {0}:{1}", field.name, field.name.GetValue(to));
						}
					}
				}
			}
		}
		for(i = lc; i < numFields; i++){
			field = CConstVar.entityStateFields[i];
			//没有变化
			field.name.SetValue(to, field.name.GetValue(from));
		}

		if(print > 0){
			if(bit == 0){
				endBit = curPos * 8 - CConstVar.GENTITYNUM_BITS;
			}else{
				endBit = (curPos - 1) * 8 + bit - CConstVar.GENTITYNUM_BITS;
			}
			CLog.Info("Read Delta Entity Finished. ({0} bits)", endBit - startBit);
		}

	}

	public void ReadDeltaPlayerstate(PlayerState from, PlayerState to)
	{
		int i,lc;
		int bits;

	}

	public void WriteDeltaPlayerstate(PlayerState from, PlayerState to)
	{

	}

	public void WriteDeltaEntity(ref EntityState from, ref EntityState to, bool force)
	{

	}
	
	public void WriteFirstInt(int value){
		var byts = BitConverter.GetBytes(value);
		Array.Copy(byts, bytes, bytes.Length);
	}

	public void WriteInt(int value)
	{
		WriteBits(value, 32);
	}

	public void WriteShort(short value)
	{
		WriteBits(value, 16);
	}

	public void WriteByte(byte value)
	{
		WriteBits(value, 8);
	}

	public void WirteDeltaUserCmdKey(int key, ref UserCmd from, ref UserCmd to){
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


	public void WriteBits(int value, int bits)
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
				value = CopyLittleShort();
				curSize += 2;
				bit += 16;
			}else if(bits ==32){
				value = CopyLittleLong();
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
					HuffmanMsg.PutBit((value & 1), bytes, ref bit);
					value = (value >> 1);
				}
				bits = bits - nbits;
			}
			if(bits != 0){
				for(i = 0; i < bits; i += 8){
					HuffmanMsg.OffsetTransmit(HuffmanMsg.compresser, (value & 0xff), bytes, bit);
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

			WriteData(byts, -1, byts.Length);
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
