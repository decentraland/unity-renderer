using DCL;

namespace DCLServices.ScreencaptureCamera.Service
{
    public interface IScreencaptureCameraService : IService
    {
        void EnableScreencaptureCamera(string source);
    }
}
