using UnityEngine;

namespace MainScripts.DCL.Controllers.SettingsDesktop.SettingsControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/FPS Limit", fileName = "FPSLimitControlController")]
    public class FPSLimitControlController : SpinBoxSettingsControlControllerDesktop
    {
        private int[] allFpsValues;

        public override void Initialize()
        {
            allFpsValues = (int[])System.Enum.GetValues(typeof(FpsCapMode));
            base.Initialize();
            SetupLabels();
        }

        public override object GetStoredValue() =>
            currentDisplaySettings.fpsCapIndex;

        private void SetupLabels()
        {
            int length = allFpsValues.Length;
            var fpsLabels = new string[length];

            for (var i = 0; i < length; i++)
                fpsLabels[i] = allFpsValues[i] > 0 ? allFpsValues[i] + " FPS" : "Max";

            RaiseOnOverrideIndicatorLabel(fpsLabels);
        }

        public override void UpdateSetting(object newValue)
        {
            currentDisplaySettings.fpsCapIndex = (int)newValue;
            Application.targetFrameRate = allFpsValues[(int)newValue];

            ApplySettings();
        }
    }
}
