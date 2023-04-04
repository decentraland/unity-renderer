using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class MeshColliderHandler : IECSComponentHandler<PBMeshCollider>
    {
        private const int LAYER_PHYSICS = (int)ColliderLayer.ClPhysics;
        private const int LAYER_POINTER = (int)ColliderLayer.ClPointer;
        private const int LAYER_PHYSICS_POINTER = LAYER_PHYSICS | LAYER_POINTER;
        private const int LAYER_CUSTOM1 = (int)ColliderLayer.ClCustom1;
        private const int LAYER_CUSTOM2 = (int)ColliderLayer.ClCustom2;
        private const int LAYER_CUSTOM3 = (int)ColliderLayer.ClCustom3;
        private const int LAYER_CUSTOM4 = (int)ColliderLayer.ClCustom4;
        private const int LAYER_CUSTOM5 = (int)ColliderLayer.ClCustom5;
        private const int LAYER_CUSTOM6 = (int)ColliderLayer.ClCustom6;
        private const int LAYER_CUSTOM7 = (int)ColliderLayer.ClCustom7;
        private const int LAYER_CUSTOM8 = (int)ColliderLayer.ClCustom8;

        internal GameObject colliderGameObject;
        private Collider collider;
        private AssetPromise_PrimitiveMesh primitiveMeshPromise;
        private PBMeshCollider prevModel;

        private readonly IInternalECSComponent<InternalColliders> pointerColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> physicColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> customLayerColliderComponent;

        public MeshColliderHandler(
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalColliders> physicColliderComponent,
            IInternalECSComponent<InternalColliders> customLayerColliderComponent)
        {
            this.pointerColliderComponent = pointerColliderComponent;
            this.physicColliderComponent = physicColliderComponent;
            this.customLayerColliderComponent = customLayerColliderComponent;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            colliderGameObject = new GameObject("MeshCollider");
            colliderGameObject.transform.SetParent(entity.gameObject.transform);
            colliderGameObject.transform.ResetLocalTRS();
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            pointerColliderComponent.RemoveCollider(scene, entity, collider);
            physicColliderComponent.RemoveCollider(scene, entity, collider);
            AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromise);
            Object.Destroy(colliderGameObject);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBMeshCollider model)
        {
            if (prevModel != null && prevModel.Equals(model))
                return;

            bool shouldUpdateMesh = (prevModel?.MeshCase != model.MeshCase)
                                    || (model.MeshCase == PBMeshCollider.MeshOneofCase.Cylinder && !prevModel.Cylinder.Equals(model.Cylinder));
            prevModel = model;

            if (shouldUpdateMesh)
            {
                pointerColliderComponent.RemoveCollider(scene, entity, collider);
                physicColliderComponent.RemoveCollider(scene, entity, collider);
                Object.Destroy(collider);
                collider = null;

                CreateCollider(model);
            }

            SetColliderLayer(model);
            SetInternalColliderComponents(scene, entity, model);
        }

        private void CreateCollider(PBMeshCollider model)
        {
            AssetPromise_PrimitiveMesh prevMeshPromise = primitiveMeshPromise;

            switch (model.MeshCase)
            {
                case PBMeshCollider.MeshOneofCase.Box:
                    collider = colliderGameObject.AddComponent<BoxCollider>();
                    break;
                case PBMeshCollider.MeshOneofCase.Plane:
                    BoxCollider box = colliderGameObject.AddComponent<BoxCollider>();
                    collider = box;
                    box.size = new UnityEngine.Vector3(1, 1, 0.01f);
                    break;
                case PBMeshCollider.MeshOneofCase.Sphere:
                    SphereCollider sphere = colliderGameObject.AddComponent<SphereCollider>();
                    collider = sphere;
                    sphere.radius = 1;
                    break;
                case PBMeshCollider.MeshOneofCase.Cylinder:
                    MeshCollider meshCollider = colliderGameObject.AddComponent<MeshCollider>();
                    collider = meshCollider;
                    primitiveMeshPromise = new AssetPromise_PrimitiveMesh(
                        AssetPromise_PrimitiveMesh_Model.CreateCylinder(
                            model.Cylinder.GetTopRadius(),
                            model.Cylinder.GetBottomRadius()));

                    primitiveMeshPromise.OnSuccessEvent += asset => meshCollider.sharedMesh = asset.mesh;
                    primitiveMeshPromise.OnFailEvent += (mesh, exception) => Debug.LogException(exception);
                    AssetPromiseKeeper_PrimitiveMesh.i.Keep(primitiveMeshPromise);
                    break;
            }
            AssetPromiseKeeper_PrimitiveMesh.i.Forget(prevMeshPromise);
        }

        private void SetColliderLayer(PBMeshCollider model)
        {
            if (collider)
            {
                colliderGameObject.SetActive(true);
            }

            int colliderLayer = model.GetColliderLayer();

            // The order in this checks is important: from broader to more specific
            if ((colliderLayer & LAYER_CUSTOM1) != 0 || (colliderLayer & LAYER_CUSTOM2) != 0
              || (colliderLayer & LAYER_CUSTOM3) != 0 || (colliderLayer & LAYER_CUSTOM4) != 0
              || (colliderLayer & LAYER_CUSTOM5) != 0 || (colliderLayer & LAYER_CUSTOM6) != 0
              || (colliderLayer & LAYER_CUSTOM7) != 0 || (colliderLayer & LAYER_CUSTOM8) != 0)
            {
                colliderGameObject.layer = PhysicsLayers.sdkCustomLayer;
            }
            else if ((colliderLayer & LAYER_PHYSICS_POINTER) == LAYER_PHYSICS_POINTER)
            {
                colliderGameObject.layer = PhysicsLayers.defaultLayer;
            }
            else if ((colliderLayer & LAYER_PHYSICS) == LAYER_PHYSICS)
            {
                colliderGameObject.layer = PhysicsLayers.characterOnlyLayer;
            }
            else if ((colliderLayer & LAYER_POINTER) == LAYER_POINTER)
            {
                colliderGameObject.layer = PhysicsLayers.onPointerEventLayer;
            }
            else if (collider != null)
            {
                colliderGameObject.SetActive(false);
            }
        }

        private void SetInternalColliderComponents(IParcelScene scene, IDCLEntity entity, PBMeshCollider model)
        {
            int colliderLayer = model.GetColliderLayer();

            if ((colliderLayer & LAYER_POINTER) != 0)
                pointerColliderComponent.AddCollider(scene, entity, collider);
            else
                pointerColliderComponent.RemoveCollider(scene, entity, collider);

            if ((colliderLayer & LAYER_PHYSICS) != 0)
                physicColliderComponent.AddCollider(scene, entity, collider);
            else
                physicColliderComponent.RemoveCollider(scene, entity, collider);

            if (LayerMaskUtils.LayerMaskHasAnySDKCustomLayer((uint)colliderLayer))
                customLayerColliderComponent.AddCollider(scene, entity, collider);
            else
                customLayerColliderComponent.RemoveCollider(scene, entity, collider);
        }
    }
}
