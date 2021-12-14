using System;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;

namespace DCL.Components
{
    public class LoadableShape : BaseShape, IAssetCatalogReferenceHolder
    {
        [System.Serializable]
        public new class Model : BaseShape.Model
        {
            public string src;
            public string assetId;

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }
        }

        public bool isLoaded { get; protected set; }

        public Action<LoadableShape> OnLoaded;

        protected Model previousModel = new Model();

        protected static Dictionary<GameObject, LoadWrapper> attachedLoaders = new Dictionary<GameObject, LoadWrapper>();

        public static LoadWrapper GetLoaderForEntity(IDCLEntity entity)
        {
            if (entity.meshRootGameObject == null)
            {
                Debug.LogWarning("NULL meshRootGameObject at GetLoaderForEntity()");
                return null;
            }

            attachedLoaders.TryGetValue(entity.meshRootGameObject, out LoadWrapper result);
            return result;
        }

        public static T GetOrAddLoaderForEntity<T>(IDCLEntity entity)
            where T : LoadWrapper, new()
        {
            if (!attachedLoaders.TryGetValue(entity.meshRootGameObject, out LoadWrapper result))
            {
                result = new T();
                attachedLoaders.Add(entity.meshRootGameObject, result);
            }

            return result as T;
        }

        public LoadableShape() { model = new Model(); }

        public override int GetClassId() { return -1; }

        public override IEnumerator ApplyChanges(BaseModel newModel) { return null; }

        public override bool IsVisible()
        {
            Model model = (Model) this.model;
            return model.visible;
        }

        public override bool HasCollisions()
        {
            Model model = (Model) this.model;
            return model.withCollisions;
        }

