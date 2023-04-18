using DCL;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;

namespace DCLServices.MapRendererV2
{
    public interface IMapRenderer : IService
    {
        IMapCameraController RentCamera(in MapCameraInput cameraInput);
    }
}
