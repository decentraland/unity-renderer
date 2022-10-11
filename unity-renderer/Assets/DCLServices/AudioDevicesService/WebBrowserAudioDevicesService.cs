using System;
using System.Collections.Generic;
using DCL.Interface;
using UnityEngine;

namespace DCL.Services
{
    public class WebBrowserAudioDevicesService : IAudioDevicesService
    {
        private readonly IAudioDevicesBridge bridge;

        public WebBrowserAudioDevicesService (IAudioDevicesBridge bridge) => this.bridge = bridge;
        public event Action AduioDeviceCached;

        public bool HasRecievedKernelMessage { get; private set; }
        public Dictionary<string, string> InputDevices { get; private set; }

        public void Initialize()
        {
            
            if (bridge.AudioDevices == null)
                bridge.OnAudioDevicesRecieved += OnAudioDevicesRecieved;
            else
                CacheAudioDevices();
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
            CacheAudioDevices();
        }

        private void CacheAudioDevices()
        {
            HasRecievedKernelMessage = true;

            InputDevices = new Dictionary<string, string>();
            
            foreach (AudioDevice device in bridge.AudioDevices.inputDevices)
            {
                Debug.Log($"{device.deviceId} --- {device.label}");
                InputDevices.Add(device.deviceId, device.label);
            }
            AduioDeviceCached?.Invoke();
        }
    }
}