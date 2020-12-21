using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneObjectDropController : MonoBehaviour
{
    public CatalogGroupListView catalogGroupListView;
    public event Action<SceneObject> OnSceneObjectDropped;

    public void SceneObjectDropped(BaseEventData data)
    {
        CatalogItemAdapter adapter = catalogGroupListView.GetLastSceneObjectDragged();
        SceneObject sceneObject = adapter.GetContent();

        OnSceneObjectDropped?.Invoke(sceneObject);
    }
}
