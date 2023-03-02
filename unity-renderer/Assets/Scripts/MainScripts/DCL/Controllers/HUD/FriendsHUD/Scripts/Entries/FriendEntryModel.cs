using DCL.Helpers;
using DCL.Social.Friends;
using UnityEngine;

public class FriendEntryModel
{
    public string userId;
    public PresenceStatus status;
    public string userName;
    public Vector2 coords;
    public string realm;
    public string realmServerName;
    public string realmLayerName;
    public ILazyTextureObserver avatarSnapshotObserver;
    public bool blocked;

    public FriendEntryModel()
    {
    }

    public FriendEntryModel(FriendEntryModel model)
    {
        userId = model.userId;
        status = model.status;
        userName = model.userName;
        coords = model.coords;
        realm = model.realm;
        realmServerName = model.realmServerName;
        realmLayerName = model.realmLayerName;
        avatarSnapshotObserver = model.avatarSnapshotObserver;
        blocked = model.blocked;
    }

    public virtual void CopyFrom(UserStatus status)
    {
        userId = status.userId;
        this.status = status.presence;
        coords = status.position;

        if (status.realm != null)
        {
            realm = $"{status.realm.serverName.ToUpperFirst()} {status.realm.layer.ToUpperFirst()}";
            realmServerName = status.realm.serverName;
            realmLayerName = status.realm.layer;
        }
        else
        {
            realm = string.Empty;
            realmServerName = string.Empty;
            realmLayerName = string.Empty;
        }
    }

    public virtual void CopyFrom(UserProfile userProfile)
    {
        userId = userProfile.userId;
        userName = userProfile.userName;
        avatarSnapshotObserver = userProfile.snapshotObserver;
    }
}
