using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using GPUSkinning;
using UnityEngine;
using static WearableLiterals;

namespace DCL
{
    public class AvatarRenderer : MonoBehaviour, IAvatarRenderer
    {
        private static readonly int BASE_COLOR_PROPERTY = Shader.PropertyToID("_BaseColor");

        private const int MAX_RETRIES = 5;

        public Material defaultMaterial;
        public Material eyeMaterial;
        public Material eyebrowMaterial;
        public Material mouthMaterial;
        public MeshRenderer impostorRenderer;
        public MeshFilter impostorMeshFilter;

        private AvatarModel model;
        private AvatarMeshCombinerHelper avatarMeshCombiner;
        private SimpleGPUSkinning gpuSkinning = null;
        private GPUSkinningThrottler gpuSkinningThrottler = null;
        private int gpuSkinningFramesBetweenUpdates = 1;
        private bool initializedImpostor = false;

        private Renderer mainMeshRenderer
        {
            get
            {
                if (gpuSkinning != null)
                    return gpuSkinning.renderer;
                return avatarMeshCombiner.renderer;
            }
        }

        public event Action<IAvatarRenderer.VisualCue> OnVisualCue;
        public event Action OnSuccessEvent;
        public event Action<float> OnImpostorAlphaValueUpdate;
        public event Action<float> OnAvatarAlphaValueUpdate;
        public event Action<bool> OnFailEvent;

        internal BodyShapeController bodyShapeController;
        internal Dictionary<WearableItem, WearableController> wearableControllers = new Dictionary<WearableItem, WearableController>();
        internal FacialFeatureController eyesController;
        internal FacialFeatureController eyebrowsController;
        internal FacialFeatureController mouthController;
        internal AvatarAnimatorLegacy animator;
        internal StickersController stickersController;

        private long lastStickerTimestamp = -1;

        public bool isLoading;
        public bool isReady => bodyShapeController != null && bodyShapeController.isReady && wearableControllers != null && wearableControllers.Values.All(x => x.isReady);
        public float maxY { get; private set; } = 0;

        private Coroutine loadCoroutine;
        private AssetPromise_Texture bodySnapshotTexturePromise;
        private List<string> wearablesInUse = new List<string>();
        private bool isDestroyed = false;

        private void Awake()
        {
            animator = GetComponent<AvatarAnimatorLegacy>();
            stickersController = GetComponent<StickersController>();
            avatarMeshCombiner = new AvatarMeshCombinerHelper();
            avatarMeshCombiner.prepareMeshForGpuSkinning = true;
            avatarMeshCombiner.uploadMeshToGpu = true;

            if (impostorRenderer != null)
                SetImpostorVisibility(false);
        }

        public void ApplyModel(AvatarModel model, Action onSuccess, Action onFail)
        {
            if ( this.model != null )
            {
                if (model != null && this.model.Equals(model))
                {
                    onSuccess?.Invoke();
                    return;
                }

                bool wearablesChanged = !this.model.HaveSameWearablesAndColors(model);
                bool expressionsChanged = !this.model.HaveSameExpressions(model);

                if (!wearablesChanged && expressionsChanged)
                {
                    this.model.expressionTriggerId = model.expressionTriggerId;
                    this.model.expressionTriggerTimestamp = model.expressionTriggerTimestamp;
                    this.model.stickerTriggerId = model.stickerTriggerId;
                    this.model.stickerTriggerTimestamp = model.stickerTriggerTimestamp;
                    UpdateExpression();
                    onSuccess?.Invoke();
                    return;
                }
            }

            this.model = new AvatarModel();
            this.model.CopyFrom(model);

            ResetImpostor();

            // TODO(Brian): Find a better approach than overloading callbacks like this. This code is not readable.
            void onSuccessWrapper()
            {
                onSuccess?.Invoke();
                this.OnSuccessEvent -= onSuccessWrapper;
            }

            this.OnSuccessEvent += onSuccessWrapper;

            void onFailWrapper(bool isFatalError)
            {
                onFail?.Invoke();
                this.OnFailEvent -= onFailWrapper;
            }

            this.OnFailEvent += onFailWrapper;

            isLoading = false;

            if (model == null)
            {
                CleanupAvatar();
                this.OnSuccessEvent?.Invoke();
                return;
            }

            StopLoadingCoroutines();

            isLoading = true;
            loadCoroutine = CoroutineStarter.Start(LoadAvatar());
        }

