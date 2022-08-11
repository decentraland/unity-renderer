using DCL.Camera;
using DCL.CameraTool;
using UnityEngine;

public class FirstPersonCameraEntityReference : MonoBehaviour
{
    public CameraController cameraController;
    public Transform cameraPosition;

    private Transform nextParent, firstPersonParent, initialParent;

    private void Awake()
    {
        // Assign the camera position to the game object
        if (cameraPosition != null)
        {
            transform.position = cameraPosition.position;
        }

        initialParent = transform.parent;
        
        firstPersonParent = cameraController.GetCamera().transform;

        // Listen to changes on the camera mode
        CommonScriptableObjects.cameraMode.OnChange += OnCameraModeChange;

        // Trigger the initial camera set
        OnCameraModeChange(CommonScriptableObjects.cameraMode, CommonScriptableObjects.cameraMode);

        //There is no blend in the first set so we parent to the correct initial transform
        SetNextParent();
    }

    private void UpdateForward(Vector3 newForward, Vector3 prev) { transform.forward = newForward; }

    private void OnDestroy()
    {
        CommonScriptableObjects.cameraMode.OnChange -= OnCameraModeChange;
        CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;
    }

    private void OnCameraModeChange(CameraMode.ModeId newMode, CameraMode.ModeId prev)
    {
        CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;

        if (newMode == CameraMode.ModeId.FirstPerson)
        {
            CommonScriptableObjects.cameraForward.OnChange += UpdateForward;
            nextParent = firstPersonParent;

            if (cameraController != null)
                cameraController.onCameraBlendFinished += SetNextParent;
        }
        else
        {
            if (newMode == CameraMode.ModeId.ThirdPerson)
                transform.forward = initialParent.forward;

            nextParent = initialParent;

            if (cameraController != null)
                cameraController.onCameraBlendStarted += SetNextParent;
        }
    }

    private void SetNextParent()
    {
        if (cameraController != null)
        {
            cameraController.onCameraBlendFinished -= SetNextParent;
            cameraController.onCameraBlendStarted -= SetNextParent;
        }

        transform.SetParent(nextParent);
    }
}