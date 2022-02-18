using DCL.Interface;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Invert Y Axis", fileName = "InvertYAxisControlController")]
    public class InvertYAxisControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() { return currentGeneralSettings.invertYAxis; }

        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.invertYAxis = (bool)newValue;
            SceneReferences.i.thirdPersonCamera.m_YAxis.m_InvertInput = !currentGeneralSettings.invertYAxis;
            DataStore.i.camera.invertYAxis.Set((bool)newValue);
        }
    }
}