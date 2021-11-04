using System;
using DCL;
using DCL.Builder;
using DCL.Controllers;
using UnityEngine;

public class BIWSaveController : BIWController, IBIWSaveController
{
    private const float MS_BETWEEN_SAVES = 5000f;

    public int numberOfSaves { get; set; } = 0;

    private BuilderInWorldBridge bridge;

    private float nextTimeToSave;
    private bool canActivateSave = true;
    public int GetSaveTimes() { return numberOfSaves; }

    public override void Initialize(IContext context)
    {
        base.Initialize(context);

        bridge = context.sceneReferences.bridges.GetComponent<BuilderInWorldBridge>();
        if (bridge != null)
            bridge.OnKernelUpdated += TryToSave;

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnSaveSceneInfoAction += SaveSceneInfo;
            context.editorContext.editorHUD.OnConfirmPublishAction += ConfirmPublishScene;
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        if (bridge != null)
            bridge.OnKernelUpdated -= TryToSave;

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnSaveSceneInfoAction -= SaveSceneInfo;
            context.editorContext.editorHUD.OnConfirmPublishAction -= ConfirmPublishScene;
        }
    }

    public void ResetSaveTime() { nextTimeToSave = 0; }

    public void ResetNumberOfSaves() { numberOfSaves = 0; }

    public override void EnterEditMode(IParcelScene scene)
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

            context.editorContext.editorHUD?.SaveSceneInfo();
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
        context.editorContext.editorHUD?.SceneSaved();
        numberOfSaves++;
    }

    public void SaveSceneInfo(string sceneName, string sceneDescription, string sceneScreenshot) { bridge.SaveSceneInfo(sceneToEdit, sceneName, sceneDescription, sceneScreenshot); }

    internal void ConfirmPublishScene(string sceneName, string sceneDescription, string sceneScreenshot) { ResetNumberOfSaves(); }
}