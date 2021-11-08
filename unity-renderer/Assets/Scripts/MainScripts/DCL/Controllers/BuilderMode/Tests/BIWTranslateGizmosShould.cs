using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
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
    public void SetUp()
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
        var snapinfo = new SnapInfo();
        snapinfo.position = 1;
        gizmos.SetSnapFactor(snapinfo);

        //Act
        var value = gizmos.TransformEntity(mockedEntityGameObject.transform, axis, 1f);

        //Assert
        Assert.AreEqual(value, 1f);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(mockedGizmosGameObject);
        GameObject.Destroy(mockedEntityGameObject);
    }
}