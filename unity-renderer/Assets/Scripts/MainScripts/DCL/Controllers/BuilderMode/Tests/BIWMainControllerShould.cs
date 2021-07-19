using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWMainControllerShould : IntegrationTestSuite
{
    private BIWMainController mainController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        mainController = new BIWMainController();
        var referencesController = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
        );
        mainController.Initialize();
    }

    [Test]
    public void EnterBuilderInWorld()
    {
        //Act
        mainController.TryStartEnterEditMode();

        //Assert
        Assert.IsTrue(mainController.isBuilderInWorldActivated);
    }

    [Test]
    public void ExitBuilderInWorld()
    {
        //Arrange
        mainController.TryStartEnterEditMode();

        //Act
        mainController.ExitEditMode();

        //Assert
        Assert.IsFalse(mainController.isBuilderInWorldActivated);
    }

    protected override IEnumerator TearDown()
    {
        mainController.Dispose();

        yield return base.TearDown();
    }
}