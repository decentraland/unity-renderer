using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BIWModeControllerShould : IntegrationTestSuite_Legacy
{
    private BuilderInWorldController controller;
    private BIWModeController biwModeController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        controller = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];

        controller.InitGameObjects();
        controller.FindSceneToEdit();
        controller.InitControllers();

        biwModeController = controller.biwModeController;
        biwModeController.EnterEditMode(scene);

    }

    [Test]
    public void SetFirstPersonMode()
    {
        //Arrange
        biwModeController.SetBuildMode(BIWModeController.EditModeState.Inactive);

        //Act
        biwModeController.SetBuildMode(BIWModeController.EditModeState.FirstPerson);

        //Assert
        Assert.IsTrue(biwModeController.GetCurrentStateMode() == BIWModeController.EditModeState.FirstPerson);
        Assert.IsTrue(biwModeController.GetCurrentMode() == biwModeController.firstPersonMode);
    }

    [Test]
    public void SetGodMode()
    {
        //Arrange
        biwModeController.SetBuildMode(BIWModeController.EditModeState.Inactive);

        //Act
        biwModeController.SetBuildMode(BIWModeController.EditModeState.GodMode);

        //Assert
        Assert.IsTrue(biwModeController.GetCurrentStateMode() == BIWModeController.EditModeState.GodMode);
        Assert.IsTrue(biwModeController.GetCurrentMode() == biwModeController.editorMode);
    }

    [Test]
    public void InactiveMode()
    {
        //Arrange
        biwModeController.SetBuildMode(BIWModeController.EditModeState.GodMode);

        //Act
        biwModeController.SetBuildMode(BIWModeController.EditModeState.Inactive);

        //Assert
        Assert.IsTrue(biwModeController.GetCurrentStateMode() == BIWModeController.EditModeState.Inactive);
        Assert.IsTrue(biwModeController.GetCurrentMode() == null);
    }

    protected override IEnumerator TearDown()
    {
        controller.CleanItems();
        yield return base.TearDown();
    }
}