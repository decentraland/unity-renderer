using DCL;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Max HQ Avatars", fileName = "MaxHQAvatarsControlController")]
    public class MaxHQAvatarsControlController : SliderSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();

            KernelConfig.i.EnsureConfigInitialized()
                        .Then(config =>
                        {
                            KernelConfig.i.OnChange += OnKernelConfigChanged;
                            OnKernelConfigChanged(config, null);
                        });

            DataStore.i.avatarsLOD.maxAvatars.Set(currentQualitySetting.maxHQAvatars);
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { Resources.Load<BooleanVariable>("ScriptableObjects/AvatarLODsDisabled").Set(!current.features.enableAvatarLODs); }
        public override object GetStoredValue() { return currentQualitySetting.maxHQAvatars; }

        public override void UpdateSetting(object newValue)
        {
            int valueAsInt = (int)(float)newValue;
            currentQualitySetting.maxHQAvatars = valueAsInt;
            DataStore.i.avatarsLOD.maxAvatars.Set(valueAsInt);
        }
    }
}