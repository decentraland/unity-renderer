using DCL.Camera;
using UnityEngine;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    public class ScreenshotCamera : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private CameraController cameraController;

        [Space]
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private InputAction_Trigger cameraInputAction;

        private bool isInitialized;

        private Camera screenshotCamera;

        private void OnEnable()
        {
            cameraInputAction.OnTriggered += ToggleScreenshotCamera;
        }

        private void OnDisable()
        {
            cameraInputAction.OnTriggered -= ToggleScreenshotCamera;
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger action)
        {
            bool cameraIsEnabled = isInitialized && screenshotCamera.enabled;

            if (!cameraIsEnabled)
                EnableScreenshotCamera();

            screenshotCamera.enabled = !cameraIsEnabled;
            cameraController.SetCameraEnabledState(!screenshotCamera.enabled);
        }

        private void EnableScreenshotCamera()
        {
            if (!isInitialized)
            {
                screenshotCamera = Instantiate(cameraPrefab).GetComponent<Camera>();
                isInitialized = true;
            }

            CopyMainCamera();
        }

        private void CopyMainCamera()
        {
            screenshotCamera.CopyFrom(cameraController.GetCamera());
            screenshotCamera.gameObject.layer = characterController.gameObject.layer;
        }
    }
}
