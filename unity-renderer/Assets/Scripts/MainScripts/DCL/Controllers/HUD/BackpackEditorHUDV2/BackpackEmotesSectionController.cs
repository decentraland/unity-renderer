using Cysharp.Threading.Tasks;
using DCL.Emotes;
using DCL.EmotesCustomization;
using DCL.Interface;
using DCL.Tasks;
using DCLServices.CustomNftCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEmotesSectionController : IBackpackEmotesSectionController
    {
        private const string URL_SELL_COLLECTIBLE_GENERIC = "https://market.decentraland.org/account";
        private const string URL_SELL_SPECIFIC_COLLECTIBLE = "https://market.decentraland.org/contracts/{collectionId}/tokens/{tokenId}";

        public event Action<string> OnNewEmoteAdded;
        public event Action<string> OnEmotePreviewed;
        public event Action<string> OnEmoteEquipped;
        public event Action<string> OnEmoteUnEquipped;

        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IEmotesCatalogService emotesCatalogService;
        private readonly ICustomNftCollectionService customNftCollectionService;
        private readonly IEmotesCustomizationComponentController emotesCustomizationComponentController;
        private CancellationTokenSource loadEmotesCts = new ();
        private List<Nft> ownedNftCollectionsL1 = new ();
        private List<Nft> ownedNftCollectionsL2 = new ();

        public BackpackEmotesSectionController(
            DataStore dataStore,
            Transform emotesSectionTransform,
            IUserProfileBridge userProfileBridge,
            IEmotesCatalogService emotesCatalogService,
            ICustomNftCollectionService customNftCollectionService)
        {
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.emotesCatalogService = emotesCatalogService;
            this.customNftCollectionService = customNftCollectionService;

            emotesCustomizationComponentController = new EmotesCustomizationComponentController(
                dataStore.emotesCustomization,
                dataStore.emotes,
                dataStore.exploreV2,
                dataStore.HUDs,
                emotesSectionTransform,
                "EmotesCustomization/EmotesCustomizationSectionV2");

            emotesCustomizationComponentController.SetEquippedBodyShape(userProfileBridge.GetOwn().avatar.bodyShape);

            userProfileBridge.GetOwn().OnUpdate += LoadUserProfile;

            dataStore.emotesCustomization.currentLoadedEmotes.OnAdded += NewEmoteAdded;
            emotesCustomizationComponentController.onEmotePreviewed += EmotePreviewed;
            emotesCustomizationComponentController.onEmoteEquipped += EmoteEquipped;
            emotesCustomizationComponentController.onEmoteUnequipped += EmoteUnEquipped;
            emotesCustomizationComponentController.onEmoteSell += EmoteSell;
        }

        public void Dispose()
        {
            loadEmotesCts.SafeCancelAndDispose();
            loadEmotesCts = null;

            dataStore.emotesCustomization.currentLoadedEmotes.OnAdded -= NewEmoteAdded;
            emotesCustomizationComponentController.onEmotePreviewed -= EmotePreviewed;
            emotesCustomizationComponentController.onEmoteEquipped -= EmoteEquipped;
            emotesCustomizationComponentController.onEmoteUnequipped -= EmoteUnEquipped;
            emotesCustomizationComponentController.onEmoteSell -= EmoteSell;
        }

        private void LoadUserProfile(UserProfile userProfile) =>
            QueryNftCollections(userProfile.userId);

        private void QueryNftCollections(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return;

            Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userProfileBridge.GetOwn().userId, NftCollectionsLayer.ETHEREUM)
                       .Then(nft => ownedNftCollectionsL1 = nft)
                       .Catch(Debug.LogError);

            Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userProfileBridge.GetOwn().userId, NftCollectionsLayer.MATIC)
                       .Then((nft) => ownedNftCollectionsL2 = nft)
                       .Catch(Debug.LogError);
        }

        public void LoadEmotes()
        {
            async UniTaskVoid LoadEmotesAsync(CancellationToken ct = default)
            {
                try
                {
                    EmbeddedEmotesSO embeddedEmotesSo = await emotesCatalogService.GetEmbeddedEmotes();
                    List<WearableItem> allEmotes = new ();
                    allEmotes.AddRange(embeddedEmotesSo.emotes);
                    allEmotes.AddRange(await emotesCatalogService.RequestOwnedEmotesAsync(userProfileBridge.GetOwn().userId, ct) ?? Array.Empty<WearableItem>());
                    allEmotes.AddRange(await FetchCustomEmoteCollections(ct));
                    allEmotes.AddRange(await FetchCustomEmoteItems(ct));

                    dataStore.emotesCustomization.UnequipMissingEmotes(allEmotes);
                    emotesCustomizationComponentController.SetEmotes(allEmotes.ToArray());
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogError($"Error loading emotes: {e.Message}"); }
            }

            loadEmotesCts = loadEmotesCts.SafeRestart();
            LoadEmotesAsync(loadEmotesCts.Token).Forget();
        }

        public void RestoreEmoteSlots() =>
            emotesCustomizationComponentController.RestoreEmoteSlots();

        public void SetEquippedBodyShape(string bodyShapeId) =>
            emotesCustomizationComponentController.SetEquippedBodyShape(bodyShapeId);

        private void NewEmoteAdded(string emoteId) =>
            OnNewEmoteAdded?.Invoke(emoteId);

        private void EmotePreviewed(string emoteId) =>
            OnEmotePreviewed?.Invoke(emoteId);

        private void EmoteEquipped(string emoteId) =>
            OnEmoteEquipped?.Invoke(emoteId);

        private void EmoteUnEquipped(string emoteId) =>
            OnEmoteUnEquipped?.Invoke(emoteId);

        private void EmoteSell(string collectibleId)
        {
            var ownedCollectible = ownedNftCollectionsL1.FirstOrDefault(nft => nft.urn == collectibleId) ??
                                   ownedNftCollectionsL2.FirstOrDefault(nft => nft.urn == collectibleId);

            WebInterface.OpenURL(ownedCollectible != null ? URL_SELL_SPECIFIC_COLLECTIBLE.Replace("{collectionId}", ownedCollectible.collectionId).Replace("{tokenId}", ownedCollectible.tokenId) : URL_SELL_COLLECTIBLE_GENERIC);
        }

        private async UniTask<IEnumerable<WearableItem>> FetchCustomEmoteItems(CancellationToken cancellationToken)
        {
            IReadOnlyList<string> customItems = await customNftCollectionService.GetConfiguredCustomNftItemsAsync(cancellationToken);

            IEnumerable<string> publishedItems = customItems
               .Where(nftId => nftId.StartsWith("urn", StringComparison.OrdinalIgnoreCase));

            IEnumerable<string> itemsInBuilder = customItems
               .Where(nftId => !nftId.StartsWith("urn", StringComparison.OrdinalIgnoreCase));

            List<WearableItem> emotes = new ();

            foreach (string nftId in publishedItems)
            {
                try
                {
                    WearableItem wearable = await emotesCatalogService.RequestEmoteAsync(nftId, cancellationToken);

                    if (wearable == null)
                    {
                        Debug.LogWarning($"Custom emote item skipped is null: {nftId}");
                        continue;
                    }

                    emotes.Add(wearable);
                }
                catch (Exception e) when (e is not OperationCanceledException) { Debug.LogException(e); }
            }

            foreach (string nftId in itemsInBuilder)
            {
                try
                {
                    WearableItem wearable = await emotesCatalogService.RequestEmoteFromBuilderAsync(nftId, cancellationToken);

                    if (wearable == null)
                    {
                        Debug.LogWarning($"Custom emote item skipped is null: {nftId}");
                        continue;
                    }

                    emotes.Add(wearable);
                }
                catch (Exception e) when (e is not OperationCanceledException) { Debug.LogException(e); }
            }

            return emotes;
        }

        private async UniTask<IEnumerable<WearableItem>> FetchCustomEmoteCollections(CancellationToken cancellationToken)
        {
            IReadOnlyList<string> customCollections =
                await customNftCollectionService.GetConfiguredCustomNftCollectionAsync(cancellationToken);

            IEnumerable<string> publishedCollections = customCollections
               .Where(collectionId => collectionId.StartsWith("urn", StringComparison.OrdinalIgnoreCase));

            IEnumerable<string> collectionsInBuilder = customCollections
               .Where(collectionId => !collectionId.StartsWith("urn", StringComparison.OrdinalIgnoreCase));

            List<WearableItem> emotes = new ();

            try { emotes.AddRange(await emotesCatalogService.RequestEmoteCollectionAsync(publishedCollections, cancellationToken)); }
            catch (Exception e) when (e is not OperationCanceledException) { Debug.LogException(e); }

            try { emotes.AddRange(await emotesCatalogService.RequestEmoteCollectionInBuilderAsync(collectionsInBuilder, cancellationToken)); }
            catch (Exception e) when (e is not OperationCanceledException) { Debug.LogException(e); }

            return emotes;
        }
    }
}
