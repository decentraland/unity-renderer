using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridContainerComponentModel
{
    public int numColumns;
    public Vector2Int itemSize;
    public Vector2Int spaceBetweensItems;
    public List<BaseComponentView> items;
}