using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CarouselComponentModel : BaseComponentModel
{
    public List<BaseComponentView> items;
    public float spaceBetweenItems = 10f;
    public float timeBetweenItems = 3f;
    public float animationTransitionTime = 1f;
    public AnimationCurve animationCurve;
    public Color backgroundColor;
    public bool showManualControls = true;
}