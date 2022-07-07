using System;
using UnityEngine;

[Serializable]
public class UserStatus
{
    [Serializable]
    public class Realm
    {
        public string serverName;
        public string layer;
    }

    public Realm realm;
    public Vector2 position;
    public string userId;
    public FriendshipStatus friendshipStatus;
    public PresenceStatus presence;
    [NonSerialized] public DateTime friendshipStartedTime;
}