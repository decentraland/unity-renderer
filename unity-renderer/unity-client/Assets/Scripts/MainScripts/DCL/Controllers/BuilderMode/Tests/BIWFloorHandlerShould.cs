using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWFloorHandlerShould : IntegrationTestSuite_Legacy
{
    private BuilderInWorldEntityHandler entityHandler;
    private BuilderInWorldController controller;
    private BIWFloorHandler biwFloorHandler;
    private BIWCreatorController biwCreatorController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        controller = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];

        biwCreatorController = controller.biwCreatorController;
        biwCreatorController.Init();
        biwFloorHandler = controller.biwFloorHandler;
        biwFloorHandler.Init();
        entityHandler = controller.builderInWorldEntityHandler;
        entityHandler.Init();
        entityHandler.EnterEditMode(scene);

        biwFloorHandler.dclBuilderMeshLoadIndicatorController.Init();
    }

    [Test]
    public void CreateFloor()
    {
        //Arrange
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocalMultipleFloorObjects();
        CatalogItem floorItem = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        controller.InitGameObjects();
        controller.FindSceneToEdit();
        controller.InitControllers();

        biwCreatorController.EnterEditMode(scene);
        biwFloorHandler.EnterEditMode(scene);

        //Act
        biwFloorHandler.CreateFloor(floorItem);

        //Assert
        foreach (DCLBuilderInWorldEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                Assert.IsTrue(biwFloorHandler.ExistsFloorPlaceHolderForEntity(entity.rootEntity.entityId));
                Assert.AreEqual(entity.GetCatalogItemAssociated().id, floorItem.id);
                break;
            }
        }

        foreach (DCLBuilderInWorldEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                if (!entity.rootEntity.TryGetSharedComponent(CLASS_ID.GLTF_SHAPE, out ISharedComponent component))
                    Assert.Fail("Floor doesn't contains a GLTFShape!");

                entity.rootEntity.OnShapeUpdated?.Invoke(entity.rootEntity);
                Assert.IsFalse(biwFloorHandler.ExistsFloorPlaceHolderForEntity(entity.rootEntity.entityId));
                break;
            }
        }
    }

    [Test]
    public void ChangeFloor()
    {
        //Arrange
        BIWCatalogManager.Init();

        BuilderInWorldTestHelper.CreateTestCatalogLocalMultipleFloorObjects();

        CatalogItem oldFloor = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];
        CatalogItem newFloor = DataStore.i.builderInWorld.catalogItemDict.GetValues()[1];

        controller.InitGameObjects();
        controller.FindSceneToEdit();
        controller.InitControllers();

        biwCreatorController.EnterEditMode(scene);
        biwFloorHandler.EnterEditMode(scene);

        //Act
        biwFloorHandler.CreateFloor(oldFloor);
        biwFloorHandler.ChangeFloor(newFloor);

        //Assert
        foreach (DCLBuilderInWorldEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                Assert.AreEqual(entity.GetCatalogItemAssociated().id, newFloor.id);
                Assert.AreEqual(biwFloorHandler.FindCurrentFloorCatalogItem().id, newFloor.id);
                break;
            }
        }
    }

    protected override IEnumerator TearDown()
    {
        BIWCatalogManager.ClearCatalog();
        BuilderInWorldNFTController.i.ClearNFTs();
        controller.CleanItems();
        controller.gameObject.SetActive(false);
        yield return base.TearDown();
    }
}