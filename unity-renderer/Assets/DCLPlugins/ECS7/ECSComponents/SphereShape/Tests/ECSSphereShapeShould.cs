using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class ECSSphereShapeShould 
{
    private IDCLEntity entity;
    private IParcelScene scene;
    private GameObject gameObject;
    private ECSSphereShapeComponentHandler sphereShapeComponentHandler;

    [SetUp]
    protected void SetUp()
    {
        gameObject = new GameObject();
        entity = Substitute.For<IDCLEntity>();
        scene = Substitute.For<IParcelScene>();
        sphereShapeComponentHandler = new ECSSphereShapeComponentHandler();

        entity.entityId.Returns(1);
        entity.gameObject.Returns(gameObject);
        LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
        sceneData.id = "1";
        scene.sceneData.Configure().Returns(sceneData);
    }
    
    [TearDown]
    protected void TearDown()
    {
        sphereShapeComponentHandler.OnComponentRemoved(scene,entity);
        GameObject.Destroy(gameObject);
    }
        
    [Test]
    public void UpdateComponentCorrectly()
    {
        // Arrange
        ECSShpereShape model = new ECSShpereShape();

        // Act
        sphereShapeComponentHandler.OnComponentModelUpdated(scene,entity,model);
        
        // Assert
        Assert.IsNotNull(sphereShapeComponentHandler.meshesInfo);
    }

    [Test]
    public void DisposeComponentCorrectly()
    {
        // Arrange
        ECSShpereShape model = new ECSShpereShape();
        sphereShapeComponentHandler.OnComponentModelUpdated(scene,entity,model);
        
        // Act
        sphereShapeComponentHandler.OnComponentRemoved(scene,entity);
        
        // Assert
        Assert.IsNull(sphereShapeComponentHandler.meshesInfo);
    }
}
