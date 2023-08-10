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
        private readonly ScreencaptureCameraInputSchema input;
        private readonly DataStore dataStore;

        public ScreencaptureCameraHUDController(ScreencaptureCameraHUDView view,
            ScreencaptureCameraBehaviour screencaptureCameraBehaviour,
            ScreencaptureCameraInputSchema input,
            DataStore dataStore)
        {
            this.view = view;
            this.screencaptureCameraBehaviour = screencaptureCameraBehaviour;
            this.input = input;
            this.dataStore = dataStore;
        }

        public void Initialize()
        {
            view.CloseButtonClicked += DisableScreenshotCameraMode;
            input.CloseWindowAction.OnTriggered += DisableScreenshotCameraMode;

            view.TakeScreenshotButtonClicked += CaptureScreenshot;
            input.TakeScreenshotAction.OnTriggered += CaptureScreenshot;

            view.CameraReelButtonClicked += OpenCameraReelGallery;
            input.ToggleCameraReelAction.OnTriggered += OpenCameraReelGallery;

            view.ShortcutsInfoButtonClicked += view.ToggleShortcutsInfosHelpPanel;
        }

        public void Dispose()
        {
            view.CloseButtonClicked -= DisableScreenshotCameraMode;
            input.CloseWindowAction.OnTriggered -= DisableScreenshotCameraMode;

            view.TakeScreenshotButtonClicked -= CaptureScreenshot;
            input.TakeScreenshotAction.OnTriggered -= CaptureScreenshot;

            view.CameraReelButtonClicked -= OpenCameraReelGallery;
            input.ToggleCameraReelAction.OnTriggered -= OpenCameraReelGallery;

            view.ShortcutsInfoButtonClicked -= view.ToggleShortcutsInfosHelpPanel;

            Object.Destroy(view);
        }

        public void SetVisibility(bool isVisible, bool hasStorageSpace) =>
            view.SetVisibility(isVisible, hasStorageSpace);

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

        private void OpenCameraReelGallery(DCLAction_Trigger _) =>
            OpenCameraReelGallery("Shortcut");

        private void OpenCameraReelGallery(string source)
        {
            if (!view.IsVisible) return;
            DisableScreenshotCameraMode();
            dataStore.HUDs.cameraReelOpenSource.Set(source);
            dataStore.HUDs.cameraReelSectionVisible.Set(true);
        }
    }
}
