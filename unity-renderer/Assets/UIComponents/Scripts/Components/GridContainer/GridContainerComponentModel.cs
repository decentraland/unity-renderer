using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[Serializable]
public class GridContainerComponentModel
{
    public Constraint constraint = Constraint.FixedColumnCount;
    public int constranitCount = 3;
    public bool adaptItemSizeToGridSize = false;
    public Vector2 itemSize;
    public Vector2 spaceBetweenItems;
    public List<BaseComponentView> items;
}