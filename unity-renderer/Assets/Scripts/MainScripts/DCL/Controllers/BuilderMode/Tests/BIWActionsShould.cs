using DCL;
using DCL.Controllers;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWActionsShould : IntegrationTestSuite_Legacy
{
    private const string ENTITY_ID = "1";
    private BuilderInWorldController controller;
    private ActionController actionController;
    private BuilderInWorldEntityHandler entityHandler;
    private BIWFloorHandler biwFloorHandler;
    private BIWCreatorController biwCreatorController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        controller = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];
        actionController = controller.actionController;
        entityHandler = controller.builderInWorldEntityHandler;
        biwFloorHandler = controller.biwFloorHandler;
        biwCreatorController = controller.biwCreatorController;
        entityHandler.Init();

        TestHelpers.CreateSceneEntity(scene, ENTITY_ID);
        entityHandler.EnterEditMode(scene);
    }

    [Test]
    public void UndoRedoMoveAction()
    {
        BuildInWorldCompleteAction buildModeAction = new BuildInWorldCompleteAction();

        Vector3 oldPosition = scene.entities[ENTITY_ID].gameObject.transform.position;
        Vector3 newPosition = new Vector3(5, 5, 5);

        BuilderInWorldEntityAction entityAction = new BuilderInWorldEntityAction(ENTITY_ID);
        entityAction.oldValue = oldPosition;
        entityAction.newValue = newPosition;

        buildModeAction.CreateActionType(entityAction, BuildInWorldCompleteAction.ActionType.MOVE);

        scene.entities[ENTITY_ID].gameObject.transform.position = newPosition;
        actionController.AddAction(buildModeAction);

        actionController.TryToUndoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.position == oldPosition);

        actionController.TryToRedoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.position == newPosition);
    }

    [Test]
    public void UndoRedoRotateAction()
    {
        BuildInWorldCompleteAction buildModeAction = new BuildInWorldCompleteAction();

        Vector3 oldRotation = scene.entities[ENTITY_ID].gameObject.transform.rotation.eulerAngles;
        Vector3 newRotation = new Vector3(5, 5, 5);

        BuilderInWorldEntityAction entityAction = new BuilderInWorldEntityAction(ENTITY_ID);
        entityAction.oldValue = oldRotation;
        entityAction.newValue = newRotation;

        buildModeAction.CreateActionType(entityAction, BuildInWorldCompleteAction.ActionType.ROTATE);

        scene.entities[ENTITY_ID].gameObject.transform.rotation = Quaternion.Euler(newRotation);
        actionController.AddAction(buildModeAction);

        actionController.TryToUndoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.rotation.eulerAngles == oldRotation);

        actionController.TryToRedoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.rotation.eulerAngles == newRotation);
    }

    [Test]
    public void UndoRedoScaleAction()
    {
        BuildInWorldCompleteAction buildModeAction = new BuildInWorldCompleteAction();

        Vector3 oldScale = scene.entities[ENTITY_ID].gameObject.transform.localScale;
        Vector3 newScale = new Vector3(5, 5, 5);

        BuilderInWorldEntityAction entityAction = new BuilderInWorldEntityAction(ENTITY_ID);
        entityAction.oldValue = oldScale;
        entityAction.newValue = newScale;

        buildModeAction.CreateActionType(entityAction, BuildInWorldCompleteAction.ActionType.SCALE);

        scene.entities[ENTITY_ID].gameObject.transform.localScale = newScale;
        actionController.AddAction(buildModeAction);

        actionController.TryToUndoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.localScale == oldScale);

        actionController.TryToRedoAction();
        Assert.IsTrue(scene.entities[ENTITY_ID].gameObject.transform.localScale == newScale);
    }

    [Test]
    public void UndoRedoCreateDeleteActions()
    {
        actionController.CreateActionEntityCreated(scene.entities[ENTITY_ID]);
        actionController.TryToUndoAction();
        Assert.IsFalse(scene.entities.ContainsKey(ENTITY_ID));

        actionController.TryToRedoAction();
        Assert.IsTrue(scene.entities.ContainsKey(ENTITY_ID));

        DCLBuilderInWorldEntity biwEntity = Utils.GetOrCreateComponent<DCLBuilderInWorldEntity>(scene.entities[ENTITY_ID].gameObject);
        biwEntity.Init(scene.entities[ENTITY_ID], null);

        actionController.CreateActionEntityDeleted(biwEntity);
        actionController.TryToUndoAction();
        Assert.IsTrue(scene.entities.ContainsKey(ENTITY_ID));

        actionController.TryToRedoAction();
        Assert.IsFalse(scene.entities.ContainsKey(ENTITY_ID));
    }

    [Test]
    public void UndoRedoChangeFloorAction()
    {
        BIWCatalogManager.Init();

        BuilderInWorldTestHelper.CreateTestCatalogLocalMultipleFloorObjects();

        CatalogItem oldFloor = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];
        CatalogItem newFloor = DataStore.i.builderInWorld.catalogItemDict.GetValues()[1];
        BuildInWorldCompleteAction buildModeAction = new BuildInWorldCompleteAction();

        controller.InitGameObjects();
        controller.FindSceneToEdit();
        controller.InitControllers();

        biwCreatorController.EnterEditMode(scene);
        biwFloorHandler.EnterEditMode(scene);

        biwFloorHandler.CreateFloor(oldFloor);
        biwFloorHandler.ChangeFloor(newFloor);

        buildModeAction.CreateChangeFloorAction(oldFloor, newFloor);
        actionController.AddAction(buildModeAction);

        foreach (DCLBuilderInWorldEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                Assert.AreEqual(entity.GetCatalogItemAssociated().id, newFloor.id);
                break;
            }
        }

        actionController.TryToUndoAction();

        foreach (DCLBuilderInWorldEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                Assert.AreEqual(entity.GetCatalogItemAssociated().id, oldFloor.id);

                break;
            }
        }

        actionController.TryToRedoAction();

        foreach (DCLBuilderInWorldEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                Assert.AreEqual(entity.GetCatalogItemAssociated().id, newFloor.id);
                break;
            }
        }
    }

    protected override IEnumerator TearDown()
    {
        BIWCatalogManager.ClearCatalog();
        BuilderInWorldNFTController.i.ClearNFTs();
        controller.CleanItems();
        actionController.Clear();
        yield return base.TearDown();
    }
}