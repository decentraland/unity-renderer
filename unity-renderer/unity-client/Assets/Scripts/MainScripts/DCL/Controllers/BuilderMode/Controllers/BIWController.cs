using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWController : MonoBehaviour
{
    protected ParcelScene sceneToEdit;

    protected bool isEditModeActive = false;

    public virtual void Init()
    {
        isEditModeActive = false;
    }

    public virtual void EnterEditMode(ParcelScene sceneToEdit)
    {
        this.sceneToEdit = sceneToEdit;
        isEditModeActive = true;
    }

    public virtual void ExitEditMode()
    {
        isEditModeActive = false;
        sceneToEdit = null;
    }

    protected virtual void Update()
    {
        if (!isEditModeActive)
            return;
        FrameUpdate();
    }

    protected virtual void FrameUpdate()
    {

    }
}
