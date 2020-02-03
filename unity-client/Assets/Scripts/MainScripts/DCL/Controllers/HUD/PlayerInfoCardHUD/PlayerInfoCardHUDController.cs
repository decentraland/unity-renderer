using System;
using UnityEngine;

public class PlayerInfoCardHUDController : IHUD, IDisposable
{
    internal const string CURRENT_PLAYER_NAME = "CurrentPlayerInfoCardName";

    internal PlayerInfoCardHUDView view;
    internal StringVariable currentPlayerName;
    internal UserProfile currentUserProfile;

    public PlayerInfoCardHUDController()
    {
        view = PlayerInfoCardHUDView.CreateView();
        view.Initialize(() => {currentPlayerName.Set(null); });
        currentPlayerName = Resources.Load<StringVariable>(CURRENT_PLAYER_NAME);
        currentPlayerName.OnChange += OnCurrentPlayerNameChanged;
        OnCurrentPlayerNameChanged(currentPlayerName, null);
    }

    internal void OnCurrentPlayerNameChanged(string current, string previous)
    {
        if (currentUserProfile != null)
            currentUserProfile.OnUpdate -= SetUserProfile;

        currentUserProfile = string.IsNullOrEmpty(currentPlayerName) ? null : UserProfileController.userProfilesCatalog.Get(currentPlayerName);

        if (currentUserProfile == null)
        {
            view.SetCardActive(false);
        }
        else
        {
            currentUserProfile.OnUpdate += SetUserProfile;
            SetUserProfile(currentUserProfile);
            view.SetCardActive(true);
        }
    }

    private void SetUserProfile(UserProfile userProfile)
    {
        view.SetUserProfile(userProfile);
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        if (currentUserProfile != null) 
            currentUserProfile.OnUpdate -= SetUserProfile;

        if (currentPlayerName != null)
            currentPlayerName.OnChange -= OnCurrentPlayerNameChanged;
    }
}