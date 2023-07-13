using DCL.Camera;
using UnityEngine;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    public class ScreenshotCamera : MonoBehaviour
    {
        [SerializeField] private DCLCharacterController characterController;
        [SerializeField] private CameraController cameraController;

        [Space]
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private InputAction_Trigger cameraInputAction;

        private bool isInitialized;

        private Camera screenshotCamera;
        private Transform characterCameraTransform;

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
            bool activateScreenshotCamera = !(isInitialized && screenshotCamera.gameObject.activeSelf);

            if (activateScreenshotCamera)
                EnableScreenshotCamera();

            screenshotCamera.gameObject.SetActive(activateScreenshotCamera);
            characterController.SetEnabled(!activateScreenshotCamera);
            cameraController.SetCameraEnabledState(!activateScreenshotCamera);

            CommonScriptableObjects.cameraBlocked.Set(activateScreenshotCamera);
        }

        private void EnableScreenshotCamera()
        {
            if (!isInitialized)
                CreateScreenshotCamera();

            screenshotCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
        }

        private void CreateScreenshotCamera()
        {
            screenshotCamera = Instantiate(cameraPrefab).GetComponent<Camera>();

            var characterCamera = cameraController.GetCamera();
            characterCameraTransform = characterCamera.transform;

            screenshotCamera.CopyFrom(characterCamera);
            screenshotCamera.gameObject.layer = characterController.gameObject.layer;

            isInitialized = true;
        }
    }
}
