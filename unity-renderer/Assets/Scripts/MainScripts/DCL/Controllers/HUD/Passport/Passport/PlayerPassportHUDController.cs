using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DCL;
using DCL.Helpers;
using SocialFeaturesAnalytics;

public class PlayerPassportHUDController : IHUD
{
    internal readonly PlayerPassportHUDView view;
    internal readonly StringVariable currentPlayerId;
    internal UserProfile currentUserProfile;

    private readonly IFriendsController friendsController;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IProfanityFilter profanityFilter;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly DataStore dataStore;

    private PassportPlayerInfoComponentController playerInfoController;
    private PassportPlayerPreviewComponentController playerPreviewController;
    private PassportNavigationComponentController passportNavigationController;

    public PlayerPassportHUDController(
        StringVariable currentPlayerId,
        IFriendsController friendsController,
        IUserProfileBridge userProfileBridge,
        IProfanityFilter profanityFilter,
        DataStore dataStore,
        ISocialAnalytics socialAnalytics)
    {
        this.currentPlayerId = currentPlayerId;
        this.friendsController = friendsController;
        this.userProfileBridge = userProfileBridge;
        this.profanityFilter = profanityFilter;
        this.dataStore = dataStore;
        this.socialAnalytics = socialAnalytics;

        view = PlayerPassportHUDView.CreateView();
        view.Initialize(() => currentPlayerId.Set(null));
        
        playerInfoController = new PassportPlayerInfoComponentController(currentPlayerId, view.playerInfoView, dataStore, profanityFilter, friendsController, userProfileBridge);
        playerPreviewController = new PassportPlayerPreviewComponentController(view.playerPreviewView);
        passportNavigationController = new PassportNavigationComponentController(view.passportNavigationView);

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
            Object.Destroy(view.gameObject);
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

            TaskUtils.Run(async () =>
                    {
                        view.SetPassportPanelVisibility(true);
                        await AsyncSetUserProfile(currentUserProfile);
                    })
                    .Forget();
        }
    }

    private async UniTask AsyncSetUserProfile(UserProfile userProfile)
    {
        await UniTask.SwitchToMainThread();
        playerInfoController.UpdateWithUserProfile(userProfile);
    }

    private void UpdateUserProfile(UserProfile userProfile) => TaskUtils.Run(async () => await AsyncSetUserProfile(userProfile)).Forget();

    private async UniTask<string> FilterDescription(UserProfile userProfile)
    {
        return IsProfanityFilteringEnabled()
            ? await profanityFilter.Filter(userProfile.description)
            : userProfile.description;
    }

    private bool IsProfanityFilteringEnabled()
    {
        return dataStore.settings.profanityChatFilteringEnabled.Get();
    }
}
