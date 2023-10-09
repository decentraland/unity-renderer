using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.Favorites
{
    internal interface IFavoritesMarker : IMapRendererMarker, IMapPositionProvider, IDisposable
    {
        bool IsVisible { get; }

        void SetData(string title, Vector3 position);

        void OnBecameVisible();

        void OnBecameInvisible();

        void SetZoom(float baseScale, float baseZoom, float zoom);

        void ResetScale(float baseScale);
    }
}
