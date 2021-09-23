using System;
using UnityEngine;

/// <summary>
/// Main controller for the feature "Explore V2".
/// </summary>
public class ExploreV2MenuComponentController : IHUD
{
    public event Action OnOpen;
    public event Action OnClose;

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
    }

    public void SetVisibility(bool visible)
    {
        if (view == null)
            return;

        if (visible && !view.isActive)
            OnOpen?.Invoke();
        else if (!visible && view.isActive)
            OnClose?.Invoke();

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

    internal void OnCloseButtonPressed() { OnClose?.Invoke(); }

    internal virtual IExploreV2MenuComponentView CreateView() => ExploreV2MenuComponentView.Create();
}