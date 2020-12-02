using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuilderInWorldEntityListController : MonoBehaviour
{
    public enum EntityAction
    {
        SELECT = 0,
        LOCK = 1,
        DELETE = 2,
        SHOW = 3,
        RENAME = 4
    }

    public event Action<DCLBuilderInWorldEntity> OnEntityClick;
    public event Action<DCLBuilderInWorldEntity> OnEntityDelete;
    public event Action<DCLBuilderInWorldEntity> OnEntityLock;
    public event Action<DCLBuilderInWorldEntity> OnEntityChangeVisibility;
    public event Action<DCLBuilderInWorldEntity> OnEntityRename;

    public EntityListView entityListView;

    List<DCLBuilderInWorldEntity> entitiesList;

    private void Awake()
    {
        entityListView.OnActioninvoked += EntityActionInvoked;
    }

    private void OnDestroy()
    {
        entityListView.OnActioninvoked -= EntityActionInvoked;
    }

    public void OpenEntityList()
    {        
        entityListView.SetContent(entitiesList);
        gameObject.SetActive(true);
        entityListView.gameObject.SetActive(true);
    }

    public void SetEntityList(List<DCLBuilderInWorldEntity> sceneEntities)
    {
        entitiesList = sceneEntities;
        if (entityListView.gameObject.activeSelf)
            entityListView.SetContent(entitiesList);
    }

    public void ClearList()
    {
        entitiesList.Clear();
        entityListView.RemoveAdapters();
    }

    public void CloseList()
    {
        gameObject.SetActive(false);
        entityListView.gameObject.SetActive(false);
    }

    public void EntityActionInvoked(EntityAction action, DCLBuilderInWorldEntity entityToApply,EntityListAdapter adapter)
    {
        switch (action)
        {
            case EntityAction.SELECT:

                OnEntityClick?.Invoke(entityToApply);
                break;
            case EntityAction.LOCK:

                OnEntityLock?.Invoke(entityToApply);
                break;
            case EntityAction.DELETE:

                OnEntityDelete?.Invoke(entityToApply);
                break;
            case EntityAction.SHOW:

                OnEntityChangeVisibility?.Invoke(entityToApply);
                break;
            case EntityAction.RENAME:

                OnEntityRename?.Invoke(entityToApply);
                break;
        }

    }

}
