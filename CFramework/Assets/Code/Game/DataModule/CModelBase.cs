using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlatBuffers;

public interface IModel {

	void Init();

	void Dispose();
}

public class CModelScene : IModel
{
	public void Init()
	{

	}

	public void Dispose()
	{

	}
} 

public class CModelPlayer : IModel
{	
	public int roleID;

	public int roleName;

	public void Init()
	{
		
	}

	public void CsAccountLogin()
	{
		CNetwork.Instance.SendMsg((fb)=>{
			fb.Finish(ScPlayerBasic.CreateScPlayerBasic(fb,10001,23,24,fb.CreateString("123124"), 26.0, 27f, 1).Value);
		});
		//CsPlayerBasic.StartCsPlayerBasic(fb);
	}

	public void OnScPlayerBasic(ByteBuffer buffer, ScPlayerBasic info)
	{
		ScPlayerBasic.GetRootAsScPlayerBasic(buffer);
	}

	public void Dispose()
	{

	}
}
