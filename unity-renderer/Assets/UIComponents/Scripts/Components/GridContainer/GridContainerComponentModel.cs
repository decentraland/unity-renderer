using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[Serializable]
public class GridContainerComponentModel : BaseComponentModel
{
    public Constraint constraint = Constraint.FixedColumnCount;
    public int constranitCount = 3;
    public bool adaptItemSizeToContainer = false;
    public Vector2 itemSize;
    public Vector2 spaceBetweenItems;
    public float minWidthForFlexibleItems;
    public List<BaseComponentView> items;
}