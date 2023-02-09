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

        void IDisposable.Dispose()
        {
            OnDisposed(this);
        }
    }
}
