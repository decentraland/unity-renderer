using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Controllers;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;

public class BIWSaveControllerShould : IntegrationTestSuite_Legacy
{
    public BIWSaveController biwSaveController;
    public BuilderInWorldBridge builderInWorldBridge;
    public IContext context;

    private GameObject gameObject;
    private ParcelScene scene;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();

        gameObject = new GameObject();
        builderInWorldBridge = SceneReferences.i.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();
        context = BIWTestUtils.CreateContextWithGenericMocks(SceneReferences.i);

        biwSaveController = new BIWSaveController();
        biwSaveController.Initialize(context);
        biwSaveController.EnterEditMode(scene);
    }

    [Test]
    public void TestSaveActivate()
    {
        //Arrange
        biwSaveController.ResetSaveTime();

        //Act
        builderInWorldBridge.RemoveEntityOnKernel("test", scene);

        //Assert
        Assert.IsFalse(biwSaveController.CanSave());
    }

    [Test]
    public void TestCanSave()
    {
        //Act
        biwSaveController.ResetSaveTime();

        //Assert
        Assert.IsTrue(biwSaveController.CanSave());
    }

    [Test]
    public void TestCantSave()
    {
        //Act
        biwSaveController.ForceSave();

        //Assert
        Assert.IsFalse(biwSaveController.CanSave());
    }

    protected override IEnumerator TearDown()
    {
        context.Dispose();
        Object.Destroy(gameObject);
        yield return base.TearDown();
    }
}