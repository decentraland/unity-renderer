using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneObjectDropController
{
    public CatalogGroupListView catalogGroupListView;
    public event Action<SceneObject> OnSceneObjectDropped;

    public void SceneObjectDropped()
    {
        CatalogItemAdapter adapter = catalogGroupListView.GetLastSceneObjectDragged();
        SceneObject sceneObject = adapter.GetContent();

        OnSceneObjectDropped?.Invoke(sceneObject);
    }
}