        public void InitializeImpostor()
        {
            initializedImpostor = true;

            // The fetched snapshot can take its time so it's better to assign a generic impostor first.
            AvatarRendererHelpers.RandomizeAndApplyGenericImpostor(impostorMeshFilter.mesh, impostorRenderer.material);

            UserProfile userProfile = null;
            if (!string.IsNullOrEmpty(model?.id))
                userProfile = UserProfileController.GetProfileByUserId(model.id);

            if (userProfile != null)
            {
                bodySnapshotTexturePromise = new AssetPromise_Texture(userProfile.bodySnapshotURL);
                bodySnapshotTexturePromise.OnSuccessEvent += asset => AvatarRendererHelpers.SetImpostorTexture(asset.texture, impostorMeshFilter.mesh, impostorRenderer.material);
                AssetPromiseKeeper_Texture.i.Keep(bodySnapshotTexturePromise);
            }
        }

        void StopLoadingCoroutines()
        {
            if (loadCoroutine != null)
                CoroutineStarter.Stop(loadCoroutine);

            loadCoroutine = null;
        }

        public void CleanupAvatar()
        {
            StopLoadingCoroutines();
            if (!isDestroyed)
            {
                SetGOVisibility(true);
                if (impostorRenderer != null)
                    SetImpostorVisibility(false);
            }

            avatarMeshCombiner.Dispose();
            gpuSkinningThrottler = null;
            gpuSkinning = null;
            eyebrowsController?.CleanUp();
            eyebrowsController = null;

            eyesController?.CleanUp();
            eyesController = null;

            mouthController?.CleanUp();
            mouthController = null;

            bodyShapeController?.CleanUp();
            bodyShapeController = null;

            using (var iterator = wearableControllers.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.CleanUp();
                }
            }

            wearableControllers.Clear();
            model = null;
            isLoading = false;
            OnFailEvent = null;
            OnSuccessEvent = null;

            CleanMergedAvatar();

            ResetImpostor();

            CatalogController.RemoveWearablesInUse(wearablesInUse);
            wearablesInUse.Clear();
            OnVisualCue?.Invoke(IAvatarRenderer.VisualCue.CleanedUp);
        }

        void CleanUpUnusedItems()
        {
            if (model.wearables == null)
                return;

            var ids = wearableControllers.Keys.ToArray();

            for (var i = 0; i < ids.Length; i++)
            {
                var currentId = ids[i];
                var wearable = wearableControllers[currentId];

                if (!model.wearables.Contains(wearable.id) || !wearable.IsLoadedForBodyShape(model.bodyShape))
                {
                    wearable.CleanUp();
                    wearableControllers.Remove(currentId);
                }
            }
        }

