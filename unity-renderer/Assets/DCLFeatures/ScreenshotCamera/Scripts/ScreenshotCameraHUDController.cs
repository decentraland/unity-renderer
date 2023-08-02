using DCL;
using System;
using Object = UnityEngine.Object;

namespace DCLFeatures.ScreenshotCamera
{
    public class ScreenshotCameraHUDController : IDisposable
    {
        private readonly ScreenshotCameraHUDView view;
        private readonly ScreenshotCamera screenshotCamera;

        public ScreenshotCameraHUDController(ScreenshotCameraHUDView view, ScreenshotCamera screenshotCamera)
        {
            this.view = view;
            this.screenshotCamera = screenshotCamera;
        }

        public void Initialize()
        {
            view.CloseButtonClicked += screenshotCamera.DisableScreenshotCameraMode;
            view.TakeScreenshotButtonClicked += screenshotCamera.CaptureScreenshot;
            view.ShortcutButtonClicked += view.ToggleShortcutsHelpPanel;
            view.CameraReelButtonClicked += OpenCameraReelGallery;
        }

        public void Dispose()
        {
            view.CloseButtonClicked -= screenshotCamera.DisableScreenshotCameraMode;
            view.TakeScreenshotButtonClicked -= screenshotCamera.CaptureScreenshot;
            view.ShortcutButtonClicked -= view.ToggleShortcutsHelpPanel;
            view.CameraReelButtonClicked -= OpenCameraReelGallery;

            Object.Destroy(view);
        }

        private void OpenCameraReelGallery()
        {
            screenshotCamera.DisableScreenshotCameraMode();
            DataStore.i.HUDs.cameraReelSectionVisible.Set(true);
        }
    }
}
