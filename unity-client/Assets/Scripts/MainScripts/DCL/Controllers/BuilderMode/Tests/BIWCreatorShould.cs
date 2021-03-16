using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;

public class BIWCreatorShould : IntegrationTestSuite_Legacy
{
    private BuilderInWorldEntityHandler entityHandler;
    private BuilderInWorldController controller;
    private BIWCreatorController biwCreatorController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        controller = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];

        biwCreatorController = controller.biwCreatorController;
        biwCreatorController.Init();
        entityHandler = controller.builderInWorldEntityHandler;
        entityHandler.Init();

        entityHandler.EnterEditMode(scene);
        biwCreatorController.EnterEditMode(scene);
    }

    [Test]
    public void CreateItem()
    {
        //Arrange
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        //Act
        biwCreatorController.CreateCatalogItem(item);

        //Assert
        foreach (DCLBuilderInWorldEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            Assert.IsTrue(entity.GetCatalogItemAssociated().id == item.id);
        }
    }

    [Test]
    public void CreateLastItem()
    {
        //Arrange
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        //Act
        biwCreatorController.CreateCatalogItem(item);
        biwCreatorController.CreateLastCatalogItem();

        //Assert
        int cont = 0;
        foreach (DCLBuilderInWorldEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.GetCatalogItemAssociated().id == item.id)
                cont++;
        }
        Assert.AreEqual(cont, 2);
    }

    [Test]
    public void LoadingGameObjectCreation()
    {
        //Arrange
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        //Act
        biwCreatorController.CreateCatalogItem(item);

        //Assert
        DCLBuilderInWorldEntity entity = entityHandler.GetAllEntitiesFromCurrentScene().FirstOrDefault();
        Assert.IsTrue(biwCreatorController.ExistsLoadingGameObjectForEntity(entity.rootEntity.entityId));
    }

    [Test]
    public void LoadingGameObjectDestruction()
    {
        //Arrange
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        //Act
        biwCreatorController.CreateCatalogItem(item);
        DCLBuilderInWorldEntity entity = entityHandler.GetAllEntitiesFromCurrentScene().FirstOrDefault();
        entity.rootEntity.OnShapeUpdated?.Invoke(entity.rootEntity);

        //Assert
        Assert.IsFalse(biwCreatorController.ExistsLoadingGameObjectForEntity(entity.rootEntity.entityId));
    }

    [Test]
    public void CatalogItemAddMapings()
    {
        //Arrange
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        //Act
        biwCreatorController.CreateCatalogItem(item);

        //Assert
        LoadParcelScenesMessage.UnityParcelScene data = scene.sceneData;
        foreach (KeyValuePair<string, string> content in item.contents)
        {
            ContentServerUtils.MappingPair mappingPair = new ContentServerUtils.MappingPair();
            mappingPair.file = content.Key;
            mappingPair.hash = content.Value;
            bool found = false;
            foreach (ContentServerUtils.MappingPair mappingPairToCheck in data.contents)
            {
                if (mappingPairToCheck.file == mappingPair.file)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found);
        }
    }

    protected override IEnumerator TearDown()
    {
        BIWCatalogManager.ClearCatalog();
        BuilderInWorldNFTController.i.ClearNFTs();
        controller.CleanItems();
        yield return base.TearDown();
    }
}