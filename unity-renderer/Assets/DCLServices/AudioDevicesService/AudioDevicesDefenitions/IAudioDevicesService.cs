using System;
using System.Collections.Generic;

namespace DCL.Services
{
    public interface IAudioDevicesService : IService
    {
        event Action AudioDeviceCached;

        bool HasReceivedKernelMessage { get ;  }
        AudioDevice[] InputDevices { get ;  }

        void SetInputDevice(int deviceId);
        void RequestAudioDevices();
    }
}