        // TODO(Brian): Pure functions should be extracted from this big LoadAvatar() method and unit-test separately.
        //              The current approach has tech debt that's getting very expensive and is costing many hours of debugging.
        //              Avatar Loading should be a self contained operation that doesn't depend on pool management and AvatarShape
        //              lifecycle like it does now.
        private IEnumerator LoadAvatar()
        {
            // TODO(Brian): This is an ugly hack, all the loading should be performed
            //              without being afraid of the gameObject active state.
            yield return new WaitUntil(() => gameObject.activeSelf);

            var loadSoftFailed = false;
            WearableItem resolvedBody = null;
            Exception bodyShapeError = null;

            yield return LoadBodyShape(item => resolvedBody = item, err => bodyShapeError = err);

            if (bodyShapeError != null)
            {
                Debug.LogError(bodyShapeError);
                isLoading = false;
                OnFailEvent?.Invoke(true);
                yield break;
            }
            
            // In this point, all the requests related to the avatar's wearables have been collected and sent to the CatalogController to be sent to kernel as a unique request.
            // From here we wait for the response of the requested wearables and process them.
            
            var resolvedWearables = new List<WearableItem>();
            // TODO(Brian): Evaluate using UniTask<T> here instead of Helpers.Promise.
            var replacementWearables = new List<Promise<WearableItem>>();

            void AddLoadedWearable(List<WearableItem> loadedWearables) => resolvedWearables.AddRange(loadedWearables);

            void LogWearableLoadError(Exception err)
            {
                Debug.LogException(err);
                loadSoftFailed = true;
            }

            yield return LoadWearables(RequestAllModelWearables(),
                err =>
                {
                    LogWearableLoadError(err);
                    // TODO: make some kind of strategy pattern to avoid type checking here
                    if (err is WearableMissingRepresentationException wearableMissingRepresentation)
                        replacementWearables.Add(RequestWearableReplacement(wearableMissingRepresentation.wearable.data.category));
                }, AddLoadedWearable);

            yield return LoadWearables(replacementWearables, LogWearableLoadError, AddLoadedWearable);
            yield return WearAnyMissingClothes(resolvedWearables, LogWearableLoadError, AddLoadedWearable);

            bool bodyIsDirty = false;
            if (bodyShapeController != null && bodyShapeController.id != model?.bodyShape)
            {
                bodyShapeController.CleanUp();
                bodyShapeController = null;
                bodyIsDirty = true;
            }

            if (bodyShapeController == null)
            {
                HideAll();

                bodyShapeController = new BodyShapeController(resolvedBody);
                eyesController = FacialFeatureController.CreateDefaultFacialFeature(bodyShapeController.bodyShapeId, Categories.EYES, eyeMaterial);
                eyebrowsController = FacialFeatureController.CreateDefaultFacialFeature(bodyShapeController.bodyShapeId, Categories.EYEBROWS, eyebrowMaterial);
                mouthController = FacialFeatureController.CreateDefaultFacialFeature(bodyShapeController.bodyShapeId, Categories.MOUTH, mouthMaterial);
            }

            //TODO(Brian): This logic should be performed in a testeable pure function instead of this inline approach.
            //             Moreover, this function should work against data, not wearableController instances.
            bool wearablesIsDirty = false;
            HashSet<string> unusedCategories = new HashSet<string>(Categories.ALL);
            int wearableCount = resolvedWearables.Count;
            for (int index = 0; index < wearableCount; index++)
            {
                WearableItem wearable = resolvedWearables[index];
                if (wearable == null)
                    continue;

                unusedCategories.Remove(wearable.data.category);
                if (wearableControllers.ContainsKey(wearable))
                {
                    if (!wearableControllers[wearable].IsLoadedForBodyShape(bodyShapeController.bodyShapeId))
                        wearableControllers[wearable].CleanUp();
                }
                else
                {
                    AddWearableController(wearable);
                    if (wearable.data.category != Categories.EYES && wearable.data.category != Categories.MOUTH && wearable.data.category != Categories.EYEBROWS)
                        wearablesIsDirty = true;
                }
            }

            if ( eyesController == null && !unusedCategories.Contains(Categories.EYES))
                unusedCategories.Add(Categories.EYES);

            if ( mouthController == null && !unusedCategories.Contains(Categories.MOUTH))
                unusedCategories.Add(Categories.MOUTH);

            if ( eyebrowsController == null && !unusedCategories.Contains(Categories.EYEBROWS))
                unusedCategories.Add(Categories.EYEBROWS);

            foreach (var category in unusedCategories)
            {
                switch (category)
                {
                    case Categories.EYES:
                        eyesController = FacialFeatureController.CreateDefaultFacialFeature(bodyShapeController.bodyShapeId, Categories.EYES, eyeMaterial);
                        break;
                    case Categories.MOUTH:
                        mouthController = FacialFeatureController.CreateDefaultFacialFeature(bodyShapeController.bodyShapeId, Categories.MOUTH, mouthMaterial);
                        break;
                    case Categories.EYEBROWS:
                        eyebrowsController = FacialFeatureController.CreateDefaultFacialFeature(bodyShapeController.bodyShapeId, Categories.EYEBROWS, eyebrowMaterial);
                        break;
                }
            }

            HashSet<string> hiddenList = WearableItem.CompoundHidesList(bodyShapeController.bodyShapeId, resolvedWearables);
            if (!bodyShapeController.isReady)
            {
                bodyShapeController.Load(bodyShapeController.bodyShapeId, transform, OnWearableLoadingSuccess, OnBodyShapeLoadingFail);
            }

            foreach (WearableController wearable in wearableControllers.Values)
            {
                if (bodyIsDirty)
                    wearable.boneRetargetingDirty = true;

                wearable.Load(bodyShapeController.bodyShapeId, transform, OnWearableLoadingSuccess, x => OnWearableLoadingFail(x));
                yield return null;
            }

            // TODO(Brian): Evaluate using UniTask<T> instead of this way.
            yield return new WaitUntil(() => bodyShapeController.isReady && wearableControllers.Values.All(x => x.isReady));

            eyesController.Load(bodyShapeController, model.eyeColor);
            eyebrowsController.Load(bodyShapeController, model.hairColor);
            mouthController.Load(bodyShapeController, model.skinColor);

            yield return eyesController;
            yield return eyebrowsController;
            yield return mouthController;

            if (bodyIsDirty || wearablesIsDirty)
            {
                OnVisualCue?.Invoke(IAvatarRenderer.VisualCue.Loaded);
            }

            // TODO(Brian): unusedCategories and hiddenList management is a double negative PITA.
            //              The load process should define how the avatar should look like before
            //              loading it and put this information in a positive list
            //              (i.e. not negative, because leads to double negative checks).
            bodyShapeController.SetActiveParts(unusedCategories.Contains(Categories.LOWER_BODY), unusedCategories.Contains(Categories.UPPER_BODY), unusedCategories.Contains(Categories.FEET));
            bodyShapeController.UpdateVisibility(hiddenList);
            foreach (WearableController wearableController in wearableControllers.Values)
            {
                wearableController.UpdateVisibility(hiddenList);
            }

            CleanUpUnusedItems();

            isLoading = false;

            SetupHairAndSkinColors();
            SetWearableBones();

            // TODO(Brian): Expression and sticker update shouldn't be part of avatar loading code!!!! Refactor me please.
            UpdateExpression();

            var allRenderers = wearableControllers.SelectMany( x => x.Value.GetRenderers() ).ToList();
            allRenderers.AddRange( bodyShapeController.GetRenderers() );
            bool mergeSuccess = MergeAvatar(allRenderers);

            if (mergeSuccess)
                PrepareGpuSkinning();
            else
                loadSoftFailed = true;

            maxY = allRenderers.Max(x =>
            {
                Bounds bounds = x.bounds;
                return bounds.center.y + bounds.extents.y - transform.position.y;
            });

            // TODO(Brian): The loadSoftFailed flow is too convoluted--you never know which objects are nulled or empty
            //              before reaching this branching statement. The failure should be caught with a throw or other
            //              proper language feature.
            if (loadSoftFailed)
            {
                OnFailEvent?.Invoke(false);
            }
            else
            {
                OnSuccessEvent?.Invoke();
            }
        }

