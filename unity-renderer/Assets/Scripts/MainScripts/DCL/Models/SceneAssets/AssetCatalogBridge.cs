using DCL;
using DCL.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetCatalogBridge : MonoBehaviour
{
    public static bool VERBOSE = false;

    public static System.Action<SceneObject> OnSceneObjectAdded;
    public static System.Action<SceneAssetPack> OnSceneAssetPackAdded;

    public static AssetCatalogBridge i { get; private set; }

    private static SceneAssetPackDictionary sceneAssetPackCatalogValue;
    public static SceneAssetPackDictionary sceneAssetPackCatalog
    {
        get
        {
            if (sceneAssetPackCatalogValue == null)
            {
                sceneAssetPackCatalogValue = Resources.Load<SceneAssetPackDictionary>("SceneAssetPackCatalog");
            }

            return sceneAssetPackCatalogValue;
        }
    }

    private static SceneObjectDictionary sceneObjectCatalogValue;
    public static SceneObjectDictionary sceneObjectCatalog
    {
        get
        {
            if (sceneObjectCatalogValue == null)
            {
                sceneObjectCatalogValue = Resources.Load<SceneObjectDictionary>("SceneObjectCatalog");
            }

            return sceneObjectCatalogValue;
        }
    }

    public void Awake()
    {
        i = this;
    }

    public static ContentProvider GetContentProviderForAssetIdInSceneObjectCatalog(string assetId)
    {
        if (sceneObjectCatalogValue.TryGetValue(assetId, out SceneObject sceneObject))
            return CreateContentProviderForSceneObject(sceneObject);

        return null;
    }

    static ContentProvider CreateContentProviderForSceneObject(SceneObject sceneObject)
    {
        ContentProvider contentProvider = new ContentProvider();
        contentProvider.baseUrl = BuilderInWorldSettings.BASE_URL_CATALOG;
        foreach (KeyValuePair<string, string> content in sceneObject.contents)
        {
            ContentServerUtils.MappingPair mappingPair = new ContentServerUtils.MappingPair();
            mappingPair.file = content.Key;
            mappingPair.hash = content.Value;
            contentProvider.contents.Add(mappingPair);
        }

        contentProvider.BakeHashes();
        return contentProvider;
    }

    public static SceneObject GetSceneObjectById(string assetId)
    {
        if (sceneObjectCatalogValue.TryGetValue(assetId, out SceneObject sceneObject))
            return sceneObject;
        return null;
    }

    public void AddFullSceneObjectCatalog(string payload)
    {
        JObject jObject = (JObject)JsonConvert.DeserializeObject(payload);
        if (jObject["ok"].ToObject<bool>())
        {

            JArray array = JArray.Parse(jObject["data"].ToString());

            foreach (JObject item in array)
            {
                i.AddSceneAssetPackToCatalog(item);
            }
        }
    }

    public static void ClearCatalog()
    {
        sceneObjectCatalog.Clear();
        sceneAssetPackCatalog.Clear();
    }

    public static void AddSceneObjectToCatalog(SceneObject sceneObject)
    {
        if (sceneObjectCatalog.ContainsKey(sceneObject.id))
            return;

        sceneObjectCatalog.Add(sceneObject.id, sceneObject);
        OnSceneObjectAdded?.Invoke(sceneObject);
    }

    public static void AddSceneAssetPackToCatalog(SceneAssetPack sceneAssetPack)
    {
        if (sceneAssetPackCatalog.ContainsKey(sceneAssetPack.id))
            return;

        foreach(SceneObject sceneObject in sceneAssetPack.assets)
        {
            AddSceneObjectToCatalog(sceneObject);
        }

        sceneAssetPackCatalog.Add(sceneAssetPack.id, sceneAssetPack);
        OnSceneAssetPackAdded?.Invoke(sceneAssetPack);
    }

    public void AddSceneAssetPackToCatalog(JObject payload)
    {
        if (VERBOSE)
            Debug.Log("add sceneAssetPack: " + payload);

        SceneAssetPack sceneAssetPack = JsonConvert.DeserializeObject<SceneAssetPack>(payload.ToString());
        AddSceneAssetPackToCatalog(sceneAssetPack);
    }
}
