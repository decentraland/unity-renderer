using DCLServices.MapRendererV2.MapCameraController;

namespace DCLServices.MapRendererV2.Culling
{
    internal class DummyMapCullingControllerVisibilityChecker : IMapCullingVisibilityChecker
    {
        public bool IsVisible<T>(T obj, IMapCameraControllerInternal camera) where T : IMapPositionProvider => true;
    }
}
