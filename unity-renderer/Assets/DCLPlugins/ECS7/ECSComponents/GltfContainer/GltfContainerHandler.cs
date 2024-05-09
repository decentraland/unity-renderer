using DCL.Components;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.ECSComponents
{
    public class GltfContainerHandler : IECSComponentHandler<PBGltfContainer>
    {
        private class ActiveCollidersData
        {
            public readonly List<Collider> PhysicColliders = new List<Collider>(10);
            public readonly List<Collider> PointerColliders = new List<Collider>(10);
            public readonly List<Collider> CustomLayerColliders = new List<Collider>(10);
        }

        private const uint LAYER_PHYSICS = (uint)ColliderLayer.ClPhysics;
        private const uint LAYER_POINTER = (uint)ColliderLayer.ClPointer;
        private const string SMR_UPDATE_OFFSCREEN_FEATURE_FLAG = "smr_update_offscreen";

        private readonly IInternalECSComponent<InternalColliders> pointerColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> physicColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> customLayerColliderComponent;
        private readonly IInternalECSComponent<InternalRenderers> renderersComponent;
        private readonly IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent;
        private readonly IInternalECSComponent<InternalAnimation> animationComponent;

        private readonly DataStore_ECS7 dataStoreEcs7;
        private readonly DataStore_FeatureFlag featureFlags;
        private readonly DataStore_WorldObjects dataStoreWorldObjects;

        private readonly ActiveCollidersData visibleActiveColliders = new ActiveCollidersData();
        private readonly ActiveCollidersData invisibleActiveColliders = new ActiveCollidersData();
        private readonly bool isDebugMode = false;

        public RendereableAssetLoadHelper gltfLoader;
        internal GameObject gameObject;

        private IReadOnlyCollection<Renderer> renderers;
        private Rendereable currentRendereable;

        internal GltfContainerCollidersHandler collidersHandler;
        private PBGltfContainer previousModel = null;
        private IParcelScene scene;
        private IDCLEntity entity;

        public GltfContainerHandler(IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalColliders> physicColliderComponent,
            IInternalECSComponent<InternalColliders> customLayerColliderComponent,
            IInternalECSComponent<InternalRenderers> renderersComponent,
            IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent,
            IInternalECSComponent<InternalAnimation> animationComponent,
            DataStore_ECS7 dataStoreEcs7,
            DataStore_FeatureFlag featureFlags,
            DataStore_WorldObjects dataStoreWorldObjects,
            DebugConfig debugConfig)
        {
            this.featureFlags = featureFlags;
            this.pointerColliderComponent = pointerColliderComponent;
            this.physicColliderComponent = physicColliderComponent;
            this.customLayerColliderComponent = customLayerColliderComponent;
            this.renderersComponent = renderersComponent;
            this.gltfContainerLoadingStateComponent = gltfContainerLoadingStateComponent;
            this.animationComponent = animationComponent;
            this.dataStoreEcs7 = dataStoreEcs7;
            this.dataStoreWorldObjects = dataStoreWorldObjects;
            this.isDebugMode = debugConfig.isDebugMode.Get();
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            this.entity = entity;
            gameObject = new GameObject("GLTF mesh");

            Transform transform = gameObject.transform;
            transform.SetParent(entity.gameObject.transform);
            transform.ResetLocalTRS();

            gltfLoader = new RendereableAssetLoadHelper(scene.contentProvider, scene.sceneData.baseUrlBundles);
            gltfLoader.settings.forceGPUOnlyMesh = true;
            gltfLoader.settings.parent = transform;
            gltfLoader.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION;
            gltfLoader.settings.smrUpdateWhenOffScreen = featureFlags.flags.Get().IsFeatureEnabled(SMR_UPDATE_OFFSCREEN_FEATURE_FLAG);

            collidersHandler = new GltfContainerCollidersHandler();
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            UnloadGltf(entity, previousModel?.Src);

            gltfContainerLoadingStateComponent.RemoveFor(scene, entity,
                new InternalGltfContainerLoadingState() { GltfContainerRemoved = true });

            Object.Destroy(gameObject);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBGltfContainer model)
        {
            bool srcChanged = previousModel?.Src != model.Src;
            bool visibleMeshColliderMaskChanged = previousModel?.GetVisibleMeshesCollisionMask() != model.GetVisibleMeshesCollisionMask();
            bool invisibleMeshColliderMaskChanged = previousModel?.GetInvisibleMeshesCollisionMask() != model.GetInvisibleMeshesCollisionMask();

            if (srcChanged)
            {
                OnGltfSrcChanged(previousModel, model);
            }
            else
            {
                if (visibleMeshColliderMaskChanged)
                {
                    SetUpColliders(
                        previousModel?.GetVisibleMeshesCollisionMask() ?? 0,
                        model.GetVisibleMeshesCollisionMask(),
                        collidersHandler.GetVisibleMeshesColliders(),
                        visibleActiveColliders.PointerColliders,
                        visibleActiveColliders.PhysicColliders,
                        visibleActiveColliders.CustomLayerColliders);
                }

                if (invisibleMeshColliderMaskChanged)
                {
                    SetUpColliders(
                        previousModel?.GetInvisibleMeshesCollisionMask() ?? 0,
                        model.GetInvisibleMeshesCollisionMask(),
                        collidersHandler.GetInvisibleMeshesColliders(),
                        invisibleActiveColliders.PointerColliders,
                        invisibleActiveColliders.PhysicColliders,
                        invisibleActiveColliders.CustomLayerColliders);
                }
            }

            previousModel = model;
        }

        private void OnGltfSrcChanged(PBGltfContainer prevModel, PBGltfContainer model)
        {
            if (!string.IsNullOrEmpty(prevModel?.Src))
            {
                UnloadGltf(entity, prevModel.Src);
            }

            string newGltfSrc = model.Src;

            if (!string.IsNullOrEmpty(newGltfSrc))
            {
                gltfContainerLoadingStateComponent.PutFor(scene, entity,
                    new InternalGltfContainerLoadingState() { LoadingState = LoadingState.Loading });

                gltfLoader.OnSuccessEvent += rendereable => OnLoadSuccess(rendereable, model);

                gltfLoader.OnFailEvent += exception => OnLoadFail(scene, entity, newGltfSrc, exception,
                    dataStoreEcs7, gltfContainerLoadingStateComponent);

                dataStoreEcs7.AddPendingResource(scene.sceneData.sceneNumber, newGltfSrc);
                gltfLoader.Load(newGltfSrc);
            }
        }

        private void OnLoadSuccess(
            Rendereable rendereable,
            PBGltfContainer model)
        {
            renderers = rendereable.renderers;

            InitColliders(
                rendereable.renderers,
                gameObject,
                collidersHandler,
                model.GetVisibleMeshesCollisionMask() != 0);

            SetUpRenderers(scene, entity, rendereable.renderers, renderersComponent);

            // setup colliders for visible meshes
            SetUpColliders(
                previousModel?.GetVisibleMeshesCollisionMask() ?? 0,
                model.GetVisibleMeshesCollisionMask(),
                model.GetVisibleMeshesCollisionMask() != 0 ? collidersHandler.GetVisibleMeshesColliders() : null,
                visibleActiveColliders.PointerColliders,
                visibleActiveColliders.PhysicColliders,
                visibleActiveColliders.CustomLayerColliders);

            // setup colliders for invisible meshes
            SetUpColliders(
                previousModel?.GetInvisibleMeshesCollisionMask() ?? 0,
                model.GetInvisibleMeshesCollisionMask(),
                collidersHandler.GetInvisibleMeshesColliders(),
                invisibleActiveColliders.PointerColliders,
                invisibleActiveColliders.PhysicColliders,
                invisibleActiveColliders.CustomLayerColliders);

            SetGltfLoaded(gameObject, model.Src, rendereable);
        }

        private void UnloadGltf(IDCLEntity entity, string gltfSrc)
        {
            void RemoveActiveColliders(IList<Collider> colliders, IInternalECSComponent<InternalColliders> colliderComponent)
            {
                for (int i = 0; i < colliders.Count; i++)
                {
                    colliderComponent.RemoveCollider(scene, entity, colliders[i]);
                }

                colliders.Clear();
            }

            RemoveActiveColliders(invisibleActiveColliders.PointerColliders, pointerColliderComponent);
            RemoveActiveColliders(visibleActiveColliders.PointerColliders, pointerColliderComponent);
            RemoveActiveColliders(invisibleActiveColliders.PhysicColliders, physicColliderComponent);
            RemoveActiveColliders(visibleActiveColliders.PhysicColliders, physicColliderComponent);
            RemoveActiveColliders(invisibleActiveColliders.CustomLayerColliders, customLayerColliderComponent);
            RemoveActiveColliders(visibleActiveColliders.CustomLayerColliders, customLayerColliderComponent);

            renderersComponent.RemoveRenderers(scene, entity, renderers);

            if (!string.IsNullOrEmpty(gltfSrc))
            {
                dataStoreEcs7.RemovePendingResource(scene.sceneData.sceneNumber, gltfSrc);
            }

            animationComponent.RemoveFor(scene, entity);

            collidersHandler.CleanUp();

            gltfLoader.ClearEvents();
            gltfLoader.Unload();

            if (isDebugMode)
                RemoveCurrentRendereableFromSceneMetrics();
        }

        private static void InitColliders(
            HashSet<Renderer> rendererHashSet,
            GameObject rootGameObject,
            GltfContainerCollidersHandler collidersHandler,
            bool createVisibleMeshColliders)
        {
            MeshFilter[] meshFilters = rootGameObject.GetComponentsInChildren<MeshFilter>();
            collidersHandler.InitInvisibleMeshesColliders(meshFilters);
            collidersHandler.InitVisibleMeshesColliders(rendererHashSet, createVisibleMeshColliders);
        }

        private void SetUpColliders(
            uint prevColliderLayer,
            uint colliderLayer,
            IReadOnlyList<Collider> gltfColliders,
            IList<Collider> currentPointerColliders,
            IList<Collider> currentPhysicColliders,
            IList<Collider> currentCustomLayerColliders)
        {
            if (prevColliderLayer != 0)
            {
                RemoveColliders(prevColliderLayer, currentPointerColliders,
                    currentPhysicColliders, currentCustomLayerColliders);
            }

            if (colliderLayer != 0)
            {
                SetColliders(colliderLayer, gltfColliders, currentPointerColliders,
                    currentPhysicColliders, currentCustomLayerColliders);
            }
        }

        private void RemoveColliders(
            uint colliderLayer,
            IList<Collider> currentPointerColliders,
            IList<Collider> currentPhysicColliders,
            IList<Collider> currentCustomLayerColliders)
        {
            void LocalRemoveColliders(
                IInternalECSComponent<InternalColliders> collidersComponent,
                IList<Collider> currentColliders)
            {
                var collidersModel = collidersComponent.GetFor(scene, entity)?.model;

                if (collidersModel != null)
                {
                    for (int i = 0; i < currentColliders.Count; i++)
                    {
                        currentColliders[i].enabled = false;
                        collidersModel.Value.colliders.Remove(currentColliders[i]);
                    }

                    collidersComponent.PutFor(scene, entity, collidersModel.Value);
                }

                currentColliders.Clear();
            }

            if ((colliderLayer & LAYER_PHYSICS) != 0)
            {
                LocalRemoveColliders(physicColliderComponent, currentPhysicColliders);
            }

            if ((colliderLayer & LAYER_POINTER) != 0)
            {
                LocalRemoveColliders(pointerColliderComponent, currentPointerColliders);
            }

            if (LayerMaskUtils.LayerMaskHasAnySDKCustomLayer(colliderLayer))
            {
                LocalRemoveColliders(customLayerColliderComponent, currentCustomLayerColliders);
            }
        }

        private void SetColliders(
            uint colliderLayer,
            IReadOnlyList<Collider> gltfColliders,
            IList<Collider> currentPointerColliders,
            IList<Collider> currentPhysicColliders,
            IList<Collider> currentCustomLayerColliders)
        {
            int? unityGameObjectLayer = LayerMaskUtils.SdkLayerMaskToUnityLayer(colliderLayer);
            bool hasCustomLayer = LayerMaskUtils.LayerMaskHasAnySDKCustomLayer(colliderLayer);

            var pointerColliders = pointerColliderComponent.GetFor(scene, entity)?.model;
            var physicColliders = physicColliderComponent.GetFor(scene, entity)?.model;
            var customColliders = customLayerColliderComponent.GetFor(scene, entity)?.model;

            bool hasPointerColliders = false;
            bool hasPhysicColliders = false;
            bool hasCustomColliders = false;

            for (int i = 0; i < gltfColliders.Count; i++)
            {
                Collider collider = gltfColliders[i];

                if (unityGameObjectLayer.HasValue)
                {
                    collider.gameObject.layer = unityGameObjectLayer.Value;
                }

                collider.enabled = true;

                if ((colliderLayer & LAYER_PHYSICS) != 0)
                {
                    physicColliders ??= new InternalColliders(new KeyValueSet<Collider, uint>());
                    physicColliders.Value.colliders.Add(collider, colliderLayer);
                    currentPhysicColliders.Add(collider);
                    hasPhysicColliders = true;
                }

                if ((colliderLayer & LAYER_POINTER) != 0)
                {
                    pointerColliders ??= new InternalColliders(new KeyValueSet<Collider, uint>());
                    pointerColliders.Value.colliders.Add(collider, colliderLayer);
                    currentPointerColliders.Add(collider);
                    hasPointerColliders = true;
                }

                if (hasCustomLayer)
                {
                    customColliders ??= new InternalColliders(new KeyValueSet<Collider, uint>());
                    customColliders.Value.colliders.Add(collider, colliderLayer);
                    currentCustomLayerColliders.Add(collider);
                    hasCustomColliders = true;
                }
            }

            if (hasPhysicColliders)
            {
                physicColliderComponent.PutFor(scene, entity, physicColliders.Value);
            }

            if (hasPointerColliders)
            {
                pointerColliderComponent.PutFor(scene, entity, pointerColliders.Value);
            }

            if (hasCustomColliders)
            {
                customLayerColliderComponent.PutFor(scene, entity, customColliders.Value);
            }
        }

        private static void SetUpRenderers(
            IParcelScene scene,
            IDCLEntity entity,
            HashSet<Renderer> rendererHashSet,
            IInternalECSComponent<InternalRenderers> renderersComponent)
        {
            if (rendererHashSet == null || rendererHashSet.Count == 0)
                return;

            var model = renderersComponent.GetFor(scene, entity)?.model ?? new InternalRenderers(new List<Renderer>());

            foreach (Renderer renderer in rendererHashSet)
            {
                model.renderers.Add(renderer);
            }

            renderersComponent.PutFor(scene, entity, model);
        }

        private void SetGltfLoaded(
            GameObject rootGameObject,
            string prevLoadedGltf,
            Rendereable rendereable)
        {
            gltfContainerLoadingStateComponent.PutFor(scene, entity,
                new InternalGltfContainerLoadingState() { LoadingState = LoadingState.Finished });

            Animation animation = rootGameObject.GetComponentInChildren<Animation>(true);

            if (animation)
            {
                animationComponent.PutFor(scene, entity, new InternalAnimation(animation));
            }

            if (!string.IsNullOrEmpty(prevLoadedGltf))
            {
                dataStoreEcs7.RemovePendingResource(scene.sceneData.sceneNumber, prevLoadedGltf);
            }

            if (isDebugMode)
            {
                currentRendereable = rendereable;
                AddCurrentRendereableToSceneMetrics();
            }
        }

        private static void OnLoadFail(
            IParcelScene scene,
            IDCLEntity entity,
            string gltfSrc,
            Exception exception,
            DataStore_ECS7 dataStoreEcs7,
            IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent)
        {
            gltfContainerLoadingStateComponent.PutFor(scene, entity,
                new InternalGltfContainerLoadingState() { LoadingState = LoadingState.FinishedWithError });

            dataStoreEcs7.RemovePendingResource(scene.sceneData.sceneNumber, gltfSrc);
        }

        private void AddCurrentRendereableToSceneMetrics()
        {
            dataStoreWorldObjects.AddRendereable(entity.scene.sceneData.sceneNumber, currentRendereable);

            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    dataStoreWorldObjects.AddMaterial(entity.scene.sceneData.sceneNumber, entity.entityId, material);
                }
            }
        }

        private void RemoveCurrentRendereableFromSceneMetrics()
        {
            foreach (Renderer renderer in currentRendereable.renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    dataStoreWorldObjects.RemoveMaterial(entity.scene.sceneData.sceneNumber, entity.entityId, material);
                }
            }

            dataStoreWorldObjects.RemoveRendereable(scene.sceneData.sceneNumber, currentRendereable);
        }
    }
}
