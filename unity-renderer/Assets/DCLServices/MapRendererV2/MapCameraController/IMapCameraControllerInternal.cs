using DCLServices.MapRendererV2.MapLayers;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapCameraController
{
    /// <summary>
    /// Contains methods that are not exposed publicly to consumers
    /// but used inside the MapRenderer's system only
    /// </summary>
    internal interface IMapCameraControllerInternal : IMapCameraController, IDisposable
    {
        event Action<IMapCameraControllerInternal> OnReleasing;

        Camera Camera { get; }

        void Initialize(Vector2Int textureResolution, Vector2Int zoomValues, MapLayer layers);

        void SetActive(bool active);

        Rect GetCameraRect();
    }
}
