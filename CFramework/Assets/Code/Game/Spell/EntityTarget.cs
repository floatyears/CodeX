using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EntityTarget {

	private UnitTargetTeam tarTeam;

    private UnitTargetType tarType;

    private UnitTargetFlags tarFlags;

    private BaseEntity[] curTarget;

    //表示没有上限
    private int maxTarget = -1;

    private bool random;

    private Vector3 center;

    private float radius;

    public delegate void TargetCallback(BaseEntity target);

    public UnitTargetTeam TargetTeam{
        get{
            return tarTeam;
        }
    }

    public UnitTargetType TargetType{
        get{
            return tarType;
        }
    }

    public UnitTargetFlags TargetFlags{
        get{
            return tarFlags;
        }
    }
    
    public EntityTarget(UnitTargetTeam team, UnitTargetType type, UnitTargetFlags flags, int maxTar = -1, Vector3 center = default(Vector3)){
        this.tarTeam = team;
        this.tarType = type;
        this.tarFlags = flags;
    }

    public BaseEntity[] GetTarget()
    {
        return curTarget;
    }

    public BaseEntity GetFirstTarget()
    {
        return curTarget[0];
    }

    public void ForEachTarget(TargetCallback callback)
    {
        int count = curTarget.Length;
        for(int i = 0; i < count; i++)
        {
            callback(curTarget[i]);
        } 
    }

}
