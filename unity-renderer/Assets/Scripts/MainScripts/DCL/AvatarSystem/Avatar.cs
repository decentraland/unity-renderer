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
        private const float RESCALING_BOUNDS_FACTOR = 100f;
        internal const string LOADING_VISIBILITY_CONSTRAIN = "Loading";
        private readonly IAvatarCurator avatarCurator;
        private readonly ILoader loader;
        private readonly IAnimator animator;
        private readonly IVisibility visibility;
        private readonly ILOD lod;
        private readonly IGPUSkinning gpuSkinning;
        private readonly IGPUSkinningThrottler gpuSkinningThrottler;
        private CancellationTokenSource disposeCts = new CancellationTokenSource();

        public IAvatar.Status status { get; private set; } = IAvatar.Status.Idle;
        public Vector3 extents { get; private set; }
        public int lodLevel => lod?.lodIndex ?? 0;

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
            disposeCts ??= new CancellationTokenSource();

            status = IAvatar.Status.Idle;
            CancellationToken linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, disposeCts.Token).Token;

            linkedCt.ThrowIfCancellationRequested();

            try
            {
                visibility.AddGlobalConstrain(LOADING_VISIBILITY_CONSTRAIN);
                WearableItem bodyshape = null;
                WearableItem eyes = null;
                WearableItem eyebrows = null;
                WearableItem mouth = null;
                List<WearableItem> wearables = null;

                (bodyshape, eyes, eyebrows, mouth, wearables) = await avatarCurator.Curate(settings , wearablesIds, linkedCt);

                await loader.Load(bodyshape, eyes, eyebrows, mouth, wearables, settings, linkedCt);

                //Scale the bounds due to the giant avatar not being skinned yet
                extents = loader.combinedRenderer.localBounds.extents * 2f / RESCALING_BOUNDS_FACTOR;

                animator.Prepare(settings.bodyshapeId, loader.bodyshapeContainer);

                gpuSkinning.Prepare(loader.combinedRenderer);
                gpuSkinningThrottler.Bind(gpuSkinning);

                visibility.Bind(gpuSkinning.renderer, loader.facialFeaturesRenderers);
                visibility.RemoveGlobalConstrain(LOADING_VISIBILITY_CONSTRAIN);

                lod.Bind(gpuSkinning.renderer);
                gpuSkinningThrottler.Start();

                status = IAvatar.Status.Loaded;
            }
            catch (OperationCanceledException)
            {
                Dispose();
            }
            catch (Exception e)
            {
                Dispose();
                Debug.Log($"Avatar.Load failed with wearables:[{string.Join(",", wearablesIds)}] for bodyshape:{settings.bodyshapeId} and player {settings.playerName}");
                throw;
            }
            finally
            {
                disposeCts?.Dispose();
                disposeCts = null;
            }
        }

        public void AddVisibilityConstrain(string key) { visibility.AddGlobalConstrain(key); }

        public void RemoveVisibilityConstrain(string key) { visibility.RemoveGlobalConstrain(key); }

        public void SetExpression(string expressionId, long timestamps) { animator?.PlayExpression(expressionId, timestamps); }

        public void SetLODLevel(int lodIndex) { lod.SetLodIndex(lodIndex); }

        public void SetAnimationThrottling(int framesBetweenUpdate) { gpuSkinningThrottler.SetThrottling(framesBetweenUpdate); }

        public void SetImpostorTexture(Texture2D impostorTexture) { lod.SetImpostorTexture(impostorTexture); }

        public void SetImpostorTint(Color color) { lod.SetImpostorTint(color); }

        public Transform[] GetBones() => loader.GetBones();

        public void Dispose()
        {
            status = IAvatar.Status.Idle;
            disposeCts?.Cancel();
            disposeCts?.Dispose();
            disposeCts = null;
            avatarCurator?.Dispose();
            loader?.Dispose();
            visibility?.Dispose();
            lod?.Dispose();
            gpuSkinningThrottler?.Dispose();
        }
    }
}