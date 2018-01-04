using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

	//把packet中的数据读出到data中
	public void ReadData(byte[] data, int start, int length)
	{
		if(start < 0) start = curSize;
		Array.Copy(bytes, start, data, 0, length);
		// curSize = start + length;
		curPos = start + length;
	}

	//把外面的数据写入到packet中
	public void WriteData(byte[] data, int start, int length)
	{
		if(start < 0) start = curSize;
		Array.Copy(data, 0, bytes, start, length);
		// curSize = start + length;
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

	public int ReadInt(int start = -1)
	{
		if(start < 0)
		{
			start = curPos;
		}else{

		}
		return 0;
	}

	public int ReadByte()
	{
		return 0;
	}

	public int ReadBits(int bits)
	{
		int value, get, i, nbits;
		bool sgn;

		value = 0;
		if(bits < 0)
		{
			bits = -bits;
			sgn = true;
		}else{
			sgn = false;
		}
		if(oob)
		{
			if(bits == 8)
			{
				value = bytes[curPos];
				curPos++;
				bit += 8;
			}else if(bits == 16)
			{
				short temp;

			}else if(bits == 32)
			{

			}else{
				CLog.Error("can't read %d bits", bits);
			}
		}else{
			nbits = 0;
			if((bits & 7) != 0)
			{
				nbits = bits & 7;
				for(i = 0; i < nbits; i++)
				{
					// value |= (Huff)
				}
				bits = bits - nbits;
			}
			if(bits != 0)
			{
				for(i=0; i < bits; i+= 8)
				{
					// value |= (get << (i + nbits));
				}
			}
		}

		if(sgn)
		{
			if((value & ( 1 << (bits - 1))) != 0){
				value |= -1 ^ ((1 - bits) - 1);
			}
		}
		return value;
	}

	public short ReadShot()
	{
		return 0;
	}

	//读取char
	public string ReadChars(int len)
	{
		return "";
	}

	public string ReadStringLine()
	{
		return "";
	}

	public string ReadString()
	{
		return "";
	}

	public string ReadBigString()
	{
		return "";
	}

	public void WriteInt(int value, int pos = -1)
	{
		if(pos < 0)
		{
			pos = curPos;
		}

		
	}

	public void WriteShort(short value)
	{
		
		
	}

	public void WriteByte(byte value)
	{
		
		
	}

	public void WriteString(string value)
	{
		
		
	}

}
