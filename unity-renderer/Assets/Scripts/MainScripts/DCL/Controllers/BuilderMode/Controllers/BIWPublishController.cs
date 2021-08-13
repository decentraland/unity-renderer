using DCL;
using UnityEngine;

public interface IBIWPublishController { }

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

    private bool reportSceneLimitsOverpassedAnalytic = true;
    private float startPublishingTimestamp = 0;

    public override void Init(BIWContext context)
    {
        base.Init(context);

        entityHandler = context.entityHandler;
        creatorController = context.creatorController;

        if (HUDController.i?.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnPublishAction += StartPublishFlow;
            HUDController.i.builderInWorldMainHud.OnConfirmPublishAction += StartPublishScene;
        }

        builderInWorldBridge = context.sceneReferences.builderInWorldBridge;

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

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnPublishAction -= StartPublishFlow;
            HUDController.i.builderInWorldMainHud.OnConfirmPublishAction -= StartPublishScene;
        }
        if (builderInWorldBridge != null)
            builderInWorldBridge.OnPublishEnd -= PublishEnd;
    }

    public bool CanPublish()
    {
        if (creatorController.IsAnyErrorOnEntities())
            return false;

        if (!sceneToEdit.metricsController.IsInsideTheLimits())
            return false;

        if (!entityHandler.AreAllEntitiesInsideBoundaries())
            return false;

        reportSceneLimitsOverpassedAnalytic = true;
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
        else if (!sceneToEdit.metricsController.IsInsideTheLimits())
        {
            feedbackMessage = FEEDBACK_MESSAGE_TOO_MANY_ENTITIES;
            if (reportSceneLimitsOverpassedAnalytic)
            {
                BIWAnalytics.SceneLimitsOverPassed(sceneToEdit.metricsController.GetModel());
                reportSceneLimitsOverpassedAnalytic = false;
            }
        }

        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud?.SetPublishBtnAvailability(CanPublish(), feedbackMessage);

        return feedbackMessage;
    }

    private void StartPublishFlow()
    {
        if (!CanPublish())
            return;

        HUDController.i.builderInWorldMainHud.PublishStart();
    }

    private void StartPublishScene(string sceneName, string sceneDescription, string sceneScreenshot)
    {
        startPublishingTimestamp = Time.realtimeSinceStartup;
        BIWAnalytics.StartScenePublish(sceneToEdit.metricsController.GetModel());
        builderInWorldBridge.PublishScene(sceneToEdit, sceneName, sceneDescription, sceneScreenshot);
    }

    private void PublishEnd(bool isOk, string message)
    {
        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.PublishEnd(isOk, message);
        string successString = isOk ? "Success" : message;
        BIWAnalytics.EndScenePublish(sceneToEdit.metricsController.GetModel(), successString, Time.realtimeSinceStartup - startPublishingTimestamp);
    }
}