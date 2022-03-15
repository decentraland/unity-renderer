using System;
using DCL;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Controllers;
using UnityEngine;

public class BIWSaveController : BIWController, IBIWSaveController
{
    private const float MS_BETWEEN_SAVES = 5000f;

    public int saveAttemptsSinceLastSave { get; set; } = 0;
    public int timesSaved { get; set; } = 0;

    private BuilderInWorldBridge bridge;

    private float nextTimeToSave;
    private bool canActivateSave = true;
    public int GetSaveAttempsSinceLastSave() { return saveAttemptsSinceLastSave; }
    public int GetTimesSaved() { return timesSaved; }

    public override void Initialize(IContext context)
    {
        base.Initialize(context);

        bridge = context.sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();
        if (bridge != null)
            bridge.OnKernelUpdated += TryToSave;

        if ( context.editorContext.editorHUD != null)
        {
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
            context.editorContext.editorHUD.OnConfirmPublishAction -= ConfirmPublishScene;
        }
    }

    public void ResetSaveTime() { nextTimeToSave = 0; }

    public void ResetNumberOfSaves()
    {
        saveAttemptsSinceLastSave = 0;
        timesSaved = 0;
    }

    public override void EnterEditMode(IBuilderScene scene)
    {
        base.EnterEditMode(scene);
        nextTimeToSave = DCLTime.realtimeSinceStartup + MS_BETWEEN_SAVES / 1000f;
        ResetNumberOfSaves();
    }

    public override void ExitEditMode()
    {
        if (saveAttemptsSinceLastSave > 0 && context.editorContext.publishController.HasUnpublishChanges())
        {
            ForceSave();    
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
        if(!isEditModeActive)
            return;
        
        saveAttemptsSinceLastSave++;
        
        if (CanSave())
            ForceSave();
    }

    public void ForceSave()
    {
        if (!isEditModeActive || !canActivateSave)
            return;

        // We update the manifest with the current state
        builderScene.UpdateManifestFromScene();

        //We set the manifest on the builder server
        context.builderAPIController.SetManifest(builderScene.manifest);

        nextTimeToSave = DCLTime.realtimeSinceStartup + MS_BETWEEN_SAVES / 1000f;
        context.editorContext.editorHUD?.SceneSaved();
        saveAttemptsSinceLastSave = 0;
        timesSaved++;
    }

    internal void ConfirmPublishScene(string sceneName, string sceneDescription, string sceneScreenshot) { ResetNumberOfSaves(); }
}