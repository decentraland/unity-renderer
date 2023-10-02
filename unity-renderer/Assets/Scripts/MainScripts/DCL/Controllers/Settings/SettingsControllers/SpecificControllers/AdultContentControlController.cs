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
            DataStore.i.contentModeration.adultContentAgeConfirmationResult.OnChange += OnAdultContentAgeConfirmationResultChanged;
            DataStore.i.contentModeration.adultContentSettingEnabled.Set(currentGeneralSettings.adultContent, false);
        }

        public override void OnDestroy()
        {
            DataStore.i.contentModeration.adultContentAgeConfirmationResult.OnChange -= OnAdultContentAgeConfirmationResultChanged;
            base.OnDestroy();
        }

        public override object GetStoredValue() =>
            currentGeneralSettings.adultContent;

        public override void UpdateSetting(object newValue)
        {
            var value = (bool) newValue;

            if (value)
                DataStore.i.contentModeration.adultContentAgeConfirmationVisible.Set(true, true);
            else
            {
                currentGeneralSettings.adultContent = false;
                DataStore.i.contentModeration.adultContentSettingEnabled.Set(false, true);
            }
        }

        private void OnAdultContentAgeConfirmationResultChanged(DataStore_ContentModeration.AdultContentAgeConfirmationResult result, DataStore_ContentModeration.AdultContentAgeConfirmationResult _)
        {
            if (result == DataStore_ContentModeration.AdultContentAgeConfirmationResult.Accepted)
            {
                currentGeneralSettings.adultContent = true;
                DataStore.i.contentModeration.adultContentSettingEnabled.Set(true, true);
                ApplySettings();
            }
            else
                RaiseToggleValueChanged(false);
        }
    }
}
