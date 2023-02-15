using DCLServices.MapRendererV2.Culling;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea
{
    /// <summary>
    /// Reusable wrap over reusable instance
    /// </summary>
    internal interface IHotUserMarker : IMapPositionProvider, IDisposable
    {
        string CurrentPlayerId { get; }

        void TrackPlayer(Player player);

        void OnBecameVisible();

        void OnBecameInvisible();
    }
}
