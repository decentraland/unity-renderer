using DCL.Configuration;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class MeshColliderHandler : IECSComponentHandler<PBMeshCollider>
    {
        private GameObject colliderGameObject;
        private Collider collider;
        private AssetPromise_PrimitiveMesh primitiveMeshPromise;
        private PBMeshCollider prevModel;

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            colliderGameObject = new GameObject("MeshCollider");
            colliderGameObject.transform.SetParent(entity.gameObject.transform);
            colliderGameObject.transform.ResetLocalTRS();
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
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
                Object.Destroy(collider);
                collider = null;

                CreateCollider(model);
            }

            SetColliderLayer(model);
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

            ColliderLayer colliderLayer = model.GetColliderLayer();

            if (colliderLayer == ColliderLayer.PhysicsAndPointer)
            {
                colliderGameObject.layer = PhysicsLayers.defaultLayer;
            }
            else if (colliderLayer == ColliderLayer.Physics)
            {
                colliderGameObject.layer = PhysicsLayers.characterOnlyLayer;
            }
            else if (colliderLayer == ColliderLayer.Pointer)
            {
                colliderGameObject.layer = PhysicsLayers.onPointerEventLayer;
            }
            else if (colliderLayer == ColliderLayer.None && collider != null)
            {
                colliderGameObject.SetActive(false);
            }
        }
    }
}