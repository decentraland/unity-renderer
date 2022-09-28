using System;
using System.Collections.Generic;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.ECSComponents
{
    public class GltfContainerHandler : IECSComponentHandler<PBGltfContainer>
    {
        const string POINTER_COLLIDER_NAME = "OnPointerEventCollider";

        internal RendereableAssetLoadHelper gltfLoader;
        internal GameObject gameObject;
        private IList<Collider> physicColliders;
        private IList<Collider> pointerColliders;
        private IList<Renderer> renderers;
        private string prevLoadedGltf = null;

        private readonly IInternalECSComponent<InternalColliders> pointerColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> physicColliderComponent;
        private readonly IInternalECSComponent<InternalRenderers> renderersComponent;
        private readonly DataStore_ECS7 dataStoreEcs7;

        public GltfContainerHandler(IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalColliders> physicColliderComponent,
            IInternalECSComponent<InternalRenderers> renderersComponent,
            DataStore_ECS7 dataStoreEcs7)
        {
            this.pointerColliderComponent = pointerColliderComponent;
            this.physicColliderComponent = physicColliderComponent;
            this.renderersComponent = renderersComponent;
            this.dataStoreEcs7 = dataStoreEcs7;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            gameObject = new GameObject("GLTF mesh");

            Transform transform = gameObject.transform;
            transform.SetParent(entity.gameObject.transform);
            transform.ResetLocalTRS();

            gltfLoader = new RendereableAssetLoadHelper(scene.contentProvider, scene.sceneData.baseUrlBundles);
            gltfLoader.settings.forceGPUOnlyMesh = true;
            gltfLoader.settings.parent = transform;
            gltfLoader.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            CleanUp(scene, entity);
            Object.Destroy(gameObject);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBGltfContainer model)
        {
            if (string.IsNullOrEmpty(model.Src) || prevLoadedGltf == model.Src)
                return;

            prevLoadedGltf = model.Src;

            CleanUp(scene, entity);

            gltfLoader.OnSuccessEvent += rendereable => OnLoadSuccess(scene, entity, rendereable);
            gltfLoader.OnFailEvent += exception => OnLoadFail(scene, exception);

            dataStoreEcs7.AddPendingResource(scene.sceneData.id, prevLoadedGltf);
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
            for (int i = 0; i < pointerColliders.Count; i++)
            {
                pointerColliderComponent.AddCollider(scene, entity, pointerColliders[i]);
            }
            for (int i = 0; i < physicColliders.Count; i++)
            {
                physicColliderComponent.AddCollider(scene, entity, physicColliders[i]);
            }
            for (int i = 0; i < renderers.Count; i++)
            {
                renderersComponent.AddRenderer(scene, entity, renderers[i]);
            }
            // TODO: modify Animator component to remove `AddShapeReady` usage
            dataStoreEcs7.AddShapeReady(entity.entityId, gameObject);
            dataStoreEcs7.RemovePendingResource(scene.sceneData.id, prevLoadedGltf);
        }

        private void OnLoadFail(IParcelScene scene, Exception exception)
        {
            MaterialTransitionController[] transitionController =
                gameObject.GetComponentsInChildren<MaterialTransitionController>(true);

            for (int i = 0; i < transitionController.Length; i++)
            {
                MaterialTransitionController material = transitionController[i];
                Object.Destroy(material);
            }
            dataStoreEcs7.RemovePendingResource(scene.sceneData.id, prevLoadedGltf);
        }

        private void CleanUp(IParcelScene scene, IDCLEntity entity)
        {
            int count = pointerColliders?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                pointerColliderComponent.RemoveCollider(scene, entity, pointerColliders[i]);
            }
            count = physicColliders?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                physicColliderComponent.RemoveCollider(scene, entity, physicColliders[i]);
            }
            count = renderers?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                renderersComponent.RemoveRenderer(scene, entity, renderers[i]);
            }

            physicColliders = null;
            pointerColliders = null;
            renderers = null;

            if (!string.IsNullOrEmpty(prevLoadedGltf))
            {
                dataStoreEcs7.RemovePendingResource(scene.sceneData.id, prevLoadedGltf);
            }
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

                if (!meshFilters[i].transform.parent.name.ToLower().Contains("_collider"))
                    continue;

                MeshCollider collider = meshFilters[i].gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = meshFilters[i].sharedMesh;
                meshFilters[i].gameObject.layer = PhysicsLayers.characterOnlyLayer;
                physicColliders.Add(collider);

                Object.Destroy(meshFilters[i].GetComponent<Renderer>());
            }

            return physicColliders;
        }

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

                GameObject colliderGo = new GameObject(POINTER_COLLIDER_NAME);
                colliderGo.layer = PhysicsLayers.onPointerEventLayer;
                MeshCollider collider = colliderGo.AddComponent<MeshCollider>();
                collider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                colliderGo.transform.SetParent(renderer.transform);
                colliderGo.transform.ResetLocalTRS();
                pointerColliders.Add(collider);
            }

            return (pointerColliders, rendererList);
        }
    }
}