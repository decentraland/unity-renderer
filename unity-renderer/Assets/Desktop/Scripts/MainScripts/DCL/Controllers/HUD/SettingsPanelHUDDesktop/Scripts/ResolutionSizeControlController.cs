using System.Collections.Generic;
using System.Linq;
using MainScripts.DCL.Controllers.SettingsDesktop.SettingsControllers;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Resolution Size",
        fileName = "ResolutionSizeControlController")]
    public class ResolutionSizeControlController : SpinBoxSettingsControlControllerDesktop
    {
        private readonly List<Resolution> possibleResolutions = new ();

        public override void Initialize()
        {
            SetupAvailableResolutions();
            base.Initialize();
            SetupLabels();
        }

        // Filter the smallest resolutions as no one will ever use them
        private void SetupAvailableResolutions()
        {
            possibleResolutions.AddRange(ScreenResolutionUtils.Resolutions.SkipWhile(r => r.width <= 1024));
        }

        private void SetupLabels()
        {
            var length = possibleResolutions.Count;
            var resolutionLabels = new string[length];

            for (var i = 0; i < length; i++)
            {
                var resolution = possibleResolutions[i];

                // by design we want the list to be inverted so the biggest resolutions stay on top
                // our resolutionSizeIndex is based on this decision
                resolutionLabels[length - 1 - i] = GetLabel(resolution);
            }

            RaiseOnOverrideIndicatorLabel(resolutionLabels);
        }

        private static string GetLabel(Resolution resolution) =>
            $"{resolution.width}x{resolution.height} ({GetAspectRatio(resolution.width, resolution.height)}) {resolution.refreshRate} Hz";

        public override object GetStoredValue()
        {
            int index = currentDisplaySettings.resolutionSizeIndex;
            return index >= 0 ? index : ScreenResolutionUtils.DefaultIndex;
        }

        private static string GetAspectRatio(int width, int height)
        {
            int rest;
            int tempWidth = width;
            int tempHeight = height;

            while (height != 0)
            {
                rest = width % height;
                width = height;
                height = rest;
            }

            return $"{tempWidth / width}:{tempHeight / width}";
        }

        public override void UpdateSetting(object newValue)
        {
            var value = (int)newValue;
            currentDisplaySettings.resolutionSizeIndex = value;
            ScreenResolutionUtils.Apply(currentDisplaySettings);
        }
    }
}
