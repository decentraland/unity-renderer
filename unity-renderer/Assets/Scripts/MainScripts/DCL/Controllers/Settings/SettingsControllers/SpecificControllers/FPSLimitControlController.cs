using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/FPS Limit", fileName = "FPSLimitControlController")]
    public class FPSLimitControlController : ToggleSettingsControlController
    {
        const int MAX_FPS = 240;
        
        public override object GetStoredValue() { return currentQualitySetting.fpsCap; }

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
            Application.targetFrameRate = useFPSCap ? 30 : MAX_FPS;
        }
#endif
    }
}