using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatalogGroupListView : ListView<Dictionary<string, List<SceneObject>>>
{

    public CatalogAssetGroupAdapter categoryItemAdapterPrefab;
    public System.Action<SceneObject> OnSceneObjectClicked;
    public System.Action<SceneObject, CatalogItemAdapter> OnSceneObjectFavorite;
    public System.Action<SceneObject, CatalogItemAdapter, BaseEventData> OnAdapterStartDragging;
    public System.Action<PointerEventData> OnAdapterDrag, OnAdapterEndDrag;

    public override void AddAdapters()
    {
        base.AddAdapters();

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

    void OnDrag(PointerEventData eventData)
    {
        OnAdapterDrag?.Invoke(eventData);
    }

    void OnEndDrag(PointerEventData eventData)
    {
        OnAdapterEndDrag?.Invoke(eventData);
    }

    void AdapterStartDragging(SceneObject sceneObjectClicked, CatalogItemAdapter adapter, BaseEventData data)
    {
        OnAdapterStartDragging?.Invoke(sceneObjectClicked, adapter, data);
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
