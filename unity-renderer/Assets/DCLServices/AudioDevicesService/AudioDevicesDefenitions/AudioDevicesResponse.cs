using System;

namespace DCL.Services
{
    [Serializable]
    public class AudioDevicesResponse
    {
        public string[] outputDevices;
        public string[] inputDevices;
    }
}