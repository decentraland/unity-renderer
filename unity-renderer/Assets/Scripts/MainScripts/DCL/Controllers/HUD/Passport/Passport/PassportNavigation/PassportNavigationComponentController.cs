using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.ProfanityFiltering;
using DCLServices.CopyPaste.Analytics;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly IAdditionalInfoFieldIconProvider additionalInfoFieldIconProvider;
        private readonly IClipboard clipboard;
        private readonly ICopyPasteAnalyticsService copyPasteAnalyticsService;
        private readonly Regex linksRegex = new (@"\[(.*?)\]\((.*?)\)", RegexOptions.Multiline);
        private readonly List<(Sprite logo, string title, string value)> additionalFields = new ();
        private readonly List<UserProfileModel.Link> links = new ();

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private readonly IPassportNavigationComponentView view;
        private HashSet<string> cachedAvatarEquippedWearables = new ();
        private string currentUserId;
        private CancellationTokenSource cts = new ();
        private Promise<WearableItem[]> wearablesPromise;
        private Promise<WearableItem[]> emotesPromise;

        private bool isMyAccountEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled("my_account");

        public event Action<string, string> OnClickBuyNft;
        public event Action OnClickedLink;
        public event Action OnClickCollectibles;

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
            ViewAllComponentController viewAllController,
            IAdditionalInfoFieldIconProvider additionalInfoFieldIconProvider,
            IClipboard clipboard,
            ICopyPasteAnalyticsService copyPasteAnalyticsService)
        {
            const string NAME_TYPE = "name";
            const string PARCEL_TYPE = "parcel";
            const string ESTATE_TYPE = "estate";

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
            this.additionalInfoFieldIconProvider = additionalInfoFieldIconProvider;
            this.clipboard = clipboard;
            this.copyPasteAnalyticsService = copyPasteAnalyticsService;

            view.OnClickBuyNft += (wearableId, wearableType) => OnClickBuyNft?.Invoke(wearableType is NAME_TYPE or PARCEL_TYPE or ESTATE_TYPE ? currentUserId : wearableId, wearableType);
            view.OnClickCollectibles += () => OnClickCollectibles?.Invoke();
            view.OnClickedViewAll += ClickedViewAll;
            view.OnClickDescriptionCoordinates += OpenGoToPanel;
            view.OnCopyDescription += CopyDescriptionToClipboard;
            viewAllController.OnBackFromViewAll += BackFromViewAll;
            viewAllController.OnClickBuyNft += (nftId) => OnClickBuyNft?.Invoke(nftId.Category is NAME_TYPE or PARCEL_TYPE or ESTATE_TYPE ? currentUserId : nftId.Id, nftId.Category);
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
            async UniTaskVoid UpdateWithUserProfileAsync(CancellationToken cancellationToken)
            {
                currentUserId = userProfile.userId;
                string filteredName = await FilterProfanityContentAsync(userProfile.userName, cancellationToken);
                view.SetGuestUser(userProfile.isGuest);
                view.SetName(filteredName);
                view.SetOwnUserTexts(userProfile.userId == ownUserProfile.userId);

                links.Clear();
                additionalFields.Clear();

                if (!userProfile.isGuest)
                {
                    string filteredDescription = await FilterProfanityContentAsync(userProfile.description, cancellationToken);

                    if (isMyAccountEnabled)
                    {
                        string filteredAdditionalValue;

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.Gender))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.Gender, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.GENDER),
                                AdditionalInfoField.GENDER.ToName(),
                                filteredAdditionalValue));
                        }

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.Country))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.Country, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.COUNTRY),
                                AdditionalInfoField.COUNTRY.ToName(),
                                filteredAdditionalValue));
                        }

                        if (userProfile.AdditionalInfo.BirthDate != null && userProfile.AdditionalInfo.BirthDate != new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.BIRTH_DATE),
                                AdditionalInfoField.BIRTH_DATE.ToName(),
                                userProfile.AdditionalInfo.BirthDate.Value.ToString("dd/MM/yyyy")));

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.Pronouns))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.Pronouns, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.PRONOUNS),
                                AdditionalInfoField.PRONOUNS.ToName(),
                                filteredAdditionalValue));
                        }

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.RelationshipStatus))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.RelationshipStatus, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.RELATIONSHIP_STATUS),
                                AdditionalInfoField.RELATIONSHIP_STATUS.ToName(),
                                filteredAdditionalValue));
                        }

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.SexualOrientation))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.SexualOrientation, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.SEXUAL_ORIENTATION),
                                AdditionalInfoField.SEXUAL_ORIENTATION.ToName(),
                                filteredAdditionalValue));
                        }

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.Language))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.Language, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.LANGUAGE),
                                AdditionalInfoField.LANGUAGE.ToName(),
                                filteredAdditionalValue));
                        }

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.Profession))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.Profession, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.PROFESSION),
                                AdditionalInfoField.PROFESSION.ToName(),
                                filteredAdditionalValue));
                        }

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.EmploymentStatus))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.EmploymentStatus, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.EMPLOYMENT_STATUS),
                                AdditionalInfoField.EMPLOYMENT_STATUS.ToName(),
                                filteredAdditionalValue));
                        }

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.Hobbies))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.Hobbies, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.HOBBIES),
                                AdditionalInfoField.HOBBIES.ToName(),
                                filteredAdditionalValue));
                        }

                        if (!string.IsNullOrEmpty(userProfile.AdditionalInfo.RealName))
                        {
                            filteredAdditionalValue = await FilterProfanityContentAsync(userProfile.AdditionalInfo.RealName, cancellationToken);

                            additionalFields.Add((
                                additionalInfoFieldIconProvider.Get(AdditionalInfoField.REAL_NAME),
                                AdditionalInfoField.REAL_NAME.ToName(),
                                filteredAdditionalValue));
                        }

                        if (userProfile.Links != null)
                            links.AddRange(userProfile.Links);
                    }
                    else
                        filteredDescription = ExtractLinks(filteredDescription, links);

                    view.SetDescription(filteredDescription);
                    view.SetAdditionalInfo(additionalFields);
                    view.SetLinks(links);
                    view.SetHasBlockedOwnUser(userProfile.IsBlocked(ownUserProfile.userId));
                    LoadAndShowOwnedNamesAsync(userProfile, cancellationToken).Forget();
                    LoadAndShowOwnedLandsAsync(userProfile, cancellationToken).Forget();
                    LoadAndDisplayEquippedWearablesAsync(userProfile, cancellationToken).Forget();
                }
            }

            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
            UpdateWithUserProfileAsync(cts.Token).Forget();
        }

        public void CloseAllNFTItemInfos() =>
            view.CloseAllNFTItemInfos();

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
            dataStore.HUDs.goToPanelConfirmed.OnChange -= CloseUIFromGoToPanel;
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

                    WearableItem[] wearableItems = await wearableItemResolver.Resolve(userProfile.avatar.wearables, ct);
                    view.SetEquippedWearables(wearableItems, userProfile.avatar.bodyShape);
                    return;
                }
            }
        }

        private void LoadAndShowOwnedWearables(UserProfile userProfile)
        {
            async UniTaskVoid RequestOwnedWearablesAsync(CancellationToken ct)
            {
                WearableItem[] containedWearables = Array.Empty<WearableItem>();

                try
                {
                    view.SetCollectibleWearablesLoadingActive(true);
                    view.SetViewAllButtonActive(PassportSection.Wearables, false);
                    var wearables = await wearablesCatalogService.RequestOwnedWearablesAsync(
                        userProfile.userId,
                        1,
                        MAX_NFT_COUNT,
                        true,
                        ct);

                    view.SetViewAllButtonActive(PassportSection.Wearables, wearables.totalAmount > MAX_NFT_COUNT);
                    var wearableItems = wearables.wearables.GroupBy(i => i.id);

                    containedWearables = wearableItems
                                        .Select(g => g.First())
                                        .Where(wearable => wearablesCatalogService.IsValidWearable(wearable.id))
                                        .ToArray();
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogError(e.Message); }
                finally
                {
                    view.SetCollectibleWearables(containedWearables);
                    view.SetCollectibleWearablesLoadingActive(false);
                }
            }

            RequestOwnedWearablesAsync(cts.Token).Forget();
        }

        private async UniTask LoadAndShowOwnedEmotes(UserProfile userProfile)
        {
            view.SetCollectibleEmotesLoadingActive(true);
            var emotes = await emotesCatalogService.RequestOwnedEmotesAsync(userProfile.userId, cts.Token);
            WearableItem[] emoteItems = emotes.GroupBy(i => i.id).Select(g => g.First()).Take(MAX_NFT_COUNT).ToArray();
            view.SetCollectibleEmotes(emoteItems);
            view.SetCollectibleEmotesLoadingActive(false);
        }

        private async UniTask LoadAndShowOwnedNamesAsync(UserProfile userProfile, CancellationToken ct)
        {
            NamesResponse.NameEntry[] namesResult = Array.Empty<NamesResponse.NameEntry>();
            var showViewAllButton = false;

            try
            {
                view.SetCollectibleNamesLoadingActive(true);
                view.SetViewAllButtonActive(PassportSection.Names, false);
                var names = await namesService.RequestOwnedNamesAsync(
                    userProfile.userId,
                    1,
                    MAX_NFT_COUNT,
                    true,
                    ct);

                namesResult = names.names.ToArray();
                showViewAllButton = names.totalAmount > MAX_NFT_COUNT;
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogException(e); }
            finally
            {
                view.SetCollectibleNames(namesResult);
                view.SetCollectibleNamesLoadingActive(false);
                view.SetViewAllButtonActive(PassportSection.Names, showViewAllButton);
            }
        }

        private async UniTask LoadAndShowOwnedLandsAsync(UserProfile userProfile, CancellationToken ct)
        {
            LandsResponse.LandEntry[] landsResult = Array.Empty<LandsResponse.LandEntry>();
            var showViewAllButton = false;

            try
            {
                view.SetCollectibleLandsLoadingActive(true);
                view.SetViewAllButtonActive(PassportSection.Lands, false);
                var lands = await landsService.RequestOwnedLandsAsync(
                    userProfile.userId,
                    1,
                    MAX_NFT_COUNT,
                    true,
                    ct);

                landsResult = lands.lands.ToArray();
                showViewAllButton = lands.totalAmount > MAX_NFT_COUNT;
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogError(e.Message); }
            finally
            {
                view.SetCollectibleLands(landsResult);
                view.SetCollectibleLandsLoadingActive(false);
                view.SetViewAllButtonActive(PassportSection.Lands, showViewAllButton);
            }
        }

        private async UniTask<string> FilterProfanityContentAsync(string filterContent, CancellationToken cancellationToken) =>
            IsProfanityFilteringEnabled()
                ? await profanityFilter.Filter(filterContent, cancellationToken)
                : filterContent;

        private bool IsProfanityFilteringEnabled() =>
            dataStore.settings.profanityChatFilteringEnabled.Get();

        private void OpenGoToPanel(ParcelCoordinates coordinates)
        {
            dataStore.HUDs.gotoPanelVisible.Set(true, true);
            dataStore.HUDs.gotoPanelCoordinates.Set((coordinates, null, null), true);

            dataStore.HUDs.goToPanelConfirmed.OnChange -= CloseUIFromGoToPanel;
            dataStore.HUDs.goToPanelConfirmed.OnChange += CloseUIFromGoToPanel;
        }

        private void CloseUIFromGoToPanel(bool confirmed, bool _)
        {
            if (!confirmed) return;
            dataStore.HUDs.goToPanelConfirmed.OnChange -= CloseUIFromGoToPanel;
            dataStore.exploreV2.isOpen.Set(false, true);
            dataStore.HUDs.currentPlayerId.Set((null, null));
        }

        private string ExtractLinks(string description, ICollection<UserProfileModel.Link> linkBuffer = null)
        {
            MatchCollection matches = linksRegex.Matches(description);

            if (matches.Count == 0) return description;

            foreach (Match match in matches)
            {
                linkBuffer?.Add(new UserProfileModel.Link(match.Groups[1].Value, match.Groups[2].Value));
                description = description.Replace(match.Value, "");
            }

            return description;
        }

        private void CopyDescriptionToClipboard()
        {
            UserProfile userProfile = userProfileBridge.Get(currentUserId);
            if (userProfile == null) return;
            string description = userProfile.description;
            description = ExtractLinks(description);
            clipboard.WriteText(description);
            copyPasteAnalyticsService.Copy("player_data");
        }
    }
}
