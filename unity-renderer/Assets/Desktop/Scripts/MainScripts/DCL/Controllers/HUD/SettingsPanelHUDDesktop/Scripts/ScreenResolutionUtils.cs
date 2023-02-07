using System.Collections.Generic;
using MainScripts.DCL.Controllers.SettingsDesktop;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts
{
    public static class ScreenResolutionUtils
    {
        private static Resolution[] resolutions;
        public static IReadOnlyList<Resolution> Resolutions => resolutions ??= Screen.resolutions;

        public static void Apply(DisplaySettings displaySettings)
        {
            var fullscreenMode = displaySettings.GetFullScreenMode();

            var resolution = GetResolution(displaySettings);
            Screen.SetResolution(resolution.width, resolution.height, fullscreenMode, resolution.refreshRate);
        }

        // Resolution list goes from the smallest to the biggest, our index is inverted for usage reasons so 0 is the biggest resolution available
        private static Resolution GetResolution(DisplaySettings displaySettings)
        {
            return Resolutions[Resolutions.Count - 1 - displaySettings.resolutionSizeIndex];
        }
    }
}
