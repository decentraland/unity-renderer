﻿using System.Collections.Generic;
using MainScripts.DCL.Controllers.SettingsDesktop;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts
{
    public static class ScreenResolutionUtils
    {
        private static Resolution[] resolutions;

        // this offset was made for the 0 index to be Full HD instead of MAX
        private static int defaultIndex;
        public static IReadOnlyList<Resolution> Resolutions => resolutions ??= GetResolutions();

        private static Resolution[] GetResolutions()
        {
            Resolution[] resolutions = Screen.resolutions;

            int resolutionsLength = resolutions.Length;

            for (int i = resolutionsLength - 1; i >= 0; i--)
            {
                if (resolutions[i].width > 1920) continue;
                defaultIndex = resolutionsLength - 1 - i;
                break;
            }

            return resolutions;
        }

        public static int GetDefaultIndex() =>
            defaultIndex;

        public static void Apply(DisplaySettings displaySettings)
        {
            var fullscreenMode = displaySettings.GetFullScreenMode();

            var resolution = GetResolution(displaySettings);
            Screen.SetResolution(resolution.width, resolution.height, fullscreenMode, resolution.refreshRate);
        }

        // Resolution list goes from the smallest to the biggest, our index is inverted for usage reasons so 0 is the biggest resolution available
        private static Resolution GetResolution(DisplaySettings displaySettings)
        {
            int resolutionsCount = Resolutions.Count;
            int settingsIndex = displaySettings.resolutionSizeIndex;
            if (settingsIndex < 0) settingsIndex = GetDefaultIndex();
            return Resolutions[resolutionsCount - 1 - settingsIndex];
        }
    }
}
