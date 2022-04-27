using System;
using System.Collections.Generic;
using UnityEngine.Events;

public enum EntityAction
{
    SELECT = 0,
    LOCK = 1,
    DELETE = 2,
    SHOW = 3
}

public interface IInspectorController
{
    event Action<BIWEntity> OnEntityClick;
    event Action<BIWEntity> OnEntityDelete;
    event Action<BIWEntity> OnEntityLock;
    event Action<BIWEntity> OnEntityChangeVisibility;
    event Action<BIWEntity, string> OnEntityRename;

    ISceneLimitsController sceneLimitsController { get; }

    void Initialize(IInspectorView inspectorView);
    void Dispose();
    void OpenEntityList();
    void SetEntityList(List<BIWEntity> sceneEntities);
    void ClearList();
    void CloseList();
    void EntityActionInvoked(EntityAction action, BIWEntity entityToApply, EntityListAdapter adapter);
    void EntityRename(BIWEntity entity, string newName);
    void SetCloseButtonsAction(UnityAction call);
}

public class InspectorController : IInspectorController
{
    public event Action<BIWEntity> OnEntityClick;
    public event Action<BIWEntity> OnEntityDelete;
    public event Action<BIWEntity> OnEntityLock;
    public event Action<BIWEntity> OnEntityChangeVisibility;
    public event Action<BIWEntity, string> OnEntityRename;

    public ISceneLimitsController sceneLimitsController => inspectorView.sceneLimitsController;

    internal IInspectorView inspectorView;

    public void Initialize(IInspectorView inspectorView)
    {
        this.inspectorView = inspectorView;

        inspectorView.OnEntityActionInvoked += EntityActionInvoked;
        inspectorView.OnEntityRename += EntityRename;

        inspectorView.ConfigureSceneLimits(new SceneLimitsController());
        CloseList();
    }

    public void Dispose()
    {
        if (inspectorView == null)
            return;
        
        inspectorView.OnEntityActionInvoked -= EntityActionInvoked;
        inspectorView.OnEntityRename -= EntityRename;
    }

    public void OpenEntityList()
    {
        if (inspectorView.entityList != null)
        {
            if (inspectorView.entities != null)
                inspectorView.entityList.SetContent(inspectorView.entities);

            inspectorView.entityList.SetActive(true);
        }

        inspectorView.SetActive(true);
    }

    public void SetEntityList(List<BIWEntity> sceneEntities)
    {
        inspectorView.SetEntitiesList(sceneEntities);

        if (inspectorView.entityList != null &&
            inspectorView.entityList.isActive &&
            inspectorView.entities != null)
        {
            inspectorView.entityList.SetContent(inspectorView.entities);
        }
    }

    public void ClearList()
    {
        inspectorView.ClearEntitiesList();

        if (inspectorView.entityList != null)
            inspectorView.entityList.RemoveAdapters();
    }

    public void CloseList()
    {
        inspectorView.SetActive(false);

        if (inspectorView.entityList != null)
            inspectorView.entityList.SetActive(false);
    }

    public void EntityActionInvoked(EntityAction action, BIWEntity entityToApply, EntityListAdapter adapter)
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
        }
    }

    public void EntityRename(BIWEntity entity, string newName) { OnEntityRename?.Invoke(entity, newName); }

    public void SetCloseButtonsAction(UnityAction call) { inspectorView.SetCloseButtonsAction(call); }
}