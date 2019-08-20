using System;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Components
{
    public class LoadableShape : BaseShape
    {
        [System.Serializable]
        public new class Model : BaseShape.Model
        {
            public string src;
        }

        public Model model = new Model();
        protected Model previousModel = new Model();

        public LoadableShape(ParcelScene scene) : base(scene)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            return null;
        }
    }

    public class LoadableShape<LoadWrapperType, LoadWrapperModelType> : LoadableShape
        where LoadWrapperType : LoadWrapper
        where LoadWrapperModelType : LoadableShape.Model, new()
    {
        private bool isLoaded = false;
        private bool failed = true;
        private event Action<BaseDisposable> OnReadyCallbacks;
        public System.Action<DecentralandEntity> OnEntityShapeUpdated;

        new public LoadWrapperModelType model
        {
            get { 
                    if(base.model == null)
                        base.model = new LoadWrapperModelType();

                    return base.model as LoadWrapperModelType;
                }
            set { base.model = value; }
        }

        new protected LoadWrapperModelType previousModel
        {
            get { 
                    if(base.previousModel == null)
                        base.previousModel = new LoadWrapperModelType();

                    return base.previousModel as LoadWrapperModelType;
                }
            set { base.previousModel = value; }
        }

        public LoadableShape(ParcelScene scene) : base(scene)
        {
            OnDetach += DetachShape;
            OnAttach += AttachShape;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            previousModel = model;
            model = SceneController.i.SafeFromJson<LoadWrapperModelType>(newJson);

            bool updateVisibility = previousModel.visible != model.visible;
            bool updateCollisions = previousModel.withCollisions != model.withCollisions;
            bool triggerAttachment = !string.IsNullOrEmpty(model.src) && previousModel.src != model.src;
            
            foreach (var entity in attachedEntities)
            {
                if(triggerAttachment)
                    AttachShape(entity);
                    
                if (updateVisibility)
                    ConfigureVisibility(entity);

                if (updateCollisions)
                    ConfigureColliders(entity);

                OnEntityShapeUpdated?.Invoke(entity);
            }

            return null;
        }

        protected virtual void AttachShape(DecentralandEntity entity)
        {
            if (scene.contentProvider.HasContentsUrl(model.src))
            {
                isLoaded = false;
                entity.EnsureMeshGameObject(componentName + " mesh");
                LoadWrapperType loadableShape = entity.meshGameObject.GetOrCreateComponent<LoadWrapperType>();
                loadableShape.entity = entity;
                loadableShape.useVisualFeedback = Configuration.ParcelSettings.VISUAL_LOADING_ENABLED;
                loadableShape.initialVisibility = model.visible;
                loadableShape.contentProvider = scene.contentProvider;
                loadableShape.Load(model.src, OnLoadCompleted, OnLoadFailed);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"LoadableShape '{model.src}' not found in scene '{scene.sceneData.id}' mappings");
#endif
                failed = true;
            }
        }

        void ConfigureVisibility(DecentralandEntity entity)
        {
            var loadable = entity.meshGameObject.GetComponentInChildren<LoadWrapper>();

            if (loadable != null)
                loadable.initialVisibility = model.visible;

            ConfigureVisibility(entity.meshGameObject, model.visible);
        }

        protected virtual void ConfigureColliders(DecentralandEntity entity)
        {
            ConfigureColliders(entity.meshGameObject, model.withCollisions, true);
        }

        protected void OnLoadFailed(LoadWrapper loadWrapper)
        {
            loadWrapper.gameObject.name += " - Failed loading";

            MaterialTransitionController[] transitionController =
                loadWrapper.GetComponentsInChildren<MaterialTransitionController>(true);

            for (int i = 0; i < transitionController.Length; i++)
            {
                MaterialTransitionController material = transitionController[i];
                Object.Destroy(material);
            }
            failed = true;
            OnReadyCallbacks?.Invoke(this);
            OnReadyCallbacks = null;
        }

        protected void OnLoadCompleted(LoadWrapper loadWrapper)
        {
            isLoaded = true;
            DecentralandEntity entity = loadWrapper.entity;

            if (entity.currentShape == null)
            {
                return;
            }

            var model = (entity.currentShape as LoadableShape).model;
            ConfigureVisibility(loadWrapper.entity.meshGameObject, model.visible);
            ConfigureColliders(entity);

            entity.OnComponentUpdated?.Invoke(loadWrapper);
            entity.OnShapeUpdated?.Invoke(entity);
            OnReadyCallbacks?.Invoke(this);
            OnReadyCallbacks = null;
        }

        private void DetachShape(DecentralandEntity entity)
        {
            if (entity == null || entity.meshGameObject == null)
            {
                return;
            }

            LoadWrapper loadWrapper = entity.meshGameObject.GetComponent<LoadWrapper>();

            loadWrapper?.Unload();
        }
        
        public override void CallWhenReady(Action<BaseDisposable> callback)
        {
            if (attachedEntities.Count == 0 || isLoaded || failed)
            {
                callback.Invoke(this);
            }
            else
            {
                OnReadyCallbacks += callback;
            }
        }
    }
}
