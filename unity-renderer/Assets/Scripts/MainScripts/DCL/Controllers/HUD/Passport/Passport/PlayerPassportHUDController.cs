using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DCL;
using DCL.Helpers;
using SocialFeaturesAnalytics;

namespace DCL.Social.Passports
{
    public class PlayerPassportHUDController : IHUD
    {
        internal readonly PlayerPassportHUDView view;
        internal readonly StringVariable currentPlayerId;
        internal readonly IUserProfileBridge userProfileBridge;
        private readonly ISocialAnalytics socialAnalytics;

        internal UserProfile currentUserProfile;

        private PassportPlayerInfoComponentController playerInfoController;
        private PassportPlayerPreviewComponentController playerPreviewController;
        private PassportNavigationComponentController passportNavigationController;

        public PlayerPassportHUDController(
            PlayerPassportHUDView view,
            PassportPlayerInfoComponentController playerInfoController,
            PassportPlayerPreviewComponentController playerPreviewController,
            PassportNavigationComponentController passportNavigationController,
            StringVariable currentPlayerId,
            IUserProfileBridge userProfileBridge,
            ISocialAnalytics socialAnalytics)
        {
            this.view = view;
            this.playerInfoController = playerInfoController;
            this.playerPreviewController = playerPreviewController;
            this.passportNavigationController = passportNavigationController;
            this.currentPlayerId = currentPlayerId;
            this.userProfileBridge = userProfileBridge;
            this.socialAnalytics = socialAnalytics;

            view.Initialize();
            view.OnClose += RemoveCurrentPlayer;

            currentPlayerId.OnChange += OnCurrentPlayerIdChanged;
            OnCurrentPlayerIdChanged(currentPlayerId, null);
        }

        public void SetVisibility(bool visible)
        {
            view.SetVisibility(visible);
        }

        public void Dispose()
        {
            if (view != null)
                view.Dispose();
        }

        private void OnCurrentPlayerIdChanged(string current, string previous)
        {
            if (currentUserProfile != null)
                currentUserProfile.OnUpdate -= UpdateUserProfile;

            currentUserProfile = string.IsNullOrEmpty(current)
                ? null
                : userProfileBridge.Get(current);

            if (currentUserProfile == null)
            {
                view.SetPassportPanelVisibility(false);
            }
            else
            {
                currentUserProfile.OnUpdate += UpdateUserProfile;
                view.SetPassportPanelVisibility(true);
                playerInfoController.UpdateWithUserProfile(currentUserProfile);
            }
        }

        private void UpdateUserProfile(UserProfile userProfile) => playerInfoController.UpdateWithUserProfile(userProfile);

        private void RemoveCurrentPlayer()
        {
            currentPlayerId.Set(null);
        }

    }
}