        private IEnumerator WearAnyMissingClothes(IEnumerable<WearableItem> wearables,
            Action<Exception> error,
            Action<List<WearableItem>> completed)
        {
            var missingClothesReplacements = new [] {Categories.LOWER_BODY, Categories.UPPER_BODY}
                .Except(wearables.Select(wearable => wearable.data.category))
                .Select(RequestWearableReplacement);

            yield return LoadWearables(missingClothesReplacements, error, completed);
        }

        private IEnumerator LoadWearables(IEnumerable<Promise<WearableItem>> wearablesToLoad,
            Action<Exception> error,
            Action<List<WearableItem>> completed)
        {
            var resolvedWearables = new List<WearableItem>();
            
            foreach (var wearablePromise in wearablesToLoad)
            {
                yield return wearablePromise;

                if (!string.IsNullOrEmpty(wearablePromise.error))
                    error.Invoke(new Exception(wearablePromise.error));
                else
                {
                    var wearableItem = wearablePromise.value;
                    
                    if (wearableItem.SupportsBodyShape(model.bodyShape))
                    {
                        resolvedWearables.Add(wearableItem);
                        wearablesInUse.Add(wearableItem.id);
                        model.wearables.Add(wearableItem.id);
                    }
                    else
                        error.Invoke(new WearableMissingRepresentationException(wearableItem, model.bodyShape));
                }
            }
            
            completed.Invoke(resolvedWearables);
        }

