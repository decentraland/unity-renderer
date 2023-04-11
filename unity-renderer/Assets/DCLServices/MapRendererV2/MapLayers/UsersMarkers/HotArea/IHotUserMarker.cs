using DCLServices.MapRendererV2.Culling;
using System;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea
{
    /// <summary>
    /// Reusable wrap over reusable instance
    /// </summary>
    internal interface IHotUserMarker : IMapPositionProvider, IMapRendererMarker, IMapCullingListener<IHotUserMarker>, IDisposable
    {
        string CurrentPlayerId { get; }

        void TrackPlayer(Player player);
    }
}
