using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlatBuffers;
using System;

public class CModelBase{
	public delegate void UpdateHandler();

	public UpdateHandler update;

	public virtual void Init(){

	}

	public virtual void Dispose(){

	}
}

// public interface IModel {

	
// }

public class CModelScene : CModelBase
{
	public override void Init()
	{

	}

	public override void Dispose()
	{

	}
} 


