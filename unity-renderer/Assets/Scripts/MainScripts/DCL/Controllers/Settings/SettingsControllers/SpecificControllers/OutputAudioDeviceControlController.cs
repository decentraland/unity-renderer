using DCL.Services;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Output Audio Device", fileName = nameof(OutputAudioDeviceControlController))]
    public class OutputAudioDeviceControlController : SpinBoxSettingsControlController
    {
        private IAudioDevicesService audioDevicesService;

        public override void Initialize()
        {
            base.Initialize();
            audioDevicesService = Environment.i.serviceLocator.Get<IAudioDevicesService>();

            if (audioDevicesService.OutputDevices != null)
            {
                RaiseOnOverrideIndicatorLabel(audioDevicesService.OutputDevices);
                UpdateSetting(GetStoredValue());
            }
            else
            {
                audioDevicesService.AduioDeviceCached += OnAudioDevicesCached;
            }
        }

        private void OnAudioDevicesCached()
        {
            audioDevicesService.AduioDeviceCached -= OnAudioDevicesCached;

            RaiseOnOverrideIndicatorLabel(audioDevicesService.OutputDevices);
            UpdateSetting(GetStoredValue());
        }

        public override object GetStoredValue() =>
            currentAudioSettings.outputDevice;

        public override void UpdateSetting(object newValue)
        {
            currentAudioSettings.outputDevice = (int)newValue;

            ApplySettings();
            Settings.i.ChangeAudioDevicesSettings();
        }
    }
}