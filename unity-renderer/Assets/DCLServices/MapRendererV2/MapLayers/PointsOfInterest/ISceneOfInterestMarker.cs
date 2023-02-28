using DCLServices.MapRendererV2.Culling;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PointsOfInterest
{
    internal interface ISceneOfInterestMarker : IMapPositionProvider, IDisposable
    {
        void SetData(string title, Vector3 position);

        void OnBecameVisible();

        void OnBecameInvisible();
    }
}
