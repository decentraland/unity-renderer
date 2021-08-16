using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatalogItemDropController
{
    public CatalogGroupListView catalogGroupListView;
    public event Action<CatalogItem> OnCatalogItemDropped;

    public void CatalogitemDropped()
    {
        CatalogItemAdapter adapter = catalogGroupListView.GetLastCatalogItemDragged();
        if (adapter == null)
            return;
        CatalogItem catalogItem = adapter.GetContent();
        GameObject.Destroy(adapter.gameObject);
        OnCatalogItemDropped?.Invoke(catalogItem);
    }
}