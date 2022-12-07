using AvatarSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentController
    {
        private readonly IProfanityFilter profanityFilter;
        private readonly IWearableItemResolver wearableItemResolver;
        private readonly DataStore dataStore;

        private readonly IPassportNavigationComponentView view;
        private HashSet<string> cachedAvatarEquippedWearables = new ();
        public event Action<string> OnClickBuyNft;

        public PassportNavigationComponentController(IPassportNavigationComponentView view, IProfanityFilter profanityFilter, IWearableItemResolver wearableItemResolver, DataStore dataStore)
        {
            this.view = view;
            this.profanityFilter = profanityFilter;
            this.wearableItemResolver = wearableItemResolver;
            this.dataStore = dataStore;
            view.OnClickBuyNft += (wearableId) => OnClickBuyNft?.Invoke(wearableId);
        }

        public void UpdateWithUserProfile(UserProfile userProfile) => UpdateWithUserProfileAsync(userProfile).Forget();

        private async UniTaskVoid UpdateWithUserProfileAsync(UserProfile userProfile)
        {
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
            foreach (var t in userProfile.avatar.wearables)
            {
                if (!cachedAvatarEquippedWearables.Contains(t))
                {
                    view.InitializeView();
                    cachedAvatarEquippedWearables = new HashSet<string>(userProfile.avatar.wearables);
                    WearableItem[] wearableItems =  await wearableItemResolver.Resolve(userProfile.avatar.wearables, ct);
                    view.SetEquippedWearables(wearableItems);
                    view.SetCollectibleWearables(wearableItems);
                    return;
                }
            }
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
