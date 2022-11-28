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

        private IPassportNavigationComponentView view;

        public PassportNavigationComponentController(IPassportNavigationComponentView view, IProfanityFilter profanityFilter, DataStore dataStore)
        {
            this.view = view;
            this.profanityFilter = profanityFilter;
            this.dataStore = dataStore;
        }

        public void UpdateWithUserProfile(UserProfile userProfile) => UpdateWithUserProfileAsync(userProfile);

        private async UniTask UpdateWithUserProfileAsync(UserProfile userProfile)
        {
            string filteredName = await FilterContent(userProfile.name);
            view.SetName(filteredName);
            view.SetGuestUser(userProfile.isGuest);
            if (!userProfile.isGuest)
            {
                string filteredDescription = await FilterContent(userProfile.description);
                view.SetDescription(filteredDescription);
            }
        }

        private async UniTask<string> FilterContent(string filterContent)
        {
            return IsProfanityFilteringEnabled()
                ? await profanityFilter.Filter(filterContent)
                : filterContent;
        }

        private bool IsProfanityFilteringEnabled()
        {
            return dataStore.settings.profanityChatFilteringEnabled.Get();
        }
    }
}
