using System;
using DCL;
using DCL.Builder;
using UnityEngine;

public class BIWPublishController : BIWController, IBIWPublishController
{
    private IBIWEntityHandler entityHandler;
    private IBIWCreatorController creatorController;

    private BuilderInWorldBridge builderInWorldBridge;

    private int checkerSceneLimitsOptimizationCounter = 0;

    private const int FRAMES_BEETWEN_UPDATES = 10;
    private const string FEEDBACK_MESSAGE_ENTITY_ERROR = "Some entities have errors (marked as pink cubes).";
    private const string FEEDBACK_MESSAGE_OUTSIDE_BOUNDARIES = "Some entities are outside of the Scene boundaries.";
    private const string FEEDBACK_MESSAGE_TOO_MANY_ENTITIES = "Too many entities in the scene. Check scene limits.";

    private float startPublishingTimestamp = 0;

    public override void Initialize(IContext context)
    {
        base.Initialize(context);

        entityHandler = context.editorContext.entityHandler;
        creatorController = context.editorContext.creatorController;

        if (context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnPublishAction += StartPublishFlow;
            context.editorContext.editorHUD.OnConfirmPublishAction += StartPublishScene;
        }

        builderInWorldBridge = context.sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();

        if (builderInWorldBridge != null)
            builderInWorldBridge.OnPublishEnd += PublishEnd;
    }

    public override void Update()
    {
        base.Update();
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

    public override void Dispose()
    {
        base.Dispose();

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnPublishAction -= StartPublishFlow;
            context.editorContext.editorHUD.OnConfirmPublishAction -= StartPublishScene;
        }

        if (builderInWorldBridge != null)
            builderInWorldBridge.OnPublishEnd -= PublishEnd;
    }

    public bool CanPublish()
    {
        if (creatorController.IsAnyErrorOnEntities())
            return false;

        if (!sceneToEdit.metricsCounter.IsInsideTheLimits())
            return false;

        if (!entityHandler.AreAllEntitiesInsideBoundaries())
            return false;

        return true;
    }

    /// <summary>
    /// This function will check if you are able to publish the scene to the content server. If no error are present, an empty message will be returned
    /// </summary>
    /// <returns>A message the with the reason telling you why you can't publish. If you can publish an empty message will be returned </returns>
    public string CheckPublishConditions()
    {
        string feedbackMessage = "";
        if (creatorController.IsAnyErrorOnEntities())
        {
            feedbackMessage = FEEDBACK_MESSAGE_ENTITY_ERROR;
        }
        else if (!entityHandler.AreAllEntitiesInsideBoundaries())
        {
            feedbackMessage = FEEDBACK_MESSAGE_OUTSIDE_BOUNDARIES;
        }
        else if (!sceneToEdit.metricsCounter.IsInsideTheLimits())
        {
            feedbackMessage = FEEDBACK_MESSAGE_TOO_MANY_ENTITIES;
        }

        if ( context.editorContext.editorHUD != null)
            context.editorContext.editorHUD.SetPublishBtnAvailability(CanPublish(), feedbackMessage);

        return feedbackMessage;
    }

    private void StartPublishFlow()
    {
        if (!CanPublish())
            return;

        if (DataStore.i.builderInWorld.isDevBuild.Get())
        {
            //TODO: Implement project publish
        }
        else
        {
            context.editorContext.editorHUD.PublishStart();
        }
    }

    private void StartPublishScene(string sceneName, string sceneDescription, string sceneScreenshot)
    {
        startPublishingTimestamp = Time.realtimeSinceStartup;
        BIWAnalytics.StartScenePublish(sceneToEdit.metricsCounter.currentCount);
        builderInWorldBridge.PublishScene(sceneToEdit, sceneName, sceneDescription, sceneScreenshot);
    }

    private void PublishEnd(bool isOk, string message)
    {
        if ( context.editorContext.editorHUD != null)
            context.editorContext.editorHUD.PublishEnd(isOk, message);
        string successString = isOk ? "Success" : message;
        BIWAnalytics.EndScenePublish(sceneToEdit.metricsCounter.currentCount, successString, Time.realtimeSinceStartup - startPublishingTimestamp);
    }
}