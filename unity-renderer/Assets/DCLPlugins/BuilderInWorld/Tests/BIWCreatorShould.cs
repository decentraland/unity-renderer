using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Builder;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityGLTF;

public class BIWCreatorShould : IntegrationTestSuite_Legacy
{
    private BIWEntityHandler entityHandler;
    private BIWCreatorController biwCreatorController;
    private IContext context;
    private ParcelScene scene;
    private AssetCatalogBridge assetCatalogBridge;
    private CoreComponentsPlugin coreComponentsPlugin;
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        BuilderInWorldPlugin.RegisterRuntimeComponents();
        coreComponentsPlugin = new CoreComponentsPlugin();
        scene = TestUtils.CreateTestScene();

        biwCreatorController = new BIWCreatorController();
        entityHandler = new BIWEntityHandler();
        assetCatalogBridge = TestUtils.CreateComponentWithGameObject<AssetCatalogBridge>("AssetCatalogBridge");

        context = BIWTestUtils.CreateContextWithGenericMocks(
            entityHandler,
            biwCreatorController
        );
        var builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);

        biwCreatorController.Initialize(context);
        entityHandler.Initialize(context);

        entityHandler.EnterEditMode(builderScene);
        biwCreatorController.EnterEditMode(builderScene);
    }

    [Test]
    public void CreateItem()
    {
        //Arrange
        BIWCatalogManager.Init();
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        //Act
        biwCreatorController.CreateCatalogItem(item);

        //Assert
        foreach (BIWEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            Assert.IsTrue(entity.GetCatalogItemAssociated().id == item.id);
            Assert.AreEqual(Vector3.zero, entity.GetEulerRotation());
        }
    }

    [Test]
    public void CreateLastItem()
    {
        //Arrange
        BIWCatalogManager.Init();
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        //Act
        biwCreatorController.CreateCatalogItem(item);
        biwCreatorController.CreateLastCatalogItem();

        //Assert
        int cont = 0;
        foreach (BIWEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
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
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        //Act
        biwCreatorController.CreateCatalogItem(item);

        //Assert
        BIWEntity entity = entityHandler.GetAllEntitiesFromCurrentScene().FirstOrDefault();
        Assert.IsTrue(biwCreatorController.ExistsLoadingGameObjectForEntity(entity.rootEntity.entityId));
    }

    [Test]
    public void LoadingGameObjectDestruction()
    {
        //Arrange
        BIWCatalogManager.Init();
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        //Act
        biwCreatorController.CreateCatalogItem(item);
        BIWEntity entity = entityHandler.GetAllEntitiesFromCurrentScene().FirstOrDefault();
        biwCreatorController.RemoveLoadingObject(entity.rootEntity.entityId);

        //Assert
        Assert.IsFalse(biwCreatorController.ExistsLoadingGameObjectForEntity(entity.rootEntity.entityId));
    }

    [Test]
    public void ErrorGameObjectCreation()
    {
        // Arrange
        BIWCatalogManager.Init();
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];
        biwCreatorController.CreateCatalogItem(item);
        BIWEntity entity = entityHandler.GetAllEntitiesFromCurrentScene().FirstOrDefault();

        // Act
        biwCreatorController.CreateErrorOnEntity(entity);

        // Assert
        Assert.IsTrue(biwCreatorController.IsAnyErrorOnEntities());
    }

    [Test]
    public void ErrorGameObjectDestruction()
    {
        //Arrange
        BIWCatalogManager.Init();
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];
        biwCreatorController.CreateCatalogItem(item);
        BIWEntity entity = entityHandler.GetAllEntitiesFromCurrentScene().FirstOrDefault();
        biwCreatorController.CreateErrorOnEntity(entity);

        //Act
        biwCreatorController.DeleteErrorOnEntity(entity);

        //Assert
        Assert.IsFalse(biwCreatorController.IsAnyErrorOnEntities());
    }

    [Test]
    public void CatalogItemAddMappings()
    {
        //Arrange
        BIWCatalogManager.Init();
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);
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
        yield return new DCL.WaitUntil( () => GLTFComponent.downloadingCount == 0 );
        yield return null;

        Object.Destroy( assetCatalogBridge.gameObject );
        BIWCatalogManager.ClearCatalog();
        BIWNFTController.i.ClearNFTs();

        foreach (var placeHolder in Object.FindObjectsOfType<BIWLoadingPlaceHolder>())
        {
            placeHolder.Dispose();
        }

        context.Dispose();

        coreComponentsPlugin.Dispose();
        BuilderInWorldPlugin.UnregisterRuntimeComponents();

        yield return base.TearDown();
    }
}