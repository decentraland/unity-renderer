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
    public System.Action<PointerEventData, CatalogItemAdapter> OnPointerEnterInAdapter;
    public System.Action<PointerEventData, CatalogItemAdapter> OnPointerExitInAdapter;

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
            SubscribeToEvents(adapter);
        }
    }

    public void SubscribeToEvents(CatalogItemAdapter adapter)
    {
        adapter.OnCatalogItemClicked += CatalogItemClicked;
        adapter.OnCatalogItemFavorite += CatalogItemFavorite;
        adapter.OnAdapterStartDrag += AdapterStartDragging;
        adapter.OnAdapterDrag += OnDrag;
        adapter.OnAdapterEndDrag += OnEndDrag;
        adapter.OnPointerEnterInAdapter += OnPointerEnter;
        adapter.OnPointerExitInAdapter += OnPointerExit;
    }

    public void UnsubscribeToEvents(CatalogItemAdapter adapter)
    {
        adapter.OnCatalogItemClicked -= CatalogItemClicked;
        adapter.OnCatalogItemFavorite -= CatalogItemFavorite;
        adapter.OnAdapterStartDrag -= AdapterStartDragging;
        adapter.OnAdapterDrag -= OnDrag;
        adapter.OnAdapterEndDrag -= OnEndDrag;
        adapter.OnPointerEnterInAdapter -= OnPointerEnter;
        adapter.OnPointerExitInAdapter -= OnPointerExit;
    }

    public void RemoveAdapters()
    {
        for (int i = 0; i < categoryContentGO.transform.childCount; i++)
        {
            CatalogItemAdapter toRemove = categoryContentGO.transform.GetChild(i).GetComponent<CatalogItemAdapter>();
            if (toRemove != null)
            {
                UnsubscribeToEvents(toRemove);
                Destroy(toRemove.gameObject);
            }
        }
    }

    private void OnDrag(PointerEventData eventData) { OnAdapterDrag?.Invoke(eventData); }

    private void OnEndDrag(PointerEventData eventData) { OnAdapterEndDrag?.Invoke(eventData); }

    private void CatalogItemClicked(CatalogItem catalogItemClicked) { OnCatalogItemClicked?.Invoke(catalogItemClicked); }

    private void CatalogItemFavorite(CatalogItem catalogItemClicked, CatalogItemAdapter adapter) { OnCatalogItemFavorite?.Invoke(catalogItemClicked, adapter); }

    private void AdapterStartDragging(CatalogItem catalogItemClicked, CatalogItemAdapter adapter, BaseEventData data) { OnAdapterStartDragging?.Invoke(catalogItemClicked, adapter, data); }

    private void OnPointerEnter(PointerEventData eventData, CatalogItemAdapter adapter) { OnPointerEnterInAdapter?.Invoke(eventData, adapter); }

    private void OnPointerExit(PointerEventData eventData, CatalogItemAdapter adapter) { OnPointerExitInAdapter?.Invoke(eventData, adapter); }
}