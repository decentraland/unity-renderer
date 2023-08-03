using DCL;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCLFeatures.ScreencaptureCamera
{
    public class ScreencaptureCameraHUDController : IDisposable
    {
        private readonly ScreencaptureCameraHUDView view;
        private readonly ScreencaptureCamera screencaptureCamera;
        private readonly ScreencaptureCameraInputSchema input;

        public ScreencaptureCameraHUDController(ScreencaptureCameraHUDView view, ScreencaptureCamera screencaptureCamera, ScreencaptureCameraInputSchema input)
        {
            this.view = view;
            this.screencaptureCamera = screencaptureCamera;
            this.input = input;
        }

        public void Initialize()
        {
            view.CloseButtonClicked += DisableScreenshotCameraMode;
            input.CloseWindowAction.OnTriggered += DisableScreenshotCameraMode;

            view.TakeScreenshotButtonClicked += screencaptureCamera.CaptureScreenshot;
            input.TakeScreenshotAction.OnTriggered += CaptureScreenshot;

            view.CameraReelButtonClicked += OpenCameraReelGallery;
            input.ToggleCameraReelAction.OnTriggered += OpenCameraReelGallery;

            view.ShortcutsInfoButtonClicked += view.ToggleShortcutsInfosHelpPanel;
        }

        public void Dispose()
        {
            view.CloseButtonClicked -= DisableScreenshotCameraMode;
            input.CloseWindowAction.OnTriggered -= DisableScreenshotCameraMode;

            view.TakeScreenshotButtonClicked -= screencaptureCamera.CaptureScreenshot;
            input.TakeScreenshotAction.OnTriggered -= CaptureScreenshot;

            view.CameraReelButtonClicked -= OpenCameraReelGallery;
            input.ToggleCameraReelAction.OnTriggered -= OpenCameraReelGallery;

            view.ShortcutsInfoButtonClicked -= view.ToggleShortcutsInfosHelpPanel;

            Object.Destroy(view);
        }

        public void PlayScreenshotFX(Texture2D image, float splashDuration, float transitionDuration)
        {
            AudioScriptableObjects.takeScreenshot.Play();
            view.ScreenshotCaptureAnimation(image, splashDuration, transitionDuration);
        }

        private void CaptureScreenshot(DCLAction_Trigger _) =>
            screencaptureCamera.CaptureScreenshot();

        private void DisableScreenshotCameraMode() =>
            screencaptureCamera.SetVisibility( isVisible: false);

        private void DisableScreenshotCameraMode(DCLAction_Trigger _) =>
            DisableScreenshotCameraMode();

        private void OpenCameraReelGallery()
        {
            DisableScreenshotCameraMode();
            DataStore.i.HUDs.cameraReelSectionVisible.Set(true);
        }

        private void OpenCameraReelGallery(DCLAction_Trigger _)
            => OpenCameraReelGallery();
    }
}
