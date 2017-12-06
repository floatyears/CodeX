using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//目标队伍
public enum UnitTargetTeam{
    UNIT_TARGET_NONE                = 0,
    
    UNIT_TARGET_TEAM_FRIENDLY,

    UNIT_TARGET_TEAM_ENEMY,

    UNIT_TARGET_TEAM_BOTH,

    UNIT_TARGET_TEAM_CUSTOM,
}

//目标类型
public enum UnitTargetType {

    UNIT_TARGET_NONE                = 0,
    
    UNIT_TARGET_ALL,

    UNIT_TARGET_BASIC,               

    UNIT_TARGET_BUILDING,

    UNIT_TARGET_HERO,

    UNIT_TARGET_MEDICAL,

    UNIT_TARGET_OTHER,

	
}

//目标flags，用于筛选带有特殊状态的目标
public enum UnitTargetFlags{
    UNIT_TARGET_FLAG_NONE           = 0,

    UNIT_TARGET_FLAG_DEAD,

    UNIT_TARGET_FLAG_MANA_ONLY,

    UNIT_TARGET_FLAG_MEELE_ONLY,

    UNIT_TARGET_FLAG_RANGED_ONLY,

    UNIT_TARGET_FLAG_NO_INVIS,

    UNIT_TARGET_FLAG_INVULNERABLE,

    UNIT_TARGET_FLAG_NOT_ATTACK_IMMUNE,

    UNIT_TARGET_FLAG_NOT_ILLUSION,

    UNIT_TARGET_FLAG_NOT_MAGIC_IMMUNE_ALLIES,

    UNIT_TARGET_FLAG_NOT_SUMMONED,

    UNIT_TARGET_FLAG_PLAYER_CONTROLLED,

}
