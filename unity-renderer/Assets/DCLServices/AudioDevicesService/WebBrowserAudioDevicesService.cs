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
        public AudioDevice[] InputDevices { get; private set; }

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

        public void SetInputDevice(AudioDevice inputDevice) => 
            WebInterface.SetInputAudioDevice(inputDevice.deviceId);
        
        public void RequestAudioDevices() => bridge.RequestAudioDevices();

        private void OnAudioDevicesRecieved(AudioDevicesResponse devices)
        {
            bridge.OnAudioDevicesRecieved -= OnAudioDevicesRecieved;
            CacheAudioDevices();
        }

        private void CacheAudioDevices()
        {
            HasRecievedKernelMessage = true;

            InputDevices = bridge.AudioDevices.inputDevices;
            
            AduioDeviceCached?.Invoke();
        }
    }
}