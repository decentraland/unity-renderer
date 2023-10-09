using System;
using UnityEngine;

public interface IFriendTrackerHandler
{
    event Action<UserProfile, Color> OnFriendAddedEvent;
    event Action<UserProfile> OnFriendRemovedEvent;

    void OnFriendAdded(UserProfile profile, Color backgroundColor);
    void OnFriendRemoved(UserProfile profile);
    bool ContainCoords(Vector2Int coords);
}
