using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWMainControllerShould : IntegrationTestSuite_Legacy
{
    private BIWMainController mainController;
    private BIWContext context;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        mainController = new BIWMainController();
        context = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
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
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

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
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.ExitEditMode();

        //Assert
        Assert.IsFalse(mainController.isBuilderInWorldActivated);
    }

    [Test]
    public void InitController()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();

        //act 
        mainController.InitController(controller);
        controller.Init(context);

        //Assert
        controller.Received(1).Init(context);
    }

    [Test]
    public void FindSceneToEdit()
    {
        //Arrange
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);
        CommonScriptableObjects.playerWorldPosition.Set(new Vector3(scene.sceneData.basePosition.x, 0, scene.sceneData.basePosition.y));

        //Act
        var sceneFound = mainController.FindSceneToEdit();

        //Arrange
        Assert.AreEqual(scene, sceneFound);
    }

    [Test]
    public void ControllerEnterEditMode()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        //Act
        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Assert
        controller.Received(1).EnterEditMode(scene);
    }

    [Test]
    public void ControllerExitEditMode()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.ExitEditMode();

        //Assert
        controller.Received(1).ExitEditMode();
    }

    [Test]
    public void ControllerOnGUI()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.OnGUI();

        //Assert
        controller.Received(1).OnGUI();
    }

    [Test]
    public void ControllerLateUpdate()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.LateUpdate();

        //Assert
        controller.Received(1).LateUpdate();
    }

    [Test]
    public void ControllerNoActiveReceveingUpdate()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        //Act
        mainController.Update();

        //Assert
        controller.Received(0).Update();
    }

    [Test]
    public void ControllerUpdate()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.Update();

        //Assert
        controller.Received(1).Update();
    }

    [Test]
    public void ControllerDispose()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        //Act
        mainController.Dispose();

        //Assert
        controller.Received(1).Dispose();
    }

    protected override IEnumerator TearDown()
    {
        mainController.Dispose();
        BIWMainController.BYPASS_LAND_OWNERSHIP_CHECK = false;
        yield return base.TearDown();
    }
}