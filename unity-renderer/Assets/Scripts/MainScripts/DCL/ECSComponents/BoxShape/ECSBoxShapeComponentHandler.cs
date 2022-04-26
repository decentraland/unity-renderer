using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

public class ECSBoxShapeComponentHandler : IComponentHandler<ECSBoxShape> {

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) {  }
    
    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
    }
    
    public async void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSBoxShape model)
    {
        Mesh generatedMesh = null;
        AssetPromise_BoxShape boxShape = new AssetPromise_BoxShape(model);
        boxShape.OnSuccessEvent += shape => generatedMesh = shape.cubeMesh;
        AssetPromiseKeeper_BoxShape.i.Keep(boxShape);

        await boxShape;
        
    }
}
