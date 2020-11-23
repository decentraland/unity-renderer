using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatalogAssetGroupAdapter : MonoBehaviour
{
    public TextMeshProUGUI categoryTxt;
    public GameObject categoryContentGO;
    public System.Action<SceneObject> OnSceneObjectClicked;
    public System.Action<SceneObject, CatalogItemAdapter> OnSceneObjectFavorite;
    public System.Action<SceneObject, CatalogItemAdapter, BaseEventData> OnAdapterStartDragging;
    public System.Action<PointerEventData> OnAdapterDrag, OnAdapterEndDrag;


    [Header("Prefab References")]
    public GameObject catalogItemAdapterPrefab;


    public void SetContent(string category, List<SceneObject> sceneObjectsList)
    {
        categoryTxt.text = category.ToUpper();
        RemoveAdapters();
        foreach (SceneObject sceneObject in sceneObjectsList)
        {
            CatalogItemAdapter adapter = Instantiate(catalogItemAdapterPrefab, categoryContentGO.transform).GetComponent<CatalogItemAdapter>();
            adapter.SetContent(sceneObject);
            adapter.OnSceneObjectClicked += SceneObjectClicked;
            adapter.OnSceneObjectFavorite += SceneObjectFavorite;
            adapter.OnAdapterStartDrag += AdapterStartDragging;
            adapter.OnAdapterDrag += OnDrag;
            adapter.OnAdapterEndDrag += OnEndDrag;
        }
    }

    public void RemoveAdapters()
    {

        for (int i = 0; i < categoryContentGO.transform.childCount; i++)
        {
            GameObject toRemove = categoryContentGO.transform.GetChild(i).gameObject;
            Destroy(toRemove);
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

    void SceneObjectClicked(SceneObject sceneObjectClicked)
    {
        OnSceneObjectClicked?.Invoke(sceneObjectClicked);
    }

    void SceneObjectFavorite(SceneObject sceneObjectClicked, CatalogItemAdapter adapter)
    {
        OnSceneObjectFavorite?.Invoke(sceneObjectClicked, adapter);
    }

    void AdapterStartDragging(SceneObject sceneObjectClicked, CatalogItemAdapter adapter, BaseEventData data)
    {
        OnAdapterStartDragging?.Invoke(sceneObjectClicked, adapter, data);
    }
}
