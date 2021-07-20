using System.Collections;
using System.Collections.Generic;
using DCL;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWMainControllerShould : IntegrationTestSuite_Legacy
{
    private BIWMainController mainController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        mainController = new BIWMainController();
        var referencesController = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
        );
        BIWMainController.BYPASS_LAND_OWNERSHIP_CHECK = true;
        mainController.Initialize();
    }

    [Test]
    public void EnterBuilderInWorld()
    {
        //Arrange
        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        //Act
        mainController.TryStartEnterEditMode(false, scene, "Test");
        Environment.i.world.sceneController.CreateTestScene(scene.sceneData, "TestEntiy");

        //Assert
        Assert.IsTrue(mainController.isBuilderInWorldActivated);
    }

    [Test]
    public void ExitBuilderInWorld()
    {
        //Arrange
        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        Environment.i.world.sceneController.CreateTestScene(scene.sceneData, "TestEntiy");

        //Act
        mainController.ExitEditMode();

        //Assert
        Assert.IsFalse(mainController.isBuilderInWorldActivated);
    }

    protected override IEnumerator TearDown()
    {
        mainController.Dispose();
        BIWMainController.BYPASS_LAND_OWNERSHIP_CHECK = false;
        yield return base.TearDown();
    }
}