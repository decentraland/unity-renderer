using DCLServices.MapRendererV2.MapLayers;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapCameraController
{
    public interface IMapCameraController : IDisposable
    {
        public Action<IMapCameraController> OnDisposed { get; set; }

        MapLayer EnabledLayers { get; }

        RenderTexture GetRenderTexture();

        void TrackPlayer(/*TODO argument as needed*/);

        void SetZoom(float value);

        void SetPosition(Vector2Int coordinates);

        /// <summary>
        /// Report hover at the provided position
        /// </summary>
        /// <param name="pos">position relative to the original render texture</param>
        void Hover(Vector2 pos);

        /// <summary>
        /// Report pointer down event at the provided position
        /// </summary>
        /// <param name="pos">position relative to the original render texture</param>
        void PointerDown(Vector2 pos);

        /// <summary>
        /// Report pointer up event at the provided position
        /// </summary>
        /// <param name="pos">position relative to the original render texture</param>
        void PointerUp(Vector2 pos);

        void IDisposable.Dispose()
        {
            OnDisposed(this);
        }
    }
}
