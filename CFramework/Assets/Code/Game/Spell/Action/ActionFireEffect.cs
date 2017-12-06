using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionFireEffect : BaseAction {

	private string effectName;

	private EffectAttachType effectAttachType;

	private Dictionary<int, List<int>> controlPoints;

	private int targetPoint;

	private int effectRadius;

	private int effectDurationScale;

	private int effectLifeDurationScale;

	private Color effectColorA;

	private Color effectColorB;

	private int effectAlphaScale;

	protected override bool ExeOnTarget(BaseEntity tar)
	{
		return true;
	}

}
