using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
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
        private const string POINTER_COLLIDER_NAME = "OnPointerEventCollider";
        private const string FEATURE_GLTFAST = "gltfast";
        private const string SMR_UPDATE_OFFSCREEN_FEATURE_FLAG = "smr_update_offscreen";
        private const StringComparison IGNORE_CASE = StringComparison.CurrentCultureIgnoreCase;

        internal RendereableAssetLoadHelper gltfLoader;
        internal GameObject gameObject;
        private IList<Collider> physicColliders;
        private IList<Collider> pointerColliders;
        private IList<Renderer> renderers;
        private string prevLoadedGltf = null;

        private readonly IInternalECSComponent<InternalColliders> pointerColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> physicColliderComponent;
        private readonly IInternalECSComponent<InternalRenderers> renderersComponent;
        private readonly IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent;

        private readonly DataStore_ECS7 dataStoreEcs7;
        private readonly DataStore_FeatureFlag featureFlags;

        public GltfContainerHandler(IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalColliders> physicColliderComponent,
            IInternalECSComponent<InternalRenderers> renderersComponent,
            IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent,
            DataStore_ECS7 dataStoreEcs7,
            DataStore_FeatureFlag featureFlags)
        {
            this.featureFlags = featureFlags;
            this.pointerColliderComponent = pointerColliderComponent;
            this.physicColliderComponent = physicColliderComponent;
            this.renderersComponent = renderersComponent;
            this.gltfContainerLoadingStateComponent = gltfContainerLoadingStateComponent;
            this.dataStoreEcs7 = dataStoreEcs7;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            gameObject = new GameObject("GLTF mesh");

            Transform transform = gameObject.transform;
            transform.SetParent(entity.gameObject.transform);
            transform.ResetLocalTRS();

            gltfLoader = new RendereableAssetLoadHelper(scene.contentProvider, scene.sceneData.baseUrlBundles, () => featureFlags.flags.Get().IsFeatureEnabled(FEATURE_GLTFAST));
            gltfLoader.settings.forceGPUOnlyMesh = true;
            gltfLoader.settings.parent = transform;
            gltfLoader.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION;
            gltfLoader.settings.smrUpdateWhenOffScreen = DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(SMR_UPDATE_OFFSCREEN_FEATURE_FLAG);
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            CleanUp(scene, entity);
            gltfContainerLoadingStateComponent.RemoveFor(scene, entity, new InternalGltfContainerLoadingState() { GltfContainerRemoved = true });
            Object.Destroy(gameObject);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBGltfContainer model)
        {
            if (string.IsNullOrEmpty(model.Src) || prevLoadedGltf == model.Src)
                return;

            prevLoadedGltf = model.Src;

            CleanUp(scene, entity);

            gltfContainerLoadingStateComponent.PutFor(scene, entity, new InternalGltfContainerLoadingState() { LoadingState = LoadingState.Loading });

            gltfLoader.OnSuccessEvent += rendereable => OnLoadSuccess(scene, entity, rendereable);
            gltfLoader.OnFailEvent += exception => OnLoadFail(scene, entity, exception);

            dataStoreEcs7.AddPendingResource(scene.sceneData.sceneNumber, prevLoadedGltf);
            gltfLoader.Load(model.Src);
        }

        private void OnLoadSuccess(IParcelScene scene, IDCLEntity entity, Rendereable rendereable)
        {
            // create physic colliders
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            physicColliders = SetUpPhysicColliders(meshFilters);

            // create pointer colliders and renderers
            (pointerColliders, renderers) = SetUpPointerCollidersAndRenderers(rendereable.renderers);

            // set colliders and renderers
            pointerColliderComponent.AddColliders(scene, entity, pointerColliders, (int)ColliderLayer.ClPointer);
            physicColliderComponent.AddColliders(scene, entity, physicColliders, (int)ColliderLayer.ClPhysics);
            renderersComponent.AddRenderers(scene, entity, renderers);

            gltfContainerLoadingStateComponent.PutFor(scene, entity, new InternalGltfContainerLoadingState() { LoadingState = LoadingState.Finished });

            // TODO: modify Animator component to remove `AddShapeReady` usage
            dataStoreEcs7.AddShapeReady(entity.entityId, gameObject);
            dataStoreEcs7.RemovePendingResource(scene.sceneData.sceneNumber, prevLoadedGltf);
        }

        private void OnLoadFail(IParcelScene scene, IDCLEntity entity, Exception exception)
        {
            gltfContainerLoadingStateComponent.PutFor(scene, entity, new InternalGltfContainerLoadingState() { LoadingState = LoadingState.FinishedWithError });
            dataStoreEcs7.RemovePendingResource(scene.sceneData.sceneNumber, prevLoadedGltf);
        }

        private void CleanUp(IParcelScene scene, IDCLEntity entity)
        {
            int count = pointerColliders?.Count ?? 0;

            for (int i = 0; i < count; i++) { pointerColliderComponent.RemoveCollider(scene, entity, pointerColliders[i]); }

            count = physicColliders?.Count ?? 0;

            for (int i = 0; i < count; i++) { physicColliderComponent.RemoveCollider(scene, entity, physicColliders[i]); }

            count = renderers?.Count ?? 0;

            for (int i = 0; i < count; i++) { renderersComponent.RemoveRenderer(scene, entity, renderers[i]); }

            physicColliders = null;
            pointerColliders = null;
            renderers = null;

            if (!string.IsNullOrEmpty(prevLoadedGltf)) { dataStoreEcs7.RemovePendingResource(scene.sceneData.sceneNumber, prevLoadedGltf); }

            // TODO: modify Animator component to remove `RemoveShapeReady` usage
            dataStoreEcs7.RemoveShapeReady(entity.entityId);

            gltfLoader.ClearEvents();
            gltfLoader.Unload();
        }

        private static List<Collider> SetUpPhysicColliders(MeshFilter[] meshFilters)
        {
            List<Collider> physicColliders = new List<Collider>(meshFilters.Length);

            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].gameObject.layer == PhysicsLayers.characterOnlyLayer)
                {
                    physicColliders.Add(meshFilters[i].gameObject.GetComponent<Collider>());
                    continue;
                }

                if (!IsCollider(meshFilters[i]))
                    continue;

                MeshCollider collider = meshFilters[i].gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = meshFilters[i].sharedMesh;
                meshFilters[i].gameObject.layer = PhysicsLayers.characterOnlyLayer;
                physicColliders.Add(collider);

                Object.Destroy(meshFilters[i].GetComponent<Renderer>());
            }

            return physicColliders;
        }

        // Compatibility layer for old GLTF importer and GLTFast
        private static bool IsCollider(MeshFilter meshFilter) =>
            meshFilter.name.Contains("_collider", IGNORE_CASE)
            || meshFilter.transform.parent.name.Contains("_collider", IGNORE_CASE);

        private static (List<Collider>, List<Renderer>) SetUpPointerCollidersAndRenderers(HashSet<Renderer> renderers)
        {
            List<Collider> pointerColliders = new List<Collider>(renderers.Count);
            List<Renderer> rendererList = new List<Renderer>(renderers.Count);

            // (sadly we are stuck with a hashset here)
            foreach (var renderer in renderers)
            {
                rendererList.Add(renderer);
                Transform rendererT = renderer.transform;
                bool alreadyHasCollider = false;

                for (int i = 0; i < rendererT.childCount; i++)
                {
                    Transform child = rendererT.GetChild(i);

                    if (child.gameObject.layer != PhysicsLayers.onPointerEventLayer)
                        continue;

                    alreadyHasCollider = true;
                    pointerColliders.Add(child.GetComponent<Collider>());
                    break;
                }

                if (alreadyHasCollider)
                    continue;

                Mesh colliderMesh;

                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    colliderMesh = skinnedMeshRenderer.sharedMesh;
                }
                else
                {
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    colliderMesh = meshFilter?.sharedMesh;
                }

                if (!colliderMesh)
                    continue;

                GameObject colliderGo = new GameObject(POINTER_COLLIDER_NAME);
                colliderGo.layer = PhysicsLayers.onPointerEventLayer;
                MeshCollider collider = colliderGo.AddComponent<MeshCollider>();

                collider.sharedMesh = colliderMesh;
                colliderGo.transform.SetParent(renderer.transform);
                colliderGo.transform.ResetLocalTRS();
                pointerColliders.Add(collider);
            }

            return (pointerColliders, rendererList);
        }
    }
}
