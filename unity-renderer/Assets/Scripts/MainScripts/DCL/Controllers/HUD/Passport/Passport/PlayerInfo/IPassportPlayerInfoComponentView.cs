using System;
using UnityEngine;

namespace DCL.Social.Passports
{
    public interface IPassportPlayerInfoComponentView
    {
        event Action OnAddFriend;

        void SetName(string name);
        void SetWallet(string wallet);
        void SetPresence(PresenceStatus status);
        void SetGuestUser(bool isGuest);
    }
}