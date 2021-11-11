using System;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Helpers.NFT;
using System.IO;
using System.Linq;
using DCL;
using DCL.Builder;
using DCL.Camera;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class BIWTestUtils
{
    public static ISceneReferences CreateMocekdInitialSceneReference()
    {
        ISceneReferences sceneReferences = SceneReferences.i;

        if (sceneReferences == null)
        {
            GameObject gameObjectToDestroy = new GameObject("BIWTestGO");
            var mouseCatcher = gameObjectToDestroy.AddComponent<MouseCatcher>();
            gameObjectToDestroy.AddComponent<BuilderInWorldBridge>();
            var avatarController = gameObjectToDestroy.AddComponent<PlayerAvatarController>();
            var cameraController = gameObjectToDestroy.AddComponent<CameraController>();

            sceneReferences = Substitute.For<ISceneReferences>();
            sceneReferences.Configure().biwCameraParent.Returns(gameObjectToDestroy);
            sceneReferences.Configure().cursorCanvas.Returns(gameObjectToDestroy);
            sceneReferences.Configure().mouseCatcher.Returns(mouseCatcher);
            sceneReferences.Configure().bridgeGameObject.Returns(gameObjectToDestroy);
            sceneReferences.Configure().bridges.Returns(gameObjectToDestroy);
            sceneReferences.Configure().playerAvatarController.Returns(avatarController);
            sceneReferences.Configure().cameraController.Returns(cameraController);
            sceneReferences.Configure().mainCamera.Returns(Camera.main);

            sceneReferences.When( x => x.Dispose())
                           .Do( x => Object.Destroy(gameObjectToDestroy));
        }

        return sceneReferences;
    }

    public static IContext CreateMockedContext()
    {
        IContext context = new Context(
            Substitute.For<IBIWEditor>(),
            Substitute.For<IBuilderMainPanelController>(),
            Substitute.For<IBuilderAPIController>(),
            Substitute.For<IBuilderEditorHUDController>(),
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
            Substitute.For<IInitialSceneReferences>()
        );
        return context;
    }
    
    public static IContext CreateMockedContextForTestScene()
    {
        IContext context = new Context(
            Substitute.For<IBIWEditor>(),
            Substitute.For<IBuilderMainPanelController>(),
            Substitute.For<IBuilderAPIController>(),
            Substitute.For<IBuilderEditorHUDController>(),
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
            CreateMocekdInitialSceneReference()
        );
        return context;
    }

    public static IContext CreateContextWithGenericMocks(params object[] mocks)
    {
        IBIWEditor editor = Substitute.For<IBIWEditor>();
        IBuilderAPIController apiController = Substitute.For<IBuilderAPIController>();
        IBuilderMainPanelController panelHUD = Substitute.For<IBuilderMainPanelController>();
        IBuilderEditorHUDController editorHUD = Substitute.For<IBuilderEditorHUDController>();

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
        ISceneReferences sceneReferences =    CreateMocekdInitialSceneReference();

        foreach (var mock in mocks)
        {
            switch ( mock )
            {
                case IBIWEditor e:
                    editor = e;
                    break;
                case IBuilderMainPanelController ppc:
                    panelHUD = ppc;
                    break;
                case IBuilderAPIController ibapc:
                    apiController = ibapc;
                    break;
                case IBuilderEditorHUDController ehc:
                    editorHUD = ehc;
                    break;
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
                case ISceneReferences isr:
                    sceneReferences = isr;
                    break;
            }
        }

        IContext context = new Context(editor,
            panelHUD,
            apiController,
            editorHUD,
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

    public static CatalogItem CreateTestCatalogLocalSingleObject()
    {
        BIWCatalogManager.Init();
        AssetCatalogBridge.i.ClearCatalog();
        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/sceneObjectCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
            CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];
            return item;
        }

        return null;
    }

    public static void CreateTestSmartItemCatalogLocalSingleObject()
    {
        BIWCatalogManager.Init();
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