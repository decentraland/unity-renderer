namespace DCL.Services
{
    public interface IAudioDevicesService : IService
    {
        string[] InputDevices { get ;  }
        string[] OutputDevices { get ;  }
    }
}