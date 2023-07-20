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
        public IReadOnlyList<SkinnedMeshRenderer> originalVisibleRenderers  => originalVisibleRenderersValue;
        public List<SkinnedMeshRenderer> originalVisibleRenderersValue { get; private set; } = new List<SkinnedMeshRenderer>();
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

        public async UniTask Load(BodyWearables bodyWearables, List<WearableItem> wearables, AvatarSettings settings, SkinnedMeshRenderer bonesContainer = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<IWearableLoader> toCleanUp = new List<IWearableLoader>();
            try
            {
                status = ILoader.Status.Loading;
                await LoadBodyshape(settings, bodyWearables, toCleanUp, cancellationToken);
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

                var activeBodyParts = AvatarSystemUtils.GetActiveBodyPartsRenderers(bodyshapeLoader, settings.bodyshapeId, wearables);
                originalVisibleRenderersValue = activeBodyParts.Union(loaders.Values.SelectMany(x => x.rendereable.renderers.OfType<SkinnedMeshRenderer>())).ToList();
                combinedRenderer = await MergeAvatar(originalVisibleRenderersValue, skinnedContainer, cancellationToken);
                facialFeaturesRenderers = new List<Renderer>();

                if (activeBodyParts.Contains(bodyshapeLoader.headRenderer))
                {
                    if (bodyWearables.Eyes != null)
                    {
                        facialFeaturesRenderers.Add(bodyshapeLoader.eyesRenderer);
                        originalVisibleRenderersValue.Add(bodyshapeLoader.eyesRenderer);
                    }
                    if (bodyWearables.Eyebrows != null)
                    {
                        facialFeaturesRenderers.Add(bodyshapeLoader.eyebrowsRenderer);
                        originalVisibleRenderersValue.Add(bodyshapeLoader.eyebrowsRenderer);
                    }
                    if (bodyWearables.Mouth != null)
                    {
                        facialFeaturesRenderers.Add(bodyshapeLoader.mouthRenderer);
                        originalVisibleRenderersValue.Add(bodyshapeLoader.mouthRenderer);
                    }
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
            return loaders.Where(x => x.status == IWearableLoader.Status.Failed && AvatarSystemUtils.IsCategoryRequired(x.bodyShape.data.category))
                          .Select(x => x.bodyShape.id)
                          .ToList();
        }

        private async UniTask LoadBodyshape(AvatarSettings settings, BodyWearables bodyWearables, List<IWearableLoader> loadersToCleanUp, CancellationToken ct)
        {
            //We get a new loader if any of the subparts of the bodyshape changes
            if (!IsValidForBodyShape(bodyWearables))
            {
                loadersToCleanUp.Add(bodyshapeLoader);
                bodyshapeLoader = wearableLoaderFactory.GetBodyShapeLoader(bodyWearables);
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
                loaders.Add(loader.bodyShape.data.category, loader);
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
                    if (loader.bodyShape.id == wearable.id)
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

        public Transform[] GetBones() =>
            bodyshapeLoader?.upperBodyRenderer?.bones;

        public bool IsValidForBodyShape(BodyWearables bodyWearables) =>
            bodyshapeLoader != null && bodyshapeLoader.IsValid(bodyWearables);

        private async UniTask<SkinnedMeshRenderer> MergeAvatar(List<SkinnedMeshRenderer> renderers, SkinnedMeshRenderer bonesContainer,
            CancellationToken ct)
        {
            // AvatarMeshCombiner is a bit buggy when performing the combine of the same meshes on the same frame,
            // once that's fixed we can remove this wait
            // AttachExternalCancellation is needed because cancellation will take a wait to trigger
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct).AttachExternalCancellation(ct);
            avatarMeshCombiner.useCullOpaqueHeuristic = true;
            avatarMeshCombiner.enableCombinedMesh = false;
            bool success = avatarMeshCombiner.Combine(bonesContainer, renderers);
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
