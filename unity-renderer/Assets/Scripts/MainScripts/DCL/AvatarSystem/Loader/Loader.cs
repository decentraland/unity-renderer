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
            //TODO rethink cancellation and handle it gracefully
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            status = ILoader.Status.Loading;

            List<IWearableLoader> toCleanUp = new List<IWearableLoader>();

            if (bodyshapeLoader == null || !bodyshapeLoader.IsValidFor(bodyshape))
            {
                toCleanUp.Add(bodyshapeLoader);
                bodyshapeLoader = wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth);
            }
            await bodyshapeLoader.Load(container, settings, ct);
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            if (bodyshapeLoader.status == IWearableLoader.Status.Failed)
            {
                status = ILoader.Status.Failed_Mayor;
                return;
            }

            for (int i = 0; i < wearables.Count; i++)
            {
                WearableItem wearable = wearables[i];
                IWearableLoader loader = null;
                if (loaders.TryGetValue(wearable.data.category, out IWearableLoader existentLoader))
                {
                    loaders.Remove(wearable.data.category);
                    if (existentLoader.IsValidFor(wearable))
                        loader = existentLoader;
                    else
                        toCleanUp.Add(existentLoader);
                }
                loader ??= wearableLoaderFactory.GetWearableLoader(wearable);
                loaders.Add(wearable.data.category, loader);
            }

            await UniTask.WhenAll(loaders.Values.Select(x => x.Load(container, settings, ct)));
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            // Update Status accordingly
            status = ComposeStatus(loaders);

            if (status == ILoader.Status.Failed_Mayor)
            {
                //TODO Dispose properly
                return;
            }

            AvatarSystemUtils.CopyBones(bodyshapeLoader.upperBodyRenderer, loaders.Values.SelectMany(x => x.rendereable.renderers).OfType<SkinnedMeshRenderer>());
            (bool headVisible, bool upperBodyVisible, bool lowerBodyVisible, bool feetVisible) = AvatarSystemUtils.GetActiveBodyParts(bodyshape.id, wearables);
            var activeBodyParts = AvatarSystemUtils.GetActiveBodyPartsRenderers(bodyshapeLoader, headVisible, upperBodyVisible, lowerBodyVisible, feetVisible);

            for (int i = 0; i < toCleanUp.Count; i++)
            {
                toCleanUp[i]?.Dispose();
            }

            // AvatarMeshCombiner is a bit buggy when performing the combine of the same meshes on the same frame,
            // once that's fixed we can remove this wait
            await UniTask.WaitForEndOfFrame(ct);
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            if (!MergeAvatar(activeBodyParts.Union(loaders.Values.SelectMany(x => x.rendereable.renderers.OfType<SkinnedMeshRenderer>())), out SkinnedMeshRenderer combinedRenderer))
            {
                status = ILoader.Status.Failed_Mayor;
                //TODO Dispose properly
                return;
            }

            this.combinedRenderer = combinedRenderer;
            eyesRenderer = bodyshapeLoader.eyesRenderer;
            eyebrowsRenderer = bodyshapeLoader.eyebrowsRenderer;
            mouthRenderer = bodyshapeLoader.mouthRenderer;

            this.combinedRenderer.enabled = true;
            eyesRenderer.enabled = true;
            eyebrowsRenderer.enabled = true;
            mouthRenderer.enabled = true;
            container.SetActive(false);
        }

        private bool MergeAvatar(IEnumerable<SkinnedMeshRenderer> allRenderers, out SkinnedMeshRenderer renderer)
        {
            renderer = null;
            var renderersToCombine = allRenderers.Where((r) => !r.transform.parent.gameObject.name.Contains("Mask")).ToList();
            var featureFlags = DataStore.i.featureFlags.flags.Get();
            avatarMeshCombiner.useCullOpaqueHeuristic = featureFlags.IsFeatureEnabled("cull-opaque-heuristic");

            bool success = avatarMeshCombiner.Combine(bodyshapeLoader.upperBodyRenderer, renderersToCombine.ToArray());
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