using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBIWController
{
    void Initialize(Context context);
    void EnterEditMode(IParcelScene scene);
    void ExitEditMode();
    void OnGUI();

    void LateUpdate();

    void Update();
    void Dispose();
}

public abstract class BIWController : IBIWController
{
    internal ParcelScene sceneToEdit;

    protected bool isEditModeActive = false;
    protected Context context;
    public virtual void Initialize(Context context)
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