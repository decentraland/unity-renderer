using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Camera Movement Mode", fileName = nameof(CameraMovementModeControlController))]
    public class CameraMovementModeControlController : SpinBoxSettingsControlController
    {
        public override object GetStoredValue() =>
            currentGeneralSettings.leftMouseButtonCursorLock ? 0 : 1;

        public override void UpdateSetting(object newValue)
        {
            bool lockOnLeftMouseButton = (int)newValue == 0;

            currentGeneralSettings.leftMouseButtonCursorLock = lockOnLeftMouseButton;
            DataStore.i.camera.leftMouseButtonCursorLock.Set(lockOnLeftMouseButton);
            ApplySettings();
        }
    }
}
