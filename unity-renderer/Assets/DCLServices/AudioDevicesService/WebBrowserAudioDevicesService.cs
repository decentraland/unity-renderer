using System;
using DCL.Interface;

namespace DCL.Services
{
    public class WebBrowserAudioDevicesService : IAudioDevicesService
    {
        private readonly IAudioDevicesBridge bridge;

        public WebBrowserAudioDevicesService (IAudioDevicesBridge bridge) => this.bridge = bridge;
        public event Action AudioDeviceCached;

        public bool HasReceivedKernelMessage { get; private set; }
        public AudioDevice[] InputDevices { get; private set; }

        public void Initialize() => bridge.OnAudioDevicesRecieved += CacheAudioDevices;
        public void Dispose() => bridge.OnAudioDevicesRecieved -= CacheAudioDevices;

        private void CacheAudioDevices(AudioDevicesResponse devices)
        {
            HasReceivedKernelMessage = true;

            InputDevices = devices.inputDevices;

            AudioDeviceCached?.Invoke();
        }
        
        public void RequestAudioDevices() => bridge.RequestAudioDevices();

        public void SetInputDevice(int deviceId)
        {
            if (HasReceivedKernelMessage && deviceId <= InputDevices.Length)
                WebInterface.SetInputAudioDevice(InputDevices[deviceId].deviceId);
        }
    }
}