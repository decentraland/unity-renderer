using System;

namespace DCL.Services
{

    public interface IAudioDevicesBridge
    {

        AudioDevicesResponse AudioDevices { get; }
        event Action<AudioDevicesResponse> OnAudioDevicesRecieved;
    }

}