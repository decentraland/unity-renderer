using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatalogGroupListView : ListView<Dictionary<string, List<CatalogItem>>>
{
    public Canvas generalCanvas;
    public CatalogAssetGroupAdapter categoryItemAdapterPrefab;
    public DynamicScrollSensitivity dynamicScrollSensitivity;

    public System.Action<CatalogItem> OnCatalogItemClicked;
    public System.Action<CatalogItem, CatalogItemAdapter> OnCatalogItemFavorite;
    public System.Action<PointerEventData, CatalogItemAdapter> OnPointerEnterInAdapter;
    public System.Action<PointerEventData, CatalogItemAdapter> OnPointerExitInAdapter;

    public event System.Action OnResumeInput;
    public event System.Action OnStopInput;

    private GameObject draggedObject;
    private CatalogItemAdapter catalogItemAdapterDragged;

    public override void AddAdapters()
    {
        base.AddAdapters();

        if (contentList == null)
            return;

        foreach (Dictionary<string, List<CatalogItem>> assetPackGroups in contentList)
        {
            foreach (KeyValuePair<string, List<CatalogItem>> assetPackGroup in assetPackGroups)
            {
                CatalogAssetGroupAdapter adapter = Instantiate(categoryItemAdapterPrefab, contentPanelTransform).GetComponent<CatalogAssetGroupAdapter>();
                adapter.SetContent(assetPackGroup.Key, assetPackGroup.Value);
                SubscribeToEvents(adapter);
            }
        }

        if (dynamicScrollSensitivity != null)
            dynamicScrollSensitivity.RecalculateSensitivity();
    }

    public override void RemoveAdapters()
    {
        if (contentPanelTransform == null ||
            contentPanelTransform.transform == null ||
            contentPanelTransform.transform.childCount == 0)
            return;

        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            CatalogAssetGroupAdapter toRemove = contentPanelTransform.transform.GetChild(i).GetComponent<CatalogAssetGroupAdapter>();
            if (toRemove != null)
            {
                UnsubscribeToEvents(toRemove);
                Destroy(toRemove.gameObject);
            }
        }
    }

    public void SubscribeToEvents(CatalogAssetGroupAdapter adapter)
    {
        adapter.OnCatalogItemClicked += CatalogItemSelected;
        adapter.OnCatalogItemFavorite += CatalogItemFavorite;
        adapter.OnAdapterStartDragging += AdapterStartDragging;
        adapter.OnAdapterDrag += OnDrag;
        adapter.OnAdapterEndDrag += OnEndDrag;
        adapter.OnPointerEnterInAdapter += OnPointerEnter;
        adapter.OnPointerExitInAdapter += OnPointerExit;
    }

    public void UnsubscribeToEvents(CatalogAssetGroupAdapter adapter)
    {
        adapter.OnCatalogItemClicked -= CatalogItemSelected;
        adapter.OnCatalogItemFavorite -= CatalogItemFavorite;
        adapter.OnAdapterStartDragging -= AdapterStartDragging;
        adapter.OnAdapterDrag -= OnDrag;
        adapter.OnAdapterEndDrag -= OnEndDrag;
        adapter.OnPointerEnterInAdapter -= OnPointerEnter;
        adapter.OnPointerExitInAdapter -= OnPointerExit;
    }

    private void OnDrag(PointerEventData data) { draggedObject.transform.position = data.position; }

    private void AdapterStartDragging(CatalogItem catalogItemClicked, CatalogItemAdapter adapter, BaseEventData data)
    {
        PointerEventData eventData = data as PointerEventData;

        if (draggedObject == null)
            draggedObject = Instantiate(adapter.gameObject, generalCanvas.transform);

        CatalogItemAdapter newAdapter = draggedObject.GetComponent<CatalogItemAdapter>();

        RectTransform adapterRT = adapter.GetComponent<RectTransform>();
        newAdapter.SetContent(adapter.GetContent());
        newAdapter.EnableDragMode(adapterRT.sizeDelta);
        catalogItemAdapterDragged = newAdapter;

        OnStopInput?.Invoke();
    }

    private void OnEndDrag(PointerEventData data)
    {
        OnResumeInput?.Invoke();
        Destroy(draggedObject);
    }

    public CatalogItemAdapter GetLastCatalogItemDragged() { return catalogItemAdapterDragged; }

    private void CatalogItemSelected(CatalogItem sceneObject) { OnCatalogItemClicked?.Invoke(sceneObject); }

    private void CatalogItemFavorite(CatalogItem sceneObject, CatalogItemAdapter adapter) { OnCatalogItemFavorite?.Invoke(sceneObject, adapter); }

    private void OnPointerEnter(PointerEventData eventData, CatalogItemAdapter adapter) { OnPointerEnterInAdapter?.Invoke(eventData, adapter); }

    private void OnPointerExit(PointerEventData eventData, CatalogItemAdapter adapter) { OnPointerExitInAdapter?.Invoke(eventData, adapter); }
}