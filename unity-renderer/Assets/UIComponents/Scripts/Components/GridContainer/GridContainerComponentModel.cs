using System;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[Serializable]
public class GridContainerComponentModel : BaseComponentModel
{
    public Constraint constraint = Constraint.FixedColumnCount;
    public int constraintCount = 3;
    public bool adaptItemSizeToContainer = false;
    public Vector2 itemSize;
    public Vector2 spaceBetweenItems;
    public float minWidthForFlexibleItems;
}