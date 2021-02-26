using Builder;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BIWOutlinerController : BIWController
{
    [Header("Build References")]
    public int builderRendererIndex = 1;
    public Material outlineMaterial;
    public Material cameraOutlinerMaterial;

    public BuilderInWorldController builderInWorldController;
    public BIWInputHandler biwInputHandler;
   
    private List<DCLBuilderInWorldEntity> entitiesOutlined = new List<DCLBuilderInWorldEntity>();
    private int outlinerOptimizationCounter = 0;
    private bool isOutlineCheckActive = true;

    public override void EnterEditMode(ParcelScene scene)
    {
        base.EnterEditMode(scene);
        ActivateBuilderInWorldCamera();
    }

    public override void ExitEditMode()
    {
        base.ExitEditMode();
        DeactivateBuilderInWorldCamera();
    }

    public void SetOutlineCheckActive(bool isActive)
    {
        isOutlineCheckActive = isActive;
    }

    public void CheckOutline()
    {
        if (outlinerOptimizationCounter >= 10 && isOutlineCheckActive)
        {
            if (!BuilderInWorldUtils.IsPointerOverUIElement())
            {
                DCLBuilderInWorldEntity entity = builderInWorldController.GetEntityOnPointer();
                if (!biwInputHandler.IsMultiSelectionActive())
                    CancelAllOutlines();
                else
                    CancelUnselectedOutlines();

                if (entity != null && !entity.IsSelected)
                    OutlineEntity(entity);
            }

            outlinerOptimizationCounter = 0;
        }
        else outlinerOptimizationCounter++;
    }

    public bool IsEntityOutlined(DCLBuilderInWorldEntity entity)
    {
        return entitiesOutlined.Contains(entity);
    }

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

    public void ActivateBuilderInWorldCamera()
    {
        Camera camera = Camera.main;
        DCLBuilderOutline outliner = camera.GetComponent<DCLBuilderOutline>();

        if (outliner == null)
        {
            outliner = camera.gameObject.AddComponent(typeof(DCLBuilderOutline)) as DCLBuilderOutline;
            outliner.SetOutlineMaterial(cameraOutlinerMaterial);
        }
        else
        {
            outliner.enabled = true;
        }

        outliner.Activate();

        UniversalAdditionalCameraData additionalCameraData = camera.transform.GetComponent<UniversalAdditionalCameraData>();
        additionalCameraData.SetRenderer(builderRendererIndex);
    }

    public void DeactivateBuilderInWorldCamera()
    {
        Camera camera = Camera.main;
        DCLBuilderOutline outliner = camera.GetComponent<DCLBuilderOutline>();
        if (outliner != null)
        {
            outliner.enabled = false;
            outliner.Deactivate();
        }

        outliner.enabled = false;
        outliner.Deactivate();

        UniversalAdditionalCameraData additionalCameraData = camera.transform.GetComponent<UniversalAdditionalCameraData>();
        additionalCameraData.SetRenderer(0);
    }
}
