using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Show Avatar Names", fileName = "ShowAvatarNamesControlController")]
    public class ShowAvatarNamesControlController : ToggleSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();

            DataStore.i.HUDs.avatarNamesVisible.OnChange += AvatarNamesVisibleChanged;
        }

        public override object GetStoredValue() { return currentGeneralSettings.showAvatarNames; }

        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.showAvatarNames = (bool)newValue;
            DataStore.i.HUDs.avatarNamesVisible.Set(currentGeneralSettings.showAvatarNames);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            DataStore.i.HUDs.avatarNamesVisible.OnChange -= AvatarNamesVisibleChanged;
        }

        private void AvatarNamesVisibleChanged(bool current, bool previous)
        {
            currentGeneralSettings.showAvatarNames = current;
            ApplySettings();
        }
    }
}