using System;
using UnityEngine;

public class FriendsHandler : IFriendTrackerHandler
{
    public event Action<UserProfile, Color> OnFriendAddedEvent;
    public event Action<UserProfile> OnFriendRemovedEvent;

    private readonly IMapDataView mapInfoHandler;

    public FriendsHandler(IMapDataView mapInfoHandler) { this.mapInfoHandler = mapInfoHandler; }

    public void OnFriendAdded(UserProfile profile, Color backgroundColor) { OnFriendAddedEvent?.Invoke(profile, backgroundColor); }

    public void OnFriendRemoved(UserProfile profile) { OnFriendRemovedEvent?.Invoke(profile); }

    public bool ContainCoords(Vector2Int coords) { return mapInfoHandler.ContainCoords(coords); }
}