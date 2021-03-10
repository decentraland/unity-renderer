using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemActionPaddingDeleter : MonoBehaviour
{
    public VerticalLayoutGroup verticalLayoutGroup;

    void Start()
    {
        if(GetComponentsInParent<SmartItemListView>().Length > 1)
        {
            verticalLayoutGroup.padding.left = 0;
            verticalLayoutGroup.padding.right = 0;
        }
    }
}
