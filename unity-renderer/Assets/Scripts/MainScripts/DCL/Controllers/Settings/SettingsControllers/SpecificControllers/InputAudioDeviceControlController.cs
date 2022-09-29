using DCL.Services;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Input Audio Device", fileName = nameof(InputAudioDeviceControlController))]
    public class InputAudioDeviceControlController : SpinBoxSettingsControlController
    {
        private IAudioDevicesService audioDevicesService;

        public override void Initialize()
        {
            base.Initialize();

            audioDevicesService = Environment.i.serviceLocator.Get<IAudioDevicesService>();

            if (audioDevicesService.InputDevices != null)
            {
                RaiseOnOverrideIndicatorLabel(audioDevicesService.InputDevices);
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

            RaiseOnOverrideIndicatorLabel(audioDevicesService.InputDevices);
            UpdateSetting(GetStoredValue());
        }

        public override object GetStoredValue() =>
            currentAudioSettings.inputDevice;

        public override void UpdateSetting(object newValue)
        {
            currentAudioSettings.inputDevice = (int)newValue;
            ApplySettings();
            Settings.i.ChangeAudioDevicesSettings();
        }
    }
}