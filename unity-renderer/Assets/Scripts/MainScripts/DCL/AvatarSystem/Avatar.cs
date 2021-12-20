using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GPUSkinning;
using UnityEngine;

namespace AvatarSystem
{
    public class Avatar : IAvatar
    {
        private readonly IAvatarCurator avatarCurator;
        private readonly ILoader loader;
        private readonly IAnimator animator;
        private readonly IVisibility visibility;
        private readonly ILOD lod;
        private readonly IGPUSkinning gpuSkinning;
        private readonly IGPUSkinningThrottler gpuSkinningThrottler;

        private bool visible = false;
        private int lodIndex = 0;

        public IAvatar.Status status { get; private set; }
        public Bounds bounds { get; }

        public Avatar(IAvatarCurator avatarCurator, ILoader loader, IAnimator animator, IVisibility visibility, ILOD lod, IGPUSkinning gpuSkinning, IGPUSkinningThrottler gpuSkinningThrottler)
        {
            this.avatarCurator = avatarCurator;
            this.loader = loader;
            this.animator = animator;
            this.visibility = visibility;
            this.lod = lod;
            this.gpuSkinning = gpuSkinning;
            this.gpuSkinningThrottler = gpuSkinningThrottler;
        }

        public async UniTask Load(List<string> wearablesIds, AvatarSettings settings, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
                return;

            WearableItem bodyshape = null;
            WearableItem eyes = null;
            WearableItem eyebrows = null;
            WearableItem mouth = null;
            List<WearableItem> wearables = null;
            try
            {
                (bodyshape, eyes, eyebrows, mouth, wearables) = await avatarCurator.Curate(settings.bodyshapeId , wearablesIds, ct);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed curating avatar with wearables:[{string.Join(",", wearablesIds)}] for bodyshape:{settings.bodyshapeId}");
                Debug.LogError(e.ToString());
                throw;
            }

            //TODO Maybe we can include GPUSkinning in the loader but the throttling is a problem then.
            await loader.Load(bodyshape, eyes, eyebrows, mouth, wearables, settings, ct);

            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            if (loader.status == ILoader.Status.Failed_Mayor || loader.status == ILoader.Status.Idle)
            {
                status = IAvatar.Status.Failed;
                return;
            }

            //TODO GPUSkinning only has to be prepared when the loader has changes
            gpuSkinning.Prepare(loader.combinedRenderer);
            gpuSkinningThrottler.Bind(gpuSkinning);
            animator.Prepare(settings.bodyshapeId, loader.bodyshapeContainer);

            if (loader.status == ILoader.Status.Succeeded || loader.status == ILoader.Status.Failed_Minor)
                visibility.SetVisible(visible);
            else
                visibility.SetVisible(false);

            lod.Bind(gpuSkinning.renderer, new [] { loader.eyesRenderer, loader.eyebrowsRenderer, loader.mouthRenderer });
            lod.SetLodIndex(lodIndex, true);
            gpuSkinningThrottler.SetThrottling(lodIndex + 1);
            gpuSkinningThrottler.Start();

            status = IAvatar.Status.Loaded;
        }

        public void SetVisibility(bool visible)
        {
            this.visible = true;
            visibility.SetVisible(true);
            return;
            this.visible = visible;
            if (status != IAvatar.Status.Loaded)
                return;
            visibility.SetVisible(visible);
        }
        public void SetExpression(string expressionId, long timestamps) { animator?.PlayExpression(expressionId, timestamps); }
        public void SetLODLevel(int lodIndex)
        {
            this.lodIndex = 0;
            lod.SetLodIndex(0);
            return;
            this.lodIndex = lodIndex;
            if (status != IAvatar.Status.Loaded)
                return;
            lod.SetLodIndex(lodIndex);
            gpuSkinningThrottler.SetThrottling(lodIndex + 1);
        }

        public void SetImpostorTexture(Texture2D impostorTexture) { lod.SetImpostorTexture(impostorTexture); }
        public void SetImpostorTint(Color color) { lod.SetImpostorTint(color); }

        public void Dispose()
        {
            avatarCurator?.Dispose();
            loader?.Dispose();
            lod?.Dispose();
            gpuSkinningThrottler?.Dispose();
        }
    }
}