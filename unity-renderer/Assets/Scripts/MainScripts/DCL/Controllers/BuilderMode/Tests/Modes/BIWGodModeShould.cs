using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWGodModeShould : IntegrationTestSuite_Legacy
{
    private BIWModeController modeController;
    private BIWRaycastController raycastController;
    private BIWGizmosController gizmosController;
    private BiwGodMode godMode;
    private BIWContext context;
    private GameObject mockedGameObject;
    private List<BIWEntity> selectedEntities;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        modeController = new BIWModeController();
        raycastController = new BIWRaycastController();
        gizmosController = new BIWGizmosController();
        selectedEntities = new List<BIWEntity>();
        context = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
            modeController,
            raycastController,
            gizmosController,
            InitialSceneReferences.i
        );

        mockedGameObject = new GameObject("MockedGameObject");
        modeController.Init(context);
        raycastController.Init(context);
        gizmosController.Init(context);
        modeController.EnterEditMode(scene);
        raycastController.EnterEditMode(scene);
        gizmosController.EnterEditMode(scene);

        godMode =  (BiwGodMode) modeController.GetCurrentMode();
        godMode.SetEditorReferences(mockedGameObject, mockedGameObject, mockedGameObject, mockedGameObject, selectedEntities);
    }

    [Test]
    public void GodModeActivation()
    {
        //Act
        godMode.Activate(scene);

        //Assert
        Assert.IsTrue(godMode.IsActive());
    }

    [Test]
    public void GodModeDeactivation()
    {
        //Arrange
        godMode.Activate(scene);

        //Act
        godMode.Deactivate();

        //Assert
        Assert.IsFalse(godMode.IsActive());
    }

    [Test]
    public void TestNewObject()
    {
        //Arrange
        BIWEntity newEntity = new BIWEntity();
        newEntity.Init(TestHelpers.CreateSceneEntity(scene), null);
        raycastController.RayCastFloor(out Vector3 floorPosition);

        //Act
        modeController.CreatedEntity(newEntity);

        //Assert
        Assert.IsTrue(Vector3.Distance(mockedGameObject.transform.position, floorPosition) <= 0.001f);
    }

    [Test]
    public void ChangeTemporarySnapActive()
    {
        //Arrange
        modeController.SetSnapActive(false);
        selectedEntities.Add(new BIWEntity());

        //Act
        context.inputsReferences.multiSelectionInputAction.RaiseOnStarted();

        //Assert
        Assert.IsTrue(godMode.IsSnapActive);
    }

    [Test]
    public void ChangeTemporarySnapDeactivated()
    {
        //Arrange
        modeController.SetSnapActive(false);
        selectedEntities.Add(new BIWEntity());
        context.inputsReferences.multiSelectionInputAction.RaiseOnStarted();

        //Act
        context.inputsReferences.multiSelectionInputAction.RaiseOnFinished();

        //Assert
        Assert.IsFalse(godMode.IsSnapActive);
    }

    [Test]
    public void TranslateGizmos()
    {
        //Arrange
        godMode.RotateMode();

        //Act
        godMode.TranslateMode();

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BIWSettings.TRANSLATE_GIZMO_NAME);
    }

    [Test]
    public void RotateGizmos()
    {
        //Arrange
        godMode.TranslateMode();

        //Act
        godMode.RotateMode();

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BIWSettings.ROTATE_GIZMO_NAME);
    }

    [Test]
    public void ScaleGizmos()
    {
        //Arrange
        godMode.TranslateMode();

        //Act
        godMode.ScaleMode();

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BIWSettings.SCALE_GIZMO_NAME);
    }

    protected override IEnumerator TearDown()
    {
        context.inputsReferences.multiSelectionInputAction.RaiseOnFinished();
        raycastController.Dispose();
        modeController.Dispose();
        gizmosController.Dispose();
        ;
        context.Dispose();
        ;
        GameObject.Destroy(mockedGameObject);
        return base.TearDown();
    }
}