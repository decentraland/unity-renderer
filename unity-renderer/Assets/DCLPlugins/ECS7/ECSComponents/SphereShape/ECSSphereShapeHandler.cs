using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;


public class ECSSphereShapeComponentHandler : IECSComponentHandler<ECSShpereShape>
{
    private AssetPromise_PrimitiveMesh primitiveMeshPromisePrimitive;
    private MeshesInfo meshesInfo;

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }
    
    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        Dispose(scene);
    }

    public void Dispose(IParcelScene scene)
    {
        if (primitiveMeshPromisePrimitive != null)
            AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);
        
        Utils.CleanMaterials(meshesInfo.meshRootGameObject.GetComponent<Renderer>());
        meshesInfo.CleanReferences();

        ECSComponentsUtils.RemoveRendereableFromDataStore(scene.sceneData.id,meshesInfo.rendereable);
    }
    
    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSShpereShape model)
    {
        Mesh generatedMesh = null;
        if (primitiveMeshPromisePrimitive != null)
            AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);

        PrimitiveMeshModel primitiveMeshModelModel = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Sphere);
        primitiveMeshPromisePrimitive = new AssetPromise_PrimitiveMesh(primitiveMeshModelModel);
        primitiveMeshPromisePrimitive.OnSuccessEvent += shape =>
        {
            generatedMesh = shape.mesh;
            GenerateRenderer(generatedMesh, scene, entity, model);
        };
        AssetPromiseKeeper_PrimitiveMesh.i.Keep(primitiveMeshPromisePrimitive);
    }

    private void GenerateRenderer(Mesh mesh,IParcelScene scene, IDCLEntity entity, ECSShpereShape model)
    {
        meshesInfo = ECSComponentsUtils.GenerateMeshesInfo(entity,mesh, entity.gameObject,model.visible,model.withCollisions,model.isPointerBlocker);
        
        // Note: We should add the rendereable to the data store and dispose when it not longer exists
        meshesInfo.rendereable = ECSComponentsUtils.AddRendereableToDataStore(scene.sceneData.id,(int)CLASS_ID.SPHERE_SHAPE,mesh,entity.gameObject);
    }
}
