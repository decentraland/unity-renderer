using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using NSubstitute;
using NSubstitute.Extensions;
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

    [Test]
    public void BIWExitWhenCharacterIsFarAway()
    {
        //Arrange
        mainController.sceneToEdit = scene;
        mainController.isBuilderInWorldActivated = true;
        mainController.checkerInsideSceneOptimizationCounter = 60;
        DCLCharacterController.i.characterPosition.unityPosition = Vector3.one * 9999;

        //Act
        mainController.Update();

        //Assert
        Assert.IsFalse(mainController.isBuilderInWorldActivated);
    }

    [Test]
    public void ActivateLandAccessBackground()
    {
        //Arrange
        var profile = UserProfile.GetOwnUserProfile();
        profile.UpdateData(new UserProfileModel() { userId = "testId", ethAddress = "0x00" });

        //Act
        mainController.ActivateLandAccessBackgroundChecker();

        //Assert
        Assert.IsNotNull(mainController.updateLandsWithAcessCoroutine);
    }

    [Test]
    public void RequestCatalog()
    {
        //Arrange
        mainController.isCatalogRequested = false;

        //Act
        mainController.GetCatalog();

        //Assert
        Assert.IsTrue(mainController.isCatalogRequested);
    }

    [Test]
    public void ChangeEditModeByShortcut()
    {
        //Act
        mainController.ChangeEditModeStatusByShortcut(DCLAction_Trigger.BuildEditModeChange);

        //Assert
        Assert.IsTrue(mainController.isWaitingForPermission);
    }

    [Test]
    public void NewSceneAdded()
    {
        //Arrange
        var mockedScene = Substitute.For<IParcelScene>();
        mockedScene.Configure().sceneData.Returns(scene.sceneData);
        mainController.sceneToEditId = scene.sceneData.id;

        //Act
        mainController.NewSceneAdded(mockedScene);

        //Assert
        Assert.AreSame(mainController.sceneToEdit, base.scene);
    }

    [Test]
    public void UserHasPermission()
    {
        //Arrange
        var parcel = new Parcel();
        parcel.x = scene.sceneData.basePosition.x;
        parcel.y = scene.sceneData.basePosition.y;
        var land = new Land();
        land.parcels = new List<Parcel>() { parcel };
        var lands = new LandWithAccess[]
        {
            new LandWithAccess(land)
        };
        DataStore.i.builderInWorld.landsWithAccess.Set(lands);

        //Act
        var result = mainController.UserHasPermissionOnParcelScene(scene);

        //Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void IsParcelFromSDK()
    {
        //Arrange
        Parcel parcel = new Parcel();
        parcel.x = scene.sceneData.basePosition.x;
        parcel.y = scene.sceneData.basePosition.y;

        Vector2Int parcelCoords = new Vector2Int(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
        Land land = new Land();
        land.parcels = new List<Parcel>() { parcel };

        LandWithAccess landWithAcces = new LandWithAccess(land);
        DeployedScene deployedScene = new DeployedScene();
        deployedScene.parcelsCoord = new Vector2Int[] { parcelCoords };
        deployedScene.deploymentSource = DeployedScene.Source.SDK;

        landWithAcces.scenes = new List<DeployedScene>() { deployedScene };
        var lands = new LandWithAccess[]
        {
            landWithAcces
        };
        DataStore.i.builderInWorld.landsWithAccess.Set(lands);

        //Act
        var result = mainController.IsParcelSceneDeployedFromSDK(scene);

        //Assert
        Assert.IsTrue(result);
    }

    protected override IEnumerator TearDown()
    {
        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);
        mainController.Dispose();
        BIWMainController.BYPASS_LAND_OWNERSHIP_CHECK = false;
        yield return base.TearDown();
    }
}