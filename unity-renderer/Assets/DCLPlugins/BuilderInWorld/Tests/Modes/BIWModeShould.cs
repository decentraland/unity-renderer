using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Camera;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWModeShould
{
    private BIWMode mode;
    private GameObject mockedGameObject;
    private IContext context;
    private List<BIWEntity> selectedEntities;

    [UnitySetUp]
    protected IEnumerator SetUp()
    {
        mode = new BIWMode();
        mockedGameObject = new GameObject("BIWModeGameObject");
        context = BIWTestUtils.CreateMockedContextForTestScene();
        selectedEntities = new List<BIWEntity>();
        mode.Init(context);
        mode.SetEditorReferences(mockedGameObject, mockedGameObject, mockedGameObject, mockedGameObject, selectedEntities);
        yield break;
    }

    [Test]
    public void ActivateMode()
    {
        //Act
        mode.Activate(Substitute.For<IParcelScene>());

        //Assert
        Assert.IsTrue(mode.IsActive());
    }

    [Test]
    public void StartMultiSelection()
    {
        //Act
        mode.StartMultiSelection();

        //Assert
        Assert.IsTrue(mode.isMultiSelectionActive);
    }

    [Test]
    public void EndMultiSelection()
    {
        //Act
        mode.EndMultiSelection();

        //Assert
        Assert.IsFalse(mode.isMultiSelectionActive);
    }

    [Test]
    public void SelectedEntity()
    {
        //Arrange
        BIWEntity entity = new BIWEntity();
        var rootEntity = Substitute.For<IDCLEntity>();
        GameObject rootGameObject = new GameObject("Entity");
        rootEntity.Configure().gameObject.Returns(rootGameObject);
        selectedEntities.Add(entity);
        entity.Initialize(rootEntity, null);

        //Act
        mode.SelectedEntity(entity);

        //Assert
        Assert.IsTrue(rootEntity.gameObject.transform.parent == mockedGameObject.transform);
    }

    [Test]
    public void MouseClickOnEntity()
    {
        //Arrange
        BIWEntity entity = new BIWEntity();
        context.editorContext.raycastController.Configure().GetEntityOnPointer().Returns(entity);

        //Act
        mode.MouseClickDetected();

        //Assert
        context.editorContext.entityHandler.Received().EntityClicked(entity);
    }

    [Test]
    public void MouseClickOutsideEntity()
    {
        //Arrange
        mode.isMultiSelectionActive = false;

        //Act
        mode.MouseClickDetected();

        //Assert
        context.editorContext.entityHandler.Received().DeselectEntities();
    }

    [Test]
    public void CreatedEntityNewObjectActivation()
    {
        //Arrange
        mode.isNewObjectPlaced = false;

        //Act
        mode.CreatedEntity(null);

        //Assert
        Assert.IsTrue(mode.isNewObjectPlaced);
    }

    [Test]
    public void DeselectEntities()
    {
        //Act
        mode.OnDeselectedEntities();

        //Assert
        context.editorContext.entityHandler.Received().ReportTransform(true);
    }

    [Test]
    public void ResetScaleAndRotation()
    {
        //Act
        mode.ResetScaleAndRotation();

        //Assert
        Assert.AreEqual(mockedGameObject.transform.localScale, Vector3.one);
        Assert.AreEqual(mockedGameObject.transform.rotation.eulerAngles, Vector3.zero);
    }

    [UnityTearDown]
    protected IEnumerator TearDown()
    {
        GameObject.Destroy(mockedGameObject);
        mode.Dispose();
        context.Dispose();
        yield break;
    }
}