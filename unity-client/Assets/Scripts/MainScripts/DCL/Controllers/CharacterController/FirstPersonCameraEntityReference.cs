using UnityEngine;

public class FirstPersonCameraEntityReference : MonoBehaviour
{

    public Transform cameraPosition;

    private void Awake()
    {
        // Assign the camera position to the game object
        if (cameraPosition != null)
        {
            transform.position = cameraPosition.position;
        }

        // Listen to changes on the camera mode
        CommonScriptableObjects.cameraMode.OnChange += OnCameraModeChange;

        // Trigger the initial camera set
        OnCameraModeChange(CommonScriptableObjects.cameraMode, CommonScriptableObjects.cameraMode);
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.cameraMode.OnChange -= OnCameraModeChange;
        CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;
    }

    private void UpdateForward(Vector3 newForward, Vector3 prev) { transform.forward = newForward; }

    private void OnCameraModeChange(CameraMode.ModeId newMode, CameraMode.ModeId prev)
    {
        if (newMode == CameraMode.ModeId.ThirdPerson)
        {
            CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;
            transform.forward = transform.parent.forward;
        }
        else if (newMode == CameraMode.ModeId.BuildingToolGodMode)
        {
            CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;
        }
        else
        {
            CommonScriptableObjects.cameraForward.OnChange += UpdateForward;
        }
    }
}