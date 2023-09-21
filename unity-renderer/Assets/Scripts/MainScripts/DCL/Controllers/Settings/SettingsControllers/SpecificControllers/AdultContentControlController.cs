using DCL;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace MainScripts.DCL.Controllers.Settings.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Adult Content", fileName = "AdultContentControlController")]
    public class AdultContentControlController : ToggleSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();
            CommonScriptableObjects.adultContentSettingDeactivated.Set(!DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("content_moderation"));
        }

        public override object GetStoredValue() =>
            currentGeneralSettings.adultContent;

        public override void UpdateSetting(object newValue)
        {
            var value = (bool) newValue;
            currentGeneralSettings.adultContent = value;
            DataStore.i.settings.adultContentEnabled.Set(value);
        }
    }
}
