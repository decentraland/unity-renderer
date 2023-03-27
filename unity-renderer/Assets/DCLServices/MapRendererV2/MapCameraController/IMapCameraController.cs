using DCLServices.MapRendererV2.MapLayers;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapCameraController
{
    public interface IMapCameraController
    {
        MapLayer EnabledLayers { get; }

        RenderTexture GetRenderTexture();

        void ResizeTexture(Vector2Int textureResolution);

        IMapInteractivityController GetInteractivityController();

        float GetVerticalSizeInLocalUnits();

        float Zoom { get; }

        /// <summary>
        /// Position in local coordinates
        /// </summary>
        Vector2 LocalPosition { get; }

        /// <summary>
        /// Position in parcels
        /// </summary>
        Vector2 CoordsPosition { get; }

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

        /// <summary>
        /// Set Camera Position in local coordinates
        /// </summary>
        /// <param name="localCameraPosition"></param>
        void SetLocalPosition(Vector2 localCameraPosition);

        void SetPositionAndZoom(Vector2 coordinates, float value);

        /// <summary>
        /// Pauses rendering without releasing
        /// (all logic under the hood keeps executing)
        /// </summary>
        void SuspendRendering();

        /// <summary>
        /// Resumes rendering
        /// </summary>
        void ResumeRendering();

        void Release();
    }
}
