using System;
using DCL.Social.Friends;
using UnityEngine;

[Serializable]
public class UserStatus
{
    [Serializable]
    public class Realm
    {
        public string serverName;
        public string layer;

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (!(obj is Realm other)) return false;
            if (serverName != other.serverName) return false;
            if (layer != other.layer) return false;
            return true;
        }
    }

    public string userName;
    public Realm realm;
    public Vector2 position;
    public string userId;
    public FriendshipStatus friendshipStatus;
    public PresenceStatus presence;

    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        if (!(obj is UserStatus other)) return false;
        if (realm != null && other.realm == null) return false;
        if (realm == null && other.realm != null) return false;
        if (realm != null && !realm.Equals(other.realm)) return false;
        if (position != other.position) return false;
        if (userId != other.userId) return false;
        if (friendshipStatus != other.friendshipStatus) return false;
        return true;
    }
}
