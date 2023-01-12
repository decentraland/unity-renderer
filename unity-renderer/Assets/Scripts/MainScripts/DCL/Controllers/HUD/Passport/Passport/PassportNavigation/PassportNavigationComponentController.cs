using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentController : IDisposable
    {
        private const int MAX_NFT_COUNT = 40;

        private readonly IProfanityFilter profanityFilter;
        private readonly IWearableItemResolver wearableItemResolver;
        private readonly IWearableCatalogBridge wearableCatalogBridge;
        private readonly IEmotesCatalogService emotesCatalogService;
        private readonly INamesService namesService;
        private readonly ILandsService landsService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly DataStore dataStore;
        private string currentUserId;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private readonly IPassportNavigationComponentView view;
        private HashSet<string> cachedAvatarEquippedWearables = new ();
        private readonly List<string> loadedWearables = new List<string>();
        public event Action<string, string> OnClickBuyNft;
        public event Action OnClickCollectibles;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public PassportNavigationComponentController(
            IPassportNavigationComponentView view,
            IProfanityFilter profanityFilter,
            IWearableItemResolver wearableItemResolver,
            IWearableCatalogBridge wearableCatalogBridge,
            IEmotesCatalogService emotesCatalogService,
            INamesService namesService,
            ILandsService landsService,
            IUserProfileBridge userProfileBridge,
            DataStore dataStore)
        {
            this.view = view;
            this.profanityFilter = profanityFilter;
            this.wearableItemResolver = wearableItemResolver;
            this.wearableCatalogBridge = wearableCatalogBridge;
            this.emotesCatalogService = emotesCatalogService;
            this.namesService = namesService;
            this.landsService = landsService;
            this.userProfileBridge = userProfileBridge;
            this.dataStore = dataStore;
            view.OnClickBuyNft += (wearableId, wearableType) => OnClickBuyNft?.Invoke(wearableType is "name" or "parcel" or "estate" ? currentUserId : wearableId, wearableType);
            view.OnClickCollectibles += () => OnClickCollectibles?.Invoke();
        }

        public void UpdateWithUserProfile(UserProfile userProfile)
        {
            async UniTaskVoid UpdateWithUserProfileAsync()
            {
                var ct = cts.Token;
                currentUserId = userProfile.userId;
                wearableCatalogBridge.RemoveWearablesInUse(loadedWearables);
                string filteredName = await FilterContentAsync(userProfile.userName).AttachExternalCancellation(ct);
                view.SetGuestUser(userProfile.isGuest);
                view.SetName(filteredName);

                if (!userProfile.isGuest)
                {
                    string filteredDescription = await FilterContentAsync(userProfile.description).AttachExternalCancellation(ct);
                    view.SetDescription(filteredDescription);
                    view.SetHasBlockedOwnUser(userProfile.IsBlocked(ownUserProfile.userId));
                    LoadAndShowOwnedNamesAsync(userProfile).Forget();
                    LoadAndShowOwnedLandsAsync(userProfile).Forget();
                    LoadAndDisplayEquippedWearablesAsync(userProfile, ct).Forget();
                }
            }

            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
            UpdateWithUserProfileAsync().Forget();
        }

        public void CloseAllNFTItemInfos() => view.CloseAllNFTItemInfos();

        public void Dispose()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }



        private async UniTask LoadAndDisplayEquippedWearablesAsync(UserProfile userProfile, CancellationToken ct)
        {
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
            view.SetCollectibleWearablesLoadingActive(true);
            wearableCatalogBridge.RequestOwnedWearables(userProfile.userId)
                                 .Then(wearables =>
                                  {
                                      string[] wearableIds = wearables.GroupBy(i => i.id).Select(g => g.First().id).Take(MAX_NFT_COUNT).ToArray();
                                      userProfile.SetInventory(wearableIds);
                                      loadedWearables.AddRange(wearableIds);
                                      var containedWearables = wearables.GroupBy(i => i.id).Select(g => g.First()).Take(MAX_NFT_COUNT)
                                         .Where(wearable => wearableCatalogBridge.IsValidWearable(wearable.id));
                                      view.SetCollectibleWearables(containedWearables.ToArray());
                                      view.SetCollectibleWearablesLoadingActive(false);
                                  })
                                 .Catch(Debug.LogError);
        }

        private void LoadAndShowOwnedEmotes(UserProfile userProfile)
        {
            view.SetCollectibleEmotesLoadingActive(true);
            emotesCatalogService.RequestOwnedEmotes(userProfile.userId)
                                 .Then(emotes =>
                                  {
                                      WearableItem[] emoteItems = emotes.GroupBy(i => i.id).Select(g => g.First()).Take(MAX_NFT_COUNT).ToArray();
                                      view.SetCollectibleEmotes(emoteItems);
                                      view.SetCollectibleEmotesLoadingActive(false);
                                  })
                                 .Catch(Debug.LogError);
        }

        private async UniTask LoadAndShowOwnedNamesAsync(UserProfile userProfile)
        {
            view.SetCollectibleNamesLoadingActive(true);
            using var pagePointer = namesService.GetPaginationPointer(userProfile.userId, MAX_NFT_COUNT, CancellationToken.None);
            var response = await pagePointer.GetPageAsync(1, CancellationToken.None);
            var namesResult = Array.Empty<NamesResponse.NameEntry>();

            if (response.success)
                namesResult = response.response.Names.ToArray();
            else
                Debug.LogError("Error requesting names lambdas!");

            view.SetCollectibleNames(namesResult);
            view.SetCollectibleNamesLoadingActive(false);
        }

        private async UniTask LoadAndShowOwnedLandsAsync(UserProfile userProfile)
        {
            view.SetCollectibleLandsLoadingActive(true);
            // TODO (Santi): Use userProfile.userId here!!
            using var pagePointer = landsService.GetPaginationPointer(userProfile.userId, MAX_NFT_COUNT, CancellationToken.None);
            var response = await pagePointer.GetPageAsync(1, CancellationToken.None);
            var landsResult = Array.Empty<LandsResponse.LandEntry>();

            if (response.success)
                landsResult = response.response.Lands.ToArray();
            else
                Debug.LogError("Error requesting lands lambdas!");

            view.SetCollectibleLands(landsResult);
            view.SetCollectibleLandsLoadingActive(false);
        }

        private async UniTask<string> FilterContentAsync(string filterContent) =>
            IsProfanityFilteringEnabled()
                ? await profanityFilter.Filter(filterContent)
                : filterContent;

        private bool IsProfanityFilteringEnabled() =>
            dataStore.settings.profanityChatFilteringEnabled.Get();
    }
}
