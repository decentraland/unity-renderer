using DCL;
using DCL.Configuration;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlinerController : MonoBehaviour
{
    public Material outlineMaterial;
   
    List<DCLBuilderInWorldEntity> entitiesOutlined = new List<DCLBuilderInWorldEntity>();

    public void OutlineEntities(List<DCLBuilderInWorldEntity> entitiesToEdit)
    {
        foreach(DCLBuilderInWorldEntity entityToEdit in entitiesToEdit)
        {
            OutlineEntity(entityToEdit);
        }
    }

    public void OutlineEntity(DCLBuilderInWorldEntity entity)
    {
        if (!entity.rootEntity.meshRootGameObject && entity.rootEntity.renderers.Length <= 0)
            return;

        if (entitiesOutlined.Contains(entity))
            return;
  
        if (entity.IsLocked)
            return;

        entitiesOutlined.Add(entity);

        for (int i = 0; i < entity.rootEntity.meshesInfo.renderers.Length; i++)
        {
            entity.rootEntity.meshesInfo.renderers[i].gameObject.layer = BuilderInWorldSettings.SELECTION_LAYER;
        }
    }

    public void CancelUnselectedOutlines()
    {
        for (int i = 0; i < entitiesOutlined.Count; i++)
        {
            if (!entitiesOutlined[i].IsSelected)
            {
                CancelEntityOutline(entitiesOutlined[i]);
            }
        }
    }

    public void CancelAllOutlines()
    {
        for (int i = 0; i < entitiesOutlined.Count; i++)
        {
            CancelEntityOutline(entitiesOutlined[i]);           
        }
    }

    public void CancelEntityOutline(DCLBuilderInWorldEntity entityToQuitOutline)
    {
        if (!entitiesOutlined.Contains(entityToQuitOutline)) return;

        if (entityToQuitOutline.rootEntity.meshRootGameObject && entityToQuitOutline.rootEntity.meshesInfo.renderers.Length > 0)
        {
            for (int x = 0; x < entityToQuitOutline.rootEntity.meshesInfo.renderers.Length; x++)
            {
                entityToQuitOutline.rootEntity.meshesInfo.renderers[x].gameObject.layer = BuilderInWorldSettings.DEFAULT_LAYER;
            }
        }
        entitiesOutlined.Remove(entityToQuitOutline);

    }

}
