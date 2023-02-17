using UnityEngine;

namespace MainScripts.DCL.Controllers.SettingsDesktop.SettingsControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/FPS Limit", fileName = "FPSLimitControlController")]
    public class FPSLimitControlController : SpinBoxSettingsControlControllerDesktop
    {
        int[] allFpsValues;

        public override void Initialize()
        {
            allFpsValues = (int[])System.Enum.GetValues(typeof(FpsCapMode));
            base.Initialize();
            SetupLabels();
        }

        public override object GetStoredValue()
        {
            return currentDisplaySettings.fpsCapIndex;
        }

        private void SetupLabels()
        {
            var length = allFpsValues.Length;
            var fpsLabels = new string[length];

            for (var i = 0; i < length; i++) { fpsLabels[i] = allFpsValues[i] > 0 ? allFpsValues[i].ToString() + " FPS" : "Max"; }

            RaiseOnOverrideIndicatorLabel(fpsLabels);
        }

        public override void UpdateSetting(object newValue)
        {
            var fpsValue = (int)allFpsValues[(int)newValue];
            currentDisplaySettings.fpsCapIndex = (int)newValue;
            ToggleFPSCap(fpsValue);
        }

        public static void ToggleFPSCap(int fpsValue)
        {
            Application.targetFrameRate = fpsValue;
        }
    }
}
