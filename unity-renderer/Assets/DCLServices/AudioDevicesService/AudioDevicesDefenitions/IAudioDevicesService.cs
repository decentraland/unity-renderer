using System;
using System.Collections.Generic;

namespace DCL.Services
{
    public interface IAudioDevicesService : IService
    {
        public event Action AduioDeviceCached;

        bool HasRecievedKernelMessage { get ;  }
        Dictionary<string, string> InputDevices { get ;  }

        void SetOutputDevice(int outputDeviceId);
    }
}