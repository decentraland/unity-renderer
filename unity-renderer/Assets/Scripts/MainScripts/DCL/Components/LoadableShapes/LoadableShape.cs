using System;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public class LoadableShape : BaseShape, IAssetCatalogReferenceHolder
    {
        [Serializable]
        public new class Model : BaseShape.Model
        {
            public string src;
            public string assetId;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                switch (pbModel.PayloadCase)
                {
                    case ComponentBodyPayload.PayloadOneofCase.GltfShape:
                        var gltfModel = new Model();
                        if (pbModel.GltfShape.HasSrc) gltfModel.src = pbModel.GltfShape.Src;
                        if (pbModel.GltfShape.HasWithCollisions) gltfModel.withCollisions = pbModel.GltfShape.WithCollisions;
                        if (pbModel.GltfShape.HasVisible) gltfModel.visible = pbModel.GltfShape.Visible;
                        if (pbModel.GltfShape.HasIsPointerBlocker) gltfModel.isPointerBlocker = pbModel.GltfShape.IsPointerBlocker;

                        return gltfModel;
                    case ComponentBodyPayload.PayloadOneofCase.ObjShape:
                        var objModel = new Model();
                        if (pbModel.ObjShape.HasSrc) objModel.src = pbModel.ObjShape.Src;
                        if (pbModel.ObjShape.HasWithCollisions) objModel.withCollisions = pbModel.ObjShape.WithCollisions;
                        if (pbModel.ObjShape.HasVisible) objModel.visible = pbModel.ObjShape.Visible;
                        if (pbModel.ObjShape.HasIsPointerBlocker) objModel.isPointerBlocker = pbModel.ObjShape.IsPointerBlocker;

                        return objModel;
                    default:
                        return Utils.SafeUnimplemented<LoadableShape, Model>(
                            expected: ComponentBodyPayload.PayloadOneofCase.GltfShape, actual: pbModel.PayloadCase);
                }
            }
        }

        public bool isLoaded { get; protected set; }

        public Action<LoadableShape> OnLoaded;

        protected Model previousModel = new ();

        protected LoadableShape()
        {
            model = new Model();
        }

        public override int GetClassId() =>
            -1;

        public override IEnumerator ApplyChanges(BaseModel newModel) =>
            null;

        public override bool IsVisible() =>
            ((Model)this.model).visible;

        public override bool HasCollisions() =>
            ((Model)this.model).withCollisions;

        public string GetAssetId() =>
            ((Model)this.model).assetId;
    }

    public class LoadableShape<LoadWrapperType, LoadWrapperModelType> : LoadableShape
        where LoadWrapperType: LoadWrapper, new()
        where LoadWrapperModelType: LoadableShape.Model, new()
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

            set
            {
                base.model = value;
            }
        }

        new protected LoadWrapperModelType previousModel
        {
            get
            {
                if (base.previousModel == null)
                    base.previousModel = new LoadWrapperModelType();

                return base.previousModel as LoadWrapperModelType;
            }

            set
            {
                base.previousModel = value;
            }
        }

        public LoadableShape()
        {
            OnDetach += DetachShape;
            OnAttach += AttachShape;
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            LoadWrapperModelType model = (LoadWrapperModelType)newModel;

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

                LoadWrapperType loadableShape =
                    Environment.i.world.state.GetOrAddLoaderForEntity<LoadWrapperType>(entity);

                if (loadableShape is LoadWrapper_GLTF gltfLoadWrapper)
                    gltfLoadWrapper.customContentProvider = provider;

                entity.meshesInfo.currentShape = this;

                loadableShape.entity = entity;

                bool initialVisibility = model.visible;

                if (!DataStore.i.debugConfig.isDebugMode.Get())
                    initialVisibility &= entity.isInsideSceneBoundaries;

                loadableShape.initialVisibility = initialVisibility;

                loadableShape.Load(model.src, OnLoadCompleted, OnLoadFailed);
            }
            else
            {
                var message = $"LoadableShape '{model.src}' not found";

                if (DataStore.i.debugConfig.isDebugMode.Get())
                {
                    Debug.LogError(message);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning(message);
#endif
                }

                failed = true;
            }
        }

        void ConfigureVisibility(IDCLEntity entity)
        {
            var loadable = Environment.i.world.state.GetLoaderForEntity(entity);

            bool initialVisibility = model.visible;

            if (!DataStore.i.debugConfig.isDebugMode.Get())
                initialVisibility &= entity.isInsideSceneBoundaries;

            if (loadable != null)
                loadable.initialVisibility = initialVisibility;

            ConfigureVisibility(entity.meshRootGameObject, initialVisibility, entity.meshesInfo.renderers);

            UpdateAnimationStatus(entity, initialVisibility);
        }

        private void UpdateAnimationStatus(IDCLEntity entity, bool enabled)
        {
            if (!scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.ANIMATOR) &&
                entity.meshesInfo.animation != null)
                entity.meshesInfo.animation.enabled = enabled;
        }

        protected virtual void ConfigureColliders(IDCLEntity entity)
        {
            CollidersManager.i.ConfigureColliders(entity.meshRootGameObject, model.withCollisions && entity.isInsideSceneBoundaries, true, entity, CalculateCollidersLayer(model));
        }

        protected void OnLoadFailed(LoadWrapper loadWrapper, Exception exception)
        {
            if (loadWrapper != null)
                CleanFailedWrapper(loadWrapper);

            failed = true;
            OnFinishCallbacks?.Invoke(this);
            OnFinishCallbacks = null;
        }

        private void CleanFailedWrapper(LoadWrapper loadWrapper)
        {
            if (loadWrapper == null)
                return;

            if (loadWrapper.entity == null)
                return;

            if (loadWrapper.entity.gameObject == null)
                return;

            GameObject go = loadWrapper.entity.gameObject;

            go.name += " - Failed loading";
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

            entity.meshesInfo.meshRootGameObject = entity.meshRootGameObject;
            entity.OnInsideBoundariesChanged += OnInsideBoundariesChanged;

            ConfigureVisibility(entity);
            ConfigureColliders(entity);
            RaiseOnShapeUpdated(entity);
            RaiseOnShapeLoaded(entity);

            OnFinishCallbacks?.Invoke(this);
            OnFinishCallbacks = null;
        }

        private void OnInsideBoundariesChanged(IDCLEntity entity, bool isInsideBoundaries)
        {
            UpdateAnimationStatus(entity, isInsideBoundaries);
        }

        protected virtual void DetachShape(IDCLEntity entity)
        {
            if (entity == null || entity.meshRootGameObject == null)
                return;

            LoadWrapper loadWrapper = Environment.i.world.state.GetLoaderForEntity(entity);

            if (loadWrapper != null)
            {
                entity.OnInsideBoundariesChanged -= OnInsideBoundariesChanged;
                loadWrapper.Unload();
            }

            Environment.i.world.state.RemoveLoaderForEntity(entity);
            entity.meshesInfo.CleanReferences();
        }

        public override void CallWhenReady(Action<ISharedComponent> callback)
        {
            if (attachedEntities.Count == 0 || isLoaded || failed) { callback.Invoke(this); }
            else { OnFinishCallbacks += callback; }
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
