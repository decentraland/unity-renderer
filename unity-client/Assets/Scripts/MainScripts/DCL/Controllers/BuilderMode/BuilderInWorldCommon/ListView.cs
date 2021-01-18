using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  This class is a generic implementantion for a view that list items. It should be use when you want to list items in the same content panel
///
///  The general logic of this class is that it will instanciate one adapter per one item in the list you set. The adapter will manage the display of the content data,
///  while this class will manage the availability of the adapters. This way you can switch adapter to show them in diferrent forms or show different information of the same data
/// 
///  You will need to implement at least one adapter in the AddAdapters function and instanciated it as a children of the contentPanel.
/// 
///  You are required to implement the AddAdapters with your own adapter logic, this is done in order to mantain it more flexible.
/// </summary>
public class ListView<T> : MonoBehaviour
{

    public Transform contentPanelTransform;

    protected List<T> contentList;
    public void SetContent(List<T> content)
    {
        contentList = content;
        RefreshDisplay();
    }

    public virtual void RefreshDisplay()
    {
        RemoveAdapters();
        AddAdapters();
    }

    public virtual void AddAdapters()
    {

    }

    public virtual void RemoveAdapters()
    {
        if (contentPanelTransform == null||
            contentPanelTransform.transform == null ||
            contentPanelTransform.transform.childCount <= 0)
            return;


        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            GameObject toRemove = contentPanelTransform.transform.GetChild(i).gameObject;
            if(toRemove != null)
                Destroy(toRemove);
        }
    }
}
