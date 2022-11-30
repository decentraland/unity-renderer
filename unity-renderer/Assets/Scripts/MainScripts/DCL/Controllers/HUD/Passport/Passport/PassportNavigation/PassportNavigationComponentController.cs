using AvatarSystem;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentController
    {
        private readonly IProfanityFilter profanityFilter;
        private readonly IWearableItemResolver wearableItemResolver;
        private readonly DataStore dataStore;

        private IPassportNavigationComponentView view;

        public PassportNavigationComponentController(IPassportNavigationComponentView view, IProfanityFilter profanityFilter, IWearableItemResolver wearableItemResolver, DataStore dataStore)
        {
            this.view = view;
            this.profanityFilter = profanityFilter;
            this.wearableItemResolver = wearableItemResolver;
            this.dataStore = dataStore;
        }

        public void UpdateWithUserProfile(UserProfile userProfile) => UpdateWithUserProfileAsync(userProfile);

        private async UniTask UpdateWithUserProfileAsync(UserProfile userProfile)
        {
            view.InitializeView();
            string filteredName = await FilterContent(userProfile.userName);
            view.SetGuestUser(userProfile.isGuest);
            view.SetName(filteredName);
            if (!userProfile.isGuest)
            {
                string filteredDescription = await FilterContent(userProfile.description);
                view.SetDescription(filteredDescription);
                await LoadAndDisplayEquippedWearables(userProfile);
            }
        }

        private async UniTask LoadAndDisplayEquippedWearables(UserProfile userProfile)
        {
            CancellationToken ct = new CancellationToken();
            WearableItem[] wearableItems =  await wearableItemResolver.Resolve(userProfile.avatar.wearables, ct);
            view.SetEquippedWearables(wearableItems);
        }

        private async UniTask<string> FilterContent(string filterContent)
        {
            return IsProfanityFilteringEnabled()
                ? await profanityFilter.Filter(filterContent)
                : filterContent;
        }

        private bool IsProfanityFilteringEnabled() =>
            dataStore.settings.profanityChatFilteringEnabled.Get();
    }
}
