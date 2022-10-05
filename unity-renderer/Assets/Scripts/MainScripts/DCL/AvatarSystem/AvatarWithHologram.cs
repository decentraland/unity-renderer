using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using GPUSkinning;
using UnityEngine;

namespace AvatarSystem
{
    // [ADR 65 - https://github.com/decentraland/adr]
    public class AvatarWithHologram : IAvatar
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
        private readonly IEmoteAnimationEquipper emoteAnimationEquipper;
        private CancellationTokenSource disposeCts = new CancellationTokenSource();
        private readonly IBaseAvatar baseAvatar;

        public IAvatar.Status status { get; private set; } = IAvatar.Status.Idle;
        public Vector3 extents { get; private set; }
        public int lodLevel => lod?.lodIndex ?? 0;

        public AvatarWithHologram(IBaseAvatar baseAvatar, IAvatarCurator avatarCurator, ILoader loader, IAnimator animator, IVisibility visibility, ILOD lod, IGPUSkinning gpuSkinning, IGPUSkinningThrottler gpuSkinningThrottler, IEmoteAnimationEquipper emoteAnimationEquipper)
        {
            this.baseAvatar = baseAvatar;
            this.avatarCurator = avatarCurator;
            this.loader = loader;
            this.animator = animator;
            this.visibility = visibility;
            this.lod = lod;
            this.gpuSkinning = gpuSkinning;
            this.gpuSkinningThrottler = gpuSkinningThrottler;
            this.emoteAnimationEquipper = emoteAnimationEquipper;
        }

        /// <summary>
        /// Starts the loading process for the Avatar. 
        /// </summary>
        /// <param name="wearablesIds"></param>
        /// <param name="settings"></param>
        /// <param name="ct"></param>
        public async UniTask Load(List<string> wearablesIds, List<string> emotesIds, AvatarSettings settings, CancellationToken ct = default)
        {
            disposeCts ??= new CancellationTokenSource();

            status = IAvatar.Status.Idle;
            CancellationToken linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, disposeCts.Token).Token;

            linkedCt.ThrowIfCancellationRequested();

            try
            {
                WearableItem bodyshape = null;
                WearableItem eyes = null;
                WearableItem eyebrows = null;
                WearableItem mouth = null;
                List<WearableItem> wearables = null;
                List<WearableItem> emotes = null;

                baseAvatar.Initialize();
                animator.Prepare(settings.bodyshapeId, baseAvatar.GetArmatureContainer());
                (bodyshape, eyes, eyebrows, mouth, wearables, emotes) = await avatarCurator.Curate(settings, wearablesIds, emotesIds, linkedCt);
                if (!loader.IsValidForBodyShape(bodyshape, eyes, eyebrows, mouth))
                {
                    visibility.AddGlobalConstrain(LOADING_VISIBILITY_CONSTRAIN);
                }
                await loader.Load(bodyshape, eyes, eyebrows, mouth, wearables, settings, baseAvatar.GetMainRenderer(), linkedCt);

                //Scale the bounds due to the giant avatar not being skinned yet
                extents = loader.combinedRenderer.localBounds.extents * 2f / RESCALING_BOUNDS_FACTOR;

                emoteAnimationEquipper.SetEquippedEmotes(settings.bodyshapeId, emotes);
                gpuSkinning.Prepare(loader.combinedRenderer);
                gpuSkinningThrottler.Bind(gpuSkinning);

                visibility.Bind(gpuSkinning.renderer, loader.facialFeaturesRenderers);
                visibility.RemoveGlobalConstrain(LOADING_VISIBILITY_CONSTRAIN);

                lod.Bind(gpuSkinning.renderer);
                gpuSkinningThrottler.Start();

                status = IAvatar.Status.Loaded;
                await baseAvatar.FadeOut(loader.combinedRenderer.GetComponent<MeshRenderer>(), visibility.IsMainRenderVisible(), linkedCt);
            }
            catch (OperationCanceledException)
            {
                Dispose();
                throw;
            }
            catch (Exception e)
            {
                Dispose();
                Debug.Log($"Avatar.Load failed with wearables:[{string.Join(",", wearablesIds)}] for bodyshape:{settings.bodyshapeId} and player {settings.playerName}");
                if (e.InnerException != null)
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                else
                    throw;
            }
            finally
            {
                disposeCts?.Dispose();
                disposeCts = null;
            }
        }

        public void AddVisibilityConstraint(string key)
        {
            visibility.AddGlobalConstrain(key);
            baseAvatar.CancelTransition();
        }

        public void RemoveVisibilityConstrain(string key)
        {
            visibility.RemoveGlobalConstrain(key);
        }

        public void PlayEmote(string emoteId, long timestamps) { animator?.PlayEmote(emoteId, timestamps); }

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