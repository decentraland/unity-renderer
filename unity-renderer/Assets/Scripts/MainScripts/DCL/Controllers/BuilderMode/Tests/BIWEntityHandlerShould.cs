using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DCL.Camera;
using DCL.Components;
using DCL.Models;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.Extensions;
using UnityEngine.TestTools;

/// <summary>
/// TODO: This is using IntegrationTestSuite_Legacy instead of the normal because there is a bug in the NSustitute library
/// where the IDCLEntity are not mocked correctly. After it is fixed, we should go to IntegrationTestSuite 
/// </summary>
public class BIWEntityHandlerShould : IntegrationTestSuite_Legacy
{
    private const string ENTITY_ID = "1";
    private BIWEntity entity;
    private BIWEntityHandler entityHandler;
    private BIWContext context;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        entityHandler = new BIWEntityHandler();
        context = BIWTestHelper.CreateMockUpReferenceController();
        entityHandler.Init(context);

        TestHelpers.CreateSceneEntity(scene, ENTITY_ID);
        entityHandler.EnterEditMode(scene);
        entity = entityHandler.GetAllEntitiesFromCurrentScene().FirstOrDefault();
    }

    [Test]
    public void EntitySelectDeselect()
    {
        Assert.IsFalse(entity.isSelected);
        entityHandler.SelectEntity(entity);
        Assert.IsTrue(entity.isSelected);

        Assert.AreEqual(entityHandler.GetSelectedEntityList().Count, 1);
        Assert.AreEqual(entityHandler.GetSelectedEntityList().FirstOrDefault(), entity);

        entityHandler.DeselectEntity(entity);
        Assert.IsFalse(entity.isSelected);
    }

    [Test]
    public void EntitySelectionOperations()
    {
        BIWEntity createdEntity = entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);

        int entityAmount = entityHandler.GetAllEntitiesFromCurrentScene().Count;
        entityHandler.SelectEntity(createdEntity);
        entityHandler.DuplicateSelectedEntities();

        Assert.Greater(entityHandler.GetAllEntitiesFromCurrentScene().Count, entityAmount);


        entityAmount = entityHandler.GetAllEntitiesFromCurrentScene().Count;
        entityHandler.DeleteSelectedEntities();

        Assert.Less(entityHandler.GetAllEntitiesFromCurrentScene().Count, entityAmount);
    }

    [Test]
    public void EntityCreationDelete()
    {
        BIWEntity createdEntity = entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);
        Assert.IsNotNull(createdEntity);
        Assert.AreEqual(entityHandler.GetAllEntitiesFromCurrentScene().Count, 2);

        entityHandler.DeleteEntity(createdEntity.rootEntity.entityId);
        Assert.AreEqual(entityHandler.GetAllEntitiesFromCurrentScene().Count, 1);
    }

    [Test]
    public void EntityDuplicateName()
    {
        string name = "Test";

        entityHandler.SetEntityName(entity, name);
        BIWEntity createdEntity = entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);

        entityHandler.SetEntityName(createdEntity, name);

        Assert.AreNotEqual(createdEntity.GetDescriptiveName(), name);
    }

    [Test]
    public void EntityDuplicate()
    {
        IDCLEntity duplicateEntity = entityHandler.DuplicateEntity(entity).rootEntity;
        BIWEntity convertedEntity = entityHandler.GetConvertedEntity(duplicateEntity);

        Assert.IsNotNull(convertedEntity);
    }

    [Test]
    public void ReportTransform()
    {
        //Act
        var currentTime = DCLTime.realtimeSinceStartup;
        entityHandler.ReportTransform();

        //Assert
        Assert.IsTrue(Mathf.Abs(entityHandler.GetLastTimeReport() - currentTime) <= 0.1f);
    }

    [Test]
    public void TestEntityInPointer()
    {
        //Act
        bool isEntityInPointer = entityHandler.IsPointerInSelectedEntity();

        //Assert
        Assert.IsFalse(isEntityInPointer);
    }

    [Test]
    public void EntityClicked()
    {
        //Arrange
        bool entitiyIsSelected = entity.isSelected;

        //Act
        entityHandler.EntityClicked(entity);

        //Assert
        Assert.AreNotEqual(entitiyIsSelected, entity.isSelected);
    }

    [Test]
    public void EntityDoubleClicked()
    {
        //Act
        entityHandler.EntityClicked(entity);
        entityHandler.EntityClicked(entity);

        //Assert
        context.modeController.Received(1).EntityDoubleClick(entity);
    }

    [Test]
    public void CancelLastSelection()
    {
        //Arrange
        entityHandler.SelectEntity(entity);

        //Act
        entityHandler.CancelSelection();

        //Assert
        Assert.IsFalse(entityHandler.IsAnyEntitySelected());
    }

    [Test]
    public void ChangeLockStateSelectedEntities()
    {
        //Arrange
        entity.isLocked = false;
        entityHandler.SelectEntity(entity);

        //Act
        entityHandler.ChangeLockStateSelectedEntities();

        //Assert
        Assert.IsTrue(entity.isLocked);
    }

    [Test]
    public void ChangeShowStateSelectedEntities()
    {
        //Arrange
        entityHandler.SelectEntity(entity);
        entity.isVisible = true;

        //Act
        entityHandler.ChangeShowStateSelectedEntities();

        //Assert
        Assert.IsFalse(entity.isVisible);
    }

    [Test]
    public void ShowAllEntities()
    {
        //Arrange
        entity.isVisible = false;

        //Act
        entityHandler.ShowAllEntities();

        //Assert
        Assert.IsTrue(entity.isVisible);
    }

    [Test]
    public void GetAllVoxelEntities()
    {
        //Arrange
        entity.isVoxel = true;

        //Act
        var voxelsEntities = entityHandler.GetAllVoxelsEntities();

        //Assert
        Assert.Greater(voxelsEntities.Count, 0);
    }

    [Test]
    public void DestroyLastCreatedEntities()
    {
        //Arrange
        var newEntity = entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero, false);
        newEntity.isNew = true;
        entityHandler.SelectEntity(newEntity);
        //Act
        entityHandler.DestroyLastCreatedEntities();

        //Assert
        Assert.IsTrue(newEntity.isDeleted);
    }

    [Test]
    public void DeleteSingleEntity()
    {
        //Arrange
        var newEntity = entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero, false);
        newEntity.isNew = true;

        //Act
        entityHandler.DeleteSingleEntity(newEntity);

        //Assert
        Assert.IsTrue(newEntity.isDeleted);
    }

    [UnityTest]
    public IEnumerator DeleteEntitiesOutsideBoundaries()
    {
        //Arrange
        var newEntity = entityHandler.CreateEmptyEntity(scene, Vector3.one * 99999, Vector3.one * 99999, false);
        newEntity.isNew = true;
        newEntity.rootEntity.gameObject.transform.position = Vector3.one * 444;

        TestHelpers.CreateAndSetShape(scene, newEntity.rootEntity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[newEntity.rootEntity.entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        //Act
        entityHandler.DeleteEntitiesOutsideSceneBoundaries();

        //Assert
        Assert.IsTrue(newEntity.isDeleted);
    }

    [Test]
    public void CheckEntitiesOutsideBoundaries()
    {
        //Arrange
        var newEntity = entityHandler.CreateEmptyEntity(scene, Vector3.one * 99999, Vector3.one * 99999, false);
        newEntity.isNew = true;

        //Act
        bool result =  entityHandler.AreAllEntitiesInsideBoundaries();

        //Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void InputMouseDown()
    {
        //Arrange
        entityHandler.isSecondayClickPressed = false;

        //Act
        entityHandler.OnInputMouseDown(1, Vector3.zero);

        //Assert
        Assert.IsTrue(entityHandler.isSecondayClickPressed);
    }

    [Test]
    public void InputMouseUp()
    {
        //Arrange
        entityHandler.isSecondayClickPressed = true;

        //Act
        entityHandler.OnInputMouseUp(1, Vector3.zero);

        //Assert
        Assert.IsFalse(entityHandler.isSecondayClickPressed);
    }

    [Test]
    public void SetMultiSelectionActive()
    {
        //Arrange
        entityHandler.isMultiSelectionActive = false;

        //Act
        entityHandler.SetMultiSelectionActive(true);

        //Assert
        Assert.IsTrue(entityHandler.isMultiSelectionActive);
    }

    [Test]
    public void ReportTransformUpdateTime()
    {
        //Act
        entityHandler.ReportTransform();

        //Assert
        Assert.IsTrue(Mathf.Abs(entityHandler.GetLastTimeReport() - DCLTime.realtimeSinceStartup) <= 0.01f);
    }

    [Test]
    public void IsPointerInSelectedEntity()
    {
        //Arrange
        entityHandler.SelectEntity(entity);
        context.raycastController.Configure().GetEntityOnPointer().Returns(entity);

        //Act
        var result = entityHandler.IsPointerInSelectedEntity();

        //Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void DeleteSelectedEntitiesInput()
    {
        //Arrange
        BIWEntity newEntity = new BIWEntity();
        entityHandler.SelectEntity(newEntity);

        //Act
        entityHandler.DeleteSelectedEntities();

        //Assert
        Assert.IsTrue(newEntity.isDeleted);
    }

    [Test]
    public void DuplicateSelectedEntitiesInput()
    {
        //Arrange
        BIWEntity newEntity = new BIWEntity();
        entityHandler.SelectEntity(newEntity);
        int selectedCount = entityHandler.GetSelectedEntityList().Count;

        //Act
        entityHandler.DuplicateSelectedEntities();

        //Assert
        Assert.IsTrue(selectedCount == entityHandler.GetSelectedEntityList().Count - 1);
    }

    [Test]
    public void SelectEntityFromList()
    {
        //Arrange
        BIWEntity newEntity = new BIWEntity();
        int selectedCount = entityHandler.GetSelectedEntityList().Count;

        //Act
        entityHandler.ChangeEntitySelectionFromList(newEntity);

        //Assert
        Assert.IsTrue(selectedCount == entityHandler.GetSelectedEntityList().Count - 1);
    }

    protected override IEnumerator TearDown()
    {
        entity.isVisible = true;
        entity.isLocked = false;
        entity.isVoxel = false;

        entityHandler.Dispose();

        yield return base.TearDown();
    }
}