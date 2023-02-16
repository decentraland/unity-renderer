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

        /// <summary>
        /// Zoom level normalized between 0 and 1
        /// </summary>
        /// <param name="value"></param>
        void SetZoom(float value);

        void SetPosition(Vector2 coordinates);

        void SetActive(bool active);

        void IDisposable.Dispose()
        {
            OnDisposed(this);
        }
    }
}
