using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IInspectorView
{
    EntityListView entityList { get; set; }
    List<DCLBuilderInWorldEntity> entities { get; }
    ISceneLimitsController sceneLimitsController { get; }

    event Action<EntityAction, DCLBuilderInWorldEntity, EntityListAdapter> OnEntityActionInvoked;
    event Action<DCLBuilderInWorldEntity, string> OnEntityRename;

    void ClearEntitiesList();
    void ConfigureSceneLimits(ISceneLimitsController sceneLimitsController);
    void EntityActionInvoked(EntityAction action, DCLBuilderInWorldEntity entityToApply, EntityListAdapter adapter);
    void EntityRename(DCLBuilderInWorldEntity entity, string newName);
    void SetActive(bool isActive);
    void SetCloseButtonsAction(UnityAction call);
    void SetEntitiesList(List<DCLBuilderInWorldEntity> entities);
}

public class InspectorView : MonoBehaviour, IInspectorView
{
    public EntityListView entityList 
    {
        get 
        { 
            return entityListView; 
        }
        set 
        {
            entityListView = value;
        }
    }
    public List<DCLBuilderInWorldEntity> entities => entitiesList;
    public ISceneLimitsController sceneLimitsController { get; internal set; }

    public event Action<EntityAction, DCLBuilderInWorldEntity, EntityListAdapter> OnEntityActionInvoked;
    public event Action<DCLBuilderInWorldEntity, string> OnEntityRename;

    [SerializeField] internal EntityListView entityListView;
    [SerializeField] internal SceneLimitsView sceneLimitsView;
    [SerializeField] internal Button[] closeEntityListBtns;

    internal List<DCLBuilderInWorldEntity> entitiesList;

    private const string VIEW_PATH = "GodMode/Inspector/InspectorView";

    internal static InspectorView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<InspectorView>();
        view.gameObject.name = "_InspectorView";

        return view;
    }

    private void Awake()
    {
        entityListView.OnActionInvoked += EntityActionInvoked;
        entityListView.OnEntityRename += EntityRename;
    }

    private void OnDestroy()
    {
        entityListView.OnActionInvoked -= EntityActionInvoked;
        entityListView.OnEntityRename -= EntityRename;

        if (sceneLimitsController != null)
            sceneLimitsController.Dispose();
    }

    public void EntityActionInvoked(EntityAction action, DCLBuilderInWorldEntity entityToApply, EntityListAdapter adapter)
    {
        OnEntityActionInvoked?.Invoke(action, entityToApply, adapter);
    }

    public void EntityRename(DCLBuilderInWorldEntity entity, string newName)
    {
        OnEntityRename?.Invoke(entity, newName);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void SetEntitiesList(List<DCLBuilderInWorldEntity> entities)
    {
        entitiesList = entities;
    }

    public void ClearEntitiesList()
    {
        entitiesList.Clear();
    }

    public void SetCloseButtonsAction(UnityAction call)
    {
        foreach (Button closeEntityListBtn in closeEntityListBtns)
        {
            closeEntityListBtn.onClick.AddListener(call);
        }
    }

    public void ConfigureSceneLimits(ISceneLimitsController sceneLimitsController)
    {
        this.sceneLimitsController = sceneLimitsController;
        this.sceneLimitsController.Initialize(sceneLimitsView);
    }
}
