using DCL;
using DCL.Camera;
using DCL.Helpers;
using System.IO;
using UnityEngine;
using Environment = DCL.Environment;

namespace UI.InWorldCamera.Scripts
{
    public class ScreenshotCamera : MonoBehaviour
    {
        [Header("EXTERNAL DEPENDENCIES")]
        [SerializeField] private DCLCharacterController characterController;
        [SerializeField] private CameraController cameraController;

        [Header("MAIN COMPONENTS")]
        [SerializeField] private Camera cameraPrefab;
        [SerializeField] private ScreenshotHUDView screenshotHUDViewPrefab;

        [Header("INPUT ACTIONS")]
        [SerializeField] private InputAction_Trigger cameraInputAction;
        [SerializeField] private InputAction_Trigger takeScreenshotAction;

        private bool isInstantiated;
        private bool isInScreenshotMode;

        private Camera screenshotCamera;
        private ScreenshotHUDView screenshotHUDView;

        private Transform characterCameraTransform;
        private IAvatarsLODController avatarsLODControllerLazyValue;

        private ScreenshotCapture screenshotCaptureLazyValue;

        private IAvatarsLODController avatarsLODController => avatarsLODControllerLazyValue ??= Environment.i.serviceLocator.Get<IAvatarsLODController>();

        private ScreenshotCapture screenshotCapture
        {
            get
            {
                if (isInstantiated)
                    return screenshotCaptureLazyValue;

                InstantiateCameraObjects();

                return screenshotCaptureLazyValue;
            }
        }

        private void OnEnable()
        {
            cameraInputAction.OnTriggered += ToggleScreenshotCamera;
            takeScreenshotAction.OnTriggered += CaptureScreenshot;
        }

        private void OnDisable()
        {
            cameraInputAction.OnTriggered -= ToggleScreenshotCamera;
            takeScreenshotAction.OnTriggered -= CaptureScreenshot;
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger _)
        {
            bool activateScreenshotCamera = !(isInstantiated && screenshotCamera.gameObject.activeSelf);

            if (activateScreenshotCamera)
                EnableScreenshotCamera();

            screenshotCamera.gameObject.SetActive(activateScreenshotCamera);
            screenshotHUDView.SwitchVisibility(activateScreenshotCamera);

            CommonScriptableObjects.isScreenshotCameraActive.Set(activateScreenshotCamera);

            Utils.LockCursor();

            CommonScriptableObjects.allUIHidden.Set(activateScreenshotCamera);
            CommonScriptableObjects.cameraModeInputLocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.cameraBlocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.userMovementKeysBlocked.Set(activateScreenshotCamera);

            cameraController.SetCameraEnabledState(!activateScreenshotCamera);
            characterController.SetEnabled(!activateScreenshotCamera);

            avatarsLODController.SetCamera(activateScreenshotCamera ? screenshotCamera : cameraController.GetCamera());

            isInScreenshotMode = activateScreenshotCamera;
        }

        private void CaptureScreenshot(DCLAction_Trigger _)
        {
            if (isInScreenshotMode)
                SaveScreenshot(screenshotCapture.CaptureScreenshot());

            void SaveScreenshot(byte[] fileBytes)
            {
                string filePath = Path.Combine(Application.temporaryCachePath, "screenshot1.jpg"); // Application.persistentDataPath

                File.WriteAllBytes(filePath, fileBytes);
                Application.OpenURL(filePath);
                Debug.Log(filePath);
            }
        }

        private void EnableScreenshotCamera()
        {
            if (!isInstantiated)
                InstantiateCameraObjects();

            screenshotCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
        }

        private void InstantiateCameraObjects()
        {
            screenshotCamera = Instantiate(cameraPrefab);
            screenshotHUDView = Instantiate(screenshotHUDViewPrefab);

            characterCameraTransform = cameraController.GetCamera().transform;
            screenshotCamera.gameObject.layer = characterController.gameObject.layer;

            screenshotCaptureLazyValue = new ScreenshotCapture(screenshotCamera, screenshotHUDView.RectTransform, screenshotHUDView.RefImage);

            isInstantiated = true;
        }
    }
}
