using UnityEngine;

internal interface IFriendTrackerHandler
{
    void OnFriendAdded(UserProfile profile, Color backgroundColor);
    void OnFriendRemoved(UserProfile profile);
    bool ContainCoords(Vector2Int coords);
}