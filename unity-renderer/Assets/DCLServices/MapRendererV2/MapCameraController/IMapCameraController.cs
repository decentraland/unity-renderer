using DCLServices.MapRendererV2.MapLayers;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapCameraController
{
    public interface IMapCameraController
    {
        MapLayer EnabledLayers { get; }

        RenderTexture GetRenderTexture();

        IMapInteractivityController GetInteractivityController();

        float Zoom { get; }

        Vector2 Position { get; }

        /// <summary>
        /// Zoom level normalized between 0 and 1
        /// </summary>
        /// <param name="value"></param>
        void SetZoom(float value);

        /// <summary>
        /// Sets Camera Position
        /// </summary>
        /// <param name="coordinates">Parcel-based unclamped coordinates</param>
        void SetPosition(Vector2 coordinates);

        void SetZoomAndPosition(Vector2 coordinates, float value);

        void Release();
    }
}
