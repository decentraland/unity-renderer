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
            DataStore.i.contentModeration.resetAdultContentSetting.OnChange += OnResetAdultContentSetting;
        }

        public override void OnDestroy()
        {
            DataStore.i.contentModeration.resetAdultContentSetting.OnChange -= OnResetAdultContentSetting;
            base.OnDestroy();
        }

        public override object GetStoredValue() =>
            currentGeneralSettings.adultContent;

        public override void UpdateSetting(object newValue)
        {
            var value = (bool) newValue;
            currentGeneralSettings.adultContent = value;

            if (value)
                DataStore.i.contentModeration.adultContentAgeConfirmationVisible.Set(true, true);
            else
                DataStore.i.contentModeration.adultContentSettingEnabled.Set(false, true);
        }

        private void OnResetAdultContentSetting(bool isReset, bool _)
        {
            if (!isReset)
                return;

            RaiseToggleValueChanged(false);
        }
    }
}
