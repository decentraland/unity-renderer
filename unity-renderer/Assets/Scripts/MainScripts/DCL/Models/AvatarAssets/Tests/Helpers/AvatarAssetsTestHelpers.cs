using System.Collections.Generic;
using DCL.Helpers;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AvatarAssetsTestHelpers
{
    public static WearableItemDummy CreateWearableItemDummy(string json)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<WearableItemDummy>(json);
    }

    public static void PrepareWearableItemDummy(WearableItemDummy wid)
    {
        wid.emoteDataV0 = null;
        wid.baseUrl = TestAssetsUtils.GetPath() + "/Avatar/Assets/";

        foreach (var rep in wid.data.representations)
        {
            rep.contents = rep.contents.Select((x) =>
                {
                    x.hash = x.key;
                    return x;
                })
                .ToArray();
        }

        wid.thumbnail = "";
    }

    public static BaseDictionary<string, WearableItem> CreateTestCatalogLocal()
    {
        List<WearableItemDummy> dummyWearables = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<WearableItemDummyListVariable>("TestCatalogArrayLocalAssets.asset")).list;

        foreach (var wearableItem in dummyWearables)
        {
            PrepareWearableItemDummy(wearableItem);
        }

        CatalogController.Clear();

        var wearables = dummyWearables.Select(x => x as WearableItem).ToArray();

        CatalogController.i.AddWearablesToCatalog(wearables);

        return CatalogController.wearableCatalog;
    }
}
