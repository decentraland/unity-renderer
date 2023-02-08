using System.Collections.Generic;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;
using System.Linq;
using UnityEngine;

public static class AvatarAssetsTestHelpers
{
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

    public static BaseDictionary<string, WearableItem> CreateTestCatalogLocal(IWearablesCatalogService wearablesCatalogService)
    {
        List<WearableItemDummy> dummyWearables = Object.Instantiate(Resources.Load<WearableItemDummyListVariable>("TestCatalogArrayLocalAssets")).list;

        foreach (var wearableItem in dummyWearables)
        {
            PrepareWearableItemDummy(wearableItem);
        }

        wearablesCatalogService.Clear();

        var wearables = dummyWearables.Select(x => x as WearableItem).ToArray();

        wearablesCatalogService.AddWearablesToCatalog(wearables);

        return wearablesCatalogService.WearablesCatalog;
    }
}
