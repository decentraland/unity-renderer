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

    private void Awake()
    {
        CommonScriptableObjects.rendererState.OnLockAdded += AddLock;
        CommonScriptableObjects.rendererState.OnLockRemoved += RemoveLock;
        CommonScriptableObjects.rendererState.Set(false);

        renderingActivatedAckLock.OnAllLocksRemoved += ActivateRendering_Internal;
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.rendererState.OnLockAdded -= AddLock;
        CommonScriptableObjects.rendererState.OnLockRemoved -= RemoveLock;
        renderingActivatedAckLock.OnAllLocksRemoved -= ActivateRendering_Internal;
    }

    private void DeactivateRendering_Internal()
    {
        if (!CommonScriptableObjects.rendererState.Get()) return;

        CommonScriptableObjects.rendererState.Set(false);
        WebInterface.ReportControlEvent(new WebInterface.DeactivateRenderingACK());
    }

    private void ActivateRendering_Internal()
    {
        if (CommonScriptableObjects.rendererState.Get()) return;

        if (!firstActivationTimeHasBeenSet)
        {
            firstActivationTime = Time.realtimeSinceStartup;
            firstActivationTimeHasBeenSet = true;
        }

        if (!activatedRenderingBefore)
        {
            Utils.UnlockCursor();
            activatedRenderingBefore = true;
        }

        CommonScriptableObjects.rendererState.Set(true);
        WebInterface.ReportControlEvent(new WebInterface.ActivateRenderingACK());
    }

    private void AddLock(object id)
    {
        if (VERBOSE)
            Debug.Log("Add lock: " + id);

        renderingActivatedAckLock.AddLock(id);
        DeactivateRendering_Internal();
    }

    private void RemoveLock(object id)
    {
        if (VERBOSE)
            Debug.Log("remove lock: " + id);

        renderingActivatedAckLock.RemoveLock(id);
    }
}
