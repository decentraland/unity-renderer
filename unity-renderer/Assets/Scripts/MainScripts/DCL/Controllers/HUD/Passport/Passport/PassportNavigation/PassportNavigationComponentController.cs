using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.ProfanityFiltering;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
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
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly IEmotesCatalogService emotesCatalogService;
        private readonly INamesService namesService;
        private readonly ILandsService landsService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly DataStore dataStore;
        private readonly ViewAllComponentController viewAllController;
        private string currentUserId;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private readonly IPassportNavigationComponentView view;
        private HashSet<string> cachedAvatarEquippedWearables = new ();
        public event Action<string, string> OnClickBuyNft;
        public event Action OnClickedLink;
        public event Action OnClickCollectibles;

        private CancellationTokenSource cts = new CancellationTokenSource();
        private Promise<WearableItem[]> wearablesPromise;
        private Promise<WearableItem[]> emotesPromise;

        public PassportNavigationComponentController(
            IPassportNavigationComponentView view,
            IProfanityFilter profanityFilter,
            IWearableItemResolver wearableItemResolver,
            IWearablesCatalogService wearablesCatalogService,
            IEmotesCatalogService emotesCatalogService,
            INamesService namesService,
            ILandsService landsService,
            IUserProfileBridge userProfileBridge,
            DataStore dataStore,
            ViewAllComponentController viewAllController)
        {
            this.view = view;
            this.profanityFilter = profanityFilter;
            this.wearableItemResolver = wearableItemResolver;
            this.wearablesCatalogService = wearablesCatalogService;
            this.emotesCatalogService = emotesCatalogService;
            this.namesService = namesService;
            this.landsService = landsService;
            this.userProfileBridge = userProfileBridge;
            this.dataStore = dataStore;
            this.viewAllController = viewAllController;

            view.OnClickBuyNft += (wearableId, wearableType) => OnClickBuyNft?.Invoke(wearableType is "name" or "parcel" or "estate" ? currentUserId : wearableId, wearableType);
            view.OnClickCollectibles += () => OnClickCollectibles?.Invoke();
            view.OnClickedViewAll += ClickedViewAll;
            viewAllController.OnBackFromViewAll += BackFromViewAll;
            viewAllController.OnClickBuyNft += (wearableId, wearableType) => OnClickBuyNft?.Invoke(wearableType is "name" or "parcel" or "estate" ? currentUserId : wearableId, wearableType);
        }

        private void BackFromViewAll()
        {
            view.OpenCollectiblesTab();
        }

        private void ClickedViewAll(PassportSection section)
        {
            view.CloseAllSections();
            viewAllController.SetViewAllVisibility(true);
            viewAllController.OpenViewAllSection(section);

        }

        public void UpdateWithUserProfile(UserProfile userProfile)
        {
            async UniTaskVoid UpdateWithUserProfileAsync()
            {
                var ct = cts.Token;
                currentUserId = userProfile.userId;
                string filteredName = await FilterContentAsync(userProfile.userName).AttachExternalCancellation(ct);
                view.SetGuestUser(userProfile.isGuest);
                view.SetName(filteredName);
                view.SetOwnUserTexts(userProfile.userId == ownUserProfile.userId);

                if (!userProfile.isGuest)
                {
                    string filteredDescription = await FilterContentAsync(userProfile.description).AttachExternalCancellation(ct);
                    view.SetDescription(filteredDescription);
                    view.SetHasBlockedOwnUser(userProfile.IsBlocked(ownUserProfile.userId));
                    LoadAndShowOwnedNamesAsync(userProfile, ct).Forget();
                    LoadAndShowOwnedLandsAsync(userProfile, ct).Forget();
                    LoadAndDisplayEquippedWearablesAsync(userProfile, ct).Forget();
                }
            }

            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
            UpdateWithUserProfileAsync().Forget();
        }

        public void CloseAllNFTItemInfos() => view.CloseAllNFTItemInfos();

        public void ResetNavigationTab()
        {
            view.SetInitialPage();
            viewAllController.SetViewAllVisibility(false);
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;

            wearablesPromise?.Dispose();
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
                    LoadAndShowOwnedEmotes(userProfile).Forget();

                    WearableItem[] wearableItems =  await wearableItemResolver.Resolve(userProfile.avatar.wearables, ct);
                    view.SetEquippedWearables(wearableItems, userProfile.avatar.bodyShape);
                    return;
                }
            }
        }

        private void LoadAndShowOwnedWearables(UserProfile userProfile)
        {
            async UniTaskVoid RequestOwnedWearablesAsync(CancellationToken ct)
            {
                try
                {
                    view.SetViewAllButtonActive(PassportSection.Wearables, false);
                    var wearables = await wearablesCatalogService.RequestOwnedWearablesAsync(
                        userProfile.userId,
                        1,
                        MAX_NFT_COUNT,
                        true,
                        ct);

                    view.SetViewAllButtonActive(PassportSection.Wearables, wearables.totalAmount > MAX_NFT_COUNT);
                    IGrouping<string, WearableItem>[] wearableItems = wearables.wearables.GroupBy(i => i.id).ToArray();

                    var containedWearables = wearableItems
                                            .Select(g => g.First())
                                            .Where(wearable => wearablesCatalogService.IsValidWearable(wearable.id));

                    view.SetCollectibleWearables(containedWearables.ToArray());
                    view.SetCollectibleWearablesLoadingActive(false);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

            view.SetCollectibleWearablesLoadingActive(true);
            RequestOwnedWearablesAsync(cts.Token).Forget();
        }

        private async UniTask LoadAndShowOwnedEmotes(UserProfile userProfile)
        {
            view.SetCollectibleEmotesLoadingActive(true);
            WearableItem[] emotes = await emotesCatalogService.RequestOwnedEmotesAsync(userProfile.userId, cts.Token);
            WearableItem[] emoteItems = emotes.GroupBy(i => i.id).Select(g => g.First()).Take(MAX_NFT_COUNT).ToArray();
            view.SetCollectibleEmotes(emoteItems);
            view.SetCollectibleEmotesLoadingActive(false);
        }

        private async UniTask LoadAndShowOwnedNamesAsync(UserProfile userProfile, CancellationToken ct)
        {
            view.SetCollectibleNamesLoadingActive(true);
            view.SetViewAllButtonActive(PassportSection.Names, false);
            using var pagePointer = namesService.GetPaginationPointer(userProfile.userId, MAX_NFT_COUNT, CancellationToken.None);
            var response = await pagePointer.GetPageAsync(1, ct);
            var namesResult = Array.Empty<NamesResponse.NameEntry>();
            var showViewAllButton = false;

            if (response.success)
            {
                namesResult = response.response.Names.ToArray();
                showViewAllButton = response.response.TotalAmount > MAX_NFT_COUNT;
            }
            else
                Debug.LogError("Error requesting names lambdas!");

            view.SetCollectibleNames(namesResult);
            view.SetCollectibleNamesLoadingActive(false);
            view.SetViewAllButtonActive(PassportSection.Names, showViewAllButton);
        }

        private async UniTask LoadAndShowOwnedLandsAsync(UserProfile userProfile, CancellationToken ct)
        {
            view.SetCollectibleLandsLoadingActive(true);
            view.SetViewAllButtonActive(PassportSection.Lands, false);
            using var pagePointer = landsService.GetPaginationPointer(userProfile.userId, MAX_NFT_COUNT, CancellationToken.None);
            var response = await pagePointer.GetPageAsync(1, ct);
            var landsResult = Array.Empty<LandsResponse.LandEntry>();
            var showViewAllButton = false;

            if (response.success)
            {
                landsResult = response.response.Lands.ToArray();
                showViewAllButton = response.response.TotalAmount > MAX_NFT_COUNT;
            }
            else
                Debug.LogError("Error requesting lands lambdas!");

            view.SetCollectibleLands(landsResult);
            view.SetCollectibleLandsLoadingActive(false);
            view.SetViewAllButtonActive(PassportSection.Lands, showViewAllButton);
        }

        private async UniTask<string> FilterContentAsync(string filterContent) =>
            IsProfanityFilteringEnabled()
                ? await profanityFilter.Filter(filterContent)
                : filterContent;

        private bool IsProfanityFilteringEnabled() =>
            dataStore.settings.profanityChatFilteringEnabled.Get();
    }
}
