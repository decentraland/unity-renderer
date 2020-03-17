using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using UnityGLTF;
public class RenderingController : MonoBehaviour
{
    public static RenderingController i { get; private set; }

    public void Awake()
    {
        i = this;
    }

    public CompositeLock renderingActivatedAckLock = new CompositeLock();

    public System.Action<bool> OnRenderingStateChanged;
    public bool renderingEnabled { get; private set; } = true;
    public bool activatedRenderingBefore { get; private set; } = false;

    [ContextMenu("Disable Rendering")]
    public void DeactivateRendering()
    {
        if (!renderingEnabled)
            return;

        DeactivateRendering_Internal();
    }

    void DeactivateRendering_Internal()
    {
        renderingEnabled = false;

        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
        MessagingBus.renderingIsDisabled = true;
        PointerEventsController.renderingIsDisabled = true;
        InputController_Legacy.renderingIsDisabled = true;
        GLTFSceneImporter.renderingIsDisabled = true;

        AssetPromiseKeeper_GLTF.i.useTimeBudget = false;
        AssetPromiseKeeper_AB.i.useTimeBudget = false;
        AssetPromiseKeeper_AB_GameObject.i.useTimeBudget = false;
        AssetPromise_AB.limitTimeBudget = false;

        DCLCharacterController.i.SetEnabled(false);

        OnRenderingStateChanged?.Invoke(renderingEnabled);
    }


    [ContextMenu("Enable Rendering")]
    public void ActivateRendering()
    {
        if (renderingEnabled)
            return;

        if (!renderingActivatedAckLock.isUnlocked)
        {
            renderingActivatedAckLock.OnAllLocksRemoved -= ActivateRendering_Internal;
            renderingActivatedAckLock.OnAllLocksRemoved += ActivateRendering_Internal;
            return;
        }

        ActivateRendering_Internal();
    }

    private void ActivateRendering_Internal()
    {
        renderingActivatedAckLock.OnAllLocksRemoved -= ActivateRendering_Internal;
        renderingEnabled = true;

        if (!activatedRenderingBefore)
        {
            Utils.UnlockCursor();
            activatedRenderingBefore = true;
        }

        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = true;
        MessagingBus.renderingIsDisabled = false;
        GLTFSceneImporter.renderingIsDisabled = false;
        PointerEventsController.renderingIsDisabled = false;
        InputController_Legacy.renderingIsDisabled = false;
        DCLCharacterController.i.SetEnabled(true);

        AssetPromise_AB.limitTimeBudget = true;

        AssetPromiseKeeper_GLTF.i.useTimeBudget = true;
        AssetPromiseKeeper_AB.i.useTimeBudget = true;
        AssetPromiseKeeper_AB_GameObject.i.useTimeBudget = true;

        OnRenderingStateChanged?.Invoke(renderingEnabled);

        MemoryManager.i.CleanupPoolsIfNeeded(true);
        ParcelScene.parcelScenesCleaner.ForceCleanup();

        WebInterface.ReportControlEvent(new WebInterface.ActivateRenderingACK());
    }
}
