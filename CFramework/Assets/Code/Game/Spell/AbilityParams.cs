using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AbilityParams {
	public string textureName;

	public float coolDown;

	public float duration;

	public int castRange;

	public float castPoint;

	public int manaCost;

	public float channelThink;

	public float channelTime;

	public int behavior;

	public int requiredLevel;

	public int levelBetweenUpgrades;

	public int toggleState;

	public DamageType damageType;

	public SpellImmunityType spellImmunityType;

	public AbilityType abilityType;

	public AnimationState abilityCastAnimation;

	public int animationPlaybackRate;

	public float animationIgnoresModeScale;

	public UnitTargetTeam abilityUnitTargetTeam;

	public UnitTargetType abilityUnitTargetType;

	public UnitTargetFlags abilityUnitTargetFlags;

}

public struct AbilityBehavior{


	public const int ABILITY_BEHAVIOR_HIDDEN = 1;

	public const int ABILITY_BEHAVIOR_PASSIVE = 1 << 1;

	public const int ABILITY_BEHAVIOR_NO_TARGET = 1 << 2;

	public const int ABILITY_BEHAVIOR_UNIT_TARGET = 1 << 3;

	public const int ABILITY_BEHAVIOR_POINT = 1 << 4;

	public const int ABILITY_BEHAVIOR_AOE = 1 << 5;

	public const int ABILITY_BEHAVIOR_NOT_LEARNABLE = 1 << 6;

	public const int ABILITY_BEHAVIOR_CHANNELLED = 1 << 7;

	public const int ABILITY_BEHAVIOR_ITEM = 1 << 8;

	public const int ABILITY_BEHAVIOR_TOGGLE = 1 << 9;

	public const int ABILITY_BEHAVIOR_DIRECTIONAL = 1 << 10;

	public const int ABILITY_BEHAVIOR_IMMEDIATE = 1 << 11;

	public const int ABILITY_BEHAVIOR_AUTOCAST = 1 << 12;

	public const int ABILITY_BEHAVIOR_NOASSIST = 1 << 13;

	public const int ABILITY_BEHAVIOR_AURA = 1 << 14;

	public const int ABILITY_BEHAVIOR_ATTACK = 1 << 15;

	public const int ABILITY_BEHAVIOR_DONT_RESUME_MOVEMENT = 1 << 16;

	public const int ABILITY_BEHAVIOR_ROOT_DISABLES = 1 << 17;

	public const int ABILITY_BEHAVIOR_UNRESTRICTED = 1 << 18;

	public const int ABILITY_BEHAVIOR_IGNORE_PSEUDO_QUEUE = 1 << 19;

	public const int ABILITY_BEHAVIOR_IGNORE_CHANNEL = 1 << 20;

	public const int ABILITY_BEHAVIOR_DONT_CANCEL_MOVEMENT = 1 << 21;

	public const int ABILITY_BEHAVIOR_DONT_ALERT_TARGET = 1 << 22;

	public const int ABILITY_BEHAVIOR_DONT_RESUME_ATTACK = 1 << 23;

	public const int ABILITY_BEHAVIOR_NORMAL_WHEN_STOLEN = 1 << 24;

	public const int ABILITY_BEHAVIOR_IGNORE_BACKSWING = 1 << 25;

}

public enum AbilityType{
	ABILITY_TYPE_BASIC = 1,

	ABILITY_TYPE_ULTIMATE = 2,

	ABILITY_TYPE_ATTRIBUTES = 3,

}
