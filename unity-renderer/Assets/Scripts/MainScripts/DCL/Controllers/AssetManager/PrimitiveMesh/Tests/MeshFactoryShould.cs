using System.Collections;
using System.Collections.Generic;
using DCL.ECSComponents;
using NUnit.Framework;
using UnityEngine;

public class MeshFactoryShould 
{
    [Test]
    public void CreateBoxMeshCorrectly()
    {
        // Arrange
        PrimitiveMeshFactory primitiveMeshFactory = new PrimitiveMeshFactory();
        PrimitiveMeshModel meshModel = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Box);
        meshModel.primitiveModel = new PBBoxShape();
        // Act
        Mesh mesh = primitiveMeshFactory.CreateMesh(meshModel);
        
        // Assert
        Assert.NotNull(mesh);
    }
    
    [Test]
    public void CreateSphereMeshCorrectly()
    {
        // Arrange
        PrimitiveMeshFactory primitiveMeshFactory = new PrimitiveMeshFactory();
        PrimitiveMeshModel meshModel = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Sphere);
        meshModel.primitiveModel = new PBBoxShape();   
        // Act
        Mesh mesh = primitiveMeshFactory.CreateMesh(meshModel);
        
        // Assert
        Assert.NotNull(mesh);
    }
}
