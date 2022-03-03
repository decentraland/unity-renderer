using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Night Mode", fileName = "NightModeControlController")]
    public class NightModeControlController : ToggleSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();

            RenderProfileManifest.i.OnChangeProfile += OnChangeProfile;
            OnChangeProfile(RenderProfileManifest.i.currentProfile);
        }

        public override object GetStoredValue() { return currentGeneralSettings.nightMode; }

        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.nightMode = (bool)newValue;
            RenderProfileManifest.i.currentProfile = currentGeneralSettings.nightMode ? RenderProfileManifest.i.nightProfile : RenderProfileManifest.i.defaultProfile;
            RenderProfileManifest.i.currentProfile.Apply();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            RenderProfileManifest.i.OnChangeProfile -= OnChangeProfile;
        }

        private void OnChangeProfile(RenderProfileWorld profile)
        {
            currentGeneralSettings.nightMode = profile == RenderProfileManifest.i.nightProfile;
            ApplySettings();
        }
    }
}