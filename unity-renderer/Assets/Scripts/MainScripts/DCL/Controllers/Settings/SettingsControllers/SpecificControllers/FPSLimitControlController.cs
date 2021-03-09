using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/FPS Limit", fileName = "FPSLimitControlController")]
    public class FPSLimitControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.fpsCap;
        }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.fpsCap = (bool)newValue;
            ToggleFPSCap(currentQualitySetting.fpsCap);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] public static extern void ToggleFPSCap(bool useFPSCap);
#else
        public static void ToggleFPSCap(bool useFPSCap)
        {
        }
#endif
    }
}