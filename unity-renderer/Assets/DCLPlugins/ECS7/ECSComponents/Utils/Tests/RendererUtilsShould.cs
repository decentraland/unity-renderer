using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

public class RendererUtilsShould
{
    private GameObject gameObject;
    
    [SetUp]
    public void SetUp()
    {

        gameObject = new GameObject();
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(gameObject);
        DataStore.i.sceneWorldObjects.sceneData.Clear();
    }
    
    [Test]
    public void UpdateRenderer()
    {
        // Arrange
        IDCLEntity entity = Substitute.For<IDCLEntity>();
        MeshesInfo meshesInfo = new MeshesInfo();
        meshesInfo.colliders = new List<Collider>();
        entity.Configure().meshesInfo.Returns(meshesInfo);
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer.enabled = true;
        Renderer[] renderers = new Renderer[] { meshRenderer };
        
        // Act
        ECSComponentsUtils.UpdateRenderer(entity, gameObject, renderers, false, false, false);

        // Assert
        Assert.IsFalse(meshRenderer.enabled);
    }

    [Test]
    public void AddRendereableCorrectly()
    {
        // Arrange
        string sceneId = "SceneId";
        int componentId = 5;

        Mesh mesh =  PrimitiveMeshBuilder.BuildCube(1f);
        DataStore_WorldObjects.SceneData sceneData = new DataStore_WorldObjects.SceneData();
        DataStore.i.sceneWorldObjects.sceneData.Add(sceneId, sceneData);
        int sceneDataMeshesCount = sceneData.meshes.Count();
        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);

        // Act
        var rendereable = ECSComponentsUtils.AddRendereableToDataStore(sceneId, componentId, mesh, gameObject,renderers);

        // Assert
        Assert.IsNotNull(rendereable);
        Assert.AreNotEqual(sceneDataMeshesCount, DataStore.i.sceneWorldObjects.sceneData[sceneId].meshes.Count());
    }
    
    [Test]
    public void RemoveRendereableCorrectly()
    {
        // Arrange
        string sceneId = "SceneId";
        int componentId = 5;

        Mesh mesh =  PrimitiveMeshBuilder.BuildCube(1f);
        DataStore_WorldObjects.SceneData sceneData = new DataStore_WorldObjects.SceneData();
        DataStore.i.sceneWorldObjects.sceneData.Add(sceneId, sceneData);
        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        var rendereable = ECSComponentsUtils.AddRendereableToDataStore(sceneId, componentId, mesh, gameObject,renderers);
        int sceneDataMeshesCount = sceneData.meshes.Count();
        
        // Act
        ECSComponentsUtils.RemoveRendereableFromDataStore(sceneId,rendereable);

        // Assert
        Assert.AreNotEqual(sceneDataMeshesCount, DataStore.i.sceneWorldObjects.sceneData[sceneId].meshes.Count());
    }
}
