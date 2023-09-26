using DCL;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;

namespace DCLServices.MapRendererV2
{
    public interface IMapRenderer : IService
    {
        IMapCameraController RentCamera(in MapCameraInput cameraInput);
        void ToggleLayer(MapLayer mask, bool isActive);
    }
}
