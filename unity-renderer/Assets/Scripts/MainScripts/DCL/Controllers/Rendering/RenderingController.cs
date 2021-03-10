using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

public class RenderingController : MonoBehaviour
{
    public static float firstActivationTime { get; private set; }
    private bool firstActivationTimeHasBeenSet = false;
    private bool VERBOSE = false;

    public CompositeLock renderingActivatedAckLock = new CompositeLock();

    private bool activatedRenderingBefore { get; set; } = false;

    void Awake()
    {
        CommonScriptableObjects.rendererState.OnLockAdded += AddLock;
        CommonScriptableObjects.rendererState.OnLockRemoved += RemoveLock;
        CommonScriptableObjects.rendererState.Set(false);
    }

    void OnDestroy()
    {
        CommonScriptableObjects.rendererState.OnLockAdded -= AddLock;
        CommonScriptableObjects.rendererState.OnLockRemoved -= RemoveLock;
    }

    [ContextMenu("Disable Rendering")]
    public void DeactivateRendering()
    {
        if (!CommonScriptableObjects.rendererState.Get())
            return;

        DeactivateRendering_Internal();
    }

    void DeactivateRendering_Internal()
    {
        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
        CommonScriptableObjects.rendererState.Set(false);
    }

    [ContextMenu("Enable Rendering")]
    public void ActivateRendering()
    {
        ActivateRendering(forceActivate: false);
    }

    public void ForceActivateRendering()
    {
        ActivateRendering(forceActivate: true);
    }

    public void ActivateRendering(bool forceActivate)
    {
        if (CommonScriptableObjects.rendererState.Get())
            return;

        if (!firstActivationTimeHasBeenSet)
        {
            firstActivationTime = Time.realtimeSinceStartup;
            firstActivationTimeHasBeenSet = true;
        }

        if (!forceActivate && !renderingActivatedAckLock.isUnlocked)
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

        if (!activatedRenderingBefore)
        {
            Utils.UnlockCursor();
            activatedRenderingBefore = true;
        }

        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = true;
        CommonScriptableObjects.rendererState.Set(true);

        WebInterface.ReportControlEvent(new WebInterface.ActivateRenderingACK());
    }

    private void AddLock(object id)
    {
        if (CommonScriptableObjects.rendererState.Get()) return;

        if (VERBOSE)
            Debug.Log("Add lock: " + id);

        renderingActivatedAckLock.AddLock(id);
    }

    private void RemoveLock(object id)
    {
        if (VERBOSE)
            Debug.Log("remove lock: " + id);

        renderingActivatedAckLock.RemoveLock(id);
    }
}