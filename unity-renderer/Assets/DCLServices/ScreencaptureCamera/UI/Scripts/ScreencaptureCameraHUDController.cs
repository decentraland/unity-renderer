using DCL;
using DCLFeatures.ScreencaptureCamera.CameraObject;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCLFeatures.ScreencaptureCamera.UI
{
    public class ScreencaptureCameraHUDController : IDisposable
    {
        private readonly ScreencaptureCameraHUDView view;
        private readonly ScreencaptureCameraBehaviour screencaptureCameraBehaviour;
        private readonly DataStore dataStore;

        private InputAction_Trigger takeScreenshotAction;
        private InputAction_Trigger closeWindowAction;
        private InputAction_Hold mouseFirstClick;

        public ScreencaptureCameraHUDController(ScreencaptureCameraHUDView view, ScreencaptureCameraBehaviour screencaptureCameraBehaviour,
            DataStore dataStore)
        {
            this.view = view;
            this.screencaptureCameraBehaviour = screencaptureCameraBehaviour;
            this.dataStore = dataStore;
        }

        public void Initialize()
        {
            takeScreenshotAction = Resources.Load<InputAction_Trigger>("TakeScreenshot");
            closeWindowAction = Resources.Load<InputAction_Trigger>("CloseWindow");
            mouseFirstClick = Resources.Load<InputAction_Hold>("MouseFirstClickDown");

            view.CloseButtonClicked += DisableScreenshotCameraMode;
            closeWindowAction.OnTriggered += DisableScreenshotCameraMode;

            view.TakeScreenshotButtonClicked += CaptureScreenshot;
            takeScreenshotAction.OnTriggered += CaptureScreenshot;

            view.CameraReelButtonClicked += OpenCameraReelGallery;

            view.ShortcutsInfoButtonClicked += view.ToggleShortcutsInfosHelpPanel;

            mouseFirstClick.OnStarted += HideShortcutsInfoPanel;
        }

        public void Dispose()
        {
            view.CloseButtonClicked -= DisableScreenshotCameraMode;
            closeWindowAction.OnTriggered -= DisableScreenshotCameraMode;

            view.TakeScreenshotButtonClicked -= CaptureScreenshot;
            takeScreenshotAction.OnTriggered -= CaptureScreenshot;

            view.CameraReelButtonClicked -= OpenCameraReelGallery;

            view.ShortcutsInfoButtonClicked -= view.ToggleShortcutsInfosHelpPanel;


            Object.Destroy(view.gameObject);
        }

        public void SetVisibility(bool isVisible, bool hasStorageSpace) =>
            view.SetVisibility(isVisible, hasStorageSpace);

        private void HideShortcutsInfoPanel(DCLAction_Hold _)
        {
            view.ToggleShortcutsInfosHelpPanel();
            mouseFirstClick.OnStarted -= HideShortcutsInfoPanel;
        }

        public void PlayScreenshotFX(Texture2D image, float splashDuration, float middlePauseDuration, float transitionDuration)
        {
            AudioScriptableObjects.takeScreenshot.Play();
            view.ScreenshotCaptureAnimation(image, splashDuration, middlePauseDuration, transitionDuration);
        }

        private void CaptureScreenshot(DCLAction_Trigger _) =>
            screencaptureCameraBehaviour.CaptureScreenshot("Shortcut");

        private void CaptureScreenshot() =>
            screencaptureCameraBehaviour.CaptureScreenshot("Button");

        private void DisableScreenshotCameraMode() =>
            screencaptureCameraBehaviour.ToggleScreenshotCamera(isEnabled: false);

        private void DisableScreenshotCameraMode(DCLAction_Trigger _) =>
            DisableScreenshotCameraMode();

        private void OpenCameraReelGallery() =>
            OpenCameraReelGallery("Camera");

        private void OpenCameraReelGallery(string source)
        {
            if (!view.IsVisible) return;
            DisableScreenshotCameraMode();
            dataStore.HUDs.cameraReelOpenSource.Set(source);
            dataStore.HUDs.cameraReelSectionVisible.Set(true);
        }
    }
}
