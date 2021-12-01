using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using UnityEngine;

namespace AvatarSystem
{
    public class Loader : ILoader
    {
        public GameObject combinedAvatar { get; }
        public GameObject[] facialFeatures { get; }

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

        public async UniTaskVoid Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, WearableItem[] wearables, AvatarSettings settings)
        {
            // TODO: Add cancellation token

            status = ILoader.Status.Loading;
            // TODO Reuse loaders with wearables that are already loaded
            ClearLoaders();

            // Get new loaders
            bodyshapeLoader = wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth);
            for (int i = 0; i < wearables.Length; i++)
            {
                WearableItem wearable = wearables[i];
                loaders.Add(wearable.data.category, wearableLoaderFactory.GetWearableLoader(wearable));
            }

            await bodyshapeLoader.Load(container, settings);
            if (bodyshapeLoader.status == IWearableLoader.Status.Failed)
            {
                status = ILoader.Status.Failed_Mayor;
                return;
            }

            await loaders.Values.Select(x => x.Load(container, settings));

            // Update Status accordingly
            status = ComposeStatus(loaders);

            if (status == ILoader.Status.Failed_Mayor)
            {
                ClearLoaders();
                return;
            }

            AvatarSystemUtils.CopyBones((SkinnedMeshRenderer)bodyshapeLoader.rendereable.renderers.First(), loaders.Values.SelectMany(x => x.rendereable.renderers).OfType<SkinnedMeshRenderer>());

            if (!MergeAvatar(container.GetComponentsInChildren<SkinnedMeshRenderer>()))
                status = ILoader.Status.Failed_Mayor;
            else
                status = ILoader.Status.Succeeded;
        }

        private bool MergeAvatar(IEnumerable<SkinnedMeshRenderer> allRenderers)
        {
            var renderersToCombine = allRenderers.Where((r) => !r.transform.parent.gameObject.name.Contains("Mask")).ToList();
            var featureFlags = DataStore.i.featureFlags.flags.Get();
            var avatarMeshCombiner = new AvatarMeshCombinerHelper();
            avatarMeshCombiner.useCullOpaqueHeuristic = featureFlags.IsFeatureEnabled("cull-opaque-heuristic");

            bool success = avatarMeshCombiner.Combine(bodyshapeLoader.upperBodyRenderer, renderersToCombine.ToArray());
            if (!success)
                return false;

            avatarMeshCombiner.container.transform.SetParent(container.transform, true);
            avatarMeshCombiner.container.transform.localPosition = Vector3.zero;

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

        public void Dispose() { ClearLoaders(); }
    }
}