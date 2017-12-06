
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
		
public class CMessageID{

	private static Dictionary<int, Type> dicMsgID;
	private static Dictionary<Type, int> dicMsgType;

	public static Type GetMsgType(int msgID)
	{
		return dicMsgID[msgID];
	}

	public static int GetMsgID(Type type)
	{
		return dicMsgType[type];
	}
	
	public CMessageID(){
		dicMsgID = new Dictionary<int, Type>();
		dicMsgType = new Dictionary<Type, int>();
		dicMsgID.Add(10001, typeof(CsAccountLogin));
		dicMsgType.Add(typeof(CsAccountLogin), 10001);
		dicMsgID.Add(10002, typeof(ScAccountLogin));
		dicMsgType.Add(typeof(ScAccountLogin), 10002);
		dicMsgID.Add(10003, typeof(CsPlayerBasic));
		dicMsgType.Add(typeof(CsPlayerBasic), 10003);
		dicMsgID.Add(10004, typeof(ScPlayerBasic));
		dicMsgType.Add(typeof(ScPlayerBasic), 10004);
	}
}
