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

        public async UniTask Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings, CancellationToken ct = default)
        {
            await Load(bodyshape, eyes, eyebrows, mouth, wearables, settings, null, ct);
        }

        public async UniTask Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings, SkinnedMeshRenderer bonesContainer = null, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            List<IWearableLoader> toCleanUp = new List<IWearableLoader>();
            try
            {
                Debug.Log("AA");
                status = ILoader.Status.Loading;
                Debug.Log("AB");
                await LoadBodyshape(settings, bodyshape, eyes, eyebrows, mouth, toCleanUp, ct);
                Debug.Log("AC");
                await LoadWearables(wearables, settings, toCleanUp, ct);
                Debug.Log("AD");
                SkinnedMeshRenderer skinnedContainer = bonesContainer == null ? bodyshapeLoader.upperBodyRenderer : bonesContainer;
                Debug.Log("AF");
                // Update Status accordingly
                status = ComposeStatus(loaders);
                Debug.Log("AG");

                if (status == ILoader.Status.Failed_Major)
                    throw new Exception($"Couldnt load (nor fallback) wearables with required category: {string.Join(", ", ConstructRequiredFailedWearablesList(loaders.Values))}");
                Debug.Log("AH");

                AvatarSystemUtils.CopyBones(skinnedContainer, loaders.Values.SelectMany(x => x.rendereable.renderers).OfType<SkinnedMeshRenderer>());
                Debug.Log("AI");

                if (bodyshapeLoader.rendereable != null)
                    AvatarSystemUtils.CopyBones(skinnedContainer, bodyshapeLoader.rendereable.renderers.OfType<SkinnedMeshRenderer>());
                Debug.Log("AJ");
                (bool headVisible, bool upperBodyVisible, bool lowerBodyVisible, bool feetVisible) = AvatarSystemUtils.GetActiveBodyParts(settings.bodyshapeId, wearables);
                Debug.Log("AK");
                combinedRenderer = await MergeAvatar(settings, wearables, headVisible, upperBodyVisible, lowerBodyVisible, feetVisible, skinnedContainer, ct);
                Debug.Log("AL");
                facialFeaturesRenderers = new List<Renderer>();
                Debug.Log("AM");
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
                Debug.Log("AN");
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
            Debug.Log("BA");
            //We get a new loader if any of the subparts of the bodyshape changes
            if (!IsValidForBodyShape(bodyshape, eyes, eyebrows, mouth))
            {
                loadersToCleanUp.Add(bodyshapeLoader);
                Debug.Log("BC");

                bodyshapeLoader = wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth);
                Debug.Log("BD");

            }
            Debug.Log("BE");

            await bodyshapeLoader.Load(container, settings, ct);
            Debug.Log("BF");

            if (bodyshapeLoader.status == IWearableLoader.Status.Failed)
            {
                status = ILoader.Status.Failed_Major;
                throw new Exception($"Couldnt load bodyshape");
            }
            Debug.Log("BG");

        }

        private async UniTask LoadWearables(List<WearableItem> wearables, AvatarSettings settings, List<IWearableLoader> loadersToCleanUp, CancellationToken ct)
        {
            Debug.Log("CC");

            (List<IWearableLoader> notReusableLoaders, List<IWearableLoader> newLoaders) = GetNewLoaders(wearables, loaders, wearableLoaderFactory);
            Debug.Log("CD");

            loadersToCleanUp.AddRange(notReusableLoaders);
            Debug.Log("CE");

            loaders.Clear();
            Debug.Log("CF");

            for (int i = 0; i < newLoaders.Count; i++)
            {
                Debug.Log("CG");

                IWearableLoader loader = newLoaders[i];
                Debug.Log("CH");

                loaders.Add(loader.wearable.data.category, loader);
                Debug.Log("CI");

            }
            Debug.Log("CJ");

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
            Debug.Log("DA");
            var activeBodyParts = AvatarSystemUtils.GetActiveBodyPartsRenderers(bodyshapeLoader, headVisible, upperBodyVisible, lowerBodyVisible, feetVisible);
            Debug.Log("DB");
            IEnumerable<SkinnedMeshRenderer> allRenderers = activeBodyParts.Union(loaders.Values.SelectMany(x => x.rendereable.renderers.OfType<SkinnedMeshRenderer>()));
            Debug.Log("DC");
            // AvatarMeshCombiner is a bit buggy when performing the combine of the same meshes on the same frame,
            // once that's fixed we can remove this wait
            // AttachExternalCancellation is needed because cancellation will take a wait to trigger
            Debug.Log("DE");
            await UniTask.WaitForEndOfFrame(ct).AttachExternalCancellation(ct);
            Debug.Log("DF");
            var featureFlags = DataStore.i.featureFlags.flags.Get();
            Debug.Log("DG");
            avatarMeshCombiner.useCullOpaqueHeuristic = true;
            Debug.Log("DH");
            avatarMeshCombiner.enableCombinedMesh = false;
            Debug.Log("DI");
            bool success = avatarMeshCombiner.Combine(bonesContainer, allRenderers.ToArray());
            Debug.Log("DJ");
            if (!success)
            {
                status = ILoader.Status.Failed_Major;
                throw new Exception("Couldnt merge avatar");
            }
            Debug.Log("DK");
            avatarMeshCombiner.container.transform.SetParent(container.transform, true);
            Debug.Log("DL");
            avatarMeshCombiner.container.transform.localPosition = Vector3.zero;
            Debug.Log("DM");
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
            status = ILoader.Status.Idle;
            ClearLoaders();
        }
    }
}