using System;
using UnityEngine;

internal class FriendsHandler : IFriendTrackerHandler
{
    public event Action<UserProfile, Color> onFriendAdded;
    public event Action<UserProfile> onFriendRemoved;

    private readonly IMapDataView mapInfoHandler;

    public FriendsHandler(IMapDataView mapInfoHandler)
    {
        this.mapInfoHandler = mapInfoHandler;
    }

    public void OnFriendAdded(UserProfile profile, Color backgroundColor)
    {
        onFriendAdded?.Invoke(profile, backgroundColor);
    }

    public void OnFriendRemoved(UserProfile profile)
    {
        onFriendRemoved?.Invoke(profile);
    }

    public bool ContainCoords(Vector2Int coords)
    {
        return mapInfoHandler.ContainCoords(coords);
    }
}
