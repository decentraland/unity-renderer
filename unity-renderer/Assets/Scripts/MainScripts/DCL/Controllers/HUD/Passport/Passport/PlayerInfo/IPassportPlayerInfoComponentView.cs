using System;
using UnityEngine;
using SocialFeaturesAnalytics;

namespace DCL.Social.Passports
{
    public interface IPassportPlayerInfoComponentView
    {
        event Action OnAddFriend;

        void SetName(string name);
        void SetWallet(string wallet);
        void SetPresence(PresenceStatus status);
        void SetGuestUser(bool isGuest);
        void InitializeJumpInButton(IFriendsController friendsController, string userId, ISocialAnalytics socialAnalytics);
    }
}