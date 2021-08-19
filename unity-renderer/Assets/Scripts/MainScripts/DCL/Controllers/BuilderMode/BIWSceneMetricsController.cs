using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

public class BIWSceneMetricsController : SceneMetricsController
{
    private string currentOverpassLimitTypes = "";

    public BIWSceneMetricsController(ParcelScene sceneOwner) : base(sceneOwner)
    {
        Enable();
        isDirty = true;
    }

    protected override void OnEntityAdded(IDCLEntity e)
    {
        e.OnMeshesInfoUpdated += OnEntityMeshInfoUpdated;
        e.OnMeshesInfoCleaned += OnEntityMeshInfoCleaned;
    }

    protected override void OnEntityRemoved(IDCLEntity e)
    {
        e.OnMeshesInfoUpdated -= OnEntityMeshInfoUpdated;
        e.OnMeshesInfoCleaned -= OnEntityMeshInfoCleaned;

        if (!e.components.ContainsKey(CLASS_ID_COMPONENT.SMART_ITEM))
        {
            SubstractMetrics(e);
            model.entities = entitiesMetrics.Count;
        }
        CheckSceneLimitOverPassedAnalaytics();
    }

    protected override void OnEntityMeshInfoUpdated(IDCLEntity entity)
    {
        //builder should only check scene limits for not smart items entities
        if (!entity.components.ContainsKey(CLASS_ID_COMPONENT.SMART_ITEM))
        {
            AddOrReplaceMetrics(entity);
            model.entities = entitiesMetrics.Count;
        }
        else
        {
            SubstractMetrics(entity);
            model.entities = entitiesMetrics.Count;
        }
        CheckSceneLimitOverPassedAnalaytics();
    }

    private void CheckSceneLimitOverPassedAnalaytics()
    {
        if (!IsInsideTheLimits())
        {
            string overpassLimit = BIWAnalytics.GetLimitsPassedArray(scene.metricsController.GetModel(), scene.metricsController.GetLimits());
            if (overpassLimit != currentOverpassLimitTypes)
            {
                BIWAnalytics.SceneLimitsExceeded(scene.metricsController.GetModel(), scene.metricsController.GetLimits());
                currentOverpassLimitTypes = overpassLimit;
                Debug.Log("Send overpass limit" + currentOverpassLimitTypes);
            }
        }
        else
        {
            currentOverpassLimitTypes = "";
        }
    }
}