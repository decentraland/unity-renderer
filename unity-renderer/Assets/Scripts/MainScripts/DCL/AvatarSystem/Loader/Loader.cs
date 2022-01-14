using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using UnityEngine;

namespace AvatarSystem
{
    public class Loader : ILoader
    {
        public GameObject bodyshapeContainer => bodyshapeLoader?.rendereable?.container;
        public SkinnedMeshRenderer combinedRenderer { get; private set; }
        public Renderer eyesRenderer { get; private set; }
        public Renderer eyebrowsRenderer { get; private set; }
        public Renderer mouthRenderer { get; private set; }
        public ILoader.Status status { get; private set; } = ILoader.Status.Idle;

        private readonly IWearableLoaderFactory wearableLoaderFactory;
        private readonly GameObject container;

        private IBodyshapeLoader bodyshapeLoader;
        private readonly Dictionary<string, IWearableLoader> loaders = new Dictionary<string, IWearableLoader>();
        private readonly AvatarMeshCombinerHelper avatarMeshCombiner;

        public Loader(IWearableLoaderFactory wearableLoaderFactory, GameObject container)
        {
            this.wearableLoaderFactory = wearableLoaderFactory;
            this.container = container;

            avatarMeshCombiner = new AvatarMeshCombinerHelper();
            avatarMeshCombiner.prepareMeshForGpuSkinning = true;
            avatarMeshCombiner.uploadMeshToGpu = true;
        }

        public async UniTask Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            List<IWearableLoader> toCleanUp = new List<IWearableLoader>();

            void DisposeCleanUpLoaders()
            {
                for (int i = 0; i < toCleanUp.Count; i++)
                {
                    if (toCleanUp[i] == null)
                        continue;
                    toCleanUp[i].Dispose();
                }
            }

            try
            {
                status = ILoader.Status.Loading;

                if (bodyshapeLoader == null || bodyshapeLoader.wearable.id != bodyshape.id || bodyshapeLoader.eyes.id != eyes.id || bodyshapeLoader.eyebrows.id != eyebrows.id || bodyshapeLoader.mouth.id != mouth.id)
                {
                    toCleanUp.Add(bodyshapeLoader);
                    bodyshapeLoader = wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth);
                }

                await bodyshapeLoader.Load(container, settings, ct);

                if (bodyshapeLoader.status == IWearableLoader.Status.Failed)
                {
                    status = ILoader.Status.Failed_Mayor;
                    throw new Exception($"Couldnt load bodyshape");
                }

                // Mark for cleanUp unneeded loaders
                List<string> unnededCategories = new List<string>();
                foreach ((string category, IWearableLoader existentLoader) in loaders)
                {
                    if (!wearables.Contains(existentLoader.wearable))
                    {
                        toCleanUp.Add(existentLoader);
                        unnededCategories.Add(category);
                    }
                }
                for (int index = 0; index < unnededCategories.Count; index++)
                {
                    loaders.Remove(unnededCategories[index]);
                }

                // Get loaders for the new set of wearables (reusing current ones already on use)
                for (int i = 0; i < wearables.Count; i++)
                {
                    WearableItem wearable = wearables[i];
                    IWearableLoader loader = null;
                    if (loaders.TryGetValue(wearable.data.category, out IWearableLoader existentLoader))
                    {
                        loaders.Remove(wearable.data.category);
                        if (existentLoader.wearable.id == wearable.id)
                            loader = existentLoader;
                        else
                            toCleanUp.Add(existentLoader);
                    }
                    if (loader == null)
                        loader = wearableLoaderFactory.GetWearableLoader(wearable);
                    loaders.Add(wearable.data.category, loader);
                }

                await UniTask.WhenAll(loaders.Values.Select(x => x.Load(container, settings, ct)));

                // Update Status accordingly
                status = ComposeStatus(loaders);
                if (status == ILoader.Status.Failed_Mayor)
                {
                    List<string> failedWearables = loaders.Values
                                                          .Where(x => x.status == IWearableLoader.Status.Failed && AvatarSystemUtils.IsCategoryRequired(x.wearable.data.category))
                                                          .Select(x => x.wearable.id)
                                                          .ToList();

                    throw new Exception($"Couldnt load (nor fallback) wearables with required category: {string.Join(", ", failedWearables)}");
                }

