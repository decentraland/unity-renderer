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

    private SceneAssetPackDictionary sceneAssetPackCatalogValue;
    public SceneAssetPackDictionary sceneAssetPackCatalog
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

    private SceneObjectDictionary sceneObjectCatalogValue;
    public SceneObjectDictionary sceneObjectCatalog
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

    public void Awake() { i = this; }

    public ContentProvider GetContentProviderForAssetIdInSceneObjectCatalog(string assetId)
    {
        if (sceneObjectCatalogValue.TryGetValue(assetId, out SceneObject sceneObject))
            return CreateContentProviderForSceneObject(sceneObject);

        return null;
    }

    ContentProvider CreateContentProviderForSceneObject(SceneObject sceneObject)
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

    public SceneObject GetSceneObjectById(string assetId)
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

    public void ClearCatalog()
    {
        sceneObjectCatalog.Clear();
        sceneAssetPackCatalog.Clear();
    }

    public void AddSceneObjectToCatalog(SceneObject sceneObject)
    {
        if (sceneObjectCatalog.ContainsKey(sceneObject.id))
            return;

        //TODO: SmartItems This quit all the smart items from the catalog
        if (sceneObject.IsSmartItem())
            return;

        sceneObjectCatalog.Add(sceneObject.id, sceneObject);
        OnSceneObjectAdded?.Invoke(sceneObject);
    }

    public void AddSceneAssetPackToCatalog(SceneAssetPack sceneAssetPack)
    {
        if (sceneAssetPackCatalog.ContainsKey(sceneAssetPack.id))
            return;

        foreach (SceneObject sceneObject in sceneAssetPack.assets)
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