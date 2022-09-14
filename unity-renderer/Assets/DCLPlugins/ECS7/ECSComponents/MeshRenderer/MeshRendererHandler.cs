using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class MeshRendererHandler : IECSComponentHandler<PBMeshRenderer>
    {
        private readonly IInternalECSComponent<InternalTexturizable> texturizableInternalComponent;
        private readonly IInternalECSComponent<InternalRenderers> renderersInternalComponent;
        private readonly DataStore_ECS7 ecs7DataStore;

        private GameObject componentGameObject;
        private MeshFilter componentMeshFilter;
        private MeshRenderer componentMeshRenderer;

        private AssetPromise_PrimitiveMesh primitiveMeshPromise;
        private PBMeshRenderer prevModel;

        public MeshRendererHandler(
            DataStore_ECS7 dataStoreEcs7,
            IInternalECSComponent<InternalTexturizable> texturizableComponent,
            IInternalECSComponent<InternalRenderers> renderersComponent)
        {
            texturizableInternalComponent = texturizableComponent;
            renderersInternalComponent = renderersComponent;
            ecs7DataStore = dataStoreEcs7;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            componentGameObject = new GameObject("MeshRenderer");
            componentGameObject.transform.SetParent(entity.gameObject.transform);
            componentGameObject.transform.ResetLocalTRS();
            componentMeshFilter = componentGameObject.AddComponent<MeshFilter>();
            componentMeshRenderer = componentGameObject.AddComponent<MeshRenderer>();
            componentMeshRenderer.sharedMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
            texturizableInternalComponent.AddRenderer(scene, entity, componentMeshRenderer);
            renderersInternalComponent.AddRenderer(scene, entity, componentMeshRenderer);
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            texturizableInternalComponent.RemoveRenderer(scene, entity, componentMeshRenderer);
            renderersInternalComponent.RemoveRenderer(scene, entity, componentMeshRenderer);

            if (prevModel != null)
                ecs7DataStore.RemovePendingResource(scene.sceneData.id, prevModel);

            AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromise);
            Object.Destroy(componentGameObject);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBMeshRenderer model)
        {
            AssetPromise_PrimitiveMesh_Model? promiseModel = null;

            switch (model.MeshCase)
            {
                case PBMeshRenderer.MeshOneofCase.Box:
                    promiseModel = AssetPromise_PrimitiveMesh_Model.CreateBox(model.Box.Uvs);
                    break;
                case PBMeshRenderer.MeshOneofCase.Sphere:
                    promiseModel = AssetPromise_PrimitiveMesh_Model.CreateSphere();
                    break;
                case PBMeshRenderer.MeshOneofCase.Cylinder:
                    promiseModel = AssetPromise_PrimitiveMesh_Model.CreateCylinder(model.Cylinder.GetTopRadius(), model.Cylinder.GetBottomRadius());
                    break;
                case PBMeshRenderer.MeshOneofCase.Plane:
                    promiseModel = AssetPromise_PrimitiveMesh_Model.CreatePlane(model.Plane.Uvs);
                    break;
            }

            if (!promiseModel.HasValue)
            {
                AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromise);

                if (prevModel != null)
                    ecs7DataStore.RemovePendingResource(scene.sceneData.id, prevModel);

                return;
            }

            AssetPromise_PrimitiveMesh prevPromise = primitiveMeshPromise;
            prevModel = model;

            primitiveMeshPromise = new AssetPromise_PrimitiveMesh(promiseModel.Value);
            primitiveMeshPromise.OnSuccessEvent += shape =>
            {
                componentMeshFilter.sharedMesh = shape.mesh;
                ecs7DataStore.RemovePendingResource(scene.sceneData.id, model);
            };
            primitiveMeshPromise.OnFailEvent += (mesh, exception) =>
            {
                ecs7DataStore.RemovePendingResource(scene.sceneData.id, model);
                Debug.LogException(exception);
            };

            ecs7DataStore.AddPendingResource(scene.sceneData.id, model);
            AssetPromiseKeeper_PrimitiveMesh.i.Keep(primitiveMeshPromise);
            AssetPromiseKeeper_PrimitiveMesh.i.Forget(prevPromise);
        }
    }
}