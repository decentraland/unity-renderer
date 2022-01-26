using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWAudioShould : IntegrationTestSuite
{
    private BuilderInWorldAudioHandler audioHandler;
    private GameObject gameObjectToDestroy;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        var context = BIWTestUtils.CreateMockedContextForTestScene();
        gameObjectToDestroy = Object.Instantiate(context.projectReferencesAsset.audioPrefab);
        audioHandler = gameObjectToDestroy.GetComponent<BuilderInWorldAudioHandler>();
        audioHandler.Initialize(context);
    }

    [Test]
    public void EnterBiw()
    {
        //Act
        audioHandler.EnterEditMode(null);

        //Assert
        Assert.IsTrue(audioHandler.gameObject.activeSelf);
    }

    [Test]
    public void ExitBiw()
    {
        //Act
        audioHandler.ExitEditMode();

        //Assert
        Assert.IsFalse(audioHandler.gameObject.activeSelf);
    }

    protected override IEnumerator TearDown()
    {
        audioHandler.Dispose();
        GameObject.Destroy(gameObjectToDestroy);
        yield return base.TearDown();
    }
}