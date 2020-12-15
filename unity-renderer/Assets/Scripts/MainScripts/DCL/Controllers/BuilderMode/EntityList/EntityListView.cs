using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityListView : ListView<DCLBuilderInWorldEntity>
{
    public EntityListAdapter entityListAdapter;

    public System.Action<BuilderInWorldEntityListController.EntityAction, DCLBuilderInWorldEntity, EntityListAdapter> OnActioninvoked;


    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (DCLBuilderInWorldEntity entity in contentList)
        {
            EntityListAdapter adapter = Instantiate(entityListAdapter, contentPanelTransform).GetComponent<EntityListAdapter>();
            adapter.SetContent(entity);
            adapter.OnActionInvoked += EntityActionInvoked;
        }
    }

    public override void RemoveAdapters()
    {
        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            EntityListAdapter toRemove = contentPanelTransform.transform.GetChild(i).gameObject.GetComponent<EntityListAdapter>();
            if (toRemove != null)   toRemove.OnActionInvoked -= EntityActionInvoked;
            Destroy(toRemove.gameObject);
        }
    }

    public void EntityActionInvoked(BuilderInWorldEntityListController.EntityAction action, DCLBuilderInWorldEntity entityToApply,EntityListAdapter adapter)
    {
        OnActioninvoked?.Invoke(action, entityToApply,adapter);
    }
}
