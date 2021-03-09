using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DCL/Create AnimationCurveContainer", fileName = "AnimationCurveContainer", order = 0)]
public class AnimationCurveContainer : ScriptableObject
{
    public AnimationCurve[] curves;
}