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
    public BuilderInWorldBridge biwBridge;
    public IContext context;

    private ParcelScene scene;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();

        biwBridge = MainSceneFactory.CreateBuilderInWorldBridge();
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
        biwBridge.RemoveEntityOnKernel("test", scene);

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
        Object.Destroy(biwBridge.gameObject);
        yield return base.TearDown();
    }
}