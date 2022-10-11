using System;

namespace DCL.Services
{

    public interface IAudioDevicesBridge
    {
        event Action<AudioDevicesResponse> OnAudioDevicesRecieved;
        AudioDevicesResponse AudioDevices { get; }
        void RequestAudioDevices();
    }

}