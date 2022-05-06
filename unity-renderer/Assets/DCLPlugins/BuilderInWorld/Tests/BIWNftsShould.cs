using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

public class BIWNftsShould : IntegrationTestSuite
{
    private ParcelScene scene;
    private const long ENTITY_ID = 1;

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<ISceneController>(() => new SceneController());
        serviceLocator.Register<IWorldState>(() => new WorldState());
        serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
        serviceLocator.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker());
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();
        TestUtils.CreateSceneEntity(scene, ENTITY_ID);
        TestUtils_NFT.RegisterMockedNFTShape(Environment.i.world.componentFactory);
        BIWCatalogManager.Init();
        BIWTestUtils.CreateNFT();
    }

    [Test]
    public void NftsOneTimeUsage()
    {
        string idToTest = BIWNFTController.i.GetNfts()[0].assetContract.address;

        Assert.IsFalse(BIWNFTController.i.IsNFTInUse(idToTest));

        BIWNFTController.i.UseNFT(idToTest);
        Assert.IsTrue(BIWNFTController.i.IsNFTInUse(idToTest));

        BIWNFTController.i.StopUsingNFT(idToTest);
        Assert.IsFalse(BIWNFTController.i.IsNFTInUse(idToTest));
    }

    [Test]
    public void NftComponent()
    {
        BIWTestUtils.CreateNFT();
        CatalogItem catalogItem = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        BIWEntity biwEntity = new BIWEntity();
        biwEntity.Initialize(scene.entities[ENTITY_ID], null);

        NFTShape nftShape = (NFTShape) scene.componentsManagerLegacy.SceneSharedComponentCreate(catalogItem.id, Convert.ToInt32(CLASS_ID.NFT_SHAPE));
        nftShape.model = new NFTShape.Model();
        nftShape.model.color = new Color(0.6404918f, 0.611472f, 0.8584906f);
        nftShape.model.src = catalogItem.model;
        nftShape.model.assetId = catalogItem.id;

        scene.componentsManagerLegacy.SceneSharedComponentAttach(biwEntity.rootEntity.entityId, nftShape.id);

        Assert.IsTrue(biwEntity.IsEntityNFT());

        CatalogItem associatedCatalogItem = biwEntity.GetCatalogItemAssociated();
        Assert.IsTrue(associatedCatalogItem.IsNFT());
        Assert.AreEqual(associatedCatalogItem, catalogItem);
    }

    protected override IEnumerator TearDown()
    {
        DataStore.i.builderInWorld.catalogItemDict.Clear();
        BIWCatalogManager.ClearCatalog();
        BIWNFTController.i.ClearNFTs();
        yield return base.TearDown();
        PoolManager.i.Dispose();
    }
}