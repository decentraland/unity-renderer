using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using NSubstitute;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWGodModeShould : IntegrationTestSuite_Legacy
{
    private BiwGodMode godMode;
    private GameObject mockedGameObject;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        godMode = new BiwGodMode();
        var context = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
            InitialSceneReferences.i
        );
        mockedGameObject = new GameObject("MockedGameObject");

        godMode.Init(context);
        godMode.SetEditorReferences(mockedGameObject, mockedGameObject, mockedGameObject, mockedGameObject, new List<BIWEntity>());
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

    protected override IEnumerator TearDown()
    {
        godMode.Dispose();
        GameObject.Destroy(mockedGameObject);
        return base.TearDown();
    }
}