                AvatarSystemUtils.CopyBones(bodyshapeLoader.upperBodyRenderer, loaders.Values.SelectMany(x => x.rendereable.renderers).OfType<SkinnedMeshRenderer>());
                (bool headVisible, bool upperBodyVisible, bool lowerBodyVisible, bool feetVisible) = AvatarSystemUtils.GetActiveBodyParts(bodyshape.id, wearables);
                var activeBodyParts = AvatarSystemUtils.GetActiveBodyPartsRenderers(bodyshapeLoader, headVisible, upperBodyVisible, lowerBodyVisible, feetVisible);

                // AvatarMeshCombiner is a bit buggy when performing the combine of the same meshes on the same frame,
                // once that's fixed we can remove this wait
                // AttachExternalCancellation is needed because cancellation will take a wait to trigger
                await UniTask.WaitForEndOfFrame(ct).AttachExternalCancellation(ct);

                if (!MergeAvatar(activeBodyParts.Union(loaders.Values.SelectMany(x => x.rendereable.renderers.OfType<SkinnedMeshRenderer>())), out SkinnedMeshRenderer combinedRenderer))
                {
                    status = ILoader.Status.Failed_Mayor;
                    throw new Exception("Couldnt merge avatar");
                }

                this.combinedRenderer = combinedRenderer;
                eyesRenderer = bodyshapeLoader.eyesRenderer;
                eyebrowsRenderer = bodyshapeLoader.eyebrowsRenderer;
                mouthRenderer = bodyshapeLoader.mouthRenderer;
            }
            catch (OperationCanceledException)
            {
                Dispose();
                throw;
            }
            catch
            {
                Dispose();
                Debug.LogError("Failed Loading avatar");
                throw;
            }
            finally
            {
                DisposeCleanUpLoaders();
            }
        }

        public Transform[] GetBones() { return bodyshapeLoader?.upperBodyRenderer?.bones; }

        private bool MergeAvatar(IEnumerable<SkinnedMeshRenderer> allRenderers, out SkinnedMeshRenderer renderer)
        {
            renderer = null;
            var featureFlags = DataStore.i.featureFlags.flags.Get();
            avatarMeshCombiner.useCullOpaqueHeuristic = featureFlags.IsFeatureEnabled("cull-opaque-heuristic");
            avatarMeshCombiner.enableCombinedMesh = false;

            bool success = avatarMeshCombiner.Combine(bodyshapeLoader.upperBodyRenderer, allRenderers.ToArray());
            if (!success)
                return false;

            avatarMeshCombiner.container.transform.SetParent(container.transform, true);
            avatarMeshCombiner.container.transform.localPosition = Vector3.zero;

            renderer = avatarMeshCombiner.renderer;
            return true;
        }

        private static ILoader.Status ComposeStatus(Dictionary<string, IWearableLoader> loaders)
        {
            ILoader.Status composedStatus = ILoader.Status.Succeeded;
            foreach ((string category, IWearableLoader loader) in loaders)
            {
                if (loader.status == IWearableLoader.Status.Defaulted)
                    composedStatus = ILoader.Status.Failed_Minor;
                else if (loader.status == IWearableLoader.Status.Failed)
                {
                    if (AvatarSystemUtils.IsCategoryRequired(category))
                        return ILoader.Status.Failed_Mayor;
                    composedStatus = ILoader.Status.Failed_Minor;
                }
            }
            return composedStatus;
        }

        private void ClearLoaders()
        {
            bodyshapeLoader?.Dispose();
            foreach (IWearableLoader wearableLoader in loaders.Values)
            {
                wearableLoader.Dispose();
            }
            loaders.Clear();
        }

        public void Dispose()
        {
            avatarMeshCombiner.Dispose();
            status = ILoader.Status.Idle;
            ClearLoaders();
        }
    }
}