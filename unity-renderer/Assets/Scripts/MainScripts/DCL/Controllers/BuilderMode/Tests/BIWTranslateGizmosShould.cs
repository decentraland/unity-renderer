using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWTranslateGizmosShould
{
    private GameObject mockedGizmosGameObject;
    private GameObject mockedEntityGameObject;
    private BIWTranslateGizmos gizmos;
    private BIWGizmosAxis axis;

    [SetUp]
    private void SetUp()
    {
        mockedGizmosGameObject = new GameObject("ScaleGizmos");
        mockedEntityGameObject = new GameObject("EntityScaleGizmos");
        gizmos = mockedGizmosGameObject.AddComponent<BIWTranslateGizmos>();
        axis = mockedGizmosGameObject.AddComponent<BIWGizmosAxis>();
        axis.SetGizmo(gizmos);
        gizmos.activeAxis = axis;

    }

    [Test]
    public void TransformEntity()
    {
        //Arrange
        var snapinfo = new BIWGizmosController.SnapInfo();
        snapinfo.position = 1;
        gizmos.SetSnapFactor(snapinfo);

        //Act
        var value = gizmos.TransformEntity(mockedEntityGameObject.transform, axis, 1f);

        //Assert
        Assert.AreEqual(value, 1f);
    }

    [TearDown]
    private void TearDown()
    {
        GameObject.Destroy(mockedGizmosGameObject);
        GameObject.Destroy(mockedEntityGameObject);
    }
}