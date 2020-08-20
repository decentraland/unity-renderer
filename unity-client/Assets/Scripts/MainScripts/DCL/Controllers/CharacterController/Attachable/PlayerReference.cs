using UnityEngine;
using DCL.Models;
using DCL.Controllers;
using System.Collections.Generic;

public class PlayerReference : Attachable
{         

    public static PlayerReference i { get; private set; }        

    protected override void Setup()
    {
        // Singleton setup
        if (i != null)
        {
            Destroy(gameObject);
            return;
        }

        i = this;

        // Listen to changes on the camera mode
        CommonScriptableObjects.cameraMode.OnChange += OnCameraModeChange;
        
        // Trigger the initial camera set
        OnCameraModeChange(CommonScriptableObjects.cameraMode, CommonScriptableObjects.cameraMode);
    }

    protected override void Cleanup()
    {
        CommonScriptableObjects.cameraMode.OnChange -= OnCameraModeChange;
        CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;
    }

    private void UpdateForward(Vector3 newFordward, Vector3 prev)
    {       
        transform.forward = newForward;
    }   

    private void OnCameraModeChange(CameraMode.ModeId newMode, CameraMode.ModeId prev)
    {
        if (newMode == CameraMode.ModeId.ThirdPerson)
        {
            CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;
            transform.forward = transform.parent.forward;
        }
        else
        {
            CommonScriptableObjects.cameraForward.OnChange += UpdateForward;
        }
    }    
}
