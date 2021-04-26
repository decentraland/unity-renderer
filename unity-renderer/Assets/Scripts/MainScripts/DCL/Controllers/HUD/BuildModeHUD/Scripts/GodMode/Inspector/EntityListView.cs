using UnityEngine;

public class EntityListView : ListView<DCLBuilderInWorldEntity>
{
    [SerializeField] internal EntityListAdapter entityListAdapter;
    [SerializeField] internal DynamicScrollSensitivity dynamicScrollSensitivity;

    public System.Action<EntityAction, DCLBuilderInWorldEntity, EntityListAdapter> OnActionInvoked;
    public System.Action<DCLBuilderInWorldEntity, string> OnEntityRename;

    public bool isActive => gameObject.activeSelf;

    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (DCLBuilderInWorldEntity entity in contentList)
        {
            EntityListAdapter adapter = Instantiate(entityListAdapter, contentPanelTransform).GetComponent<EntityListAdapter>();
            adapter.SetContent(entity);
            adapter.OnActionInvoked += EntityActionInvoked;
            adapter.OnEntityRename += EntityRename;
        }

        if (dynamicScrollSensitivity != null)
            dynamicScrollSensitivity.RecalculateSensitivity();
    }

    public override void RemoveAdapters()
    {
        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            EntityListAdapter toRemove = contentPanelTransform.transform.GetChild(i).gameObject.GetComponent<EntityListAdapter>();
            if (toRemove != null)
            {
                toRemove.OnActionInvoked -= EntityActionInvoked;
                toRemove.OnEntityRename -= EntityRename;
            }
            Destroy(toRemove.gameObject);
        }

        if (dynamicScrollSensitivity != null)
            dynamicScrollSensitivity.RecalculateSensitivity();
    }

    public void EntityActionInvoked(EntityAction action, DCLBuilderInWorldEntity entityToApply, EntityListAdapter adapter) { OnActionInvoked?.Invoke(action, entityToApply, adapter); }

    public void EntityRename(DCLBuilderInWorldEntity entity, string newName) { OnEntityRename?.Invoke(entity, newName); }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }
}