        public string GetAssetId()
        {
            Model model = (Model) this.model;
            return model.assetId;
        }
    }

    public class LoadableShape<LoadWrapperType, LoadWrapperModelType> : LoadableShape
        where LoadWrapperType : LoadWrapper, new()
        where LoadWrapperModelType : LoadableShape.Model, new()
    {
        private bool failed = false;
        private event Action<BaseDisposable> OnFinishCallbacks;
        public System.Action<IDCLEntity> OnEntityShapeUpdated;

        new public LoadWrapperModelType model
        {
            get
            {
                if (base.model == null)
                    base.model = new LoadWrapperModelType();

                return base.model as LoadWrapperModelType;
            }
            set { base.model = value; }
        }

        new protected LoadWrapperModelType previousModel
        {
            get
            {
                if (base.previousModel == null)
                    base.previousModel = new LoadWrapperModelType();

                return base.previousModel as LoadWrapperModelType;
            }
            set { base.previousModel = value; }
        }

        public LoadableShape()
        {
            OnDetach += DetachShape;
            OnAttach += AttachShape;
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            LoadWrapperModelType model = (LoadWrapperModelType) newModel;

            bool updateVisibility = true;
            bool updateCollisions = true;
            bool triggerAttachment = true;

            if (previousModel != null)
            {
                updateVisibility = previousModel.visible != model.visible;
                updateCollisions = previousModel.withCollisions != model.withCollisions || previousModel.isPointerBlocker != model.isPointerBlocker;
                triggerAttachment = (!string.IsNullOrEmpty(model.src) && previousModel.src != model.src) ||
                                    (!string.IsNullOrEmpty(model.assetId) && previousModel.assetId != model.assetId);
            }

            foreach (var entity in attachedEntities)
            {
                if (triggerAttachment)
                    AttachShape(entity);

                if (updateVisibility)
                    ConfigureVisibility(entity);

                if (updateCollisions)
                    ConfigureColliders(entity);

                RaiseOnShapeUpdated(entity);
            }

            previousModel = model;
            return null;
        }

        protected virtual void AttachShape(IDCLEntity entity)
        {
            ContentProvider provider = null;

            if (!string.IsNullOrEmpty(model.assetId))
            {
                provider = AssetCatalogBridge.i.GetContentProviderForAssetIdInSceneObjectCatalog(model.assetId);
                if (string.IsNullOrEmpty(model.src))
                {
                    SceneObject sceneObject = AssetCatalogBridge.i.GetSceneObjectById(model.assetId);
                    if (sceneObject == null)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning($"LoadableShape '{model.assetId}' not found in catalog, probably asset pack deleted");
#endif
                        failed = true;
                        OnLoadFailed(null, new Exception($"LoadableShape '{model.assetId}' not found in catalog, probably asset pack deleted"));
                        return;
                    }

                    model.src = sceneObject.model;
                }
            }

            if (provider == null)
                provider = scene.contentProvider;

            if (provider.HasContentsUrl(model.src))
            {
                isLoaded = false;
                entity.EnsureMeshGameObject(componentName + " mesh");

                LoadWrapperType loadableShape = GetOrAddLoaderForEntity<LoadWrapperType>(entity);

                if (loadableShape is LoadWrapper_GLTF gltfLoadWrapper)
                    gltfLoadWrapper.customContentProvider = provider;

                entity.meshesInfo.currentShape = this;

                loadableShape.entity = entity;
                loadableShape.useVisualFeedback = Configuration.ParcelSettings.VISUAL_LOADING_ENABLED;
                loadableShape.initialVisibility = model.visible;
                loadableShape.Load(model.src, OnLoadCompleted, OnLoadFailed);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"LoadableShape '{model.src}' not found in scene '{scene.sceneData.id}' mappings");
#endif
                failed = true;
            }
        }

        void ConfigureVisibility(IDCLEntity entity)
        {
            var loadable = GetLoaderForEntity(entity);

            if (loadable != null)
                loadable.initialVisibility = model.visible;

            ConfigureVisibility(entity.meshRootGameObject, model.visible, entity.meshesInfo.renderers);
        }

        protected virtual void ConfigureColliders(IDCLEntity entity) { CollidersManager.i.ConfigureColliders(entity.meshRootGameObject, model.withCollisions, true, entity, CalculateCollidersLayer(model)); }

        protected void OnLoadFailed(LoadWrapper loadWrapper, Exception exception)
        {
            if (loadWrapper != null)
                CleanFailedWrapper(loadWrapper);

            failed = true;
            OnFinishCallbacks?.Invoke(this);
            OnFinishCallbacks = null;
        }

        void CleanFailedWrapper(LoadWrapper loadWrapper)
        {
            if (loadWrapper == null)
                return;
            if (loadWrapper.entity == null)
                return;
            if (loadWrapper.entity.gameObject == null)
                return;

            GameObject go = loadWrapper.entity.gameObject;

            go.name += " - Failed loading";

            MaterialTransitionController[] transitionController =
                go.GetComponentsInChildren<MaterialTransitionController>(true);

            for (int i = 0; i < transitionController.Length; i++)
            {
                MaterialTransitionController material = transitionController[i];
                Object.Destroy(material);
            }
        }

        protected void OnLoadCompleted(LoadWrapper loadWrapper)
        {
            IDCLEntity entity = loadWrapper.entity;

            if (entity.meshesInfo.currentShape == null)
            {
                OnLoadFailed(loadWrapper, new Exception($"Entity {entity.entityId} current shape of mesh information is null"));
                return;
            }

            isLoaded = true;
            OnLoaded?.Invoke(this);

            entity.meshesInfo.renderers = entity.meshRootGameObject.GetComponentsInChildren<Renderer>();

            var model = (Model) (entity.meshesInfo.currentShape as LoadableShape).GetModel();

            ConfigureVisibility(entity.meshRootGameObject, model.visible, loadWrapper.entity.meshesInfo.renderers);
            ConfigureColliders(entity);
            RaiseOnShapeUpdated(entity);
            RaiseOnShapeLoaded(entity);

            OnFinishCallbacks?.Invoke(this);
            OnFinishCallbacks = null;
        }

        protected virtual void DetachShape(IDCLEntity entity)
        {
            if (entity == null || entity.meshRootGameObject == null)
                return;

            LoadWrapper loadWrapper = GetLoaderForEntity(entity);

            loadWrapper?.Unload();

            entity.meshesInfo.CleanReferences();
        }

        public override void CallWhenReady(Action<ISharedComponent> callback)
        {
            if (attachedEntities.Count == 0 || isLoaded || failed)
            {
                callback.Invoke(this);
            }
            else
            {
                OnFinishCallbacks += callback;
            }
        }

        private void RaiseOnShapeUpdated(IDCLEntity entity)
        {
            if (!isLoaded)
                return;

            entity.OnShapeUpdated?.Invoke(entity);
        }

        private void RaiseOnShapeLoaded(IDCLEntity entity)
        {
            if (!isLoaded)
                return;

            entity.OnShapeLoaded?.Invoke(entity);
        }
    }
}