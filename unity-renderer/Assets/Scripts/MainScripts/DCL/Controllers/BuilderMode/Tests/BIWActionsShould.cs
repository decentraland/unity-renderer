using DCL;
using DCL.Controllers;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;
using WaitUntil = UnityEngine.WaitUntil;

public class BIWActionsShould : IntegrationTestSuite_Legacy
{
    private const string ENTITY_ID = "1";
    private BIWContext context;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        TestHelpers.CreateSceneEntity(scene, ENTITY_ID);

        var biwActionController = new BIWActionController();
        var entityHandler = new BIWEntityHandler();
        var biwFloorHandler = new BIWFloorHandler();
        var biwCreatorController = new BIWCreatorController();

        context = BIWTestUtils.CreateContextWithGenericMocks(
            biwActionController,
            entityHandler,
            biwFloorHandler,
            biwCreatorController
        );

        biwActionController.Initialize(context);
        entityHandler.Initialize(context);
        biwFloorHandler.Initialize(context);
        biwCreatorController.Initialize(context);

        biwActionController.EnterEditMode(scene);
        entityHandler.EnterEditMode(scene);
        biwFloorHandler.EnterEditMode(scene);
        biwCreatorController.EnterEditMode(scene);
    }

    [Test]
    public void UndoRedoMoveAction()
    {
        BIWCompleteAction buildModeAction = new BIWCompleteAction();

        Vector3 oldPosition = scene.entities[ENTITY_ID].gameObject.transform.position;
        Vector3 newPosition = new Vector3(5, 5, 5);

        BIWEntityAction entityAction = new BIWEntityAction(ENTITY_ID);
        entityAction.oldValue = oldPosition;
        entityAction.newValue = newPosition;

        buildModeAction.CreateActionType(entityAction, BIWCompleteAction.ActionType.MOVE);

        scene.entities[ENTITY_ID].gameObject.transform.position = newPosition;
        context.actionController.AddAction(buildModeAction);

        context.actionController.TryToUndoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.position == oldPosition);

        context.actionController.TryToRedoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.position == newPosition);
    }

    [Test]
    public void UndoRedoRotateAction()
    {
        BIWCompleteAction buildModeAction = new BIWCompleteAction();

        Vector3 oldRotation = scene.entities[ENTITY_ID].gameObject.transform.rotation.eulerAngles;
        Vector3 newRotation = new Vector3(5, 5, 5);

        BIWEntityAction entityAction = new BIWEntityAction(ENTITY_ID);
        entityAction.oldValue = oldRotation;
        entityAction.newValue = newRotation;

        buildModeAction.CreateActionType(entityAction, BIWCompleteAction.ActionType.ROTATE);

        scene.entities[ENTITY_ID].gameObject.transform.rotation = Quaternion.Euler(newRotation);
        context.actionController.AddAction(buildModeAction);

        context.actionController.TryToUndoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.rotation.eulerAngles == oldRotation);

        context.actionController.TryToRedoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.rotation.eulerAngles == newRotation);
    }

    [Test]
    public void UndoRedoScaleAction()
    {
        BIWCompleteAction buildModeAction = new BIWCompleteAction();

        Vector3 oldScale = scene.entities[ENTITY_ID].gameObject.transform.localScale;
        Vector3 newScale = new Vector3(5, 5, 5);

        BIWEntityAction entityAction = new BIWEntityAction(ENTITY_ID);
        entityAction.oldValue = oldScale;
        entityAction.newValue = newScale;

        buildModeAction.CreateActionType(entityAction, BIWCompleteAction.ActionType.SCALE);

        scene.entities[ENTITY_ID].gameObject.transform.localScale = newScale;
        context.actionController.AddAction(buildModeAction);

        context.actionController.TryToUndoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.localScale == oldScale);

        context.actionController.TryToRedoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.localScale == newScale);
    }

    [Test]
    public void UndoRedoCreateDeleteActions()
    {
        context.actionController.CreateActionEntityCreated(scene.entities[ENTITY_ID]);
        context.actionController.TryToUndoAction();
        Assert.IsFalse(scene.entities.ContainsKey(ENTITY_ID));

        context.actionController.TryToRedoAction();
        Assert.IsTrue(scene.entities.ContainsKey(ENTITY_ID));

        BIWEntity biwEntity = new BIWEntity();
        biwEntity.Initialize(scene.entities[ENTITY_ID], null);

        context.actionController.CreateActionEntityDeleted(biwEntity);
        context.actionController.TryToUndoAction();
        Assert.IsTrue(scene.entities.ContainsKey(ENTITY_ID));

        context.actionController.TryToRedoAction();
        Assert.IsFalse(scene.entities.ContainsKey(ENTITY_ID));
    }

    [UnityTest]
    public IEnumerator UndoRedoChangeFloorAction()
    {
        BIWCatalogManager.Init();

        BIWTestUtils.CreateTestCatalogLocalMultipleFloorObjects();

        CatalogItem oldFloor = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];
        CatalogItem newFloor = DataStore.i.builderInWorld.catalogItemDict.GetValues()[1];
        BIWCompleteAction buildModeAction = new BIWCompleteAction();

        context.floorHandler.CreateFloor(oldFloor);
        context.floorHandler.ChangeFloor(newFloor);

        buildModeAction.CreateChangeFloorAction(oldFloor, newFloor);
        context.actionController.AddAction(buildModeAction);

        yield return new WaitUntil( () => GLTFComponent.downloadingCount == 0 );

        foreach (BIWEntity entity in context.entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                Assert.AreEqual(entity.GetCatalogItemAssociated().id, newFloor.id);
                break;
            }
        }

        context.actionController.TryToUndoAction();

        foreach (BIWEntity entity in context.entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                Assert.AreEqual(entity.GetCatalogItemAssociated().id, oldFloor.id);
                break;
            }
        }

        context.actionController.TryToRedoAction();

        foreach (BIWEntity entity in context.entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                Assert.AreEqual(entity.GetCatalogItemAssociated().id, newFloor.id);
                break;
            }
        }

        context.floorHandler.Dispose();
    }

    protected override IEnumerator TearDown()
    {
        BIWCatalogManager.ClearCatalog();
        BIWNFTController.i.ClearNFTs();
        context.Dispose();
        yield return base.TearDown();
    }
}