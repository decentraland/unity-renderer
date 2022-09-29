using System;

namespace DCL.Services
{
    public interface IAudioDevicesService : IService
    {

        string[] InputDevices { get ;  }
        string[] OutputDevices { get ;  }
        bool HasRecievedKernelMessage { get ;  }
        public event Action AduioDeviceCached;

        void SetOutputDevice(int outputDeviceId);
        void SetInputDevice(int inputDeviceId);
    }
}