using DCL;
using System;
using UnityEngine;

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

        if (string.IsNullOrEmpty(ownUserProfile.userId))
            ownUserProfile.OnUpdate += OnProfileUpdated;
        else
            OnProfileUpdated(ownUserProfile);

        view.OnCloseButtonPressed += OnCloseButtonPressed;
        DataStore.i.taskbar.isExploreV2Enabled.OnChange += OnActivateFromTaskbar;
        DataStore.i.exploreV2.isInitialized.Set(true);
    }

    public void Dispose()
    {
        if (view == null)
            return;

        if (view.go != null)
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

    internal void OnProfileUpdated(UserProfile profile)
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