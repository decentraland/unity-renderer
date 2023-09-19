using DCL;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace MainScripts.DCL.Controllers.Settings.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Adult Scenes Filtering", fileName = "AdultScenesFilteringControlController")]
    public class AdultScenesFilterControlController : ToggleSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();
            CommonScriptableObjects.contentModerationSettingDeactivated.Set(!DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("content_moderation"));
        }

        public override object GetStoredValue() =>
            currentGeneralSettings.adultScenesFiltering;

        public override void UpdateSetting(object newValue)
        {
            var value = (bool) newValue;
            currentGeneralSettings.adultScenesFiltering = value;
            DataStore.i.settings.adultScenesFilteringEnabled.Set(value);
        }
    }
}
