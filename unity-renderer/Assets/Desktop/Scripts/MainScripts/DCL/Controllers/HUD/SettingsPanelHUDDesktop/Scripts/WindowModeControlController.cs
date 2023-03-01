using System;
using MainScripts.DCL.Controllers.SettingsDesktop;
using MainScripts.DCL.Controllers.SettingsDesktop.SettingsControllers;
using MainScripts.DCL.ScriptableObjectsDesktop;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Window Mode",
        fileName = "WindowModeControlController")]
    public class WindowModeControlController : SpinBoxSettingsControlControllerDesktop
    {
        public override object GetStoredValue()
        {
            return (int)currentDisplaySettings.windowMode;
        }

        public override void UpdateSetting(object newValue)
        {
            currentDisplaySettings.windowMode = (WindowMode)(int)newValue;

            switch (currentDisplaySettings.windowMode)
            {
                case WindowMode.Windowed:
                case WindowMode.FullScreen:
                    UpdateResolution();
                    break;
                case WindowMode.Borderless:
                    SetupBorderless();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CommonScriptableObjectsDesktop.disableVSync.Set(currentDisplaySettings.windowMode == WindowMode.Windowed);
            CommonScriptableObjectsDesktop.disableScreenResolution.Set(currentDisplaySettings.windowMode == WindowMode.Borderless);
        }

        private void SetupBorderless()
        {
            ApplySettings();
            UpdateResolution();
        }

        private void UpdateResolution()
        {
            ScreenResolutionUtils.Apply(currentDisplaySettings);
        }
    }
}
