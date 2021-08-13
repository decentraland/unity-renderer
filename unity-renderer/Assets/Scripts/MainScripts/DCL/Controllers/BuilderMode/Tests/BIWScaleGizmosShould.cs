using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWScaleGizmosShould
{
    private GameObject mockedScaleGizmosGameObject;
    private GameObject mockedEntityGameObject;
    private BIWScaleGizmos gizmos;
    private BIWGizmosAxis axis;

    [UnitySetUp]
    protected IEnumerator SetUp()
    {
        mockedScaleGizmosGameObject = new GameObject("ScaleGizmos");
        mockedEntityGameObject = new GameObject("EntityScaleGizmos");
        gizmos = mockedScaleGizmosGameObject.AddComponent<BIWScaleGizmos>();
        axis = mockedScaleGizmosGameObject.AddComponent<BIWGizmosAxis>();
        axis.SetGizmo(gizmos);
        gizmos.activeAxis = axis;

        yield break;
    }

    [UnityTearDown]
    protected IEnumerator TearDown()
    {
        GameObject.Destroy(mockedScaleGizmosGameObject);
        GameObject.Destroy(mockedEntityGameObject);
        yield break;
    }

    [Test]
    public void TransformEntity()
    {
        //Act
        var value = gizmos.TransformEntity(mockedEntityGameObject.transform, axis, 5f);

        //Assert
        Assert.AreEqual(value, 5f);
    }

    [Test]
    public void ProportionalTransform()
    {
        //Arrange
        gizmos.axisProportionalScale = axis;

        //Act
        var value = gizmos.TransformEntity(mockedEntityGameObject.transform, axis, 5f);

        //Assert
        Assert.AreEqual(value, 5f);
    }

    [Test]
    public void SetPreviousAxisValues()
    {
        //Act
        gizmos.SetPreviousAxisValue(5f, 0f);

        //Assert
        Assert.AreEqual(gizmos.previousAxisValue, 5f);
    }

    [Test]
    public void GetHitPointToAxisValue()
    {
        //Arrange
        gizmos.axisProportionalScale = axis;

        //Act
        gizmos.GetHitPointToAxisValue(axis, Vector3.zero, Vector2.zero);

        //Assert
        Assert.AreEqual(gizmos.lastHitPoint, Vector3.zero);
    }
}