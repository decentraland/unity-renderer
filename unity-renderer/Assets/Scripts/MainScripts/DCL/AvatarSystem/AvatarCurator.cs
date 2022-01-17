using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace AvatarSystem
{
    public class AvatarCurator : IAvatarCurator
    {
        private readonly IWearableItemResolver wearableItemResolver;

        public AvatarCurator(IWearableItemResolver wearableItemResolver)
        {
            Assert.IsNotNull(wearableItemResolver);
            this.wearableItemResolver = wearableItemResolver;
        }

        public async UniTask<(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables)> Curate(string bodyshapeId, IEnumerable<string> wearablesId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                WearableItem[] wearableItems =  await wearableItemResolver.Resolve(wearablesId, ct);
                HashSet<string> hiddenCategories = WearableItem.ComposeHiddenCategories(bodyshapeId, wearableItems);

                Dictionary<string, WearableItem> wearablesByCategory = new Dictionary<string, WearableItem>();
                for (int i = 0; i < wearableItems.Length; i++)
                {
                    WearableItem wearableItem = wearableItems[i];

                    // Ignore hidden categories
                    if (hiddenCategories.Contains(wearableItem.data.category))
                        continue;

                    // Avoid having two items with the same category.
                    if (wearableItem == null || wearablesByCategory.ContainsKey(wearableItem.data.category) )
                        continue;

                    // Filter wearables without representation for the bodyshape
                    if (!wearableItem.TryGetRepresentation(bodyshapeId, out var representation))
                        continue;

                    wearablesByCategory.Add(wearableItem.data.category, wearableItem);
                }

                WearableItem[] fallbackWearables = await GetFallbackForMissingNeededCategories(bodyshapeId, wearablesByCategory, hiddenCategories, ct);

                for (int i = 0; i < fallbackWearables.Length; i++)
                {
                    WearableItem wearableItem = fallbackWearables[i];
                    if (wearableItem == null)
                        throw new Exception($"Fallback wearable is null");
                    if (!wearableItem.TryGetRepresentation(bodyshapeId, out var representation))
                        throw new Exception($"Fallback wearable {wearableItem} doesn't contain a representation for {bodyshapeId}");
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

                return (
                    wearablesByCategory[WearableLiterals.Categories.BODY_SHAPE],
                    wearablesByCategory[WearableLiterals.Categories.EYES],
                    wearablesByCategory[WearableLiterals.Categories.EYEBROWS],
                    wearablesByCategory[WearableLiterals.Categories.MOUTH],
                    wearables
                );
            }
            catch (OperationCanceledException)
            {
                //No Disposing required
                throw;
            }
            catch
            {
                Debug.LogError("Failed curating avatar wearables");
                throw;
            }
        }

        private async UniTask<WearableItem[]> GetFallbackForMissingNeededCategories(string bodyshapeId, Dictionary<string, WearableItem> wearablesByCategory, HashSet<string> hiddenCategories , CancellationToken ct)
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

        public void Dispose() { wearableItemResolver.Dispose(); }
    }
}