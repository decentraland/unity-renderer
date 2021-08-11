using System.Collections.Generic;
using DCL.Helpers;
using System.Linq;
using UnityEngine;

public static class AvatarAssetsTestHelpers
{
    public static BaseDictionary<string, WearableItem> CreateTestCatalogLocal()
    {
        List<WearableItemDummy> wearables = Object.Instantiate(Resources.Load<WearableItemDummyListVariable>("TestCatalogArrayLocalAssets")).list;
        foreach (var wearableItem in wearables)
        {
            wearableItem.baseUrl = TestAssetsUtils.GetPath() + "/Avatar/Assets/";

            foreach (var rep in wearableItem.data.representations)
            {
                rep.contents = rep.contents.Select((x) =>
                    {
                        x.hash = x.key;
                        return x;
                    })
                    .ToArray();
            }

            wearableItem.thumbnail = "";
        }

        CatalogController.wearableCatalog.Clear();
        var dummyWereables = wearables.Select(x => new KeyValuePair<string, WearableItem>(x.id, x)).ToArray();
        foreach (var item in dummyWereables)
        {
            CatalogController.wearableCatalog.Add(item.Key, item.Value);
        }

        return CatalogController.wearableCatalog;
    }
}