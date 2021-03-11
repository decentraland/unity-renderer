using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatalogAssetGroupAdapter : MonoBehaviour
{
    public TextMeshProUGUI categoryTxt;
    public GameObject categoryContentGO;
    public System.Action<CatalogItem> OnCatalogItemClicked;
    public System.Action<CatalogItem, CatalogItemAdapter> OnCatalogItemFavorite;
    public System.Action<CatalogItem, CatalogItemAdapter, BaseEventData> OnAdapterStartDragging;
    public System.Action<PointerEventData> OnAdapterDrag, OnAdapterEndDrag;

    [Header("Prefab References")]
    public GameObject catalogItemAdapterPrefab;

    public void SetContent(string category, List<CatalogItem> catalogItemList)
    {
        categoryTxt.text = category.ToUpper();
        RemoveAdapters();
        foreach (CatalogItem catalogItem in catalogItemList)
        {
            CatalogItemAdapter adapter = Instantiate(catalogItemAdapterPrefab, categoryContentGO.transform).GetComponent<CatalogItemAdapter>();
            adapter.SetContent(catalogItem);
            AddAdapter(adapter);
        }
    }

    public void AddAdapter(CatalogItemAdapter adapter)
    {
        adapter.OnCatalogItemClicked += CatalogItemClicked;
        adapter.OnCatalogItemFavorite += CatalogItemFavorite;
        adapter.OnAdapterStartDrag += AdapterStartDragging;
        adapter.OnAdapterDrag += OnDrag;
        adapter.OnAdapterEndDrag += OnEndDrag;
    }

    public void RemoveAdapters()
    {
        for (int i = 0; i < categoryContentGO.transform.childCount; i++)
        {
            GameObject toRemove = categoryContentGO.transform.GetChild(i).gameObject;
            Destroy(toRemove);
        }
    }

    private void OnDrag(PointerEventData eventData)
    {
        OnAdapterDrag?.Invoke(eventData);
    }

    private void OnEndDrag(PointerEventData eventData)
    {
        OnAdapterEndDrag?.Invoke(eventData);
    }

    private void CatalogItemClicked(CatalogItem catalogItemClicked)
    {
        OnCatalogItemClicked?.Invoke(catalogItemClicked);
    }

    private void CatalogItemFavorite(CatalogItem catalogItemClicked, CatalogItemAdapter adapter)
    {
        OnCatalogItemFavorite?.Invoke(catalogItemClicked, adapter);
    }

    private void AdapterStartDragging(CatalogItem catalogItemClicked, CatalogItemAdapter adapter, BaseEventData data)
    {
        OnAdapterStartDragging?.Invoke(catalogItemClicked, adapter, data);
    }
}
