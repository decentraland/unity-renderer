public class BIWPublishController : BIWController
{
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public BuilderInWorldBridge builderInWorldBridge;
    public BIWSaveController biwSaveController;
    public BIWCreatorController biwCreatorController;

    private int checkerSceneLimitsOptimizationCounter = 0;

    private const int FRAMES_BEETWEN_UPDATES = 10;
    private const string FEEDBACK_MESSAGE_ENTITY_ERROR = "Some entities have errors (marked as pink cubes).";
    private const string FEEDBACK_MESSAGE_OUTSIDE_BOUNDARIES = "Some entities are outside of the Scene boundaries.";
    private const string FEEDBACK_MESSAGE_TOO_MANY_ENTITIES = "Too many entities in the scene. Check scene limits.";

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
        if (biwCreatorController.IsAnyErrorOnEntities())
            return false;

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

        string feedbackMessage = "";
        if (biwCreatorController.IsAnyErrorOnEntities())
        {
            feedbackMessage = FEEDBACK_MESSAGE_ENTITY_ERROR;
        }
        else if (!builderInWorldEntityHandler.AreAllEntitiesInsideBoundaries())
        {
            feedbackMessage = FEEDBACK_MESSAGE_OUTSIDE_BOUNDARIES;
        }
        else if (!sceneToEdit.metricsController.IsInsideTheLimits())
        {
            feedbackMessage = FEEDBACK_MESSAGE_TOO_MANY_ENTITIES;
        }

        HUDController.i.builderInWorldMainHud.SetPublishBtnAvailability(CanPublish(), feedbackMessage);
    }

    void StartPublishFlow()
    {
        if (!CanPublish())
            return;

        HUDController.i.builderInWorldMainHud.PublishStart();
    }

    void ConfirmPublishScene(string sceneName, string sceneDescription, string sceneScreenshot) { builderInWorldBridge.PublishScene(sceneToEdit, sceneName, sceneDescription, sceneScreenshot); }
}