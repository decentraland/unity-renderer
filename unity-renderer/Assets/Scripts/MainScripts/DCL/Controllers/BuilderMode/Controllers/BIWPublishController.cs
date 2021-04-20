using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWPublishController : BIWController
{
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public BuilderInWorldBridge builderInWorldBridge;

    private int checkerSceneLimitsOptimizationCounter = 0;

    private const int FRAMES_BEETWEN_UPDATES = 10;

    public override void Init()
    {
        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnPublishAction += StartPublishFlow;
            HUDController.i.builderInWorldMainHud.OnConfirmPublishAction += ConfirmPublishScene;
        }
    }

    protected override void FrameUpdate()
    {
        base.FrameUpdate();
        if (checkerSceneLimitsOptimizationCounter >= FRAMES_BEETWEN_UPDATES)
        {
            checkerSceneLimitsOptimizationCounter = 0;
            CheckPublishConditions();
        }
        else
        {
            checkerSceneLimitsOptimizationCounter++;
        }
    }

    private void OnDestroy()
    {
        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnPublishAction -= StartPublishFlow;
            HUDController.i.builderInWorldMainHud.OnConfirmPublishAction -= ConfirmPublishScene;
        }
    }

    public bool CanPublish()
    {
        if (!sceneToEdit.metricsController.IsInsideTheLimits())
            return false;

        if (!builderInWorldEntityHandler.AreAllEntitiesInsideBoundaries())
            return false;
        return true;
    }

    void CheckPublishConditions()
    {
        if (HUDController.i.builderInWorldMainHud is null)
            return;

        HUDController.i.builderInWorldMainHud.SetPublishBtnAvailability(CanPublish());
    }

    void StartPublishFlow()
    {
        if (!CanPublish())
            return;

        HUDController.i.builderInWorldMainHud.PublishStart();
    }

    void ConfirmPublishScene() { builderInWorldBridge.PublishScene(sceneToEdit); }
}