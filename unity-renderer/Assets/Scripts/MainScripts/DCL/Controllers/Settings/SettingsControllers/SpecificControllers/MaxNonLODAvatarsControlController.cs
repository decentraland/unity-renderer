using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Max Non-LOD Avatars", fileName = "MaxNonLODAvatarsControlController")]
    public class MaxNonLODAvatarsControlController : SliderSettingsControlController
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

            UpdateSetting(currentGeneralSettings.maxNonLODAvatars);
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { Resources.Load<BooleanVariable>("ScriptableObjects/AvatarLODsDisabled").Set(!current.features.enableAvatarLODs); }
        public override object GetStoredValue() { return currentGeneralSettings.maxNonLODAvatars; }

        public override void UpdateSetting(object newValue) { DataStore.i.avatarsLOD.maxAvatars.Set((int)((float)newValue)); }
    }
}