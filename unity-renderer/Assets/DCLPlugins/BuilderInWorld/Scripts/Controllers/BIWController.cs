using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

public abstract class BIWController : IBIWController
{
    internal ParcelScene sceneToEdit;

    protected bool isEditModeActive = false;
    protected IContext context;
    public virtual void Initialize(IContext context)
    {
        this.context = context;
        isEditModeActive = false;
    }

    public virtual void EnterEditMode(IParcelScene scene)
    {
        this.sceneToEdit = (ParcelScene)scene;
        isEditModeActive = true;
    }

    public virtual void ExitEditMode()
    {
        isEditModeActive = false;
        sceneToEdit = null;
    }

    public virtual void OnGUI() { }

    public virtual void LateUpdate() { }

    public virtual void Update() { }

    public virtual void Dispose() { }
}