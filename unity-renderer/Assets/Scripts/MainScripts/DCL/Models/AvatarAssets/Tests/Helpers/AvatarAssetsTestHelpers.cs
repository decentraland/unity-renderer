using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;
using NSubstitute;
using System.Linq;
using System.Threading;
using UnityEngine;

public static class AvatarAssetsTestHelpers
{
    private static void PrepareWearableItemDummy(WearableItemDummy wid)
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

    public static IWearablesCatalogService CreateTestCatalogLocal()
    {
        IWearablesCatalogService wearablesCatalogService = Substitute.For<IWearablesCatalogService>();
        List<WearableItemDummy> dummyWearables = Object.Instantiate(Resources.Load<WearableItemDummyListVariable>("TestCatalogArrayLocalAssets")).list;
        BaseDictionary<string, WearableItem> dummyCatalog = new ();

        foreach (var wearableItem in dummyWearables)
        {
            PrepareWearableItemDummy(wearableItem);
            dummyCatalog.Add(wearableItem.id, wearableItem);
        }

        wearablesCatalogService.WearablesCatalog.Returns(dummyCatalog);

        wearablesCatalogService
           .RequestOwnedWearablesAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
           .Returns(_ =>
            {
                UniTaskCompletionSource<IReadOnlyList<WearableItem>> mockedResult = new UniTaskCompletionSource<IReadOnlyList<WearableItem>>();
                mockedResult.TrySetResult(new List<WearableItem>());
                return mockedResult.Task;
            });

        wearablesCatalogService
           .RequestBaseWearablesAsync(Arg.Any<CancellationToken>())
           .Returns(_ =>
            {
                UniTaskCompletionSource<IReadOnlyList<WearableItem>> mockedResult = new UniTaskCompletionSource<IReadOnlyList<WearableItem>>();
                mockedResult.TrySetResult(new List<WearableItem>());
                return mockedResult.Task;
            });

        wearablesCatalogService
           .RequestThirdPartyWearablesByCollectionAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
           .Returns(_ =>
            {
                UniTaskCompletionSource<IReadOnlyList<WearableItem>> mockedResult = new UniTaskCompletionSource<IReadOnlyList<WearableItem>>();
                mockedResult.TrySetResult(new List<WearableItem>());
                return mockedResult.Task;
            });

        wearablesCatalogService
           .RequestWearableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
           .Returns(_ =>
            {
                UniTaskCompletionSource<WearableItem> mockedResult = new UniTaskCompletionSource<WearableItem>();
                mockedResult.TrySetResult(null);
                return mockedResult.Task;
            });

        return wearablesCatalogService;
    }
}
