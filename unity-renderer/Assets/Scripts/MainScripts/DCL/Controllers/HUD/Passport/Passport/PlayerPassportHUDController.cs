using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;

public class PlayerPassportHUDController : IHUD
{
    internal readonly PlayerPassportHUDView view;
    internal readonly StringVariable currentPlayerId;
    internal UserProfile currentUserProfile;

    private readonly IUserProfileBridge userProfileBridge;

    public PlayerPassportHUDController(
        StringVariable currentPlayerId,
        IUserProfileBridge userProfileBridge)
    {
        view = PlayerPassportHUDView.CreateView();
        
        this.currentPlayerId = currentPlayerId;
        this.userProfileBridge = userProfileBridge;

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
            currentUserProfile.OnUpdate -= SetUserProfile;

        currentUserProfile = string.IsNullOrEmpty(current)
            ? null
            : userProfileBridge.Get(current);

        if (currentUserProfile == null)
        {
            /*
            if (playerInfoCardVisibleState.Get())
                socialAnalytics.SendPassportClose(Time.realtimeSinceStartup - passportOpenStartTime);
            */
            view.SetPassportPanelVisibility(false);
            //wearableCatalogBridge.RemoveWearablesInUse(loadedWearables);
            //loadedWearables.Clear();
        }
        else
        {
            currentUserProfile.OnUpdate += SetUserProfile;

            /*TaskUtils.Run(async () =>
                     {
                         await AsyncSetUserProfile(currentUserProfile);
                         view.SetCardActive(true);
                         socialAnalytics.SendPassportOpen();
                     })
                     .Forget();

            passportOpenStartTime = Time.realtimeSinceStartup;
            */
            view.SetPassportPanelVisibility(true);
        }
    }

    private void SetUserProfile(UserProfile userProfile)
    {
        //Assert.IsTrue(userProfile != null, "userProfile can't be null");

        //TaskUtils.Run(async () => await AsyncSetUserProfile(userProfile)).Forget();
    }
}
