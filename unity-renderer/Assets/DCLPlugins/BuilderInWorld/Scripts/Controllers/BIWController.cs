using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

public abstract class BIWController : IBIWController
{
    internal IParcelScene sceneToEdit;
    internal IBuilderScene builderScene;

    protected bool isEditModeActive = false;
    protected IContext context;
    public virtual void Initialize(IContext context)
    {
        this.context = context;
        isEditModeActive = false;
    }

    public virtual void EnterEditMode(IBuilderScene scene)
    {
        builderScene = scene;
        sceneToEdit = builderScene.scene;
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