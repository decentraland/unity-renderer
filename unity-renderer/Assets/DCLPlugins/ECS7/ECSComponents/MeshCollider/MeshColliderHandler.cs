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

        private const int SPHERE_COLLIDER_LONGITUDE = 12;
        private const int SPHERE_COLLIDER_LATITUDE = 6;

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
                    box.size = new Vector3(1, 1, 0.01f);
                    break;
                case PBMeshCollider.MeshOneofCase.Sphere:
                {
                    // since unity's `SphereCollider` can't be deformed using it parent transform scaling
                    // we a sphere mesh to set as collider
                    MeshCollider meshCollider = colliderGameObject.AddComponent<MeshCollider>();
                    collider = meshCollider;

                    primitiveMeshPromise = new AssetPromise_PrimitiveMesh(
                        AssetPromise_PrimitiveMesh_Model.CreateSphere(0.5f, SPHERE_COLLIDER_LONGITUDE, SPHERE_COLLIDER_LATITUDE));

                    primitiveMeshPromise.OnSuccessEvent += asset => meshCollider.sharedMesh = asset.mesh;
                    primitiveMeshPromise.OnFailEvent += (mesh, exception) => Debug.LogException(exception);
                    AssetPromiseKeeper_PrimitiveMesh.i.Keep(primitiveMeshPromise);
                    break;
                }
                case PBMeshCollider.MeshOneofCase.Cylinder:
                {
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
            }

            AssetPromiseKeeper_PrimitiveMesh.i.Forget(prevMeshPromise);
        }

        private void SetColliderLayer(PBMeshCollider model)
        {
            if (collider)
            {
                colliderGameObject.SetActive(true);
            }

            uint colliderLayer = model.GetColliderLayer();
            int? layer = LayerMaskUtils.SdkLayerMaskToUnityLayer(colliderLayer);

            if (layer.HasValue)
            {
                colliderGameObject.layer = layer.Value;
            }
            else
            {
                colliderGameObject.SetActive(false);
            }
        }

        private void SetInternalColliderComponents(IParcelScene scene, IDCLEntity entity, PBMeshCollider model)
        {
            uint colliderLayer = model.GetColliderLayer();

            if ((colliderLayer & LAYER_POINTER) != 0)
                pointerColliderComponent.AddCollider(scene, entity, collider, colliderLayer);
            else
                pointerColliderComponent.RemoveCollider(scene, entity, collider);

            if ((colliderLayer & LAYER_PHYSICS) != 0)
                physicColliderComponent.AddCollider(scene, entity, collider, colliderLayer);
            else
                physicColliderComponent.RemoveCollider(scene, entity, collider);

            if (LayerMaskUtils.LayerMaskHasAnySDKCustomLayer((uint)colliderLayer))
                customLayerColliderComponent.AddCollider(scene, entity, collider, colliderLayer);
            else
                customLayerColliderComponent.RemoveCollider(scene, entity, collider);
        }
    }
}
