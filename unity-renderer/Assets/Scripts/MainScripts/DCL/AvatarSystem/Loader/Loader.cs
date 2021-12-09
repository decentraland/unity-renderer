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
        public Renderer combinedRenderer { get; private set; }
        public Renderer eyesRenderer { get; private set; }
        public Renderer eyebrowsRenderer { get; private set; }
        public Renderer mouthRenderer { get; private set; }

        private readonly IWearableLoaderFactory wearableLoaderFactory;
        private readonly GameObject container;

        private IBodyshapeLoader bodyshapeLoader;
        private readonly Dictionary<string, IWearableLoader> loaders = new Dictionary<string, IWearableLoader>();

        public ILoader.Status status { get; private set; } = ILoader.Status.Idle;

        public Loader(IWearableLoaderFactory wearableLoaderFactory, GameObject container)
        {
            this.wearableLoaderFactory = wearableLoaderFactory;
            this.container = container;
        }

        public async UniTask Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            status = ILoader.Status.Loading;
            // TODO Reuse loaders with wearables that are already loaded
            ClearLoaders();

            // Get new loaders
            bodyshapeLoader = wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth);
            for (int i = 0; i < wearables.Count; i++)
            {
                WearableItem wearable = wearables[i];
                loaders.Add(wearable.data.category, wearableLoaderFactory.GetWearableLoader(wearable));
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

            if (!MergeAvatar(activeBodyParts.Union(loaders.Values.SelectMany(x => x.rendereable.renderers.OfType<SkinnedMeshRenderer>())), out Renderer combinedRenderer))
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

        private bool MergeAvatar(IEnumerable<SkinnedMeshRenderer> allRenderers, out Renderer renderer)
        {
            renderer = null;
            var renderersToCombine = allRenderers.Where((r) => !r.transform.parent.gameObject.name.Contains("Mask")).ToList();
            var featureFlags = DataStore.i.featureFlags.flags.Get();
            var avatarMeshCombiner = new AvatarMeshCombinerHelper();
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
            status = ILoader.Status.Idle;
            ClearLoaders();
        }
    }
}