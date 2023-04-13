using DCLServices.MapRendererV2.Culling;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PointsOfInterest
{
    internal interface ISceneOfInterestMarker : IMapRendererMarker, IMapPositionProvider, IDisposable
    {
        bool IsVisible { get; }

        void SetData(string title, Vector3 position);

        void OnBecameVisible();

        void OnBecameInvisible();
    }
}
