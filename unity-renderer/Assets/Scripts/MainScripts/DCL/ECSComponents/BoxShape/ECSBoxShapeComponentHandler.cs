using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;
using UnityEngine.XR;

public class ECSBoxShapeComponentHandler : IComponentHandler<ECSBoxShape>
{
    private AssetPromise_Mesh meshPromise;
    private MeshesInfo meshesInfo;

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
    {
     
    }
    
    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        Dispose(scene);
    }

    public void Dispose(IParcelScene scene)
    {
        if (meshPromise != null)
            AssetPromiseKeeper_Mesh.i.Forget(meshPromise);
        
        Utils.CleanMaterials(meshesInfo.meshRootGameObject.GetComponent<Renderer>());
        meshesInfo.CleanReferences();

        ECSComponentsUtils.RemoveRendereableFromDataStore(scene.sceneData.id,meshesInfo.rendereable);
    }
    
    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSBoxShape model)
    {
        Mesh generatedMesh = null;
        if (meshPromise != null)
            AssetPromiseKeeper_Mesh.i.Forget(meshPromise);
        meshPromise = new AssetPromise_Mesh(model);
        meshPromise.OnSuccessEvent += shape =>
        {
            generatedMesh = shape.cubeMesh;
            GenerateRenderer(generatedMesh, scene, entity, model);
        };
        AssetPromiseKeeper_Mesh.i.Keep(meshPromise);
    }

    private void GenerateRenderer(Mesh mesh,IParcelScene scene, IDCLEntity entity, ECSBoxShape model)
    {
        meshesInfo = ECSComponentsUtils.GenerateMeshesInfo(entity,scene.sceneData.id,(int)CLASS_ID.BOX_SHAPE,mesh, entity.gameObject,model.visible,model.withCollisions,model.isPointerBlocker);
    }
}
