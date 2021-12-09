using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AvatarSystem
{
    public class Avatar : IAvatar
    {
        private readonly IWearableItemResolver wearableItemResolver;
        private readonly ILoader loader;
        private readonly IAnimator animator;
        private readonly IVisibility visibility;
        private readonly ILOD lod;

        private int lodIndex = 0;

        public IAvatar.Status status { get; private set; }

        public Avatar(IWearableItemResolver wearableItemResolver, ILoader loader, IAnimator animator, IVisibility visibility, ILOD lod)
        {
            this.wearableItemResolver = wearableItemResolver;
            this.loader = loader;
            this.animator = animator;
            this.visibility = visibility;
            this.lod = lod;
        }

        public async UniTask Load(List<string> wearablesIds, AvatarSettings settings, CancellationToken ct = default)
        {
            WearableItem[] wearableItems = await wearableItemResolver.Resolve(wearablesIds, ct);
            (WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables) = AvatarSystemUtils.SplitWearables(wearableItems);

            await loader.Load(bodyshape, eyes, eyebrows, mouth, wearables, settings, ct);

            if (loader.status == ILoader.Status.Failed_Mayor)
            {
                status = IAvatar.Status.Failed;
                return;
            }
            UnityEngine.Debug.Log("Hey");
            animator.Prepare(settings.bodyshapeId, loader.bodyshapeContainer);

            if (loader.status == ILoader.Status.Succeeded || loader.status == ILoader.Status.Failed_Minor)
                visibility.SetVisible(true);
            else
                visibility.SetVisible(false);

            lod.SetDependencies(loader.combinedRenderer, new [] { loader.eyesRenderer, loader.eyebrowsRenderer, loader.mouthRenderer });
            lod.SetLodIndex(lodIndex, true);
            status = IAvatar.Status.Loaded;
        }

        public void SetVisibility(bool visible) { visibility.SetVisible(visible); }
        public void SetExpression(string expressionId, long timestamps) { animator?.PlayExpression(expressionId, timestamps); }
        public void SetLODLevel(int lodIndex)
        {
            this.lodIndex = lodIndex;
            if (status != IAvatar.Status.Loaded)
                return;
            lod.SetLodIndex(lodIndex);
        }

        public void Dispose()
        {
            wearableItemResolver?.Dispose();
            loader?.Dispose();
            lod?.Dispose();
        }
    }
}