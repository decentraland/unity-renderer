using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Camera;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityGLTF;

public class BIWEditorControllerShould : IntegrationTestSuite_Legacy
{
    private BuilderInWorldEditor mainController;
    private IBuilderAPIController apiSubstitute;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);
        mainController = new BuilderInWorldEditor();
        apiSubstitute = Substitute.For<IBuilderAPIController>();
        mainController.Initialize(BIWTestUtils.CreateContextWithGenericMocks(apiSubstitute));
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
        
        // Act
        mainController.EnterEditMode(scene);

        // Assert
        controller.Received(1).EnterEditMode(scene);
    }

    [Test]
    public void ControllerExitEditMode()
    {
        // Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);
        mainController.EnterEditMode(scene);

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
        mainController.InitController(controller);
        mainController.EnterEditMode(scene);

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
        mainController.InitController(controller);
        mainController.EnterEditMode(scene);

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
        mainController.InitController(controller);
        mainController.EnterEditMode(scene);

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
        BIWTestUtils.CreateTestCatalogLocalMultipleFloorObjects();
        ((EditorContext) mainController.context.editorContext).floorHandlerReference = Substitute.For<IBIWFloorHandler>();
        mainController.sceneToEdit = scene;
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
        AssetCatalogBridge.i.ClearCatalog();
        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);
        mainController.context.Dispose();
        mainController.Dispose();
        yield return base.TearDown();
    }
}