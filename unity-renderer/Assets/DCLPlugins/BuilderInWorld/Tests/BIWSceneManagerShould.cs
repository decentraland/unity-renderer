using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Camera;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityGLTF;
using Environment = DCL.Environment;

public class BIWSceneManagerShould :  IntegrationTestSuite_Legacy
{
    private BuilderInWorldBridge biwBridge;
    private SceneManager mainController;
    private IBuilderAPIController apiSubstitute;
    private IBuilderScene builderScene;
    private ParcelScene scene;

    protected override List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        result.Add(MainSceneFactory.CreateBridges());
        result.Add(MainSceneFactory.CreateEnvironment());
        result.AddRange(MainSceneFactory.CreatePlayerSystems());
        result.Add(MainSceneFactory.CreateNavMap());
        result.Add(MainSceneFactory.CreateAudioHandler());
        result.Add(MainSceneFactory.CreateHudController());
        result.Add(MainSceneFactory.CreateMouseCatcher());
        result.Add(MainSceneFactory.CreateEventSystem());
        return result;
    }


    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();

        biwBridge = MainSceneFactory.CreateBuilderInWorldBridge();

        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);
        mainController = new SceneManager();
        apiSubstitute = Substitute.For<IBuilderAPIController>();
        mainController.Initialize(BIWTestUtils.CreateContextWithGenericMocks(apiSubstitute));
        mainController.initialLoadingController.Dispose();
        mainController.initialLoadingController = Substitute.For<IBuilderInWorldLoadingController>();
        mainController.initialLoadingController.Configure().isActive.Returns(true);

        builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);
    }

    [Test]
    [TestCase(IBuilderScene.SceneType.LAND)]
    [TestCase(IBuilderScene.SceneType.PROJECT)]
    public void StartExitModeScreenShot(IBuilderScene.SceneType sceneType)
    {
        // Arrange
        mainController.context.editorContext.saveController.Configure().GetSaveAttempsSinceLastSave().Returns(2);
        builderScene.Configure().sceneType.Returns(sceneType);
        mainController.sceneToEdit = builderScene;
        
        // Act
        mainController.StartExitMode();

        // Assert
        mainController.context.cameraController.Received().TakeSceneScreenshotFromResetPosition(Arg.Any<IScreenshotCameraController.OnSnapshotsReady>());
    }

    [Test]
    public void SetFlagProperlyWhenBuilderInWorldIsEntered()
    {
        // Arrange
        mainController.sceneToEdit = builderScene;
        mainController.CatalogLoaded();
        scene.CreateEntity(1);
        mainController.sceneToEditId = scene.sceneData.id;

        // Act
        mainController.StartFlow(builderScene, "source");
        scene.CreateEntity(2);
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        // Assert
        Assert.AreEqual(SceneManager.State.SCENE_LOADED, mainController.currentState );
    }

    [Test]
    public void SetFlagProperlyWhenBuilderInWorldIsExited()
    {
        // Arrange
        mainController.sceneToEdit = builderScene;
        mainController.CatalogLoaded();
        scene.CreateEntity(1);

        mainController.StartFlowFromLandWithPermission(scene, "Test");
        scene.CreateEntity(2);
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        // Act
        mainController.ExitEditMode();

        // Assert
        Assert.AreEqual(mainController.currentState, SceneManager.State.IDLE );
    }

    [Test]
    public void FindSceneToEdit()
    {
        // Arrange
        scene.CreateEntity(1);
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);
        DataStore.i.player.playerWorldPosition.Set(new Vector3(scene.sceneData.basePosition.x, 0, scene.sceneData.basePosition.y));

        // Act
        var sceneFound = mainController.FindSceneToEdit();

        // Arrange
        Assert.AreEqual(scene, sceneFound);
    }

    [Test]
    public void ActivateLandAccessBackground()
    {
        // Arrange
        var profile = UserProfile.GetOwnUserProfile();
        profile.UpdateData(new UserProfileModel() { userId = "testId", ethAddress = "0x00" });

        // Act
        mainController.ActivateLandAccessBackgroundChecker();

        // Assert
        Assert.IsNotNull(mainController.updateLandsWithAcessCoroutine);
    }

    [Test]
    public void RequestCatalog()
    {
        // Arrange
        mainController.currentState = SceneManager.State.LOADING_CATALOG;
        ((Context)mainController.context).builderAPIController = Substitute.For<IBuilderAPIController>();
        Promise<bool> resultOkPromise = new Promise<bool>();
        mainController.context.builderAPIController.Configure().GetCompleteCatalog(Arg.Any<string>()).Returns(resultOkPromise);
        mainController.sceneToEdit = builderScene;
        // Act
        mainController.GetCatalog();
        resultOkPromise.Resolve(true);

        // Assert
        Assert.GreaterOrEqual(mainController.currentState, SceneManager.State.CATALOG_LOADED );
    }

    [Test]
    public void ChangeEditModeByShortcut()
    {
        // Act
        mainController.ChangeEditModeStatusByShortcut(DCLAction_Trigger.BuildEditModeChange);

        // Assert
        Assert.IsTrue(mainController.isWaitingForPermission);
    }

    [Test]
    public void NewSceneAdded()
    {
        // Arrange
        mainController.sceneToEdit = builderScene;
        var mockedScene = Substitute.For<IParcelScene>();
        mockedScene.Configure().sceneData.Returns(scene.sceneData);
        mainController.sceneToEditId = scene.sceneData.id;

        // Act
        mainController.NewSceneAdded(mockedScene);

        // Assert
        Assert.AreSame(mainController.sceneToEdit.scene, scene);
    }

    [Test]
    public void UserHasPermission()
    {
        // Arrange
        AddSceneToPermissions();

        // Act
        var result = mainController.UserHasPermissionOnParcelScene(scene);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void ReturnTrueWhenParcelSceneDeployedFromSDKIsCalled()
    {
        // Arrange
        Parcel parcel = new Parcel();
        parcel.x = scene.sceneData.basePosition.x;
        parcel.y = scene.sceneData.basePosition.y;

        Vector2Int parcelCoords = new Vector2Int(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
        Land land = new Land();
        land.parcels = new List<Parcel>() { parcel };

        LandWithAccess landWithAccess = new LandWithAccess(land);

        {
            Scene scene = new Scene();
            scene.parcelsCoord = new Vector2Int[] { parcelCoords };
            scene.deploymentSource = Scene.Source.SDK;
            landWithAccess.scenes = new List<Scene>() { scene };
        }

        var lands = new LandWithAccess[]
        {
            landWithAccess
        };
        DataStore.i.builderInWorld.landsWithAccess.Set(lands);

        // Act
        var result = mainController.IsParcelSceneDeployedFromSDK(scene);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void CatalogReceived()
    {
        // Arrange
        ((Context)mainController.context).builderAPIController = Substitute.For<IBuilderAPIController>();
        Promise<bool> resultOkPromise = new Promise<bool>();
        mainController.context.builderAPIController.Configure().GetCompleteCatalog(Arg.Any<string>()).Returns(resultOkPromise);
        mainController.sceneToEdit = Substitute.For<IBuilderScene>();
        mainController.sceneToEdit.scene.Configure().sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene { id = "Test id" });
        // Act
        mainController.GetCatalog();
        resultOkPromise.Resolve(true);

        // Assert
        Assert.IsTrue(mainController.catalogLoaded);
    }

    [Test]
    public void CheckSceneToEditByShortcut()
    {
        // Arrange
        mainController.currentState = SceneManager.State.IDLE;
        mainController.sceneToEdit = builderScene;
        AddSceneToPermissions();
        ((Context)mainController.context).builderAPIController = Substitute.For<IBuilderAPIController>();
        Promise<bool> resultOkPromise = new Promise<bool>();
        mainController.context.builderAPIController.Configure().GetCompleteCatalog(Arg.Any<string>()).Returns(resultOkPromise);
        Promise<InitialStateResponse> statePromise = new Promise<InitialStateResponse>();
        mainController.initialStateManager = Substitute.For<IInitialStateManager>();
        mainController.initialStateManager.Configure()
                      .GetInitialManifest(Arg.Any<IBuilderAPIController>(), Arg.Any<string>(), Arg.Any<Scene>(), Arg.Any<Vector2Int>())
                      .Returns(statePromise);

        InitialStateResponse initialStateResponse = new InitialStateResponse();
        initialStateResponse.manifest = Substitute.For<IManifest>();
            
        // Act
        mainController.CheckSceneToEditByShorcut();
        statePromise.Resolve(initialStateResponse);
        resultOkPromise.Resolve(true);

        // Assert
        Assert.AreNotEqual(mainController.currentState, SceneManager.State.IDLE);
    }

    [Test]
    public void ExitAfterTeleport()
    {
        // Arrange
        mainController.sceneToEdit = builderScene;
        mainController.currentState = SceneManager.State.EDITING;

        // Act
        mainController.ExitAfterCharacterTeleport(new DCLCharacterPosition());

        // Assert
        Assert.AreEqual(mainController.currentState,  SceneManager.State.IDLE);
    }

    private void AddSceneToPermissions()
    {
        var parcel = new Parcel();
        parcel.x = scene.sceneData.basePosition.x;
        parcel.y = scene.sceneData.basePosition.y;

        var land = new Land();
        land.parcels = new List<Parcel>() { parcel };

        var landWithAccess = new LandWithAccess(land);
        landWithAccess.scenes = new List<Scene>();

        var lands = new LandWithAccess[]
        {
            landWithAccess
        };

        DataStore.i.builderInWorld.landsWithAccess.Set(lands);
    }

    protected override IEnumerator TearDown()
    {
        yield return new DCL.WaitUntil( () => GLTFComponent.downloadingCount == 0 );
        UnityEngine.Object.Destroy(biwBridge.gameObject);
        mainController.context.Dispose();
        mainController.Dispose();
        SceneManager.BYPASS_LAND_OWNERSHIP_CHECK = false;
        yield return base.TearDown();
    }
}