using Cysharp.Threading.Tasks;
using DCL.Emotes;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace AvatarSystem
{
    public class AvatarCurator : IAvatarCurator
    {
        private readonly IWearableItemResolver wearableItemResolver;
        private readonly IEmotesCatalogService emotesCatalog;
        private EmbeddedEmotesSO embedEmotes;
        private string[] embedEmotesId;

        public AvatarCurator(IWearableItemResolver wearableItemResolver, IEmotesCatalogService emotesCatalog)
        {
            Assert.IsNotNull(wearableItemResolver);
            this.wearableItemResolver = wearableItemResolver;
            this.emotesCatalog = emotesCatalog;

        }

        /// <summary>
        /// Curate a flattened into IDs set of wearables.
        /// Bear in mind that the bodyshape must be part of the list of wearables
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="wearablesId"></param>
        /// <param name="emoteIds"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async UniTask<(
            BodyWearables bodyWearables,
            List<WearableItem> wearables,
            List<WearableItem> emotes
            )> Curate(AvatarSettings settings, IEnumerable<string> wearablesId, IEnumerable<string> emoteIds, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                //Old flow contains emotes among the wearablesIds
                (List<WearableItem> wearableItems, List<WearableItem> emotes) = await wearableItemResolver.ResolveAndSplit(wearablesId, ct);

                HashSet<string> hiddenCategories = WearableItem.ComposeHiddenCategoriesOrdered(settings.bodyshapeId, settings.forceRender, wearableItems);

                //New emotes flow use the emotes catalog
                if (emoteIds != null)
                {
                    embedEmotes ??= await emotesCatalog.GetEmbeddedEmotes();
                    embedEmotesId ??= embedEmotes.GetAllIds();

                    DateTime startLoadTime = DateTime.Now;

                    var emoteIdsList = emoteIds.Select(ExtendedUrnParser.GetShortenedUrn).ToList();
                    IReadOnlyList<WearableItem> resolvedEmotes = await emotesCatalog.RequestEmotesAsync(emoteIdsList, ct);
                    List<WearableItem> nonPublishedEmotes = ListPool<WearableItem>.Get();

                    foreach (string nonPublishedEmoteId in emoteIdsList)
                    {
                        if (nonPublishedEmoteId.StartsWith("urn")) continue;
                        bool wasResolved = resolvedEmotes?.Any(item => item?.id == nonPublishedEmoteId) ?? false;
                        if (wasResolved) continue;
                        bool isEmbedded = embedEmotesId.Contains(nonPublishedEmoteId);
                        if (isEmbedded) continue;
                        WearableItem nonPublishedEmote = await emotesCatalog.RequestEmoteFromBuilderAsync(nonPublishedEmoteId, ct);
                        if (nonPublishedEmote != null)
                            nonPublishedEmotes.Add(nonPublishedEmote);
                    }

                    var loadTimeDelta = DateTime.Now - startLoadTime;

                    if (loadTimeDelta.TotalSeconds > 5)
                    {
                        //This error is good to have to detect too long load times early
                        Debug.LogError("Curate: emotes load time is too high: " + (DateTime.Now - startLoadTime));
                    }

                    //this filter is needed to make sure there will be no duplicates coming from two sources of emotes
                    var loadedEmotesFilter = new HashSet<string>();
                    emotes.ForEach(e => loadedEmotesFilter.Add(e.id));

                    if (resolvedEmotes != null)
                    {
                        foreach (var otherEmote in resolvedEmotes)
                            if (otherEmote != null)
                            {
                                if (loadedEmotesFilter.Contains(otherEmote.id))
                                    continue;

                                emotes.Add(otherEmote);
                            }
                    }

                    foreach (WearableItem emote in nonPublishedEmotes)
                    {
                        if (emote == null) continue;
                        if (loadedEmotesFilter.Contains(emote.id)) continue;
                        emotes.Add(emote);
                    }

                    ListPool<WearableItem>.Release(nonPublishedEmotes);
                }

                Dictionary<string, WearableItem> wearablesByCategory = new Dictionary<string, WearableItem>();

                for (int i = 0; i < wearableItems.Count; i++)
                {
                    WearableItem wearableItem = wearableItems[i];

                    // Ignore hidden categories
                    if (hiddenCategories.Contains(wearableItem.data.category))
                        continue;

                    // Avoid having two items with the same category.
                    if (wearableItem == null || wearablesByCategory.ContainsKey(wearableItem.data.category))
                        continue;

                    // Filter wearables without representation for the bodyshape
                    if (!wearableItem.TryGetRepresentation(settings.bodyshapeId, out var representation))
                        continue;

                    wearablesByCategory.Add(wearableItem.data.category, wearableItem);
                }

                if (!wearablesByCategory.ContainsKey(WearableLiterals.Categories.BODY_SHAPE))
                    throw new Exception("Set of wearables doesn't contain a bodyshape (or couldn't be resolved)");

                WearableItem[] fallbackWearables = await GetFallbackForMissingNeededCategories(settings.bodyshapeId, wearablesByCategory, hiddenCategories, ct);

                for (int i = 0; i < fallbackWearables.Length; i++)
                {
                    WearableItem wearableItem = fallbackWearables[i];

                    if (wearableItem == null)
                        throw new Exception($"Fallback wearable is null");

                    if (!wearableItem.TryGetRepresentation(settings.bodyshapeId, out var representation))
                        throw new Exception($"Fallback wearable {wearableItem} doesn't contain a representation for {settings.bodyshapeId}");

                    if (wearablesByCategory.ContainsKey(wearableItem.data.category))
                        throw new Exception($"A wearable in category {wearableItem.data.category} already exists trying to add fallback wearable {wearableItem}");

                    wearablesByCategory.Add(wearableItem.data.category, wearableItem);
                }

                // Wearables that are not bodyshape or facialFeatures
                List<WearableItem> wearables = wearablesByCategory.Where(
                                                                       x =>
                                                                           x.Key != WearableLiterals.Categories.BODY_SHAPE &&
                                                                           x.Key != WearableLiterals.Categories.EYES &&
                                                                           x.Key != WearableLiterals.Categories.EYEBROWS &&
                                                                           x.Key != WearableLiterals.Categories.MOUTH)
                                                                  .Select(x => x.Value)
                                                                  .ToList();

                var bodyWearables = new BodyWearables
                (
                    wearablesByCategory[WearableLiterals.Categories.BODY_SHAPE],
                    wearablesByCategory.TryGetValue(WearableLiterals.Categories.EYES, out WearableItem eyes) ? eyes : null,
                    wearablesByCategory.TryGetValue(WearableLiterals.Categories.EYEBROWS, out WearableItem eyebrows) ? eyebrows : null,
                    wearablesByCategory.TryGetValue(WearableLiterals.Categories.MOUTH, out WearableItem mouth) ? mouth : null
                );

                return (bodyWearables, wearables, emotes.ToList());
            }
            catch (OperationCanceledException)
            {
                //No Disposing required
                throw;
            }
            catch (Exception e)
            {
                Debug.Log("Failed curating avatar wearables");
                throw;
            }
        }

        private async UniTask<WearableItem[]> GetFallbackForMissingNeededCategories(string bodyshapeId, Dictionary<string, WearableItem> wearablesByCategory, HashSet<string> hiddenCategories, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                List<UniTask<WearableItem>> neededWearablesTasks = new List<UniTask<WearableItem>>();

                foreach (string neededCategory in WearableLiterals.Categories.REQUIRED_CATEGORIES)
                {
                    // If a needed category is hidden we dont need to fallback, we skipped it on purpose
                    if (hiddenCategories.Contains(neededCategory))
                        continue;

                    // The needed category is present
                    if (wearablesByCategory.ContainsKey(neededCategory))
                        continue;

                    string fallbackWearableId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyshapeId, neededCategory);

                    if (fallbackWearableId == null)
                        throw new Exception($"Couldn't find a fallback wearable for bodyshape: {bodyshapeId} and category {neededCategory}");

                    neededWearablesTasks.Add(wearableItemResolver.Resolve(fallbackWearableId, ct));
                }

                return await UniTask.WhenAll(neededWearablesTasks).AttachExternalCancellation(ct);
            }
            catch (OperationCanceledException)
            {
                //No disposing required
                throw;
            }
        }

        public void Dispose()
        {
            wearableItemResolver.Dispose();
        }
    }
}
