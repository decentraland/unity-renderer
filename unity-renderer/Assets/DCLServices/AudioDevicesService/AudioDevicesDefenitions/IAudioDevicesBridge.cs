using System;

namespace DCL.Services
{

    public interface IAudioDevicesBridge
    {
        event Action<AudioDevicesResponse> OnAudioDevicesRecieved;
        
        void RequestAudioDevices();
    }

}