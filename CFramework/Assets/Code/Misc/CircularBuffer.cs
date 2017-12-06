using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//环形缓冲
public class CircularBuffer<T> {

	private T[] buffer;
	
	private int bufferSize;

	private int head;

	private int tail;

	private int length;

	public CircularBuffer(int bufferSize)
	{
		this.buffer = new T[bufferSize];
		this.bufferSize = bufferSize;
		this.head = bufferSize - 1;
	}

	public void Enqueue(T item)
	{
		head = NextPos(head);
		buffer[head] = item;
		if(IsFull)
		{
			tail = NextPos(tail);
			CLog.Error(typeof(T).GetType().ToString(), " Circular Buffer is full, the tail item will be overrideed, and some error will occur!");
		}else{
			length++;
		}
	}

	public T Dequeue()
	{
		if(!IsEmpty)
		{
			T tmp = buffer[tail];
			tail = NextPos(tail);
			length--;
			return tmp;
		}else
		{
			CLog.Error(typeof(T).GetType().ToString(), " Circular Buffer is Empty, Dequeue failed!");
			return default(T);
		}
	}

	private int NextPos(int pos)
	{
		return (pos + 1) % bufferSize;
	}

	//是否是空
	public bool IsEmpty
	{
		get{
			return length == 0;
		}
	}

	public bool IsFull{
		get{
			return length == bufferSize;
		}
	}
}
