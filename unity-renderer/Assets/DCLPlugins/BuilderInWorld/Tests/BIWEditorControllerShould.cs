using System.Collections;
using DCL;
using DCL.Builder;
using DCL.Controllers;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityGLTF;

public class BIWEditorControllerShould : IntegrationTestSuite_Legacy
{
    private BuilderInWorldEditor mainController;
    private IBuilderAPIController apiSubstitute;
    private ParcelScene scene;
    private AssetCatalogBridge assetCatalogBridge;
    private BuilderInWorldBridge biwBridge;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        scene = TestUtils.CreateTestScene();

        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);

        mainController = new BuilderInWorldEditor();
        assetCatalogBridge = TestUtils.CreateComponentWithGameObject<AssetCatalogBridge>("AssetCatalogBridge");
        apiSubstitute = Substitute.For<IBuilderAPIController>();

        biwBridge = MainSceneFactory.CreateBuilderInWorldBridge();

        mainController.Initialize(
            BIWTestUtils.CreateContextWithGenericMocks(apiSubstitute)
        );
    }

    [Test]
    public void InitializeController()
    {
        // Arrange
        IBIWController controller = Substitute.For<IBIWController>();

        // Act 
        mainController.InitController(controller);

        // Assert
        controller.Received(1).Initialize(mainController.context);
    }

    [Test]
    public void ControllerEnterEditMode()
    {
        // Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);
var builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);
        // Act
        mainController.EnterEditMode(builderScene);

        // Assert
        controller.Received(1).EnterEditMode(builderScene);
    }

    [Test]
    public void ControllerExitEditMode()
    {
        // Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        var builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);
        mainController.InitController(controller);
        mainController.EnterEditMode(builderScene);

        // Act
        mainController.ExitEditMode();

        // Assert
        controller.Received(1).ExitEditMode();
    }

    [Test]
    public void ControllerOnGUI()
    {
        // Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        var builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);
        mainController.InitController(controller);
        mainController.EnterEditMode(builderScene);

        // Act
        mainController.OnGUI();

        // Assert
        controller.Received(1).OnGUI();
    }

    [Test]
    public void ControllerLateUpdate()
    {
        // Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        var builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);
        mainController.InitController(controller);
        mainController.EnterEditMode(builderScene);

        // Act
        mainController.LateUpdate();

        // Assert
        controller.Received(1).LateUpdate();
    }

    [Test]
    public void ControllerNoActiveReceivingUpdate()
    {
        // Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        // Act
        mainController.Update();

        // Assert
        controller.Received(0).Update();
    }

    [Test]
    public void ControllerUpdate()
    {
        // Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        var builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);
        mainController.InitController(controller);
        mainController.EnterEditMode(builderScene);

        // Act
        mainController.Update();

        // Assert
        controller.Received(1).Update();
    }

    [Test]
    public void SetupNewScene()
    {
        // Arrange
        BIWCatalogManager.Init();
        BIWTestUtils.CreateTestCatalogLocalMultipleFloorObjects(assetCatalogBridge);
        ((EditorContext) mainController.context.editorContext).floorHandlerReference = Substitute.For<IBIWFloorHandler>();
        var builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);
        mainController.sceneToEdit = builderScene;
        mainController.EnterBiwControllers();

        // Act
        mainController.SetupNewScene();

        // Assert
        mainController.context.editorContext.floorHandler.Received(1).CreateDefaultFloor();
    }

    protected override IEnumerator TearDown()
    {
        yield return new DCL.WaitUntil( () => GLTFComponent.downloadingCount == 0 );

        DataStore.i.builderInWorld.catalogItemDict.Clear();

        Object.Destroy( assetCatalogBridge.gameObject );
        Object.Destroy( biwBridge.gameObject );

        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);
        mainController.context.Dispose();
        mainController.Dispose();
        yield return base.TearDown();
    }
}