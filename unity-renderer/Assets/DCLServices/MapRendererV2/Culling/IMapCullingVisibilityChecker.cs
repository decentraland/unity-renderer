using DCLServices.MapRendererV2.MapCameraController;
using System;

namespace DCLServices.MapRendererV2.Culling
{
    internal interface IMapCullingVisibilityChecker
    {
        bool IsVisible<T>(T obj, CameraState camera) where T: IMapPositionProvider;
    }
}
