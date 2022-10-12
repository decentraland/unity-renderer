﻿using System;
using System.Collections.Generic;

namespace DCL.Services
{
    public interface IAudioDevicesService : IService
    {
        public event Action AudioDeviceCached;

        bool HasRecievedKernelMessage { get ;  }
        AudioDevice[] InputDevices { get ;  }

        void SetInputDevice(int deviceId);
        void RequestAudioDevices();
    }
}