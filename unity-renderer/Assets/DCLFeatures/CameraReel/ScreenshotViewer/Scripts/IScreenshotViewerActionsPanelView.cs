using System;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public interface IScreenshotViewerActionsPanelView
    {
        event Action DownloadClicked;
        event Action DeleteClicked;
        event Action LinkClicked;
        event Action TwitterClicked;
        event Action InfoClicked;
    }
}
