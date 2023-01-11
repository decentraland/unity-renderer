using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Left Mouse Button Cursor Lock", fileName = "LeftMouseButtonCursorLockControlController")]
    public class LeftMouseButtonCursorLockControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() =>
            currentGeneralSettings.leftMouseButtonCursorLock;

        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.leftMouseButtonCursorLock = (bool)newValue;
            DataStore.i.camera.leftMouseButtonCursorLock.Set((bool)newValue);
            ApplySettings();
        }
    }
}
