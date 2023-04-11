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
        public List<Renderer> facialFeaturesRenderers { get; private set; }
        public ILoader.Status status { get; private set; } = ILoader.Status.Idle;

        private readonly IWearableLoaderFactory wearableLoaderFactory;
        private readonly GameObject container;

        internal IBodyshapeLoader bodyshapeLoader;
        internal readonly Dictionary<string, IWearableLoader> loaders = new Dictionary<string, IWearableLoader>();
        private readonly IAvatarMeshCombinerHelper avatarMeshCombiner;

        public Loader(IWearableLoaderFactory wearableLoaderFactory, GameObject container, IAvatarMeshCombinerHelper avatarMeshCombiner)
        {
            this.wearableLoaderFactory = wearableLoaderFactory;
            this.container = container;

            this.avatarMeshCombiner = avatarMeshCombiner;
            avatarMeshCombiner.prepareMeshForGpuSkinning = true;
            avatarMeshCombiner.uploadMeshToGpu = true;
        }

        public async UniTask Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings, SkinnedMeshRenderer bonesContainer = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<IWearableLoader> toCleanUp = new List<IWearableLoader>();
            try
            {
                status = ILoader.Status.Loading;
                await LoadBodyshape(settings, bodyshape, eyes, eyebrows, mouth, toCleanUp, cancellationToken);
                await LoadWearables(wearables, settings, toCleanUp, cancellationToken);
                SkinnedMeshRenderer skinnedContainer = bonesContainer == null ? bodyshapeLoader.upperBodyRenderer : bonesContainer;
                // Update Status accordingly
                status = ComposeStatus(loaders);
                if (status == ILoader.Status.Failed_Major)
                    throw new Exception($"Couldnt load (nor fallback) wearables with required category: {string.Join(", ", ConstructRequiredFailedWearablesList(loaders.Values))}");


                foreach (IWearableLoader wearableLoader in loaders.Values)
                {
                    wearableLoader.SetBones(skinnedContainer.rootBone, skinnedContainer.bones);
                }

                if (bodyshapeLoader.rendereable != null)
                {
                    bodyshapeLoader.SetBones(skinnedContainer.rootBone, skinnedContainer.bones);
                }

                (bool headVisible, bool upperBodyVisible, bool lowerBodyVisible, bool feetVisible) = AvatarSystemUtils.GetActiveBodyParts(settings.bodyshapeId, wearables);

                combinedRenderer = await MergeAvatar(settings, wearables, headVisible, upperBodyVisible, lowerBodyVisible, feetVisible, skinnedContainer, cancellationToken);
                facialFeaturesRenderers = new List<Renderer>();
                if (headVisible)
                {
                    if (eyes != null)
                        facialFeaturesRenderers.Add(bodyshapeLoader.eyesRenderer);
                    if (eyebrows != null)
                        facialFeaturesRenderers.Add(bodyshapeLoader.eyebrowsRenderer);
                    if (mouth != null)
                        facialFeaturesRenderers.Add(bodyshapeLoader.mouthRenderer);
                }
                else
                {
                    if(bodyshapeLoader != null)
                        bodyshapeLoader.DisableFacialRenderers();
                }
            }
            catch (OperationCanceledException)
            {
                Dispose();
                throw;
            }
            catch
            {
                Dispose();
                Debug.Log("Failed Loading avatar");
                throw;
            }
            finally
            {
                for (int i = 0; i < toCleanUp.Count; i++)
                {
                    if (toCleanUp[i] == null)
                        continue;
                    toCleanUp[i].Dispose();
                }
            }
        }

        private static List<string> ConstructRequiredFailedWearablesList(IEnumerable<IWearableLoader> loaders)
        {
            return loaders.Where(x => x.status == IWearableLoader.Status.Failed && AvatarSystemUtils.IsCategoryRequired(x.wearable.data.category))
                          .Select(x => x.wearable.id)
                          .ToList();
        }

        private async UniTask LoadBodyshape(AvatarSettings settings, WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<IWearableLoader> loadersToCleanUp, CancellationToken ct)
        {
            //We get a new loader if any of the subparts of the bodyshape changes
            if (!IsValidForBodyShape(bodyshape, eyes, eyebrows, mouth))
            {
                loadersToCleanUp.Add(bodyshapeLoader);
                bodyshapeLoader = wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth);
            }

            await bodyshapeLoader.Load(container, settings, ct);

            if (bodyshapeLoader.status == IWearableLoader.Status.Failed)
            {
                status = ILoader.Status.Failed_Major;
                throw new Exception($"Couldnt load bodyshape");
            }
        }

        private async UniTask LoadWearables(List<WearableItem> wearables, AvatarSettings settings, List<IWearableLoader> loadersToCleanUp, CancellationToken ct)
        {
            (List<IWearableLoader> notReusableLoaders, List<IWearableLoader> newLoaders) = GetNewLoaders(wearables, loaders, wearableLoaderFactory);
            loadersToCleanUp.AddRange(notReusableLoaders);
            loaders.Clear();
            for (int i = 0; i < newLoaders.Count; i++)
            {
                IWearableLoader loader = newLoaders[i];
                loaders.Add(loader.wearable.data.category, loader);
            }

            await UniTask.WhenAll(loaders.Values.Select(x => x.Load(container, settings, ct)));
        }

        internal static (List<IWearableLoader> notReusableLoaders, List<IWearableLoader> newLoaders) GetNewLoaders(List<WearableItem> wearables, Dictionary<string, IWearableLoader> currentLoaders, IWearableLoaderFactory wearableLoaderFactory)
        {
            // Initialize with all loaders and remove from cleaning-up the ones that can be reused
            List<IWearableLoader> notReusableLoaders = new List<IWearableLoader>(currentLoaders.Values);
            List<IWearableLoader> newLoaders = new List<IWearableLoader>();

            for (int i = 0; i < wearables.Count; i++)
            {
                WearableItem wearable = wearables[i];

                if (currentLoaders.TryGetValue(wearable.data.category, out IWearableLoader loader))
                {
                    //We can reuse this loader
                    if (loader.wearable.id == wearable.id)
                    {
                        newLoaders.Add(loader);
                        notReusableLoaders.Remove(loader);
                        continue;
                    }
                }
                newLoaders.Add(wearableLoaderFactory.GetWearableLoader(wearable));
            }

            return (notReusableLoaders, newLoaders);
        }

        public Transform[] GetBones() { return bodyshapeLoader?.upperBodyRenderer?.bones; }

        public bool IsValidForBodyShape(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth)
        {
            return bodyshapeLoader != null && bodyshapeLoader.IsValid(bodyshape, eyebrows, eyes, mouth);
        }

        private async UniTask<SkinnedMeshRenderer> MergeAvatar(AvatarSettings settings, List<WearableItem> wearables,
            bool headVisible, bool upperBodyVisible, bool lowerBodyVisible, bool feetVisible, SkinnedMeshRenderer bonesContainer,
            CancellationToken ct)
        {
            var activeBodyParts = AvatarSystemUtils.GetActiveBodyPartsRenderers(bodyshapeLoader, headVisible, upperBodyVisible, lowerBodyVisible, feetVisible);
            IEnumerable<SkinnedMeshRenderer> allRenderers = activeBodyParts.Union(loaders.Values.SelectMany(x => x.rendereable.renderers.OfType<SkinnedMeshRenderer>()));

            // AvatarMeshCombiner is a bit buggy when performing the combine of the same meshes on the same frame,
            // once that's fixed we can remove this wait
            // AttachExternalCancellation is needed because cancellation will take a wait to trigger
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct).AttachExternalCancellation(ct);
            avatarMeshCombiner.useCullOpaqueHeuristic = true;
            avatarMeshCombiner.enableCombinedMesh = false;
            bool success = avatarMeshCombiner.Combine(bonesContainer, allRenderers.ToArray());
            if (!success)
            {
                status = ILoader.Status.Failed_Major;
                throw new Exception("Couldnt merge avatar");
            }
            avatarMeshCombiner.container.transform.SetParent(container.transform, true);
            avatarMeshCombiner.container.transform.localPosition = Vector3.zero;
            avatarMeshCombiner.container.transform.localScale = Vector3.one;
            return avatarMeshCombiner.renderer;
        }

        internal static ILoader.Status ComposeStatus(Dictionary<string, IWearableLoader> loaders)
        {
            ILoader.Status composedStatus = ILoader.Status.Succeeded;
            foreach ((string category, IWearableLoader loader) in loaders)
            {
                if (loader.status == IWearableLoader.Status.Defaulted)
                    composedStatus = ILoader.Status.Failed_Minor;
                else if (loader.status == IWearableLoader.Status.Failed)
                {
                    if (AvatarSystemUtils.IsCategoryRequired(category))
                        return ILoader.Status.Failed_Major;
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
            combinedRenderer = null;
            status = ILoader.Status.Idle;
            ClearLoaders();
        }
    }
}
