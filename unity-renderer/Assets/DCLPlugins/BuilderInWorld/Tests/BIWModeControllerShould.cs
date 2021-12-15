using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Controllers;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;

public class BIWModeControllerShould : IntegrationTestSuite_Legacy
{
    private BIWModeController biwModeController;
    private IContext context;
    private ParcelScene scene;

    protected override List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        // result.Add(MainSceneFactory.CreateBridges());
        // result.Add(MainSceneFactory.CreateEnvironment());
        result.AddRange(MainSceneFactory.CreatePlayerSystems());
        // result.Add(MainSceneFactory.CreateNavMap());
        // result.Add(MainSceneFactory.CreateAudioHandler());
        // result.Add(MainSceneFactory.CreateHudController());
        result.Add(MainSceneFactory.CreateMouseCatcher());
        // result.Add(MainSceneFactory.CreateSettingsController());
        // result.Add(MainSceneFactory.CreateEventSystem());
        // result.Add(MainSceneFactory.CreateInteractionHoverCanvas());
        return result;
    }

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        biwModeController = new BIWModeController();
        CommonScriptableObjects.rendererState.Set(true);

        BIWActionController actionController = new BIWActionController();

        context = BIWTestUtils.CreateContextWithGenericMocks(
            actionController,
            biwModeController,
            SceneReferences.i
        );

        biwModeController.Initialize(context);
        actionController.Initialize(context);

        scene = TestUtils.CreateTestScene();

        biwModeController.EnterEditMode(scene);
        actionController.EnterEditMode(scene);
    }

    [Test]
    public void SetFirstPersonMode()
    {
        //Arrange
        biwModeController.SetBuildMode(IBIWModeController.EditModeState.Inactive);

        //Act
        biwModeController.SetBuildMode(IBIWModeController.EditModeState.FirstPerson);

        //Assert
        Assert.IsTrue(biwModeController.GetCurrentStateMode() == IBIWModeController.EditModeState.FirstPerson);
        Assert.IsTrue(biwModeController.GetCurrentMode().GetType() == typeof(BIWFirstPersonMode));
    }

    [Test]
    public void SetGodMode()
    {
        //Arrange
        biwModeController.SetBuildMode(IBIWModeController.EditModeState.Inactive);

        //Act
        biwModeController.SetBuildMode(IBIWModeController.EditModeState.GodMode);

        //Assert
        Assert.IsTrue(biwModeController.GetCurrentStateMode() == IBIWModeController.EditModeState.GodMode);
        Assert.IsTrue(biwModeController.GetCurrentMode().GetType() == typeof(BIWGodMode));
    }

    [Test]
    public void InactiveMode()
    {
        //Arrange
        biwModeController.SetBuildMode(IBIWModeController.EditModeState.GodMode);

        //Act
        biwModeController.SetBuildMode(IBIWModeController.EditModeState.Inactive);

        //Assert
        Assert.IsTrue(biwModeController.GetCurrentStateMode() == IBIWModeController.EditModeState.Inactive);
        Assert.IsTrue(biwModeController.GetCurrentMode() == null);
    }

    protected override IEnumerator TearDown()
    {
        context.Dispose();
        yield return base.TearDown();
    }
}