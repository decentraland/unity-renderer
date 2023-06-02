using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace MainScripts.DCL.Models.AvatarAssets.Tests.Helpers
{
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
            List<WearableItemDummy> dummyWearables = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<WearableItemDummyListVariable>("Assets/Scripts/MainScripts/DCL/Models/AvatarAssets/Tests/Helpers/TestCatalogArrayLocalAssets.asset")).list;
            BaseDictionary<string, WearableItem> dummyCatalog = new ();

            foreach (var wearableItem in dummyWearables)
            {
                PrepareWearableItemDummy(wearableItem);
                dummyCatalog.Add(wearableItem.id, wearableItem);

                wearablesCatalogService
                   .RequestWearableAsync(wearableItem.id, Arg.Any<CancellationToken>())
                   .Returns(_ => UniTask.FromResult<WearableItem>(wearableItem));
            }

            wearablesCatalogService.WearablesCatalog.Returns(dummyCatalog);

            wearablesCatalogService
               .RequestOwnedWearablesAsync(
                    Arg.Any<string>(),
                    Arg.Any<int>(),
                    Arg.Any<int>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
               .Returns(_ => UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((new List<WearableItem>(), 0)));

            wearablesCatalogService
               .RequestOwnedWearablesAsync(
                    Arg.Any<string>(),
                    Arg.Any<int>(),
                    Arg.Any<int>(),
                    Arg.Any<CancellationToken>(),
                    Arg.Any<string>(),
                    Arg.Any<NftRarity>(),
                    Arg.Any<NftCollectionType>(),
                    Arg.Any<ICollection<string>>(),
                    Arg.Any<string>(),
                    Arg.Any<(NftOrderByOperation type, bool directionAscendent)>())
               .ReturnsForAnyArgs(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((new List<WearableItem>(), 0)));

            wearablesCatalogService
               .RequestBaseWearablesAsync(Arg.Any<CancellationToken>())
               .Returns(_ => UniTask.FromResult<IReadOnlyList<WearableItem>>(new List<WearableItem>()));

            wearablesCatalogService
               .RequestThirdPartyWearablesByCollectionAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<int>(),
                    Arg.Any<int>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
               .Returns(_ => UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((new List<WearableItem>(), 0)));

            return wearablesCatalogService;
        }
    }
}
