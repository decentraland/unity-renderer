using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatalogGroupListView : ListView<Dictionary<string, List<SceneObject>>>
{
    public Canvas generalCanvas;
    public CatalogAssetGroupAdapter categoryItemAdapterPrefab;
    public System.Action<SceneObject> OnSceneObjectClicked;
    public System.Action<SceneObject, CatalogItemAdapter> OnSceneObjectFavorite;

    public event System.Action OnResumeInput;
    public event System.Action OnStopInput;

    GameObject draggedObject;
    CatalogItemAdapter catalogItemAdapterDragged;

    public override void AddAdapters()
    {
        base.AddAdapters();

        if (contentList == null) return;

        foreach (Dictionary<string, List<SceneObject>> assetPackGroups in contentList)
        {
            foreach (KeyValuePair<string, List<SceneObject>> assetPackGroup in assetPackGroups)
            {
                CatalogAssetGroupAdapter adapter = Instantiate(categoryItemAdapterPrefab, contentPanelTransform).GetComponent<CatalogAssetGroupAdapter>();

                adapter.SetContent(assetPackGroup.Key, assetPackGroup.Value);
                adapter.OnSceneObjectClicked += SceneObjectSelected;
                adapter.OnSceneObjectFavorite += SceneObjectFavorite;
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

    void AdapterStartDragging(SceneObject sceneObjectClicked, CatalogItemAdapter adapter, BaseEventData data)
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

    public CatalogItemAdapter GetLastSceneObjectDragged()
    {
        return catalogItemAdapterDragged;
    }

    void SceneObjectSelected(SceneObject sceneObject)
    {
        OnSceneObjectClicked?.Invoke(sceneObject);
    }

    void SceneObjectFavorite(SceneObject sceneObject,CatalogItemAdapter adapter)
    {
        OnSceneObjectFavorite?.Invoke(sceneObject, adapter);
    }


}