using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AvatarSystem
{
    public class Avatar : IAvatar
    {
        private readonly IWearableItemResolver wearableItemResolver;
        private readonly ILoader loader;
        private readonly IAnimator animator;
        private readonly IVisibility visibility;

        public Avatar(IWearableItemResolver wearableItemResolver, ILoader loader, IAnimator animator, IVisibility visibility)
        {
            this.wearableItemResolver = wearableItemResolver;
            this.loader = loader;
            this.animator = animator;
            this.visibility = visibility;
        }

        public async UniTask Load(List<string> wearablesIds, AvatarSettings settings, CancellationToken ct = default)
        {
            WearableItem[] wearableItems = await wearableItemResolver.Resolve(wearablesIds, ct);
            (WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables) = AvatarSystemUtils.SplitWearables(wearableItems);

            await loader.Load(bodyshape, eyes, eyebrows, mouth, wearables, settings, ct);

            animator.Prepare(settings.bodyshapeId, loader.bodyshapeContainer);

            if (loader.status == ILoader.Status.Succeeded || loader.status == ILoader.Status.Failed_Minor)
                visibility.SetVisible(true);
            else
                visibility.SetVisible(false);
        }

        public void SetVisibility(bool visible) { visibility.SetVisible(visible); }
        public void SetExpression(string expressionId, long timestamps) {  }

        public void Dispose()
        {
            wearableItemResolver?.Dispose();
            loader?.Dispose();
        }
    }
}