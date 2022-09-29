using System;
using DCL.Interface;

namespace DCL.Services
{
    public class WebBrowserAudioDevicesService : IAudioDevicesService
    {

        private readonly IAudioDevicesBridge bridge;

        public WebBrowserAudioDevicesService (IAudioDevicesBridge bridge) { this.bridge = bridge; }
        public event Action AduioDeviceCached;

        public bool HasRecievedKernelMessage { get; private set; }

        public string[] InputDevices { get; private set; }
        public string[] OutputDevices { get; private set; }

        public void Initialize()
        {
            if (bridge.AudioDevices == null)
                bridge.OnAudioDevicesRecieved += OnAudioDevicesRecieved;
            else
                ChacheAudioDevices();
        }

        public void Dispose()
        {
            if (!HasRecievedKernelMessage)
                bridge.OnAudioDevicesRecieved -= OnAudioDevicesRecieved;
        }

        public void SetOutputDevice(int outputDeviceId) =>
            WebInterface.SetOutputAudioDevice(outputDeviceId);

        public void SetInputDevice(int inputDeviceId) =>
            WebInterface.SetInputAudioDevice(inputDeviceId);

        private void OnAudioDevicesRecieved(AudioDevicesResponse devices)
        {
            bridge.OnAudioDevicesRecieved -= OnAudioDevicesRecieved;
            ChacheAudioDevices();
        }

        private void ChacheAudioDevices()
        {
            HasRecievedKernelMessage = true;

            InputDevices = bridge.AudioDevices.inputDevices;
            OutputDevices = bridge.AudioDevices.outputDevices;

            AduioDeviceCached?.Invoke();
        }
    }
}