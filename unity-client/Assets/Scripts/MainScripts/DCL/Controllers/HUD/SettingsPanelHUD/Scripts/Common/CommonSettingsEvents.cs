using DCL.SettingsPanelHUD.Controls;
using System;

namespace DCL.SettingsPanelHUD.Common
{
    public static class CommonSettingsEvents
    {
        public static event Action<SettingsControlController> OnRefreshAllSettings;
        public static void RaiseRefreshAllSettings(SettingsControlController sender)
        {
            OnRefreshAllSettings?.Invoke(sender);
        }

        public static event Action OnRefreshAllWidgetsSize;
        public static void RaiseRefreshAllWidgetsSize()
        {
            OnRefreshAllWidgetsSize?.Invoke();
        }

        public static event Action OnSetQualityPresetAsCustom;
        public static void RaiseSetQualityPresetAsCustom()
        {
            OnSetQualityPresetAsCustom?.Invoke();
        }
    }
}