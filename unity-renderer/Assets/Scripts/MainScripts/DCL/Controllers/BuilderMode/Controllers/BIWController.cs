using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BIWController
{
    protected ParcelScene sceneToEdit;

    protected bool isEditModeActive = false;

    public virtual void Init(BIWContext context) { isEditModeActive = false; }

    public virtual void EnterEditMode(ParcelScene scene)
    {
        this.sceneToEdit = scene;
        isEditModeActive = true;
    }

    public virtual void ExitEditMode()
    {
        isEditModeActive = false;
        sceneToEdit = null;
    }

    public virtual void OnGUI() { }

    public virtual void LateUpdate() { }

    public virtual void Update()
    {
        if (!isEditModeActive)
            return;
    }

    public virtual void Dispose() { }
}