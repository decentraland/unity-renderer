using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.SettingsDesktop
{
    [Serializable]
    public enum WindowMode
    {
        Windowed,
        Borderless,
        FullScreen,
    }

    [Serializable]
    public enum FpsCapMode
    {
        Max = 0,
        FPS30 = 30,
        FPS60 = 60,
        FPS90 = 90,
        FPS120 = 120,
        FPS144 = 144,
    }

    [Serializable]
    public struct DisplaySettings
    {
        public WindowMode windowMode;
        public int resolutionSizeIndex;
        public bool vSync;
        public int fpsCapIndex;

        public FullScreenMode GetFullScreenMode()
        {
            return windowMode switch
                   {
                       WindowMode.Windowed => FullScreenMode.Windowed,
                       WindowMode.Borderless => FullScreenMode.FullScreenWindow,
                       WindowMode.FullScreen => FullScreenMode.ExclusiveFullScreen,
                       _ => throw new ArgumentOutOfRangeException()
                   };
        }
    }
}
