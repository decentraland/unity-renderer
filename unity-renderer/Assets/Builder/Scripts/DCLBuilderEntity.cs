using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections;
using UnityEngine;

namespace Builder
{
    public class DCLBuilderEntity : MonoBehaviour
    {
        public static Action<DCLBuilderEntity> OnEntityShapeUpdated;
        public static Action<DCLBuilderEntity> OnEntityTransformUpdated;
        public static Action<DCLBuilderEntity> OnEntityAddedWithTransform;

        public DecentralandEntity rootEntity { protected set; get; }
        public bool hasGizmoComponent
        {
            get
            {
                if (rootEntity != null)
                {
                    return rootEntity.components.ContainsKey(CLASS_ID_COMPONENT.GIZMOS);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool hasSmartItemComponent
        {
            get
            {
                if (rootEntity != null)
                {
                    return rootEntity.components.ContainsKey(CLASS_ID_COMPONENT.SMART_ITEM);
                }
                else
                {
                    return false;
                }
            }
        }

        private DCLBuilderSelectionCollider[] meshColliders;
        private Animation[] meshAnimations;
        private Action OnShapeLoaded;

        private bool isTransformComponentSet;
        private bool isShapeComponentSet;

        private Vector3 scaleTarget;
        private bool isScalingAnimation = false;

        public void SetEntity(DecentralandEntity entity)
        {
            rootEntity = entity;

            entity.OnShapeUpdated -= OnShapeUpdated;
            entity.OnShapeUpdated += OnShapeUpdated;

            entity.OnTransformChange -= OnTransformUpdated;
            entity.OnTransformChange += OnTransformUpdated;

            entity.OnRemoved -= OnEntityRemoved;
            entity.OnRemoved += OnEntityRemoved;

            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;
            DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;

            //builder evaluate boundaries by itself
            if (DCL.SceneController.i.useBoundariesChecker)
                entity.OnShapeUpdated -= DCL.SceneController.i.boundariesChecker.EvaluateEntityPosition;

            gameObject.transform.localScale = Vector3.zero;

            isTransformComponentSet = false;
            isShapeComponentSet = false;

            scaleTarget = Vector3.one;

            DestroyColliders();

            if (HasShape())
            {
                OnShapeUpdated(entity);
            }
        }

        public bool IsInsideSceneBoundaries()
        {
            if (rootEntity != null && rootEntity.meshesInfo.renderers != null)
            {
                return rootEntity.scene.IsInsideSceneBoundaries(Utils.BuildMergedBounds(rootEntity.meshesInfo.renderers));
            }
            return true;
        }

        public void SetSelectLayer()
        {
            if (rootEntity.meshesInfo == null || rootEntity.meshesInfo.renderers == null)
            {
                return;
            }

            int selectionLayer = LayerMask.NameToLayer(DCLBuilderRaycast.LAYER_SELECTION);
            Renderer renderer;
            for (int i = 0; i < rootEntity.meshesInfo.renderers.Length; i++)
            {
                renderer = rootEntity.meshesInfo.renderers[i];
                if (renderer)
                {
                    renderer.gameObject.layer = selectionLayer;
                }
            }
        }

        public void SetDefaultLayer()
        {
            if (rootEntity.meshesInfo == null || rootEntity.meshesInfo.renderers == null)
            {
                return;
            }

            int selectionLayer = 0;
            Renderer renderer;
            for (int i = 0; i < rootEntity.meshesInfo.renderers.Length; i++)
            {
                renderer = rootEntity.meshesInfo.renderers[i];
                if (renderer)
                {
                    renderer.gameObject.layer = selectionLayer;
                }
            }
        }

        public bool HasShape()
        {
            return isShapeComponentSet;
        }

        public bool HasRenderer()
        {
            return rootEntity.meshesInfo != null && rootEntity.meshesInfo.renderers != null;
        }

        public void SetOnShapeLoaded(Action onShapeLoad)
        {
            if (HasShape())
            {
                if (onShapeLoad != null) onShapeLoad();
            }
            else
            {
                OnShapeLoaded = onShapeLoad;
            }
        }

        private void OnEntityRemoved(DecentralandEntity entity)
        {
            rootEntity.OnRemoved -= OnEntityRemoved;
            rootEntity.OnShapeUpdated -= OnShapeUpdated;
            rootEntity.OnTransformChange -= OnTransformUpdated;
            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;
            DestroyColliders();
        }

        private void OnShapeUpdated(DecentralandEntity entity)
        {
            isShapeComponentSet = true;
            OnEntityShapeUpdated?.Invoke(this);

            //let's reset scale so we generate colliders before any scaling animation
            gameObject.transform.localScale = Vector3.one;

            // We don't want animation to be running on editor
            meshAnimations = GetComponentsInChildren<Animation>();
            if (hasSmartItemComponent)
            {
                DefaultAnimationStop();
            }
            else
            {
                DefaultAnimationSample(0);
            }
            ProcessEntityShape(entity);


            if (hasGizmoComponent)
            {
                gameObject.transform.localScale = Vector3.zero;
                StartCoroutine(ScaleAnimationRoutine(0.3f));
            }
            else if (isTransformComponentSet)
            {
                gameObject.transform.localScale = scaleTarget;
            }

            if (OnShapeLoaded != null)
            {
                OnShapeLoaded();
                OnShapeLoaded = null;
            }
        }

        private void OnTransformUpdated(DCLTransform.Model transformModel)
        {
            //NOTE: there is no parenting entities in editor mode so we can set properties in world space
            gameObject.transform.position = transformModel.position;
            gameObject.transform.rotation = transformModel.rotation;

            if (isScalingAnimation)
            {
                scaleTarget = transformModel.scale;
            }
            else
            {
                scaleTarget = transformModel.scale;
                gameObject.transform.localScale = transformModel.scale;
            }

            if (!isTransformComponentSet)
            {
                isTransformComponentSet = true;
                OnEntityAddedWithTransform?.Invoke(this);
            }

            OnEntityTransformUpdated?.Invoke(this);
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            if (!hasSmartItemComponent)
            {
                if (isPreview)
                {
                    DefaultAnimationPlay();
                }
                else
                {
                    DefaultAnimationSample(0);
                }
            }

            SetCollidersActive(!isPreview);
        }

        private void ProcessEntityShape(DecentralandEntity entity)
        {
            if (entity.meshRootGameObject && entity.meshesInfo.renderers.Length > 0 && hasGizmoComponent)
            {
                CreateColliders(entity.meshesInfo);
            }
        }

        private void CreateColliders(DecentralandEntity.MeshesInfo meshInfo)
        {
            meshColliders = new DCLBuilderSelectionCollider[meshInfo.renderers.Length];
            for (int i = 0; i < meshInfo.renderers.Length; i++)
            {
                meshColliders[i] = new GameObject("BuilderSelectionCollider").AddComponent<DCLBuilderSelectionCollider>();
                meshColliders[i].Initialize(this, meshInfo.renderers[i]);
            }
        }

        private IEnumerator ScaleAnimationRoutine(float seconds)
        {
            float startingTime = Time.time;
            float normalizedTime = 0;
            Vector3 scale = Vector3.zero;

            gameObject.transform.localScale = scale;
            isScalingAnimation = true;

            while (Time.time - startingTime <= seconds)
            {
                normalizedTime = (Time.time - startingTime) / seconds;
                scale = Vector3.Lerp(scale, scaleTarget, normalizedTime);
                gameObject.transform.localScale = scale;
                yield return null;
            }
            gameObject.transform.localScale = scaleTarget;
            isScalingAnimation = false;
            OnEntityTransformUpdated?.Invoke(this);
        }

        private void DestroyColliders()
        {
            if (meshColliders != null)
            {
                for (int i = 0; i < meshColliders.Length; i++)
                {
                    if (meshColliders[i] != null)
                    {
                        Destroy(meshColliders[i].gameObject);
                    }
                }
                meshColliders = null;
            }
        }

        private void SetCollidersActive(bool active)
        {
            if (meshColliders != null)
            {
                for (int i = 0; i < meshColliders.Length; i++)
                {
                    if (meshColliders[i] != null)
                    {
                        meshColliders[i].gameObject.SetActive(active);
                    }
                }
            }
        }

        private void DefaultAnimationStop()
        {
            if (meshAnimations != null)
            {
                for (int i = 0; i < meshAnimations.Length; i++)
                {
                    meshAnimations[i].Stop();
                }
            }
        }

        private void DefaultAnimationSample(float time)
        {
            if (meshAnimations != null)
            {
                for (int i = 0; i < meshAnimations.Length; i++)
                {
                    meshAnimations[i].Stop();
                    meshAnimations[i].clip?.SampleAnimation(meshAnimations[i].gameObject, time);
                }
            }
        }

        private void DefaultAnimationPlay()
        {
            if (meshAnimations != null)
            {
                for (int i = 0; i < meshAnimations.Length; i++)
                {
                    meshAnimations[i].Play();
                }
            }
        }
    }
}
