using AvatarSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentController
    {
        private const int MAX_NFT_COUNT = 40;
        private readonly IProfanityFilter profanityFilter;
        private readonly IWearableItemResolver wearableItemResolver;
        private readonly IWearableCatalogBridge wearableCatalogBridge;
        private readonly IEmotesCatalogService emotesCatalogService;
        private readonly DataStore dataStore;

        private readonly IPassportNavigationComponentView view;
        private HashSet<string> cachedAvatarEquippedWearables = new ();
        private readonly List<string> loadedWearables = new List<string>();
        public event Action<string> OnClickBuyNft;

        public PassportNavigationComponentController(IPassportNavigationComponentView view, IProfanityFilter profanityFilter, IWearableItemResolver wearableItemResolver, IWearableCatalogBridge wearableCatalogBridge, IEmotesCatalogService emotesCatalogService, DataStore dataStore)
        {
            this.view = view;
            this.profanityFilter = profanityFilter;
            this.wearableItemResolver = wearableItemResolver;
            this.wearableCatalogBridge = wearableCatalogBridge;
            this.emotesCatalogService = emotesCatalogService;
            this.dataStore = dataStore;
            view.OnClickBuyNft += (wearableId) => OnClickBuyNft?.Invoke(wearableId);
        }

        public void UpdateWithUserProfile(UserProfile userProfile) => UpdateWithUserProfileAsync(userProfile).Forget();

        private async UniTaskVoid UpdateWithUserProfileAsync(UserProfile userProfile)
        {
            wearableCatalogBridge.RemoveWearablesInUse(loadedWearables);
            string filteredName = await FilterContentAsync(userProfile.userName);
            view.SetGuestUser(userProfile.isGuest);
            view.SetName(filteredName);
            if (!userProfile.isGuest)
            {
                string filteredDescription = await FilterContentAsync(userProfile.description);
                view.SetDescription(filteredDescription);
                await LoadAndDisplayEquippedWearablesAsync(userProfile);
            }
        }
        private async UniTask LoadAndDisplayEquippedWearablesAsync(UserProfile userProfile)
        {
            CancellationToken ct = new CancellationToken();
            foreach (var t in userProfile.avatar.wearables)
            {
                if (!cachedAvatarEquippedWearables.Contains(t))
                {
                    view.InitializeView();
                    cachedAvatarEquippedWearables = new HashSet<string>(userProfile.avatar.wearables);
                    LoadAndShowOwnedWearables(userProfile);
                    LoadAndShowOwnedEmotes(userProfile);
                    WearableItem[] wearableItems =  await wearableItemResolver.Resolve(userProfile.avatar.wearables, ct);
                    view.SetEquippedWearables(wearableItems, userProfile.avatar.bodyShape);
                    return;
                }
            }
        }

        private void LoadAndShowOwnedWearables(UserProfile userProfile)
        {
            wearableCatalogBridge.RequestOwnedWearables(userProfile.userId)
                                 .Then(wearables =>
                                  {
                                      string[] wearableIds = wearables.GroupBy(i => i.id).Select(g => g.First().id).Take(MAX_NFT_COUNT).ToArray();
                                      userProfile.SetInventory(wearableIds);
                                      loadedWearables.AddRange(wearableIds);
                                      var containedWearables = wearables.GroupBy(i => i.id).Select(g => g.First()).Take(MAX_NFT_COUNT)
                                         .Where(wearable => wearableCatalogBridge.IsValidWearable(wearable.id));
                                      view.SetCollectibleWearables(containedWearables.ToArray());
                                  })
                                 .Catch(Debug.LogError);
        }

        private void LoadAndShowOwnedEmotes(UserProfile userProfile)
        {
            emotesCatalogService.RequestOwnedEmotes(userProfile.userId)
                                 .Then(emotes =>
                                  {
                                      WearableItem[] emoteItems = emotes.GroupBy(i => i.id).Select(g => g.First()).Take(MAX_NFT_COUNT).ToArray();
                                      view.SetCollectibleEmotes(emoteItems);
                                  })
                                 .Catch(Debug.LogError);
        }

        private async UniTask<string> FilterContentAsync(string filterContent) =>
            IsProfanityFilteringEnabled()
                ? await profanityFilter.Filter(filterContent)
                : filterContent;

        private bool IsProfanityFilteringEnabled() =>
            dataStore.settings.profanityChatFilteringEnabled.Get();
    }
}
