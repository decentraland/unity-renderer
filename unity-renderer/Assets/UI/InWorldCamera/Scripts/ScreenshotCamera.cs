using DCL;
using DCL.Camera;
using DCL.Helpers;
using MainScripts.DCL.InWorldCamera.Scripts;
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

        private bool prevUiHiddenState;
        private bool prevMouseLockState;
        private bool prevMouseButtonCursorLockMode;

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

            Utils.UnlockCursor();

            if (activateScreenshotCamera)
            {
                prevUiHiddenState = CommonScriptableObjects.allUIHidden.Get();
                CommonScriptableObjects.allUIHidden.Set(true);

                prevMouseLockState = CommonScriptableObjects.cameraModeInputLocked.Get();
                CommonScriptableObjects.cameraModeInputLocked.Set(false);

                prevMouseButtonCursorLockMode = DataStore.i.camera.leftMouseButtonCursorLock.Get();
                DataStore.i.camera.leftMouseButtonCursorLock.Set(true);
            }
            else
            {
                CommonScriptableObjects.allUIHidden.Set(prevUiHiddenState);
                CommonScriptableObjects.cameraModeInputLocked.Set(prevMouseLockState);
                DataStore.i.camera.leftMouseButtonCursorLock.Set(prevMouseButtonCursorLockMode);
            }

            CommonScriptableObjects.cameraBlocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.userMovementKeysBlocked.Set(activateScreenshotCamera);

            cameraController.SetCameraEnabledState(!activateScreenshotCamera);
            characterController.SetEnabled(!activateScreenshotCamera);

            if (activateScreenshotCamera)
                EnableScreenshotCamera();

            screenshotCamera.gameObject.SetActive(activateScreenshotCamera);
            screenshotHUDView.SwitchVisibility(activateScreenshotCamera);
            avatarsLODController.SetCamera(activateScreenshotCamera ? screenshotCamera : cameraController.GetCamera());

            CommonScriptableObjects.isScreenshotCameraActive.Set(activateScreenshotCamera);
            isInScreenshotMode = activateScreenshotCamera;
        }

        private void CaptureScreenshot(DCLAction_Trigger _)
        {
            if (isInScreenshotMode)
                SaveScreenshot(screenshotCapture.CaptureScreenshot());

            void SaveScreenshot(byte[] fileBytes)
            {
                string filePath = Path.Combine(Application.persistentDataPath, "screenshot1.jpg");

                File.WriteAllBytes(filePath, fileBytes);
                Application.OpenURL(filePath);
                Debug.Log(filePath);
            }
        }

        private void EnableScreenshotCamera()
        {
            if (!isInstantiated)
                InstantiateCameraObjects();
            else
                screenshotCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
        }

        private void InstantiateCameraObjects()
        {
            characterCameraTransform = cameraController.GetCamera().transform;

            screenshotCamera = Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation);
            screenshotHUDView = Instantiate(screenshotHUDViewPrefab);

            screenshotCamera.gameObject.layer = characterController.gameObject.layer;

            screenshotCaptureLazyValue = new ScreenshotCapture(screenshotCamera, screenshotHUDView.RectTransform, screenshotHUDView.RefImage);

            isInstantiated = true;
        }
    }
}
