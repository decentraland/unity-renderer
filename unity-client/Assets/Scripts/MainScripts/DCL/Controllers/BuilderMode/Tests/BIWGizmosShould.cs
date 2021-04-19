using System.Collections;
using System.Collections.Generic;
using Builder.Gizmos;
using DCL.Configuration;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;

public class BIWGizmosShould : IntegrationTestSuite_Legacy
{
    private DCLBuilderGizmoManager gizmosController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        BuilderInWorldController controller = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];
        gizmosController = controller.biwModeController.editorMode.gizmoManager;
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
        gizmosController.SetGizmoType(BuilderInWorldSettings.TRANSLATE_GIZMO_NAME);

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BuilderInWorldSettings.TRANSLATE_GIZMO_NAME);
    }

    [Test]
    public void TestActivationRotateGizmos()
    {
        //Act
        gizmosController.SetGizmoType(BuilderInWorldSettings.ROTATE_GIZMO_NAME);

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BuilderInWorldSettings.ROTATE_GIZMO_NAME);
    }

    [Test]
    public void TestActivationScaleGizmos()
    {
        //Act
        gizmosController.SetGizmoType(BuilderInWorldSettings.SCLAE_GIZMO_NAME);

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BuilderInWorldSettings.SCLAE_GIZMO_NAME);
    }

    [Test]
    public void TestTranslateGizmosType()
    {
        //Arrange
        gizmosController.SetGizmoType(BuilderInWorldSettings.TRANSLATE_GIZMO_NAME);
        DCLBuilderGizmo translateGizmo = gizmosController.activeGizmo;

        //Assert
        Assert.IsInstanceOf(typeof(DCLBuilderTranslateGizmo), translateGizmo);
    }

    [Test]
    public void TestRotateGizmosType()
    {
        //Arrange
        gizmosController.SetGizmoType(BuilderInWorldSettings.ROTATE_GIZMO_NAME);
        DCLBuilderGizmo gizmo = gizmosController.activeGizmo;

        //Assert
        Assert.IsInstanceOf(typeof(DCLBuilderRotateGizmo), gizmo);
    }

    [Test]
    public void TestScaleGizmosType()
    {
        //Arrange
        gizmosController.SetGizmoType(BuilderInWorldSettings.SCLAE_GIZMO_NAME);
        DCLBuilderGizmo gizmo = gizmosController.activeGizmo;

        //Assert
        Assert.IsInstanceOf(typeof(DCLBuilderScaleGizmo), gizmo);
    }
}