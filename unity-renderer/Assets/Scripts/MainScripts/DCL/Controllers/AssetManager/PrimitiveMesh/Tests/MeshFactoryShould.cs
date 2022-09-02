using DCL;
using NUnit.Framework;
using UnityEngine;

public class MeshFactoryShould 
{
    [Test]
    public void CreateBoxMeshCorrectly()
    {
        Mesh mesh = PrimitiveMeshFactory.CreateMesh(AssetPromise_PrimitiveMesh_Model.CreateBox(null));
        
        // Assert
        Assert.NotNull(mesh);
        Object.DestroyImmediate(mesh);
    }
    
    [Test]
    public void CreateSphereMeshCorrectly()
    {
        // Act
        Mesh mesh = PrimitiveMeshFactory.CreateMesh(AssetPromise_PrimitiveMesh_Model.CreateSphere());
        
        // Assert
        Assert.NotNull(mesh);
        Object.DestroyImmediate(mesh);
    }
}
