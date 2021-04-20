using DCL.Helpers;
using DCL.Helpers.NFT;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;
using UnityEngine.UI;

public static class BuilderInWorldTestHelper
{
    public static DCLBuilderInWorldEntity CreateSmartItemEntity(BuilderInWorldEntityHandler entityHandler, ParcelScene scene, SmartItemComponent.Model model = null)
    {
        if (model == null)
            model = new SmartItemComponent.Model();

        DCLBuilderInWorldEntity entity = entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);

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
        AssetCatalogBridge.i.ClearCatalog();
        string jsonPath = Utils.GetTestAssetsPathRaw() + "/BuilderInWorldCatalog/multipleSceneObjectsCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
        }
    }

    public static void CreateTestCatalogLocalSingleObject()
    {
        AssetCatalogBridge.i.ClearCatalog();
        string jsonPath = Utils.GetTestAssetsPathRaw() + "/BuilderInWorldCatalog/sceneObjectCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
        }
    }

    public static void CreateTestSmartItemCatalogLocalSingleObject()
    {
        AssetCatalogBridge.i.ClearCatalog();
        string jsonPath = Utils.GetTestAssetsPathRaw() + "/BuilderInWorldCatalog/smartItemSceneObjectCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
        }
    }

    public static void CreateNFT()
    {
        string jsonPath = Utils.GetTestAssetsPathRaw() + "/BuilderInWorldCatalog/nftAsset.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            NFTOwner owner = NFTOwner.defaultNFTOwner;
            owner.assets.Add(JsonUtility.FromJson<NFTInfo>(jsonValue));
            BuilderInWorldNFTController.i.NftsFeteched(owner);
        }
    }
}