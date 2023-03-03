using DCLServices.MapRendererV2.MapLayers;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapCameraController
{
    public interface IMapCameraController
    {
        MapLayer EnabledLayers { get; }

        RenderTexture GetRenderTexture();

        IMapInteractivityController GetInteractivityController();

        /// <summary>
        /// Zoom level normalized between 0 and 1
        /// </summary>
        /// <param name="value"></param>
        void SetZoom(float value);

        void SetPosition(Vector2 coordinates);

        void Release();
    }
}
