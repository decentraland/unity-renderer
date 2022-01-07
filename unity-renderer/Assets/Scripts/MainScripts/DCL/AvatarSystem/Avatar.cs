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
        private CancellationTokenSource disposeCts = new CancellationTokenSource();

        private bool visible = true;
        private int lodIndex = 0;

        public IAvatar.Status status { get; private set; } = IAvatar.Status.Idle;

        //TODO Calculate bounds
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

        /// <summary>
        /// Starts the loading process for the Avatar. 
        /// </summary>
        /// <param name="wearablesIds"></param>
        /// <param name="settings"></param>
        /// <param name="ct"></param>
        public async UniTask Load(List<string> wearablesIds, AvatarSettings settings, CancellationToken ct = default)
        {
            CancellationToken linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, disposeCts.Token).Token;

            linkedCt.ThrowIfCancellationRequested();

            try
            {
                WearableItem bodyshape = null;
                WearableItem eyes = null;
                WearableItem eyebrows = null;
                WearableItem mouth = null;
                List<WearableItem> wearables = null;
                try
                {
                    (bodyshape, eyes, eyebrows, mouth, wearables) = await avatarCurator.Curate(settings.bodyshapeId , wearablesIds, linkedCt);
                }
                catch
                {
                    Debug.LogError($"Failed curating avatar with wearables:[{string.Join(",", wearablesIds)}] for bodyshape:{settings.bodyshapeId} and player {settings.playerName}");
                    throw;
                }

                await loader.Load(bodyshape, eyes, eyebrows, mouth, wearables, settings, linkedCt);

                if (loader.status == ILoader.Status.Failed_Mayor || loader.status == ILoader.Status.Idle)
                {
                    status = IAvatar.Status.Failed;
                    return;
                }

                // TODO Fix huge avatar on first frame
                animator.Prepare(settings.bodyshapeId, loader.bodyshapeContainer);

                //TODO GPUSkinning only has to be prepared when the loader has changes
                gpuSkinning.Prepare(loader.combinedRenderer);
                gpuSkinningThrottler.Bind(gpuSkinning);

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
            catch (OperationCanceledException)
            {
                Dispose();
                throw;
            }
        }

        public void SetVisibility(bool visible)
        {
            this.visible = visible;
            if (status != IAvatar.Status.Loaded)
                return;
            visibility.SetVisible(visible);
        }

        public void SetExpression(string expressionId, long timestamps) { animator?.PlayExpression(expressionId, timestamps); }

        public void SetLODLevel(int lodIndex)
        {
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
            status = IAvatar.Status.Idle;
            disposeCts?.Cancel();
            disposeCts = new CancellationTokenSource();
            avatarCurator?.Dispose();
            loader?.Dispose();
            lod?.Dispose();
            gpuSkinningThrottler?.Dispose();
        }
    }
}