        private IEnumerator LoadBodyShape(Action<WearableItem> success, Action<Exception> fail)
        {
            Promise<WearableItem> avatarBodyPromise = null;
            WearableItem resolvedBody = null;
            
            if (!string.IsNullOrEmpty(model.bodyShape))
                avatarBodyPromise = CatalogController.RequestWearable(model.bodyShape);
            else
                fail.Invoke(new Exception($"model.bodyShape is null or empty. Id: {model.id}, name: {model.name}, bodyShape: {model.bodyShape}"));

            if (avatarBodyPromise != null)
            {
                yield return avatarBodyPromise;

                if (!string.IsNullOrEmpty(avatarBodyPromise.error))
                {
                    fail.Invoke(new Exception(avatarBodyPromise.error));
                }
                else
                {
                    resolvedBody = avatarBodyPromise.value;
                    wearablesInUse.Add(avatarBodyPromise.value.id);
                }
            }

            if (resolvedBody == null)
                fail.Invoke(new Exception($"resolved body shape is null. Id: {model.id}, name: {model.name}"));
            
            success.Invoke(resolvedBody);
        }

        private IEnumerable<Promise<WearableItem>> RequestAllModelWearables()
        {
            // TODO(Brian): Evaluate using UniTask<T> here instead of Helpers.Promise.
            var avatarWearablePromises = new List<Promise<WearableItem>>();
            if (model.wearables == null) return avatarWearablePromises;
            for (var i = 0; i < model.wearables.Count; i++)
                avatarWearablePromises.Add(CatalogController.RequestWearable(model.wearables[i]));
            return avatarWearablePromises;
        }

        private Promise<WearableItem> RequestWearableReplacement(string category)
        {
            var defaultReplacement = DefaultWearables.GetDefaultWearable(model.bodyShape, category);
            
            if (!string.IsNullOrEmpty(defaultReplacement))
                return CatalogController.RequestWearable(defaultReplacement);

            var defaultWearableNotFoundPromise = new Promise<WearableItem>();
            defaultWearableNotFoundPromise.Reject($"Replacement wearable not found! shape: {model.bodyShape}, category {category}");
            return defaultWearableNotFoundPromise;
        }

        private void PrepareGpuSkinning()
        {
            // Sample the animation manually and force an update in the GPUSkinning to avoid giant avatars
            animator.SetIdleFrame();
            animator.animation.Sample();

            gpuSkinning = new SimpleGPUSkinning(
                avatarMeshCombiner.renderer,
                false); // Bind poses are encoded by the AvatarMeshCombiner before making the mesh unreadable.

            gpuSkinningThrottler = new GPUSkinningThrottler(gpuSkinning);
            gpuSkinningThrottler.SetThrottling(gpuSkinningFramesBetweenUpdates);
        }

        void SetupHairAndSkinColors()
        {
            bodyShapeController.SetupHairAndSkinColors(model.skinColor, model.hairColor);

            foreach ( var wearable in wearableControllers )
            {
                wearable.Value.SetupHairAndSkinColors(model.skinColor, model.hairColor);
            }
        }

