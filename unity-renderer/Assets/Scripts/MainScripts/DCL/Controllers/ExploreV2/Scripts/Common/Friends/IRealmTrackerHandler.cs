using UnityEngine;

public interface IRealmTrackerHandler
{
    void OnFriendAdded(UserProfile profile, Color backgroundColor);
    void OnFriendRemoved(UserProfile profile);
    bool ContainRealm(string serverName);
}