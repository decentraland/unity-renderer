using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Avatars LOD Distance", fileName = "AvatarsLODDistanceControlController")]
    public class AvatarsLODDistanceControlController : SliderSettingsControlController
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

            UpdateSetting(currentGeneralSettings.avatarsLODDistance);
        }
        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { Resources.Load<BooleanVariable>("ScriptableObjects/AvatarLODsDisabled").Set(!current.features.enableAvatarLODs); }

        public override object GetStoredValue() { return currentGeneralSettings.avatarsLODDistance; }

        public override void UpdateSetting(object newValue) { DataStore.i.avatarsLOD.LODDistance.Set((float)newValue); }

        public override void OnDestroy()
        {
            base.OnDestroy();
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
        }
    }
}