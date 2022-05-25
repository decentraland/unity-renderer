using DCL.Helpers;
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
}