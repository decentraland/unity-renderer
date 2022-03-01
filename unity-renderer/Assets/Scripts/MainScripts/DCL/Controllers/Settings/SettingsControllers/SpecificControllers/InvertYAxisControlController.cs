using DCL.Interface;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Invert Y Axis", fileName = "InvertYAxisControlController")]
    public class InvertYAxisControlController : ToggleSettingsControlController
    {

        public override void Initialize()
        {
            base.Initialize();
            DataStore.i.camera.invertYAxis.OnChange += InvertYAxisChanged;
        }

        public override object GetStoredValue() { return currentGeneralSettings.invertYAxis; }

        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.invertYAxis = (bool)newValue;
            DataStore.i.camera.invertYAxis.Set((bool)newValue);
        }

        private void InvertYAxisChanged(bool current, bool previous)
        {
            currentGeneralSettings.invertYAxis = current;
            ApplySettings();
        }
    }
}