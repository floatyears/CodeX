﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class AnimationStateWrap
{
	public static void Register(LuaState L)
	{
		L.BeginEnum(typeof(AnimationState));
		L.RegVar("ACT_IDLE", get_ACT_IDLE, null);
		L.RegVar("ACT_RUN", get_ACT_RUN, null);
		L.RegVar("ACT_ATTACK_01", get_ACT_ATTACK_01, null);
		L.RegFunction("IntToEnum", IntToEnum);
		L.EndEnum();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_ACT_IDLE(IntPtr L)
	{
		ToLua.Push(L, AnimationState.ACT_IDLE);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_ACT_RUN(IntPtr L)
	{
		ToLua.Push(L, AnimationState.ACT_RUN);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_ACT_ATTACK_01(IntPtr L)
	{
		ToLua.Push(L, AnimationState.ACT_ATTACK_01);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IntToEnum(IntPtr L)
	{
		int arg0 = (int)LuaDLL.lua_tonumber(L, 1);
		AnimationState o = (AnimationState)arg0;
		ToLua.Push(L, o);
		return 1;
	}
}

