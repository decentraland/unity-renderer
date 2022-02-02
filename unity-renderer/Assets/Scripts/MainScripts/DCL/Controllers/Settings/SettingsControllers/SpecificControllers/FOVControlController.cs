using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/FOV", fileName = "FOVControlController")]
    public class FOVControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentGeneralSettings.firstPersonCameraFOV; }

        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.firstPersonCameraFOV = (float)newValue;
            
            SceneReferences.i.firstPersonCamera.m_Lens.FieldOfView = currentGeneralSettings.firstPersonCameraFOV;
        }
    }
}