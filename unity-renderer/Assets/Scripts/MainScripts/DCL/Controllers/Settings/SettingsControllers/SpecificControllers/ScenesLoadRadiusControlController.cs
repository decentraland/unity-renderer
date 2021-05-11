using DCL.Interface;
using DCL.SettingsController;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Scenes Load Radius", fileName = "ScenesLoadRadiusControlController")]
    public class ScenesLoadRadiusControlController : SliderSettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentGeneralSettings.scenesLoadRadius;
        }

        public override void UpdateSetting(object newValue)
        {
            float parsedValue = (float)newValue;
            currentGeneralSettings.scenesLoadRadius = parsedValue;

            // trigger LOS update and re-load of surrounding parcels in Kernel
            WebInterface.SetScenesLoadRadius(parsedValue);
        }
    }
}