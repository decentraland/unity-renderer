using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;
using DCL.Helpers;

public class PassportPlayerInfoComponentController
{
    private IPassportPlayerInfoComponentView view;
    private readonly DataStore dataStore;
    private readonly IProfanityFilter profanityFilter;

    public PassportPlayerInfoComponentController(IPassportPlayerInfoComponentView view, DataStore dataStore, IProfanityFilter profanityFilter)
    {
        this.view = view;
        this.dataStore = dataStore;
        this.profanityFilter = profanityFilter;
    }

    public async UniTask UpdateWithUserProfile(UserProfile userProfile)
    {
        view.SetWallet(userProfile.userId);
        string filteredName = await FilterName(userProfile);
        view.SetName(filteredName);
    }

    private async UniTask<string> FilterName(UserProfile userProfile)
    {
        return IsProfanityFilteringEnabled()
            ? await profanityFilter.Filter(userProfile.userName)
            : userProfile.userName;
    }

    private bool IsProfanityFilteringEnabled()
    {
        return dataStore.settings.profanityChatFilteringEnabled.Get();
    }
}
