using System;

namespace DCL.Services
{
    [Serializable]
    public class AudioDevicesResponse
    {
        public AudioDevice[] outputDevices;
        public AudioDevice[] inputDevices;
    }
    
    
    [Serializable]
    public class AudioDevice
    {
        public string deviceId;
        public string label;
    }
}