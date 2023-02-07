using MainScripts.DCL.Controllers.SettingsDesktop.SettingsControllers;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/vSync", fileName = "VSyncControlController")]
    public class VSyncControlController : ToggleSettingsControlControllerDesktop
    {
        public override object GetStoredValue()
        {
            return currentDisplaySettings.vSync;
        }

        public override void UpdateSetting(object newValue)
        {
            var value = (bool)newValue;
            currentDisplaySettings.vSync = value;
            QualitySettings.vSyncCount = value ? 1 : 0;
        }
    }
}
