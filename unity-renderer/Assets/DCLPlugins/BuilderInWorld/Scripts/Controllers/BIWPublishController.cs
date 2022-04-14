using System;
using DCL;
using DCL.Builder;
using UnityEngine;

public class BIWPublishController : BIWController, IBIWPublishController
{
    private IBIWEntityHandler entityHandler;
    private IBIWCreatorController creatorController;
    private IBIWActionController actionController;

    private int checkerSceneLimitsOptimizationCounter = 0;
    private bool hasUnpublishedChanges = false;

    private const int FRAMES_BEETWEN_UPDATES = 10;
    private const string FEEDBACK_MESSAGE_ENTITY_ERROR = "Some entities have errors (marked as pink cubes).";
    private const string FEEDBACK_MESSAGE_OUTSIDE_BOUNDARIES = "Some entities are outside of the Scene boundaries.";
    private const string FEEDBACK_MESSAGE_TOO_MANY_ENTITIES = "Too many entities in the scene. Check scene limits.";

    internal float publishTimeStamp = 0;
    
    public override void Initialize(IContext context)
    {
        base.Initialize(context);

        entityHandler = context.editorContext.entityHandler;
        creatorController = context.editorContext.creatorController;
        actionController = context.editorContext.actionController;

        if (context.editorContext.editorHUD != null)
            context.editorContext.editorHUD.OnPublishAction += StartPublishFlow;

        context.publisher.OnPublishFinish += PublishFinish;
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

        context.publisher.OnPublishFinish -= PublishFinish;
        if ( context.editorContext.editorHUD != null)
            context.editorContext.editorHUD.OnPublishAction -= StartPublishFlow;
    }

    public override void EnterEditMode(IBuilderScene scene)
    {
        base.EnterEditMode(scene);
        publishTimeStamp = 0;
    }

    public override void ExitEditMode()
    {
        base.ExitEditMode();
        CheckIfThereAreUnpublishChanges();
    }
    
    public bool HasUnpublishChanges()
    {
        CheckIfThereAreUnpublishChanges();
        return hasUnpublishedChanges;
    }

    internal void CheckIfThereAreUnpublishChanges()
    {
        hasUnpublishedChanges = actionController.HasApplyAnyActionThisSession();
        if (hasUnpublishedChanges)
        {
            hasUnpublishedChanges = actionController.GetLastActionTimestamp() > publishTimeStamp;
        }
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
        
        // We update the manifest with the current scene to send the last state to publish
        builderScene.UpdateManifestFromScene();
        
        context.cameraController.TakeSceneScreenshot((sceneSnapshot) =>
        {
            builderScene.sceneScreenshotTexture = sceneSnapshot;
            if (builderScene.sceneType == IBuilderScene.SceneType.PROJECT)
            {
                //If it is a project, we took an aerial view of the scene too for the rotation of the scene
                context.cameraController.TakeSceneAerialScreenshot( sceneToEdit, (aerialSceenshot) =>
                {
                    builderScene.aerialScreenshotTexture = aerialSceenshot;
                    context.publisher.StartPublish(builderScene);
                });
            }
            else
            {
                context.publisher.StartPublish(builderScene);
            }
        });
    }

    private void PublishFinish(bool isOk)
    {
        if(!isEditModeActive)
            return;

        hasUnpublishedChanges = false;
        publishTimeStamp = Time.unscaledTime;
    }
}