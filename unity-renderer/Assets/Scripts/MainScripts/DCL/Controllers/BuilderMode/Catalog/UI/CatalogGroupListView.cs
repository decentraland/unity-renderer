using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CatalogGroupListView : ListView<Dictionary<string, List<CatalogItem>>>
{
    public Canvas generalCanvas;
    public CatalogAssetGroupAdapter categoryItemAdapterPrefab;
    public System.Action<CatalogItem> OnCatalogItemClicked;
    public System.Action<CatalogItem, CatalogItemAdapter> OnCatalogItemFavorite;

    public event System.Action OnResumeInput;
    public event System.Action OnStopInput;

    GameObject draggedObject;
    CatalogItemAdapter catalogItemAdapterDragged;

    public override void AddAdapters()
    {
        base.AddAdapters();

        if (contentList == null) return;

        foreach (Dictionary<string, List<CatalogItem>> assetPackGroups in contentList)
        {
            foreach (KeyValuePair<string, List<CatalogItem>> assetPackGroup in assetPackGroups)
            {
                CatalogAssetGroupAdapter adapter = Instantiate(categoryItemAdapterPrefab, contentPanelTransform).GetComponent<CatalogAssetGroupAdapter>();

                adapter.SetContent(assetPackGroup.Key, assetPackGroup.Value);
                adapter.OnCatalogItemClicked += CatalogItemSelected;
                adapter.OnCatalogItemFavorite += CatalogItemFavorite;
                adapter.OnAdapterStartDragging += AdapterStartDragging;
                adapter.OnAdapterDrag += OnDrag;
                adapter.OnAdapterEndDrag += OnEndDrag;

            }
        }
    }

    void OnDrag(PointerEventData data)
    {
        draggedObject.transform.position = data.position;
    }

    void AdapterStartDragging(CatalogItem catalogItemClicked, CatalogItemAdapter adapter, BaseEventData data)
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

    void OnEndDrag(PointerEventData data)
    {
        OnResumeInput?.Invoke();
        Destroy(draggedObject);
    }

    public CatalogItemAdapter GetLastCatalogItemDragged()
    {
        return catalogItemAdapterDragged;
    }

    void CatalogItemSelected(CatalogItem sceneObject)
    {
        OnCatalogItemClicked?.Invoke(sceneObject);
    }

    void CatalogItemFavorite(CatalogItem sceneObject,CatalogItemAdapter adapter)
    {
        OnCatalogItemFavorite?.Invoke(sceneObject, adapter);
    }


}