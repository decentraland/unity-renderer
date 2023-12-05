using AvatarSystem;
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
using UnityEngine.Pool;

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
            IAvatarEmotesController emotesController,
            ICustomNftCollectionService customNftCollectionService)
        {
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.emotesCatalogService = emotesCatalogService;
            this.customNftCollectionService = customNftCollectionService;

            emotesCustomizationComponentController = new EmotesCustomizationComponentController(
                dataStore.emotesCustomization,
                emotesController,
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
                    allEmotes.AddRange(await emotesCatalogService.RequestOwnedEmotesAsync(userProfileBridge.GetOwn().userId, ct) ?? Array.Empty<WearableItem>());

                    Dictionary<string, WearableItem> consolidatedEmotes = new Dictionary<string, WearableItem>();

                    foreach (EmbeddedEmote emote in embeddedEmotesSo.GetAllEmotes())
                        consolidatedEmotes[emote.id] = emote;

                    foreach (var emote in allEmotes)
                    {
                        if (consolidatedEmotes.TryGetValue(emote.id, out WearableItem consolidatedEmote))
                            consolidatedEmote.amount += emote.amount + 1;
                        else
                        {
                            emote.amount++;
                            consolidatedEmotes[emote.id] = emote;
                        }
                    }

                    allEmotes = consolidatedEmotes.Values.ToList();

                    try
                    {
                        await FetchCustomEmoteItems(allEmotes, ct);
                        await FetchCustomEmoteCollections(allEmotes, ct);
                    }
                    catch (Exception e) when (e is not OperationCanceledException) { Debug.LogException(e); }
                    finally { UpdateEmotes(); }

                    void UpdateEmotes()
                    {
                        dataStore.emotesCustomization.UnequipMissingEmotes(allEmotes);
                        emotesCustomizationComponentController.SetEmotes(allEmotes.ToArray());
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogException(e); }
            }

            loadEmotesCts = loadEmotesCts.SafeRestart();
            LoadEmotesAsync(loadEmotesCts.Token).Forget();
        }

        public void RestoreEmoteSlots() =>
            emotesCustomizationComponentController.RestoreEmoteSlots();

        public void SetEquippedBodyShape(string bodyShapeId) =>
            emotesCustomizationComponentController.SetEquippedBodyShape(bodyShapeId);

        // TODO: Delete?
        private void NewEmoteAdded(string emoteId) =>
            OnNewEmoteAdded?.Invoke(emoteId);

        // TODO: Delete?
        private void EmotePreviewed(string emoteId) =>
            OnEmotePreviewed?.Invoke(emoteId);

        // TODO: Delete?
        private void EmoteEquipped(string emoteId) =>
            OnEmoteEquipped?.Invoke(emoteId);

        // TODO: Delete?
        private void EmoteUnEquipped(string emoteId) =>
            OnEmoteUnEquipped?.Invoke(emoteId);

        private void EmoteSell(string collectibleId)
        {
            var ownedCollectible = ownedNftCollectionsL1.FirstOrDefault(nft => nft.urn == collectibleId) ??
                                   ownedNftCollectionsL2.FirstOrDefault(nft => nft.urn == collectibleId);

            WebInterface.OpenURL(ownedCollectible != null ? URL_SELL_SPECIFIC_COLLECTIBLE.Replace("{collectionId}", ownedCollectible.collectionId).Replace("{tokenId}", ownedCollectible.tokenId) : URL_SELL_COLLECTIBLE_GENERIC);
        }

        private async UniTask FetchCustomEmoteItems(ICollection<WearableItem> emotes,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<string> customItems = await customNftCollectionService.GetConfiguredCustomNftItemsAsync(cancellationToken);

            WearableItem[] retrievedEmotes = await UniTask.WhenAll(customItems.Select(nftId =>
                nftId.StartsWith("urn", StringComparison.OrdinalIgnoreCase)
                    ? emotesCatalogService.RequestEmoteAsync(nftId, cancellationToken)
                    : emotesCatalogService.RequestEmoteFromBuilderAsync(nftId, cancellationToken)));

            foreach (WearableItem emote in retrievedEmotes)
            {
                if (emote == null)
                {
                    Debug.LogWarning("Custom emote item skipped is null");
                    continue;
                }

                emotes.Add(emote);
            }
        }

        private async UniTask FetchCustomEmoteCollections(
            List<WearableItem> emotes,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<string> customCollections =
                await customNftCollectionService.GetConfiguredCustomNftCollectionAsync(cancellationToken);

            HashSet<string> publishedCollections = HashSetPool<string>.Get();
            HashSet<string> collectionsInBuilder = HashSetPool<string>.Get();

            foreach (string collectionId in customCollections)
            {
                if (collectionId.StartsWith("urn", StringComparison.OrdinalIgnoreCase))
                    publishedCollections.Add(collectionId);
                else
                    collectionsInBuilder.Add(collectionId);
            }

            await UniTask.WhenAll(emotesCatalogService.RequestEmoteCollectionAsync(publishedCollections, cancellationToken, emotes),
                emotesCatalogService.RequestEmoteCollectionInBuilderAsync(collectionsInBuilder, cancellationToken, emotes));

            HashSetPool<string>.Release(publishedCollections);
            HashSetPool<string>.Release(collectionsInBuilder);
        }
    }
}
