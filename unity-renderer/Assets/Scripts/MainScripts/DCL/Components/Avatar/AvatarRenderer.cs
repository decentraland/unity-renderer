using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class AvatarRenderer : MonoBehaviour
    {
        private const int MAX_RETRIES = 5;

        public Material defaultMaterial;
        public Material eyeMaterial;
        public Material eyebrowMaterial;
        public Material mouthMaterial;

        private AvatarModel model;

        public event Action OnSuccessEvent;
        public event Action OnFailEvent;

        internal BodyShapeController bodyShapeController;
        internal Dictionary<WearableItem, WearableController> wearableControllers = new Dictionary<WearableItem, WearableController>();
        internal FacialFeatureController eyesController;
        internal FacialFeatureController eyebrowsController;
        internal FacialFeatureController mouthController;
        internal AvatarAnimatorLegacy animator;

        internal bool isLoading = false;

        private Coroutine loadCoroutine;
        private HashSet<string> hiddenList = new HashSet<string>();

        private List<WearableItem> resolvedWearables = new List<WearableItem>();
        private WearableItem resolvedBody;

        private void Awake()
        {
            animator = GetComponent<AvatarAnimatorLegacy>();
        }

        public void ApplyModel(AvatarModel model, Action onSuccess, Action onFail)
        {
            if (this.model != null && model != null && this.model.Equals(model))
            {
                onSuccess?.Invoke();
                return;
            }

            this.model = new AvatarModel();
            this.model.CopyFrom(model);

            Action onSuccessWrapper = null;
            Action onFailWrapper = null;

            onSuccessWrapper = () =>
            {
                onSuccess?.Invoke();
                this.OnSuccessEvent -= onSuccessWrapper;
            };

            onFailWrapper = () =>
            {
                onFail?.Invoke();
                this.OnFailEvent -= onFailWrapper;
            };

            this.OnSuccessEvent += onSuccessWrapper;
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

        void StopLoadingCoroutines()
        {
            if (loadCoroutine != null)
                CoroutineStarter.Stop(loadCoroutine);

            loadCoroutine = null;
        }

        public void CleanupAvatar()
        {
            StopLoadingCoroutines();

            eyebrowsController?.CleanUp();
            eyebrowsController = null;

            eyesController?.CleanUp();
            eyesController = null;

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
            hiddenList.Clear();
            isLoading = false;
            OnFailEvent = null;
            OnSuccessEvent = null;
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

                if (!model.wearables.Contains(wearable.id))
                {
                    wearable.CleanUp();
                    wearableControllers.Remove(currentId);
                }
            }

            if (!model.wearables.Contains(eyebrowsController.wearable.id))
            {
                eyebrowsController.CleanUp();
            }

            if (!model.wearables.Contains(eyesController.wearable.id))
            {
                eyesController.CleanUp();
            }

            if (!model.wearables.Contains(mouthController.wearable.id))
            {
                mouthController.CleanUp();
            }
        }


        private IEnumerator LoadAvatar()
        {
            yield return new DCL.WaitUntil(() => gameObject.activeSelf);

            if (model.wearables != null)
            {
                for (int i = 0; i < model.wearables.Count; i++)
                {
                    WearableItem item = ResolveWearable(model.wearables[i]);

                    if (item == null)
                        continue;

                    resolvedWearables.Add(item);
                }
            }

            resolvedBody = ResolveWearable(model.bodyShape);

            if (resolvedBody == null)
            {
                isLoading = false;
                this.OnSuccessEvent?.Invoke();
                yield break;
            }

            bool changedBody = false;

            if (bodyShapeController != null && bodyShapeController.id != model?.bodyShape)
            {
                bodyShapeController?.CleanUp();
                bodyShapeController = null;
                changedBody = true;
            }

            if (bodyShapeController == null)
            {
                HideAll();
                bodyShapeController = new BodyShapeController(resolvedBody);
                SetupDefaultFacialFeatures(bodyShapeController.bodyShapeId);
            }
            else
            {
                //If bodyShape is downloading will call OnWearableLoadingSuccess (and therefore SetupDefaultMaterial) once ready
                if (bodyShapeController.isReady)
                    bodyShapeController.SetupDefaultMaterial(defaultMaterial, model.skinColor, model.hairColor);
            }

            HashSet<string> usedCategories = new HashSet<string>(WearableLiterals.Categories.ALL);
            int wearableCount = resolvedWearables.Count;

            for (int index = 0; index < wearableCount; index++)
            {
                var wearable = resolvedWearables[index];

                if (wearable == null)
                    continue;

                if (usedCategories.Contains(wearable.category))
                {
                    usedCategories.Remove(wearable.category);
                }

                if (!wearableControllers.ContainsKey(wearable))
                {
                    AddWearableController(wearable);
                }
                else
                {
                    UpdateWearableController(wearable);
                }
            }

            foreach (var category in usedCategories)
            {
                switch (category)
                {
                    case WearableLiterals.Categories.EYES:
                        SetDefaultEyes(bodyShapeController.bodyShapeId);
                        break;
                    case WearableLiterals.Categories.MOUTH:
                        SetDefaultMouth(bodyShapeController.bodyShapeId);
                        break;
                    case WearableLiterals.Categories.EYEBROWS:
                        SetDefaultEyebrows(bodyShapeController.bodyShapeId);
                        break;
                }
            }

            CleanUpUnusedItems();

            hiddenList = CreateHiddenList();

            bodyShapeController.SetHiddenList(hiddenList);
            if (!bodyShapeController.isReady)
            {
                bodyShapeController.Load(transform, OnWearableLoadingSuccess, OnBodyShapeLoadingFail);
            }

            foreach (var kvp in wearableControllers)
            {
                WearableController wearable = kvp.Value;

                if (changedBody)
                    wearable.boneRetargetingDirty = true;

                wearable.SetHiddenList(hiddenList);

                wearable.Load(transform, OnWearableLoadingSuccess, (x) => OnWearableLoadingFail(x));
            }

            yield return new WaitUntil(AreWearablesReady);

            eyesController.Load(bodyShapeController, model.eyeColor);
            eyebrowsController.Load(bodyShapeController, model.hairColor);
            mouthController.Load(bodyShapeController, model.skinColor);

            yield return new WaitUntil(IsFaceReady);

            bodyShapeController.RemoveUnusedParts(usedCategories);

            bodyShapeController.UpdateVisibility();

            foreach (var kvp in wearableControllers)
            {
                kvp.Value.UpdateVisibility();
            }

            isLoading = false;

            SetWearableBones();
            UpdateExpressions(model.expressionTriggerId, model.expressionTriggerTimestamp);

            OnSuccessEvent?.Invoke();
        }


        void OnWearableLoadingSuccess(WearableController wearableController)
        {
            wearableController.SetupDefaultMaterial(defaultMaterial, model.skinColor, model.hairColor);
        }

        void OnBodyShapeLoadingFail(WearableController wearableController)
        {
            Debug.LogError($"Avatar: {model.name}  -  Failed loading bodyshape: {wearableController.id}");
            AbortLoading();
        }

        void OnWearableLoadingFail(WearableController wearableController, int retriesCount = MAX_RETRIES)
        {
            if (retriesCount <= 0)
            {
                Debug.LogError($"Avatar: {model.name}  -  Failed loading wearable: {wearableController.id}");
                AbortLoading();
                return;
            }

            wearableController.Load(transform, OnWearableLoadingSuccess, (x) => OnWearableLoadingFail(x, retriesCount - 1));
        }

        void AbortLoading()
        {
            CleanupAvatar();
            isLoading = false;
            OnFailEvent?.Invoke();
        }

        bool IsFaceReady()
        {
            return eyebrowsController.isReady && eyesController.isReady && mouthController.isReady;
        }

        bool AreWearablesReady()
        {
            if (!bodyShapeController.isReady)
                return false;

            using (var iterator = wearableControllers.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var wearable = iterator.Current.Value;
                    if (!wearable.isReady)
                        return false;
                }
            }

            return true;
        }

        private void SetWearableBones()
        {
            //NOTE(Brian): Set bones/rootBone of all wearables to be the same of the baseBody,
            //             so all of them are animated together.
            var mainSkinnedRenderer = bodyShapeController.skinnedMeshRenderer;

            using (var enumerator = wearableControllers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Value.SetAnimatorBones(mainSkinnedRenderer);
                }
            }
        }

        public void UpdateExpressions(string id, long timestamp)
        {
            model.expressionTriggerId = id;
            model.expressionTriggerTimestamp = timestamp;
            animator.SetExpressionValues(id, timestamp);
        }


        private HashSet<string> CreateHiddenList()
        {
            HashSet<string> result = new HashSet<string>();

            if (resolvedWearables.Count == 0)
                return result;

            //Last wearable added has priority over the rest
            for (int i = resolvedWearables.Count - 1; i >= 0; i--)
            {
                var wearableItem = resolvedWearables[i];

                if (wearableItem == null || !wearableControllers.ContainsKey(wearableItem))
                    continue;

                if (result.Contains(wearableItem.category)) //Skip hidden elements to avoid two elements hiding each other
                    continue;

                var wearableHidesList = wearableItem.GetHidesList(bodyShapeController.bodyShapeId);

                if (wearableHidesList != null)
                {
                    result.UnionWith(wearableHidesList);
                }
            }

            return result;
        }

        private void AddWearableController(WearableItem wearable)
        {
            if (wearable == null) return;
            switch (wearable.category)
            {
                case WearableLiterals.Categories.EYES:
                    eyesController = new FacialFeatureController(wearable, bodyShapeController.bodyShapeId, eyeMaterial);
                    break;
                case WearableLiterals.Categories.EYEBROWS:
                    eyebrowsController = new FacialFeatureController(wearable, bodyShapeController.bodyShapeId, eyebrowMaterial);
                    break;
                case WearableLiterals.Categories.MOUTH:
                    mouthController = new FacialFeatureController(wearable, bodyShapeController.bodyShapeId, mouthMaterial);
                    break;
                case WearableLiterals.Categories.BODY_SHAPE:
                    break;

                default:
                    var wearableController = new WearableController(wearable, bodyShapeController.id);
                    wearableControllers.Add(wearable, wearableController);
                    break;
            }
        }

        private void UpdateWearableController(WearableItem wearable)
        {
            var wearableController = wearableControllers[wearable];
            switch (wearableController.category)
            {
                case WearableLiterals.Categories.EYES:
                case WearableLiterals.Categories.EYEBROWS:
                case WearableLiterals.Categories.MOUTH:
                case WearableLiterals.Categories.BODY_SHAPE:
                    break;
                default:
                    //If wearable is downloading will call OnWearableLoadingSuccess(and therefore SetupDefaultMaterial) once ready
                    if (wearableController.isReady)
                        wearableController.SetupDefaultMaterial(defaultMaterial, model.skinColor, model.hairColor);
                    break;
            }
        }

        WearableItem ResolveWearable(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!CatalogController.wearableCatalog.TryGetValue(id, out WearableItem wearable))
            {
                Debug.LogError($"Wearable {id} not found in catalog");
            }

            return wearable;
        }

        private void SetupDefaultFacialFeatures(string bodyShape)
        {
            SetDefaultEyes(bodyShape);
            SetDefaultEyebrows(bodyShape);
            SetDefaultMouth(bodyShape);
        }

        private void SetDefaultEyes(string bodyShape)
        {
            string eyesDefaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, WearableLiterals.Categories.EYES);
            WearableItem eyesDefaultWearable = ResolveWearable(eyesDefaultId);

            if (eyesDefaultWearable == null)
                return;

            eyesController = new FacialFeatureController(eyesDefaultWearable, bodyShapeController.bodyShapeId, eyeMaterial);
        }

        private void SetDefaultEyebrows(string bodyShape)
        {
            string eyebrowsDefaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, WearableLiterals.Categories.EYEBROWS);
            WearableItem eyebrowsDefaultWearable = ResolveWearable(eyebrowsDefaultId);

            if (eyebrowsDefaultWearable == null)
                return;

            eyebrowsController = new FacialFeatureController(eyebrowsDefaultWearable, bodyShapeController.bodyShapeId, eyebrowMaterial);
        }

        private void SetDefaultMouth(string bodyShape)
        {
            string mouthDefaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, WearableLiterals.Categories.MOUTH);
            WearableItem mouthWearable = ResolveWearable(mouthDefaultId);

            if (mouthWearable == null)
                return;

            mouthController = new FacialFeatureController(mouthWearable, bodyShapeController.bodyShapeId, mouthMaterial);
        }

        protected void CopyFrom(AvatarRenderer original)
        {
            this.wearableControllers = original.wearableControllers;
            this.mouthController = original.mouthController;
            this.bodyShapeController = original.bodyShapeController;
            this.eyebrowsController = original.eyebrowsController;
            this.eyesController = original.eyesController;
        }

        public void SetVisibility(bool newVisibility)
        {
            //NOTE(Brian): Avatar being loaded needs the renderer.enabled as false until the loading finishes.
            //             So we can' manipulate the values because it'd show an incomplete avatar. Its easier to just deactivate the gameObject.
            if (gameObject.activeSelf != newVisibility)
                gameObject.SetActive(newVisibility);
        }

        private void HideAll()
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }

        protected virtual void OnDestroy()
        {
            CleanupAvatar();
        }
    }
}