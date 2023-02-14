using DCLServices.MapRendererV2.MapCameraController;

namespace DCLServices.MapRendererV2.Culling
{
    internal interface IMapCullingVisibilityChecker
    {
        bool IsVisible<T>(T obj, IMapCameraControllerInternal camera) where T: IMapPositionProvider;
    }
}
