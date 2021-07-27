using System;
using DCL.Camera;
using UnityEngine;

public class FirstPersonCameraEntityReference : MonoBehaviour
{
    public CameraController cameraController;
    public Transform cameraPosition;

    private Transform nextParent, firtPersonParent, initialParent;

    private void Awake()
    {
        // Assign the camera position to the game object
        if (cameraPosition != null)
        {
            transform.position = cameraPosition.position;
        }
        initialParent = transform.parent;
        firtPersonParent = Camera.main.transform;

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
        if (newMode == CameraMode.ModeId.FirstPerson)
        {
            nextParent = firtPersonParent;
            cameraController.onCameraBlendFinished += SetNextParent;
            CommonScriptableObjects.cameraForward.OnChange += UpdateForward;
        }
        else
        {
            CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;
            nextParent = initialParent;
            cameraController.onCameraBlendStarted += SetNextParent;
        }
    }

    private void SetNextParent()
    {
        cameraController.onCameraBlendFinished -= SetNextParent;
        cameraController.onCameraBlendStarted -= SetNextParent;
        transform.SetParent(nextParent);
    }
}