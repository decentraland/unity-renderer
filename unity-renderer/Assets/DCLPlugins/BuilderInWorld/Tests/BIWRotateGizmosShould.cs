using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWRotateGizmosShould
{
    private GameObject mockedGizmosGameObject;
    private GameObject mockedEntityGameObject;
    private BIWGizmosRotate gizmos;
    private BIWGizmosAxis axis;

    [SetUp]
    public void SetUp()
    {
        mockedGizmosGameObject = new GameObject("ScaleGizmos");
        mockedEntityGameObject = new GameObject("EntityScaleGizmos");
        gizmos = mockedGizmosGameObject.AddComponent<BIWGizmosRotate>();
        axis = mockedGizmosGameObject.AddComponent<BIWGizmosAxis>();
        axis.SetGizmo(gizmos);
        gizmos.activeAxis = axis;
    }

    [Test]
    public void BeginDrag()
    {
        //Arrange
        Plane planeToCompare = new Plane(axis.transform.forward, mockedEntityGameObject.transform.position);

        //Act
        gizmos.OnBeginDrag(axis, mockedEntityGameObject.transform);

        Assert.AreEqual(planeToCompare, gizmos.raycastPlane);
    }

    [Test]
    public void TransformEntity()
    {
        //Act
        var value = gizmos.TransformEntity(mockedEntityGameObject.transform, axis, 1f);

        //Assert
        Assert.IsTrue(Mathf.Abs(57.2958f - value) <= 0.01f);
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
        Vector3 hitDir = (Vector3.zero - mockedGizmosGameObject.transform.position).normalized;
        var hitpoint = Vector3.SignedAngle(axis.transform.up, hitDir, axis.transform.forward) * Mathf.Deg2Rad;

        //Act
        var result = gizmos.GetHitPointToAxisValue(axis, Vector3.zero, Vector2.zero);

        //Assert
        Assert.AreEqual(hitpoint, result);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(mockedGizmosGameObject);
        GameObject.Destroy(mockedEntityGameObject);
    }
}