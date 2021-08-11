using DCL;
using DCL.Controllers;
using UnityEngine;

public interface IBIWSaveController
{
    void SetSaveActivation(bool isActive, bool tryToSave = false);
    void TryToSave();
    void ForceSave();
}

public class BIWSaveController : BIWController, IBIWSaveController
{
    private const float MS_BETWEEN_SAVES = 5000f;

    public int numberOfSaves { get; private set; } = 0;

    private BuilderInWorldBridge bridge;

    private float nextTimeToSave;
    private bool canActivateSave = true;

    public override void Init(BIWContext context)
    {
        base.Init(context);

        bridge = context.sceneReferences.builderInWorldBridge;
        if (bridge != null)
            bridge.OnKernelUpdated += TryToSave;

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnSaveSceneInfoAction += SaveSceneInfo;
            HUDController.i.builderInWorldMainHud.OnConfirmPublishAction += ConfirmPublishScene;
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        if (bridge != null)
            bridge.OnKernelUpdated -= TryToSave;

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnSaveSceneInfoAction -= SaveSceneInfo;
            HUDController.i.builderInWorldMainHud.OnConfirmPublishAction -= ConfirmPublishScene;
        }
    }

    public void ResetSaveTime() { nextTimeToSave = 0; }

    public void ResetNumberOfSaves() { numberOfSaves = 0; }

    public override void EnterEditMode(ParcelScene scene)
    {
        base.EnterEditMode(scene);
        nextTimeToSave = DCLTime.realtimeSinceStartup + MS_BETWEEN_SAVES / 1000f;
        ResetNumberOfSaves();
    }

    public override void ExitEditMode()
    {
        if (numberOfSaves > 0)
        {
            ForceSave();

            HUDController.i.builderInWorldMainHud?.SaveSceneInfo();
            ResetNumberOfSaves();
        }
        base.ExitEditMode();
    }

    public void SetSaveActivation(bool isActive, bool tryToSave = false)
    {
        canActivateSave = isActive;
        if (tryToSave)
            TryToSave();
    }

    public bool CanSave() { return DCLTime.realtimeSinceStartup >= nextTimeToSave && isEditModeActive && canActivateSave; }

    public void TryToSave()
    {
        if (CanSave())
            ForceSave();
    }

    public void ForceSave()
    {
        if (!isEditModeActive || !canActivateSave)
            return;

        bridge.SaveSceneState(sceneToEdit);
        nextTimeToSave = DCLTime.realtimeSinceStartup + MS_BETWEEN_SAVES / 1000f;
        HUDController.i.builderInWorldMainHud?.SceneSaved();
        numberOfSaves++;
    }

    public void SaveSceneInfo(string sceneName, string sceneDescription, string sceneScreenshot) { bridge.SaveSceneInfo(sceneToEdit, sceneName, sceneDescription, sceneScreenshot); }

    void ConfirmPublishScene(string sceneName, string sceneDescription, string sceneScreenshot) { ResetNumberOfSaves(); }
}