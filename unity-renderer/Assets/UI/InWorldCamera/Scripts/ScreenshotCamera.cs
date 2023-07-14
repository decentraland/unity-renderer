using DCL.Camera;
using UnityEngine;
using UnityEngine.Diagnostics;
using Utils = DCL.Helpers.Utils;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    public class ScreenshotCamera : MonoBehaviour
    {
        [SerializeField] private DCLCharacterController characterController;
        [SerializeField] private CameraController cameraController;

        [Space]
        [SerializeField] private Camera cameraPrefab;
        [SerializeField] private Canvas screenshotHUDViewPrefab;
        [SerializeField] private InputAction_Trigger cameraInputAction;

        private bool isInitialized;

        private Camera screenshotCamera;
        private Canvas screenshotHUDView;

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
            screenshotHUDView.enabled = activateScreenshotCamera;

            CommonScriptableObjects.isScreenshotCameraActive.Set(activateScreenshotCamera);

            Utils.LockCursor();

            CommonScriptableObjects.allUIHidden.Set(activateScreenshotCamera);
            CommonScriptableObjects.cameraModeInputLocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.cameraBlocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.userMovementKeysBlocked.Set(activateScreenshotCamera);

            cameraController.SetCameraEnabledState(!activateScreenshotCamera);
            characterController.SetEnabled(!activateScreenshotCamera);
        }

        private void EnableScreenshotCamera()
        {
            if (!isInitialized)
                CreateScreenshotCamera();

            screenshotCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
        }

        private void CreateScreenshotCamera()
        {
            screenshotCamera = Instantiate(cameraPrefab);
            screenshotHUDView = Instantiate(screenshotHUDViewPrefab);

            var characterCamera = cameraController.GetCamera();
            characterCameraTransform = characterCamera.transform;

            screenshotCamera.CopyFrom(characterCamera);
            screenshotCamera.gameObject.layer = characterController.gameObject.layer;

            isInitialized = true;
        }
    }
}
