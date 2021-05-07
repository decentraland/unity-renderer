using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;

public class BIWSaveController : BIWController
{
    [SerializeField]
    private float msBetweenSaves = 5000f;

    [Header("Prefab reference")]
    public BuilderInWorldBridge builderInWorldBridge;

    private float nextTimeToSave;
    private bool canActivateSave = true;

    public override void Init()
    {
        base.Init();
        if (builderInWorldBridge != null)
            builderInWorldBridge.OnKernelUpdated += TryToSave;
    }

    public void ResetSaveTime() { nextTimeToSave = 0; }

    public override void EnterEditMode(ParcelScene scene)
    {
        base.EnterEditMode(scene);
        nextTimeToSave = DCLTime.realtimeSinceStartup + msBetweenSaves / 1000f;
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

        builderInWorldBridge.SaveSceneState(sceneToEdit);
        nextTimeToSave = DCLTime.realtimeSinceStartup + msBetweenSaves / 1000f;
        HUDController.i.builderInWorldMainHud?.SceneSaved();
    }
}