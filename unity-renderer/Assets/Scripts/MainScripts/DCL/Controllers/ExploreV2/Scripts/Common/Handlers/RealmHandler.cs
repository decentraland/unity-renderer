using System;
using UnityEngine;

public class RealmHandler : IRealmTrackerHandler
{
    public event Action<UserProfile, Color> OnFriendAddedEvent;
    public event Action<UserProfile> OnFriendRemovedEvent;

    private readonly IRealmDataView realmInfoHandler;

    public RealmHandler(IRealmDataView realmInfoHandler) { this.realmInfoHandler = realmInfoHandler; }

    public void OnFriendAdded(UserProfile profile, Color backgroundColor) { OnFriendAddedEvent?.Invoke(profile, backgroundColor); }

    public void OnFriendRemoved(UserProfile profile) { OnFriendRemovedEvent?.Invoke(profile); }

    public bool ContainRealm(string serverName) { return realmInfoHandler.ContainRealm(serverName); }
}