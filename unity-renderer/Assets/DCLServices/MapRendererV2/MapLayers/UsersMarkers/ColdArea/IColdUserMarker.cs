using DCLServices.MapRendererV2.Culling;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea
{
    internal interface IColdUserMarker : IMapPositionProvider, IMapRendererMarker, IDisposable
    {
        Vector2Int Coords { get; }

        void OnRealmChanged(string realm);

        void SetActive(bool isActive);

        void SetCulled(bool culled);

        void SetData(string realm, string userRealm, Vector2Int coords, Vector3 position);
    }
}
