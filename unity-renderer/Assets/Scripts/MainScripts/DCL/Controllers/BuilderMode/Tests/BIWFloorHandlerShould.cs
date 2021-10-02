﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

public class BIWFloorHandlerShould : IntegrationTestSuite_Legacy
{
    private BIWEntityHandler entityHandler;
    private BIWFloorHandler biwFloorHandler;
    private BIWCreatorController biwCreatorController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        biwCreatorController = new BIWCreatorController();
        biwFloorHandler = new BIWFloorHandler();
        entityHandler = new BIWEntityHandler();

        var referencesController = BIWTestUtils.CreateContextWithGenericMocks(
            entityHandler,
            biwFloorHandler,
            biwCreatorController
        );

        biwCreatorController.Initialize(referencesController);
        biwFloorHandler.Initialize(referencesController);
        entityHandler.Initialize(referencesController);

        entityHandler.EnterEditMode(scene);
        biwFloorHandler.EnterEditMode(scene);
        entityHandler.EnterEditMode(scene);
    }

    [Test]
    public void CreateFloor()
    {
        //Arrange
        BIWCatalogManager.Init();
        BIWTestUtils.CreateTestCatalogLocalMultipleFloorObjects();
        CatalogItem floorItem = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        biwCreatorController.EnterEditMode(scene);
        biwFloorHandler.EnterEditMode(scene);

        //Act
        biwFloorHandler.CreateFloor(floorItem);

        //Assert
        foreach (BIWEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                Assert.IsTrue(biwFloorHandler.ExistsFloorPlaceHolderForEntity(entity.rootEntity.entityId));
                Assert.AreEqual(entity.GetCatalogItemAssociated().id, floorItem.id);
                break;
            }
        }

        foreach (BIWEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
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

        BIWTestUtils.CreateTestCatalogLocalMultipleFloorObjects();

        CatalogItem oldFloor = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];
        CatalogItem newFloor = DataStore.i.builderInWorld.catalogItemDict.GetValues()[1];

        biwCreatorController.EnterEditMode(scene);
        biwFloorHandler.EnterEditMode(scene);

        //Act
        biwFloorHandler.CreateFloor(oldFloor);
        biwFloorHandler.ChangeFloor(newFloor);

        //Assert
        foreach (BIWEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
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
        yield return new DCL.WaitUntil( () => GLTFComponent.downloadingCount == 0 );

        BIWCatalogManager.ClearCatalog();
        BIWNFTController.i.ClearNFTs();
        entityHandler.Dispose();
        biwFloorHandler.Dispose();
        biwCreatorController.Dispose();
        yield return base.TearDown();
    }
}