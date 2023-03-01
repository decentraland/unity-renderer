using DCL;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Interface;
using System;
using UnityEngine;

public class RenderingController : MonoBehaviour
{
    public static float firstActivationTime { get; private set; }
    private bool firstActivationTimeHasBeenSet;
    private readonly bool VERBOSE = false;

    public CompositeLock renderingActivatedAckLock = new ();

    private bool activatedRenderingBefore { get; set; }
    private bool isDecoupledLoadingScreenEnabled => true;
    private bool isSignUpFlow => DataStore.i.common.isSignUpFlow.Get();

    private DataStoreRef<DataStore_LoadingScreen> dataStore_LoadingScreenRef;

    private void Awake()
    {
        CommonScriptableObjects.rendererState.OnLockAdded += AddLock;
        CommonScriptableObjects.rendererState.OnLockRemoved += RemoveLock;
        CommonScriptableObjects.rendererState.Set(false);

        dataStore_LoadingScreenRef.Ref.decoupledLoadingHUD.visible.OnChange += DecoupleLoadingScreenVisibilityChange;
        DecoupleLoadingScreenVisibilityChange(true, true);
    }

    private void DecoupleLoadingScreenVisibilityChange(bool visible, bool _)
    {
        if (visible)
            DeactivateRendering_Internal();
        else
            //Coming-from-kernel condition. If we are on signup flow, then we must force the ActivateRendering
            ActivateRendering_Internal(isSignUpFlow);
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.rendererState.OnLockAdded -= AddLock;
        CommonScriptableObjects.rendererState.OnLockRemoved -= RemoveLock;
    }

    [ContextMenu("Disable Rendering")]
    public void DeactivateRendering()
    {
        if (isDecoupledLoadingScreenEnabled) return;

        DeactivateRendering_Internal();
    }

    private void DeactivateRendering_Internal()
    {
        if (!CommonScriptableObjects.rendererState.Get()) return;

        CommonScriptableObjects.rendererState.Set(false);

        if (!isDecoupledLoadingScreenEnabled)
            WebInterface.ReportControlEvent(new WebInterface.DeactivateRenderingACK());
    }

    [ContextMenu("Enable Rendering")]
    public void ActivateRendering()
    {
        if (isDecoupledLoadingScreenEnabled) return;

        ActivateRendering_Internal(forceActivate: false);
    }

    public void ForceActivateRendering()
    {
        if (isDecoupledLoadingScreenEnabled) return;

        ActivateRendering_Internal(forceActivate: true);
    }

    public void ActivateRendering_Internal(bool forceActivate)
    {
        if (CommonScriptableObjects.rendererState.Get()) return;

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

        CommonScriptableObjects.rendererState.Set(true);

        if (!isDecoupledLoadingScreenEnabled)
            WebInterface.ReportControlEvent(new WebInterface.ActivateRenderingACK());
    }

    private void AddLock(object id)
    {
        if (CommonScriptableObjects.rendererState.Get())
            return;

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
