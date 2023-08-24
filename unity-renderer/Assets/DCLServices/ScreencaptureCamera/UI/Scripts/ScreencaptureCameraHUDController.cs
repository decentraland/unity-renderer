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
        private readonly HUDController hudController;

        public ScreencaptureCameraHUDController(ScreencaptureCameraHUDView view,
            ScreencaptureCameraBehaviour screencaptureCameraBehaviour,
            ScreencaptureCameraInputSchema input,
            DataStore dataStore, HUDController hudController)
        {
            this.view = view;
            this.screencaptureCameraBehaviour = screencaptureCameraBehaviour;
            this.input = input;
            this.dataStore = dataStore;
            this.hudController = hudController;
        }

        public void Initialize()
        {
            view.CloseButtonClicked += DisableScreenshotCameraMode;
            input.CloseWindowAction.OnTriggered += DisableScreenshotCameraMode;

            view.TakeScreenshotButtonClicked += CaptureScreenshot;
            input.TakeScreenshotAction.OnTriggered += CaptureScreenshot;

            view.CameraReelButtonClicked += OpenCameraReelGallery;
            view.ShortcutsInfoButtonClicked += view.ToggleShortcutsInfosHelpPanel;

            input.MouseFirstClick.OnStarted += HideShortcutsInfoPanel;
            input.ToggleScreenshotViewVisibilityAction.OnTriggered += ToggleViewVisibility;
        }

        public void Dispose()
        {
            view.CloseButtonClicked -= DisableScreenshotCameraMode;
            input.CloseWindowAction.OnTriggered -= DisableScreenshotCameraMode;

            view.TakeScreenshotButtonClicked -= CaptureScreenshot;
            input.TakeScreenshotAction.OnTriggered -= CaptureScreenshot;

            view.CameraReelButtonClicked -= OpenCameraReelGallery;
            view.ShortcutsInfoButtonClicked -= view.ToggleShortcutsInfosHelpPanel;

            input.MouseFirstClick.OnStarted -= HideShortcutsInfoPanel;
            input.ToggleScreenshotViewVisibilityAction.OnTriggered -= ToggleViewVisibility;

            Object.Destroy(view.gameObject);
        }

        private void ToggleViewVisibility(DCLAction_Trigger _)
        {
            if (!screencaptureCameraBehaviour.isScreencaptureCameraActive.Get()) return;

            SetVisibility(!view.IsVisible, screencaptureCameraBehaviour.HasStorageSpace);

            if (view.IsVisible)
                AudioScriptableObjects.UIShow.Play();
            else
                AudioScriptableObjects.UIHide.Play();

            hudController.ToggleAllUIHiddenNotification(isHidden: !view.IsVisible, false);
        }

        public void SetVisibility(bool isVisible, bool hasStorageSpace)
        {
            // Hide AllUIHidden notification when entering camera mode
            if (isVisible)
                hudController.ToggleAllUIHiddenNotification(isHidden: false, false);

            view.SetVisibility(isVisible, hasStorageSpace);
        }

        private void HideShortcutsInfoPanel(DCLAction_Hold _)
        {
            view.ToggleShortcutsInfosHelpPanel();
            input.MouseFirstClick.OnStarted -= HideShortcutsInfoPanel;
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
