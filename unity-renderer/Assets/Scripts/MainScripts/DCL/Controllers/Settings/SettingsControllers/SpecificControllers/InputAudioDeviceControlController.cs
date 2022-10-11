using DCL.Services;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;
using UnityEngine.EventSystems;

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

            if (audioDevicesService.HasRecievedKernelMessage)
            {
                RaiseOnOverrideIndicatorLabel(DeviceNames());
                UpdateSetting(GetStoredValue());
            }
            else 
                audioDevicesService.AduioDeviceCached += OnAudioDevicesCached;
            
            void OnAudioDevicesCached()
            {
                audioDevicesService.AduioDeviceCached -= OnAudioDevicesCached;

                RaiseOnOverrideIndicatorLabel(DeviceNames());
                UpdateSetting(GetStoredValue());
            }
        }

        public override void OnPointerClicked(PointerEventData eventData)
        {
            if(!audioDevicesService.HasRecievedKernelMessage)
                audioDevicesService.RequestAudioDevices();
        }

        private string[] DeviceNames()
        {
            string[] deviceNames = new string[audioDevicesService.InputDevices.Length];
            for (int i = 0; i < audioDevicesService.InputDevices.Length; i++)
                deviceNames[i] = audioDevicesService.InputDevices[i].label;

            return deviceNames;
        }

        public override object GetStoredValue() =>
            currentAudioSettings.inputDevice;

        public override void UpdateSetting(object newValue)
        {
            if (currentAudioSettings.inputDevice == (int)newValue)
                return;

            currentAudioSettings.inputDevice = (int)newValue;
            ApplySettings();
            
            // audioDevicesService.SetInputDevice(audioDevicesService.InputDevices[(int)newValue]);
        }
    }
}