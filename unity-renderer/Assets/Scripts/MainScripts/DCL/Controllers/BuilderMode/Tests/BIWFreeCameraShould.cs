using System.Collections;
using System.Collections.Generic;
using DCL.Camera;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWFreeCameraShould : IntegrationTestSuite
{
    private FreeCameraMovement freeCameraMovement;
    private GameObject mockedGameObject;
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        mockedGameObject = new GameObject("MockedGameObject");
        freeCameraMovement = mockedGameObject.AddComponent<FreeCameraMovement>();
    }

    [Test]
    public void StartDectectingMovement()
    {
        //Arrange
        freeCameraMovement.StopDetectingMovement();

        //Act
        freeCameraMovement.StartDectectingMovement();

        //Assert
        Assert.IsTrue(freeCameraMovement.isDetectingMovement);
        Assert.IsFalse(freeCameraMovement.HasBeenMovement());
    }

    [Test]
    public void StopDectectingMovement()
    {
        //Arrange
        freeCameraMovement.StartDectectingMovement();

        //Act
        freeCameraMovement.StopDetectingMovement();

        //Assert
        Assert.IsFalse(freeCameraMovement.isDetectingMovement);
    }

    protected override IEnumerator TearDown()
    {
        GameObject.Destroy(mockedGameObject);
        yield return base.TearDown();
    }
}