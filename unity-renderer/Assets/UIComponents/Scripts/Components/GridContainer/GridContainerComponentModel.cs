using System;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.UI.GridLayoutGroup;

[Serializable]
public class GridContainerComponentModel : BaseComponentModel
{
    public Constraint constraint = Constraint.FixedColumnCount;
    public int constraintCount = 3;
    [FormerlySerializedAs("adaptItemSizeToContainer")] public bool adaptHorizontallyItemSizeToContainer = false;
    public bool adaptVerticallyItemSizeToContainer = false;
    public Vector2 itemSize;
    public Vector2 spaceBetweenItems;
    public float minWidthForFlexibleItems;
}