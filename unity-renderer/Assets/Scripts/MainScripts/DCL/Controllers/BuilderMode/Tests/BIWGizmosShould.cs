﻿using System.Collections;
using System.Collections.Generic;
using Builder.Gizmos;
using DCL;
using DCL.Configuration;
using DCL.Helpers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

public class BIWGizmosShould : IntegrationTestSuite_Legacy
{
    private BIWGizmosController gizmosController;
    private BIWGizmosAxis gizmosAxis;
    private IBIWGizmos gizmo;
    private GameObject mockedGameObject;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        mockedGameObject = new GameObject("BIWGizmosShould");

        gizmo = Substitute.For<IBIWGizmos>();
        gizmo.Configure().GetGizmoType().Returns(BIWSettings.TRANSLATE_GIZMO_NAME);

        gizmosAxis = mockedGameObject.AddComponent<BIWGizmosAxis>();
        gizmosAxis.SetGizmo(gizmo);

        var referencesController = BIWTestHelper.CreateReferencesControllerWithGenericMocks(InitialSceneReferences.i);
        gizmosController = new BIWGizmosController();
        gizmosController.Init(referencesController);
        gizmosController.EnterEditMode(scene);
    }

    [Test]
    public void TestBeginDrag()
    {
        //Arrange
        gizmosController.OnGizmoTransformObjectStart += AssertGizmosType;

        //Act
        gizmosController.OnBeginDrag(gizmosAxis);
    }

    [Test]
    public void TestDrag() { }

    [Test]
    public void TestEndDrag()
    {
        //Arrange
        gizmosController.SetGizmoType(BIWSettings.TRANSLATE_GIZMO_NAME);
        gizmosController.OnGizmoTransformObjectEnd += AssertGizmosType;

        //Act
        gizmosController.OnEndDrag();
    }

    private void AssertGizmosType(string type) { Assert.AreEqual(type, BIWSettings.TRANSLATE_GIZMO_NAME); }

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

        //Assert
        Assert.IsInstanceOf(typeof(IBIWGizmos), gizmo);
    }

    [Test]
    public void TestRotateGizmosType()
    {
        //Act
        gizmosController.SetGizmoType(BIWSettings.ROTATE_GIZMO_NAME);

        //Assert
        Assert.IsInstanceOf(typeof(IBIWGizmos), gizmo);
    }

    [Test]
    public void TestScaleGizmosType()
    {
        //Act
        gizmosController.SetGizmoType(BIWSettings.SCALE_GIZMO_NAME);

        //Assert
        Assert.IsInstanceOf(typeof(IBIWGizmos), gizmo);
    }

    [Test]
    public void TestMouseDrag()
    {
        //Arrange
        gizmosController.OnChangeTransformValue += OnChangeTransformValue;
        gizmosController.isTransformingObject = true;

        //Act
        gizmosController.OnMouseDrag(1, Vector3.zero, 5f, 5f);
    }

    private void OnChangeTransformValue(Vector3 value) { Assert.AreEqual(value, Vector3.one * 5f); }

    [Test]
    public void TestMouseUp()
    {
        //Arrange
        gizmosController.isTransformingObject = true;

        //Act
        gizmosController.OnMouseUp(0, Vector3.zero);

        //Assert
        Assert.IsFalse(gizmosController.isTransformingObject);
    }
    protected override IEnumerator TearDown()
    {
        gizmosController.OnChangeTransformValue -= OnChangeTransformValue;
        gizmosController.OnGizmoTransformObjectStart -= AssertGizmosType;
        gizmosController.OnGizmoTransformObjectEnd -= AssertGizmosType;
        GameObject.Destroy(mockedGameObject);
        gizmosController.Dispose();
        yield return base.TearDown();
    }
}