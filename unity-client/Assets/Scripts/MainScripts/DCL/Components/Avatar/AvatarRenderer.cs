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
        public Material defaultMaterial;
        public Material eyeMaterial;
        public Material eyebrowMaterial;
        public Material mouthMaterial;

        private Material eyeMaterialCopy;
        private Material eyebrowMaterialCopy;
        private Material mouthMaterialCopy;

        private AvatarModel model;

        public event Action OnSuccessEvent;
        public event Action OnFailEvent;

        internal BodyShapeController bodyShapeController;
        internal Dictionary<string, WearableController> wearableControllers = new Dictionary<string, WearableController>();
        internal FacialFeatureController eyesController;
        internal FacialFeatureController eyebrowsController;
        internal FacialFeatureController mouthController;
        internal AvatarAnimatorLegacy animator;

        internal bool isLoading = false;

        private Coroutine loadCoroutine;
        private HashSet<string> hiddenList = new HashSet<string>();


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

            if (string.IsNullOrEmpty(model.bodyShape))
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

                bodyShapeController = new BodyShapeController(ResolveWearable(model.bodyShape));
                SetupDefaultFacialFeatures(bodyShapeController.bodyShapeId);
            }
            else
            {
                //If bodyShape is downloading will call OnWearableLoadingSuccess (and therefore SetupDefaultMaterial) once ready
                if (bodyShapeController.isReady)
                    bodyShapeController.SetupDefaultMaterial(defaultMaterial, model.skinColor, model.hairColor);
            }

            int wearableCount = model.wearables.Count;

            for (int index = 0; index < wearableCount; index++)
            {
                var wearableId = model.wearables[index];

                if (!wearableControllers.ContainsKey(wearableId))
                {
                    AddWearableController(wearableId);
                }
                else
                {
                    UpdateWearableController(wearableId);
                }
            }

            CleanUpUnusedItems();

            hiddenList = CreateHiddenList();

            if (!bodyShapeController.isReady)
            {
                bodyShapeController.SetHiddenList(hiddenList);
                bodyShapeController.Load(transform, OnWearableLoadingSuccess, OnWearableLoadingFail);
            }

            foreach (var kvp in wearableControllers)
            {
                WearableController wearable = kvp.Value;

                if (changedBody)
                    wearable.boneRetargetingDirty = true;

                wearable.SetHiddenList(hiddenList);

                if (wearable.isReady)
                {
                    wearable.UpdateVisibility();
                    continue;
                }

                wearable.Load(transform, OnWearableLoadingSuccess, OnWearableLoadingFail);
            }

            yield return new WaitUntil(AreWearablesReady);

            bodyShapeController.RemoveUnusedParts();

            eyesController.Load(bodyShapeController, model.eyeColor);
            eyebrowsController.Load(bodyShapeController, model.hairColor);
            mouthController.Load(bodyShapeController, model.skinColor);

            yield return new WaitUntil(IsFaceReady);

            isLoading = false;

            SetWearableBones();
            UpdateExpressions(model.expressionTriggerId, model.expressionTriggerTimestamp);

            OnSuccessEvent?.Invoke();
        }


        void OnWearableLoadingSuccess(WearableController wearableController)
        {
            wearableController.SetupDefaultMaterial(defaultMaterial, model.skinColor, model.hairColor);
        }

        void OnWearableLoadingFail(WearableController wearableController)
        {
            Debug.LogError($"Avatar: {model.name}  -  Failed loading wearable: {wearableController.id}");
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
            HashSet<string> hiddenCategories = new HashSet<string>();
            if (model?.wearables != null)
            {
                //Last wearable added has priority over the rest
                for (int i = model.wearables.Count - 1; i >= 0; i--)
                {
                    string id = model.wearables[i];
                    if (!wearableControllers.ContainsKey(id)) continue;

                    var wearable = wearableControllers[id].wearable;

                    if (hiddenCategories.Contains(wearable.category)) //Skip hidden elements to avoid two elements hiding each other
                        continue;

                    var wearableHidesList = wearable.GetHidesList(bodyShapeController.bodyShapeId);
                    if (wearableHidesList != null)
                    {
                        hiddenCategories.UnionWith(wearableHidesList);
                    }
                }
            }

            return hiddenCategories;
        }

        private void AddWearableController(string wearableId)
        {
            var wearable = ResolveWearable(wearableId);
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
                    var wearableController = new WearableController(ResolveWearable(wearableId), bodyShapeController.id);
                    wearableControllers.Add(wearableId, wearableController);
                    break;
            }
        }

        private void UpdateWearableController(string wearableId)
        {
            var wearable = wearableControllers[wearableId];
            switch (wearable.category)
            {
                case WearableLiterals.Categories.EYES:
                case WearableLiterals.Categories.EYEBROWS:
                case WearableLiterals.Categories.MOUTH:
                case WearableLiterals.Categories.BODY_SHAPE:
                    break;
                default:
                    //If wearable is downloading will call OnWearableLoadingSuccess(and therefore SetupDefaultMaterial) once ready
                    if (wearable.isReady)
                        wearable.SetupDefaultMaterial(defaultMaterial, model.skinColor, model.hairColor);
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
            string eyesDefaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, WearableLiterals.Categories.EYES);
            eyesController = new FacialFeatureController(ResolveWearable(eyesDefaultId), bodyShapeController.bodyShapeId, eyeMaterial);

            string eyebrowsDefaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, WearableLiterals.Categories.EYEBROWS);
            eyebrowsController = new FacialFeatureController(ResolveWearable(eyebrowsDefaultId), bodyShapeController.bodyShapeId, eyebrowMaterial);

            string mouthDefaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, WearableLiterals.Categories.MOUTH);
            mouthController = new FacialFeatureController(ResolveWearable(mouthDefaultId), bodyShapeController.bodyShapeId, mouthMaterial);
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

        private void OnDisable()
        {
            if (isLoading)
            {
                CleanupAvatar();
            }
        }

        protected virtual void OnDestroy()
        {
            CleanupAvatar();
        }
    }
}