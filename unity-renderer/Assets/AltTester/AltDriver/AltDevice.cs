namespace Altom.AltDriver
{
    public class AltDevice
    {
        public string DeviceId { get; set; }
        public int LocalPort { get; set; }
        public int RemotePort { get; set; }
        public bool Active { get; set; }
        public string Platform { get; set; }
        public int Pid { get; set; }
        
        public AltDevice(string deviceId, string platform, int localPort = 13000, int remotePort = 13000, bool active = false, int pid = 0)
        {
            DeviceId = deviceId;
            LocalPort = localPort;
            RemotePort = remotePort;
            Active = active;
            Platform = platform;
            Pid = pid;
        }
    }
}

