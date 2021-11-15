using System;
using UnityEngine;

[Serializable]
public class CarouselComponentModel : BaseComponentModel
{
    public float spaceBetweenItems = 10f;
    public float timeBetweenItems = 3f;
    public float animationTransitionTime = 1f;
    public AnimationCurve animationCurve;
    public Color backgroundColor;
    public bool showManualControls = true;
    public bool automaticTransition = true;
    public bool pauseOnFocus = true;
}