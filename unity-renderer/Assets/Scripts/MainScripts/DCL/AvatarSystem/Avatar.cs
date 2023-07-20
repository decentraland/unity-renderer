using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using GPUSkinning;
using UnityEngine;

namespace AvatarSystem
{
    // [ADR 65 - https://github.com/decentraland/adr]
    public class Avatar : IAvatar
    {
        private const string NEW_CDN_FF = "ab-new-cdn";

        private const float RESCALING_BOUNDS_FACTOR = 100f;
        internal const string LOADING_VISIBILITY_CONSTRAIN = "Loading";

        protected readonly ILoader loader;
        protected readonly IVisibility visibility;
        protected readonly IAnimator animator;
        private readonly IAvatarCurator avatarCurator;
        private readonly ILOD lod;
        private readonly IGPUSkinning gpuSkinning;
        private readonly IGPUSkinningThrottlerService gpuSkinningThrottlerService;
        private readonly IEmoteAnimationEquipper emoteAnimationEquipper;

        private CancellationTokenSource disposeCts = new ();

        public IAvatar.Status status { get; private set; } = IAvatar.Status.Idle;
        public Vector3 extents { get; private set; }
        public int lodLevel => lod?.lodIndex ?? 0;
        public event Action<Renderer> OnCombinedRendererUpdate;
        private FeatureFlag featureFlags => DataStore.i.featureFlags.flags.Get();

        internal Avatar(IAvatarCurator avatarCurator, ILoader loader, IAnimator animator,
            IVisibility visibility, ILOD lod, IGPUSkinning gpuSkinning, IGPUSkinningThrottlerService gpuSkinningThrottlerService,
            IEmoteAnimationEquipper emoteAnimationEquipper)
        {
            this.avatarCurator = avatarCurator;
            this.loader = loader;
            this.animator = animator;
            this.visibility = visibility;
            this.lod = lod;
            this.gpuSkinning = gpuSkinning;
            this.gpuSkinningThrottlerService = gpuSkinningThrottlerService;
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
                await LoadTry(wearablesIds, emotesIds, settings, linkedCt);
            }
            catch (OperationCanceledException)
            {
                Dispose();
                throw;
            }
            catch (Exception e)
            {
                Dispose();
                Debug.Log($"Avatar.Load failed with wearables:[{string.Join(",", wearablesIds)}] " +
                          $"for bodyshape:{settings.bodyshapeId} and player {settings.playerName}");
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

        protected virtual async UniTask LoadTry(List<string> wearablesIds, List<string> emotesIds, AvatarSettings settings, CancellationToken linkedCt)
        {
            List<WearableItem> emotes = await LoadWearables(wearablesIds, emotesIds, settings, linkedCt: linkedCt);

            GameObject container = loader.bodyshapeContainer;

            if (featureFlags.IsFeatureEnabled(NEW_CDN_FF))
            {
                if (loader.bodyshapeContainer.transform.childCount > 0)
                {
                    loader.bodyshapeContainer.transform.Find("Armature");
                    Transform child = loader.bodyshapeContainer.transform.GetChild(0);

                    // Asset bundles assets dont have the gltf-scene name as the root, they have the file hash, for this particular object we need it to be Armature
                    child.name = "Armature";
                }
            }
            else
            {
                GameObject parent = GetDeepParentOf(loader.bodyshapeContainer, "Armature");
                container = parent != null ? parent : container;
            }

            animator.Prepare(settings.bodyshapeId, container);
            Prepare(settings, emotes, loader.bodyshapeContainer);
            Bind();
            Inform(loader.combinedRenderer);
        }

        private GameObject GetDeepParentOf(GameObject container, string childName)
        {
            foreach (Transform child in container.transform)
                return child.name == childName ? child.parent.gameObject : GetDeepParentOf(child.gameObject, childName);

            return null;
        }

        protected async UniTask<List<WearableItem>> LoadWearables(List<string> wearablesIds, List<string> emotesIds, AvatarSettings settings, SkinnedMeshRenderer bonesRenderers = null, CancellationToken linkedCt = default)
        {
            BodyWearables bodyWearables;
            List<WearableItem> wearables;
            List<WearableItem> emotes;

            (bodyWearables, wearables, emotes) = await avatarCurator.Curate(settings, wearablesIds, emotesIds, linkedCt);

            if (!loader.IsValidForBodyShape(bodyWearables))
                visibility.AddGlobalConstrain(LOADING_VISIBILITY_CONSTRAIN);

            await loader.Load(bodyWearables, wearables, settings, bonesRenderers, linkedCt);
            return emotes;
        }

        protected void Prepare(AvatarSettings settings, List<WearableItem> emotes, GameObject loaderBodyshapeContainer)
        {
            //Scale the bounds due to the giant avatar not being skinned yet
            extents = loader.combinedRenderer.localBounds.extents * 2f / RESCALING_BOUNDS_FACTOR;

            emoteAnimationEquipper.SetEquippedEmotes(settings.bodyshapeId, emotes);
            gpuSkinning.Prepare(loader.combinedRenderer);
            gpuSkinningThrottlerService.Register(gpuSkinning);
        }

        protected void Bind()
        {
            visibility.Bind(gpuSkinning.renderer, loader.facialFeaturesRenderers);
            visibility.RemoveGlobalConstrain(LOADING_VISIBILITY_CONSTRAIN);
            lod.Bind(gpuSkinning.renderer);
        }

        protected void Inform(Renderer loaderCombinedRenderer)
        {
            status = IAvatar.Status.Loaded;
            OnCombinedRendererUpdate?.Invoke(loaderCombinedRenderer);
        }

        public virtual void AddVisibilityConstraint(string key)
        {
            visibility.AddGlobalConstrain(key);
        }

        public void RemoveVisibilityConstrain(string key) =>
            visibility.RemoveGlobalConstrain(key);

        public void PlayEmote(string emoteId, long timestamps) =>
            animator?.PlayEmote(emoteId, timestamps);

        public void EquipEmote(string emoteId, EmoteClipData emoteClipData) =>
            animator?.EquipEmote(emoteId, emoteClipData);

        public void UnequipEmote(string emoteId) =>
            animator?.UnequipEmote(emoteId);

        public void SetLODLevel(int lodIndex) =>
            lod.SetLodIndex(lodIndex);

        public void SetAnimationThrottling(int framesBetweenUpdate) =>
            gpuSkinningThrottlerService.ModifyThrottling(gpuSkinning, framesBetweenUpdate);

        public void SetImpostorTexture(Texture2D impostorTexture) =>
            lod.SetImpostorTexture(impostorTexture);

        public void SetImpostorTint(Color color) =>
            lod.SetImpostorTint(color);

        public Transform[] GetBones() =>
            loader.GetBones();

        public Renderer GetMainRenderer() =>
            gpuSkinning.renderer;

        public IReadOnlyList<SkinnedMeshRenderer> originalVisibleRenderers => loader.originalVisibleRenderers;

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
            gpuSkinningThrottlerService?.Unregister(gpuSkinning);
        }
    }
}
