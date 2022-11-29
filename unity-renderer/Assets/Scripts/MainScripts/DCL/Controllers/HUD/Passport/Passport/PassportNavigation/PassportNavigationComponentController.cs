using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentController
    {
        private readonly IProfanityFilter profanityFilter;
        private readonly DataStore dataStore;

        private readonly IPassportNavigationComponentView view;
        
        public PassportNavigationComponentController(IPassportNavigationComponentView view, IProfanityFilter profanityFilter, DataStore dataStore)
        {
            this.view = view;
            this.profanityFilter = profanityFilter;
            this.dataStore = dataStore;
        }

        public void UpdateWithUserProfile(UserProfile userProfile) => UpdateWithUserProfileAsync(userProfile).Forget();

        private async UniTaskVoid UpdateWithUserProfileAsync(UserProfile userProfile)
        {
            string filteredName = await FilterName(userProfile);
            view.SetName(filteredName);
            view.SetGuestUser(userProfile.isGuest);
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
}