using System;
using DCL.Interface;

namespace DCL.Services
{
    public class WebBrowserAudioDevicesService : IAudioDevicesService
    {
        private readonly IAudioDevicesBridge bridge;

        public WebBrowserAudioDevicesService (IAudioDevicesBridge bridge) => this.bridge = bridge;
        public event Action AduioDeviceCached;

        public bool HasRecievedKernelMessage { get; private set; }
        public AudioDevice[] InputDevices { get; private set; }

        public void Initialize() => bridge.OnAudioDevicesRecieved += CacheAudioDevices;
        public void Dispose() => bridge.OnAudioDevicesRecieved -= CacheAudioDevices;

        private void CacheAudioDevices(AudioDevicesResponse devices)
        {
            HasRecievedKernelMessage = true;

            InputDevices = devices.inputDevices;

            AduioDeviceCached?.Invoke();
        }
        
        public void RequestAudioDevices() => bridge.RequestAudioDevices();

        public void SetInputDevice(int deviceId)
        {
            if (HasRecievedKernelMessage && deviceId <= InputDevices.Length)
                WebInterface.SetInputAudioDevice(InputDevices[deviceId].deviceId);
        }
    }
}