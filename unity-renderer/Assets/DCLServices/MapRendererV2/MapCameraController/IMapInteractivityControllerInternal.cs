using DCLServices.MapRendererV2.MapLayers;
using System;

namespace DCLServices.MapRendererV2.MapCameraController
{
    internal interface IMapInteractivityControllerInternal : IMapInteractivityController, IDisposable
    {
        void Initialize(MapLayer layers);

        void Release();
    }
}
