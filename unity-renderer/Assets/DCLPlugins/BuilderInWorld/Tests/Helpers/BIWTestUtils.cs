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
using CameraController = DCL.Camera.CameraController;
using Object = UnityEngine.Object;

public static class BIWTestUtils
{
    public static IBuilderScene CreateBuilderSceneFromParcelScene(ParcelScene scene)
    {
        IBuilderScene builderScene = Substitute.For<IBuilderScene>();
        builderScene.Configure().scene.Returns(scene);
        return builderScene;
    }
    
    public static ISceneReferences CreateMockedInitialSceneReference()
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
            sceneReferences.Configure().biwBridgeGameObject.Returns(gameObjectToDestroy);
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
            Substitute.For<ISceneManager>(),
            Substitute.For<ICameraController>(),
            Substitute.For<IPublisher>(),
            Substitute.For<ICommonHUD>(),
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
            Substitute.For<ISceneReferences>()
        );
        return context;
    }

    public static IContext CreateMockedContextForTestScene()
    {
        IContext context = new Context(
            Substitute.For<IBIWEditor>(),
            Substitute.For<IBuilderMainPanelController>(),
            Substitute.For<IBuilderAPIController>(),
            Substitute.For<ISceneManager>(),
            Substitute.For<ICameraController>(),
            Substitute.For<IPublisher>(),
            Substitute.For<ICommonHUD>(),
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
            CreateMockedInitialSceneReference()
        );
        return context;
    }

    public static IContext CreateContextWithGenericMocks(params object[] mocks)
    {
        IBIWEditor editor = Substitute.For<IBIWEditor>();
        IBuilderAPIController apiController = Substitute.For<IBuilderAPIController>();
        IBuilderMainPanelController panelHUD = Substitute.For<IBuilderMainPanelController>();
        IBuilderEditorHUDController editorHUD = Substitute.For<IBuilderEditorHUDController>();
        ISceneManager sceneManager = Substitute.For<ISceneManager>();
        ICameraController cameraController = Substitute.For<ICameraController>();
        IPublisher publisher =  Substitute.For<IPublisher>();
        ICommonHUD commonHUD =  Substitute.For<ICommonHUD>();

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
        ISceneReferences sceneReferences = CreateMockedInitialSceneReference();

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
                case ISceneManager ism:
                    sceneManager = ism;
                    break;
                case ICameraController icc:
                    cameraController = icc;
                    break;
                case IPublisher ip:
                    publisher = ip;
                    break;
                case ICommonHUD cHUD:
                    commonHUD = cHUD;
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
            sceneManager,
            cameraController,
            publisher,
            commonHUD,
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

        scene.componentsManagerLegacy.EntityComponentCreateOrUpdate(entity.rootEntity.entityId, CLASS_ID_COMPONENT.SMART_ITEM, model);

        return entity;
    }

    public static CatalogItemAdapter CreateCatalogItemAdapter(GameObject gameObject)
    {
        CatalogItemAdapter adapter = gameObject.GetOrCreateComponent<CatalogItemAdapter>();
        adapter.onFavoriteColor = Color.white;
        adapter.offFavoriteColor = Color.white;
        adapter.favImg = gameObject.GetOrCreateComponent<Image>();
        adapter.smartItemGO = gameObject;
        adapter.lockedGO = gameObject;
        adapter.canvasGroup = gameObject.GetOrCreateComponent<CanvasGroup>();

        GameObject newGameObject = new GameObject();
        newGameObject.transform.SetParent(gameObject.transform);
        adapter.thumbnailImg = newGameObject.GetOrCreateComponent<RawImage>();
        return adapter;
    }

    public static void CreateTestCatalogLocalMultipleFloorObjects(AssetCatalogBridge assetCatalog)
    {
        BIWCatalogManager.Init();
        assetCatalog.ClearCatalog();
        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/multipleSceneObjectsCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            assetCatalog.AddFullSceneObjectCatalog(jsonValue);
        }
    }

    public static CatalogItem CreateTestCatalogLocalSingleObject(AssetCatalogBridge assetCatalog)
    {
        BIWCatalogManager.Init();
        assetCatalog.ClearCatalog();
        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/sceneObjectCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            assetCatalog.AddFullSceneObjectCatalog(jsonValue);
            CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];
            return item;
        }

        return null;
    }

    public static void CreateTestSmartItemCatalogLocalSingleObject(AssetCatalogBridge assetCatalog)
    {
        BIWCatalogManager.Init();
        assetCatalog.ClearCatalog();
        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/smartItemSceneObjectCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            assetCatalog.AddFullSceneObjectCatalog(jsonValue);
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