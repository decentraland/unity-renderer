using DCLServices.MapRendererV2.Culling;
using System;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.Friends
{
    /// <summary>
    /// Reusable wrap over reusable instance
    /// </summary>
    internal interface IFriendUserMarker : IMapPositionProvider, IMapRendererMarker, IMapCullingListener<IFriendUserMarker>, IDisposable
    {
        string CurrentPlayerId { get; }

        void TrackPlayer(Player player);
        void SetProfilePicture(string url);
        void SetPlayerName(string name);

        void SetZoom(float baseScale, float baseZoom, float zoom);

        void ResetScale(float scale);
    }
}
