using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  This class is a generic implementation for a view that list items. It should be use when you want to list items in the same content panel
///
///  The general logic of this class is that it will instantiate one adapter per one item in the list you set. The adapter will manage the display of the content data,
///  while this class will manage the availability of the adapters. This way you can switch adapter to show them in different forms or show different information of the same data
/// 
///  You will need to implement at least one adapter in the AddAdapters function and instantiated it as a children of the contentPanel.
/// 
///  You are required to implement the AddAdapters with your own adapter logic, this is done in order to maintain it more flexible.
/// </summary>
public class ListView<T> : MonoBehaviour
{

    public Transform contentPanelTransform;
    public GameObject emptyContentMark;

    protected List<T> contentList = new List<T>();
    public void SetContent(List<T> content)
    {
        contentList = content;
        RefreshDisplay();
    }

    public virtual void RefreshDisplay()
    {
        RemoveAdapters();
        AddAdapters();
        CheckEmptyContent();
    }

    public virtual void AddAdapters() { }

    public virtual void RemoveAdapters()
    {
        if (contentPanelTransform == null ||
            contentPanelTransform.transform == null ||
            contentPanelTransform.transform.childCount <= 0)
            return;


        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            GameObject toRemove = contentPanelTransform.transform.GetChild(i).gameObject;
            if (toRemove != null)
                Destroy(toRemove);
        }
    }

    private void CheckEmptyContent()
    {
        bool contentIsEmpty = contentList.Count == 0;

        if (contentPanelTransform != null)
            contentPanelTransform.gameObject.SetActive(!contentIsEmpty);

        if (emptyContentMark != null)
            emptyContentMark.SetActive(contentIsEmpty);
    }
}