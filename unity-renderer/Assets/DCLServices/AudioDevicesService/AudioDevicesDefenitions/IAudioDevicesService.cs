using System;
using System.Collections.Generic;

namespace DCL.Services
{
    public interface IAudioDevicesService : IService
    {
        public event Action AduioDeviceCached;

        bool HasRecievedKernelMessage { get ;  }
        AudioDevice[] InputDevices { get ;  }

        void SetInputDevice(int deviceId);
        void RequestAudioDevices();
    }
}