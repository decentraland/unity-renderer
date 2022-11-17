using System;
using UnityEngine;

public interface IPassportPlayerInfoComponentView
{
    event Action OnAddFriend;

    void SetName(string name);
    void SetWallet(string wallet);
    void SetPresence(PresenceStatus status);
}