        void OnWearableLoadingSuccess(WearableController wearableController)
        {
            if (wearableController == null || model == null)
            {
                Debug.LogWarning($"WearableSuccess was called wrongly: IsWearableControllerNull=>{wearableController == null}, IsModelNull=>{model == null}");
                OnWearableLoadingFail(wearableController, 0);
            }
        }

        void OnBodyShapeLoadingFail(WearableController wearableController)
        {
            Debug.LogError($"Avatar: {model?.name}  -  Failed loading bodyshape: {wearableController?.id}");
            CleanupAvatar();
            OnFailEvent?.Invoke(true);
        }

        void OnWearableLoadingFail(WearableController wearableController, int retriesCount = MAX_RETRIES)
        {
            if (retriesCount <= 0)
            {
                Debug.LogError($"Avatar: {model?.name}  -  Failed loading wearable: {wearableController?.id}");
                CleanupAvatar();
                OnFailEvent?.Invoke(false);
                return;
            }

            wearableController.Load(bodyShapeController.id, transform, OnWearableLoadingSuccess, x => OnWearableLoadingFail(x, retriesCount - 1));
        }

        private void SetWearableBones()
        {
            // NOTE(Brian): Set bones/rootBone of all wearables to be the same of the baseBody,
            //              so all of them are animated together.
            using (var enumerator = wearableControllers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Value.SetAnimatorBones(bodyShapeController.bones, bodyShapeController.rootBone);
                }
            }
        }

        private void UpdateExpression()
        {
            SetExpression(model.expressionTriggerId, model.expressionTriggerTimestamp);

            if (lastStickerTimestamp != model.stickerTriggerTimestamp && model.stickerTriggerId != null)
            {
                lastStickerTimestamp = model.stickerTriggerTimestamp;

                if ( stickersController != null )
                    stickersController.PlaySticker(model.stickerTriggerId);
            }
        }

        public void SetExpression(string id, long timestamp)
        {
            model.expressionTriggerId = id;
            model.expressionTriggerTimestamp = timestamp;
            animator.SetExpressionValues(id, timestamp);
        }

        private void AddWearableController(WearableItem wearable)
        {
            if (wearable == null)
                return;
            switch (wearable.data.category)
            {
                case Categories.EYES:
                    eyesController = new FacialFeatureController(wearable, eyeMaterial);
                    break;
                case Categories.EYEBROWS:
                    eyebrowsController = new FacialFeatureController(wearable, eyebrowMaterial);
                    break;
                case Categories.MOUTH:
                    mouthController = new FacialFeatureController(wearable, mouthMaterial);
                    break;
                case Categories.BODY_SHAPE:
                    break;

                default:
                    var wearableController = new WearableController(wearable);
                    wearableControllers.Add(wearable, wearableController);
                    break;
            }
        }

        //TODO: Remove/replace once the class is easily mockable.
        protected void CopyFrom(AvatarRenderer original)
        {
            this.wearableControllers = original.wearableControllers;
            this.mouthController = original.mouthController;
            this.bodyShapeController = original.bodyShapeController;
            this.eyebrowsController = original.eyebrowsController;
            this.eyesController = original.eyesController;
        }

        public void SetGOVisibility(bool newVisibility)
        {
            //NOTE(Brian): Avatar being loaded needs the renderer.enabled as false until the loading finishes.
            //             So we can' manipulate the values because it'd show an incomplete avatar. Its easier to just deactivate the gameObject.
            if (gameObject.activeSelf != newVisibility)
                gameObject.SetActive(newVisibility);
        }

        public void SetRendererEnabled(bool newVisibility)
        {
            if (mainMeshRenderer == null)
                return;

            mainMeshRenderer.enabled = newVisibility;
        }

        public void SetImpostorVisibility(bool impostorVisibility)
        {
            if (impostorVisibility && !initializedImpostor)
                InitializeImpostor();

            impostorRenderer.gameObject.SetActive(impostorVisibility);
        }

        public void SetImpostorForward(Vector3 newForward) { impostorRenderer.transform.forward = newForward; }

        public void SetImpostorColor(Color newColor) { AvatarRendererHelpers.SetImpostorTintColor(impostorRenderer.material, newColor); }

        public void SetThrottling(int framesBetweenUpdates)
        {
            gpuSkinningFramesBetweenUpdates = framesBetweenUpdates;
            gpuSkinningThrottler?.SetThrottling(gpuSkinningFramesBetweenUpdates);
        }

        public void SetAvatarFade(float avatarFade)
        {
            if (bodyShapeController == null || !bodyShapeController.isReady)
                return;

            Material[] mats = mainMeshRenderer.sharedMaterials;
            for (int j = 0; j < mats.Length; j++)
            {
                mats[j].SetFloat(ShaderUtils.DitherFade, avatarFade);
            }

            OnAvatarAlphaValueUpdate?.Invoke(avatarFade);
        }

        public void SetImpostorFade(float impostorFade)
        {
            //TODO implement dither in Unlit shader
            Color current = impostorRenderer.material.GetColor(BASE_COLOR_PROPERTY);
            current.a = impostorFade;
            impostorRenderer.material.SetColor(BASE_COLOR_PROPERTY, current);

            OnImpostorAlphaValueUpdate?.Invoke(impostorFade);
        }

        private void HideAll()
        {
            // TODO: Cache this somewhere (maybe when the LoadAvatar finishes) instead of fetching this on every call
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }

        public void SetFacialFeaturesVisible(bool visible)
        {
            if (bodyShapeController == null || !bodyShapeController.isReady)
                return;

            if (isLoading)
                return;

            bodyShapeController.SetFacialFeaturesVisible(visible, true);
        }

        public void SetSSAOEnabled(bool ssaoEnabled)
        {
            if ( isLoading )
                return;

            Material[] mats = mainMeshRenderer.sharedMaterials;

            for (int j = 0; j < mats.Length; j++)
            {
                if (ssaoEnabled)
                    mats[j].DisableKeyword("_SSAO_OFF");
                else
                    mats[j].EnableKeyword("_SSAO_OFF");
            }
        }

        private bool MergeAvatar(IEnumerable<SkinnedMeshRenderer> allRenderers)
        {
            var renderersToCombine = allRenderers.Where((r) => !r.transform.parent.gameObject.name.Contains("Mask")).ToList();

            var featureFlags = DataStore.i.featureFlags.flags.Get();
            avatarMeshCombiner.useCullOpaqueHeuristic = featureFlags.IsFeatureEnabled("cull-opaque-heuristic");

            bool success = avatarMeshCombiner.Combine(
                bodyShapeController.upperBodyRenderer,
                renderersToCombine.ToArray(),
                defaultMaterial);

            if ( success )
            {
                avatarMeshCombiner.container.transform.SetParent( transform, true );
                avatarMeshCombiner.container.transform.localPosition = Vector3.zero;
            }

            return success;
        }

        private void CleanMergedAvatar() { avatarMeshCombiner.Dispose(); }

        private void ResetImpostor()
        {
            if (impostorRenderer == null)
                return;

            if (bodySnapshotTexturePromise != null)
                AssetPromiseKeeper_Texture.i.Forget(bodySnapshotTexturePromise);

            AvatarRendererHelpers.ResetImpostor(impostorMeshFilter.mesh, impostorRenderer.material);

            initializedImpostor = false;
        }

        private void LateUpdate()
        {
            if (gpuSkinning != null && mainMeshRenderer.enabled)
                gpuSkinningThrottler.TryUpdate();
        }

        protected virtual void OnDestroy()
        {
            isDestroyed = true;
            CleanupAvatar();
        }
        
        private class WearableMissingRepresentationException : Exception
        {
            public WearableItem wearable { get; }

            public WearableMissingRepresentationException(WearableItem wearable, string bodyShape)
                : base($"Wearable {wearable.id} representation not found for {bodyShape}")
            {
                this.wearable = wearable;
            }
        }
    }
}