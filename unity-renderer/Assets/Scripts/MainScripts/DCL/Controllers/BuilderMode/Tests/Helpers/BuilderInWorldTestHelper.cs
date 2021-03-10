using DCL.Helpers;
using DCL.Helpers.NFT;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public static class BuilderInWorldTestHelper 
{
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
        AssetCatalogBridge.ClearCatalog();
        string jsonPath = Utils.GetTestAssetsPathRaw() + "/BuilderInWorldCatalog/multipleSceneObjectsCatalog.json";

        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
        }
    }

    public static void CreateTestCatalogLocalSingleObject()
    {
        AssetCatalogBridge.ClearCatalog();
        string jsonPath = Utils.GetTestAssetsPathRaw() + "/BuilderInWorldCatalog/sceneObjectCatalog.json";

        if(File.Exists(jsonPath))
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
