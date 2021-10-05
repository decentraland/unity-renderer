using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridContainerComponentModel
{
    public int numColumns;
    public bool autoAdaptItemSizeToContainerWidth = false;
    public Vector2 itemSize;
    public Vector2 spaceBetweenItems;
    public List<BaseComponentView> items;
}