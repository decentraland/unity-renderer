using DCL.Interface;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Scenes Load Radius", fileName = "ScenesLoadRadiusControlController")]
    public class ScenesLoadRadiusControlController : SliderSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();

            ApplyAndReportScenesLoadRadius(currentGeneralSettings.scenesLoadRadius, true);
        }

        public override object GetStoredValue() { return currentGeneralSettings.scenesLoadRadius; }

        public override void UpdateSetting(object newValue) { ApplyAndReportScenesLoadRadius((float)newValue); }

        private void ApplyAndReportScenesLoadRadius(float newRadius, bool forceApply = false)
        {
            if (!forceApply && newRadius == currentGeneralSettings.scenesLoadRadius)
                return;

            currentGeneralSettings.scenesLoadRadius = newRadius;

            // trigger LOS update and re-load of surrounding parcels in Kernel
            WebInterface.SetScenesLoadRadius(newRadius);
        }
    }
}