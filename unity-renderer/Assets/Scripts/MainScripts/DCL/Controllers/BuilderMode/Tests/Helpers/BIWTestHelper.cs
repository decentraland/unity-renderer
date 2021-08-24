using DCL.Helpers;
using DCL.Helpers.NFT;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using Newtonsoft.Json;
using NSubstitute;
using UnityEngine;
using UnityEngine.UI;

public static class BIWTestHelper
{
    public static BIWContext CreateMockUpReferenceController()
    {
        BIWContext context = new BIWContext();
        context.Init(
            Substitute.For<IBIWOutlinerController>(),
            Substitute.For<IBIWInputHandler>(),
            Substitute.For<IBIWInputWrapper>(),
            Substitute.For<IBIWPublishController>(),
            Substitute.For<IBIWCreatorController>(),
            Substitute.For<IBIWModeController>(),
            Substitute.For<IBIWFloorHandler>(),
            Substitute.For<IBIWEntityHandler>(),
            Substitute.For<IBIWActionController>(),
            Substitute.For<IBIWSaveController>(),
            Substitute.For<IBIWRaycastController>(),
            Substitute.For<IBIWGizmosController>(),
            new InitialSceneReferences()
        );
        return context;
    }
    public static BIWContext CreateReferencesControllerWithGenericMocks(params object[] mocks)
    {
        IBIWOutlinerController outliner = Substitute.For<IBIWOutlinerController>();
        IBIWInputHandler inputHandler = Substitute.For<IBIWInputHandler>();
        IBIWInputWrapper inputWrapper = Substitute.For<IBIWInputWrapper>();
        IBIWPublishController publishController = Substitute.For<IBIWPublishController>();
        IBIWCreatorController creatorController = Substitute.For<IBIWCreatorController>();
        IBIWModeController modeController = Substitute.For<IBIWModeController>();
        IBIWFloorHandler floorHandler = Substitute.For<IBIWFloorHandler>();
        IBIWEntityHandler entityHandler = Substitute.For<IBIWEntityHandler>();
        IBIWActionController actionController = Substitute.For<IBIWActionController>();
        IBIWSaveController saveController = Substitute.For<IBIWSaveController>();
        IBIWRaycastController raycastController = Substitute.For<IBIWRaycastController>();
        IBIWGizmosController gizmosController = Substitute.For<IBIWGizmosController>();
        InitialSceneReferences sceneReferences = new InitialSceneReferences();

        foreach ( var mock in mocks)
        {
            switch ( mock )
            {
                case IBIWOutlinerController oc:
                    outliner = oc;
                    break;
                case IBIWInputHandler ih:
                    inputHandler = ih;
                    break;
                case IBIWInputWrapper iw:
                    inputWrapper = iw;
                    break;
                case IBIWPublishController pc:
                    publishController = pc;
                    break;
                case IBIWCreatorController cc:
                    creatorController = cc;
                    break;
                case IBIWModeController mc:
                    modeController = mc;
                    break;
                case IBIWFloorHandler fh:
                    floorHandler = fh;
                    break;
                case IBIWEntityHandler eh:
                    entityHandler = eh;
                    break;
                case IBIWActionController ac:
                    actionController = ac;
                    break;
                case IBIWSaveController sc:
                    saveController = sc;
                    break;
                case IBIWRaycastController rc:
                    raycastController = rc;
                    break;
                case IBIWGizmosController gc:
                    gizmosController = gc;
                    break;
                case InitialSceneReferences isr:
                    sceneReferences = isr;
                    break;
            }
        }

        BIWContext context = new BIWContext();
        context.Init(
            outliner,
            inputHandler,
            inputWrapper,
            publishController,
            creatorController,
            modeController,
            floorHandler,
            entityHandler,
            actionController,
            saveController,
            raycastController,
            gizmosController,
            sceneReferences
        );
        return context;
    }

    public static BIWEntity CreateSmartItemEntity(BIWEntityHandler entityHandler, ParcelScene scene, SmartItemComponent.Model model = null)
    {
        if (model == null)
            model = new SmartItemComponent.Model();

        BIWEntity entity = entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);

        scene.EntityComponentCreateOrUpdateWithModel(entity.rootEntity.entityId, CLASS_ID_COMPONENT.SMART_ITEM, model);

        return entity;
    }

    public static CatalogItemAdapter CreateCatalogItemAdapter(GameObject gameObject)
    {
        CatalogItemAdapter adapter = Utils.GetOrCreateComponent<CatalogItemAdapter>(gameObject);
        adapter.onFavoriteColor = Color.white;
        adapter.offFavoriteColor = Color.white;
        adapter.favImg = Utils.GetOrCreateComponent<Image>(gameObject);
        adapter.smartItemGO = gameObject;
        adapter.lockedGO = gameObject;
        adapter.canvasGroup = Utils.GetOrCreateComponent<CanvasGroup>(gameObject);

        GameObject newGameObject = new GameObject();
        newGameObject.transform.SetParent(gameObject.transform);
        adapter.thumbnailImg = Utils.GetOrCreateComponent<RawImage>(newGameObject);
        return adapter;
    }

    public static void CreateTestCatalogLocalMultipleFloorObjects()
    {
        BIWCatalogManager.Init();
        AssetCatalogBridge.i.ClearCatalog();
        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/multipleSceneObjectsCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
        }
    }

    public static void CreateTestCatalogLocalSingleObject()
    {
        BIWCatalogManager.Init();
        AssetCatalogBridge.i.ClearCatalog();
        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/sceneObjectCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
        }
    }

    public static void CreateTestSmartItemCatalogLocalSingleObject()
    {
        AssetCatalogBridge.i.ClearCatalog();
        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/smartItemSceneObjectCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
        }
    }

    public static void CreateNFT()
    {
        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/nftAsset.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            NFTOwner owner = NFTOwner.defaultNFTOwner;
            owner.assets.Add(JsonUtility.FromJson<NFTInfo>(jsonValue));
            BIWNFTController.i.NftsFeteched(owner);
        }
    }
}