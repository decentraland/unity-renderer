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
    
    [Header("Flexible configuration")]
    //The min width and height for the item
    public float minWidthForFlexibleItems;
    public float minHeightForFlexibleItems;
    
    //It will take the recommended values if it can
    public float recommendedWidthForFlexibleItems;
    public float recommendedHeightForFlexibleItems;

    //If this property is set to true, if the the recommended height or width can be achieved, it will maintain the same ration
    public bool sameHeightAndWidhtFlexibleItem = false;
}