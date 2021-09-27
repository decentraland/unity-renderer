using DCL;
using System.Collections.Generic;
using UnityEngine;
using Variables.RealmsInfo;
using System.Linq;

/// <summary>
/// Main controller for the feature "Explore V2".
/// </summary>
public class ExploreV2MenuComponentController : IExploreV2MenuComponentController
{
    internal UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    internal IExploreV2MenuComponentView view;

    public void Initialize()
    {
        view = CreateView();
        SetVisibility(false);

        DataStore.i.playerRealm.OnChange += UpdateRealmInfo;
        UpdateRealmInfo(DataStore.i.playerRealm.Get(), null);

        ownUserProfile.OnUpdate += UpdateProfileInfo;
        UpdateProfileInfo(ownUserProfile);

        view.OnCloseButtonPressed += OnCloseButtonPressed;
        DataStore.i.taskbar.isExploreV2Enabled.OnChange += OnActivateFromTaskbar;
        DataStore.i.exploreV2.isInitialized.Set(true);
    }

    public void Dispose()
    {
        DataStore.i.playerRealm.OnChange -= UpdateRealmInfo;
        ownUserProfile.OnUpdate -= UpdateProfileInfo;

        if (view != null && view.go != null)
        {
            view.OnCloseButtonPressed -= OnCloseButtonPressed;
            GameObject.Destroy(view.go);
        }

        DataStore.i.taskbar.isExploreV2Enabled.OnChange -= OnActivateFromTaskbar;
    }

    public void SetVisibility(bool visible)
    {
        if (view == null)
            return;

        if (visible && !view.isActive)
            DataStore.i.exploreV2.isOpen.Set(true);
        else if (!visible && view.isActive)
            DataStore.i.exploreV2.isOpen.Set(false);

        view.SetActive(visible);
    }

    internal void UpdateRealmInfo(CurrentRealmModel currentRealm, CurrentRealmModel previousRealm)
    {
        // Get the name of the current realm
        string currentRealmServer = currentRealm?.serverName;
        string currentRealmLayer = currentRealm?.layer;
        string formattedRealmName = currentRealmServer;
        if (!string.IsNullOrEmpty(currentRealmLayer))
        {
            formattedRealmName = $"{formattedRealmName}-{currentRealmLayer}";
        }

        view.currentRealmViewer.SetRealm(formattedRealmName);

        // Calculate number of users in the current realm
        List<RealmModel> realmList = DataStore.i.realmsInfo.Get()?.ToList();
        RealmModel currentRealmModel = realmList?.FirstOrDefault(r => r.serverName == currentRealmServer && (r.layer == null || r.layer == currentRealmLayer));
        int realmUsers = 0;
        if (currentRealmModel != null)
            realmUsers = currentRealmModel.usersCount;

        view.currentRealmViewer.SetNumberOfUsers(realmUsers);
    }

    internal void UpdateProfileInfo(UserProfile profile)
    {
        view.currentProfileCard.SetProfileName(profile.userName);
        view.currentProfileCard.SetProfileAddress(profile.ethAddress);
        view.currentProfileCard.SetLoadingIndicatorVisible(true);
        profile.snapshotObserver.AddListener(SetProfileImage);
    }

    internal void SetProfileImage(Texture2D texture)
    {
        ownUserProfile.snapshotObserver.RemoveListener(SetProfileImage);
        view.currentProfileCard.SetLoadingIndicatorVisible(false);
        view.currentProfileCard.SetProfilePicture(texture);
    }

    internal void OnCloseButtonPressed() { SetVisibility(false); }

    internal void OnActivateFromTaskbar(bool current, bool previous) { SetVisibility(current); }

    internal virtual IExploreV2MenuComponentView CreateView() => ExploreV2MenuComponentView.Create();
}