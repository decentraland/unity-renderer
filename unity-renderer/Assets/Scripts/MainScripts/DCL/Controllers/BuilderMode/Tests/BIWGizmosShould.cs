using System.Collections;
using System.Collections.Generic;
using Builder.Gizmos;
using DCL.Configuration;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;

public class BIWGizmosShould : IntegrationTestSuite_Legacy
{
    private BIWGizmosController gizmosController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        var referencesController = BIWTestHelper.CreateMockUpReferenceController();
        gizmosController = new BIWGizmosController();
        gizmosController.Init(referencesController);
        gizmosController.EnterEditMode(scene);
    }

    [Test]
    public void TestDesactivateGizmos()
    {
        //Act
        gizmosController.HideGizmo(true);

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), DCL.Components.DCLGizmos.Gizmo.NONE);
    }

    [Test]
    public void TestActivationTranslateGizmos()
    {
        //Act
        gizmosController.SetGizmoType(BIWSettings.TRANSLATE_GIZMO_NAME);

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BIWSettings.TRANSLATE_GIZMO_NAME);
    }

    [Test]
    public void TestActivationRotateGizmos()
    {
        //Act
        gizmosController.SetGizmoType(BIWSettings.ROTATE_GIZMO_NAME);

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BIWSettings.ROTATE_GIZMO_NAME);
    }

    [Test]
    public void TestActivationScaleGizmos()
    {
        //Act
        gizmosController.SetGizmoType(BIWSettings.SCALE_GIZMO_NAME);

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BIWSettings.SCALE_GIZMO_NAME);
    }

    [Test]
    public void TestTranslateGizmosType()
    {
        //Arrange
        gizmosController.SetGizmoType(BIWSettings.TRANSLATE_GIZMO_NAME);
        IBIWGizmos gizmo = gizmosController.activeGizmo;

        //Assert
        Assert.IsInstanceOf(typeof(IBIWGizmos), gizmo);
    }

    [Test]
    public void TestRotateGizmosType()
    {
        //Arrange
        gizmosController.SetGizmoType(BIWSettings.ROTATE_GIZMO_NAME);
        IBIWGizmos gizmo = gizmosController.activeGizmo;

        //Assert
        Assert.IsInstanceOf(typeof(IBIWGizmos), gizmo);
    }

    [Test]
    public void TestScaleGizmosType()
    {
        //Arrange
        gizmosController.SetGizmoType(BIWSettings.SCALE_GIZMO_NAME);
        IBIWGizmos gizmo = gizmosController.activeGizmo;

        //Assert
        Assert.IsInstanceOf(typeof(IBIWGizmos), gizmo);
    }

    protected override IEnumerator TearDown()
    {
        gizmosController.Dispose();
        yield return base.TearDown();
    }
}