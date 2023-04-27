using Cysharp.Threading.Tasks;
using DCLServices.Lambdas;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.TestTools;

namespace DCLServices.WearablesCatalogService
{
    public class LambdasWearablesCatalogServiceShould
    {
        const string USER_ID = "userId";
        private const string VALID_WEARABLE_ID = "validWearable";
        private const string WEARABLE_WITHOUT_THUMBNAIL = "wearableWithoutThumbnail";
        private const string BASE_WEARABLES_COLLECTION = "urn:decentraland:off-chain:base-avatars";
        private const string TPW_COLLECTION_ID = "tpwCollection";

        private LambdasWearablesCatalogService service;
        private ILambdasService lambdasService;
        private BaseDictionary<string, WearableItem> initialCatalog;

        [SetUp]
        public void SetUp()
        {
            lambdasService = Substitute.For<ILambdasService>();

            GivenWearableInLambdas(VALID_WEARABLE_ID,
                GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail"));

            GivenWearableInLambdas(WEARABLE_WITHOUT_THUMBNAIL,
                GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null));

            GivenCollectionInLambdas(BASE_WEARABLES_COLLECTION,
                new List<WearableItem>
                {
                    GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail"),
                    GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null),
                });

            GivenPaginatedCollectionInLambdas(TPW_COLLECTION_ID,
                new List<WearableDefinition>
                {
                    new () { definition = GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail"), },
                    new () { definition = GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null) },
                });

            GivenPaginatedWearableInLambdas(new List<WearableDefinition>
            {
                new () { definition = GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail") },
                new () { definition = GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null) },
            });

            initialCatalog = new BaseDictionary<string, WearableItem>();

            service = new LambdasWearablesCatalogService(initialCatalog, lambdasService);
            service.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            service.Dispose();
        }

        [UnityTest]
        public IEnumerator RequestWearableWhenIsInCache() =>
            UniTask.ToCoroutine(async () =>
            {
                service.AddWearablesToCatalog(new[] { GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail") });

                WearableItem resultantWearable = await service.RequestWearableAsync(VALID_WEARABLE_ID, default(CancellationToken));

                lambdasService.DidNotReceiveWithAnyArgs()
                              .Get<WearableWithoutDefinitionResponse>(endPointTemplate: default, endPoint: default);

                Assert.AreEqual(VALID_WEARABLE_ID, resultantWearable.id);
                Assert.AreEqual("description", resultantWearable.description);
                Assert.AreEqual("rare", resultantWearable.rarity);
                Assert.IsNull(resultantWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual("baseurl/thumbnail", resultantWearable.thumbnail);
                Assert.IsNull(resultantWearable.baseUrl);
                Assert.IsNull(resultantWearable.baseUrlBundles);
                Assert.IsNull(resultantWearable.emoteDataV0);
                Assert.AreEqual(resultantWearable, service.WearablesCatalog[VALID_WEARABLE_ID]);
            });

        [UnityTest]
        public IEnumerator RequestWearableWhenIsNotInCache() =>
            UniTask.ToCoroutine(async () =>
            {
                WearableItem resultantWearable = await service.RequestWearableAsync(VALID_WEARABLE_ID, default(CancellationToken));

                lambdasService.Received(1)
                              .Get<WearableWithoutDefinitionResponse>("collections/wearables/",
                                   "collections/wearables/",
                                   45,
                                   urlEncodedParams: ("wearableId", VALID_WEARABLE_ID),
                                   cancellationToken: Arg.Any<CancellationToken>());

                Assert.AreEqual(VALID_WEARABLE_ID, resultantWearable.id);
                Assert.AreEqual("description", resultantWearable.description);
                Assert.AreEqual("rare", resultantWearable.rarity);
                Assert.AreEqual("hash", resultantWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual("thumbnail", resultantWearable.thumbnail);
                Assert.AreEqual("baseurl/", resultantWearable.baseUrl);
                Assert.AreEqual("https://content-assets-as-bundle.decentraland.org/", resultantWearable.baseUrlBundles);
                Assert.IsNull(resultantWearable.emoteDataV0);
                Assert.AreEqual(resultantWearable, service.WearablesCatalog[VALID_WEARABLE_ID]);
            });

        [UnityTest]
        public IEnumerator ForceBaseUrlWhenNoThumbnail() =>
            UniTask.ToCoroutine(async () =>
            {
                await service.RequestWearableAsync(WEARABLE_WITHOUT_THUMBNAIL, default(CancellationToken));

                lambdasService.Received(1)
                              .Get<WearableWithoutDefinitionResponse>("collections/wearables/",
                                   "collections/wearables/",
                                   45,
                                   urlEncodedParams: ("wearableId", WEARABLE_WITHOUT_THUMBNAIL),
                                   cancellationToken: Arg.Any<CancellationToken>());

                WearableItem resultantWearable = service.WearablesCatalog[WEARABLE_WITHOUT_THUMBNAIL];
                Assert.AreEqual("https://interconnected.online/content/contents/", resultantWearable.baseUrl);
            });

        [Test]
        public void ValidWearableWhenIsInCache()
        {
            service.AddWearablesToCatalog(new[] { GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail") });

            bool isValidWearable = service.IsValidWearable(VALID_WEARABLE_ID);

            Assert.IsTrue(isValidWearable);
        }

        [Test]
        public void InvalidWearableWhenIsNotInCache()
        {
            bool isValidWearable = service.IsValidWearable(VALID_WEARABLE_ID);

            Assert.IsFalse(isValidWearable);
        }

        [UnityTest]
        public IEnumerator RequestBaseWearables() =>
            UniTask.ToCoroutine(async () =>
            {
                IReadOnlyList<WearableItem> baseWearables = await service.RequestBaseWearablesAsync(default(CancellationToken));

                lambdasService.Received(1)
                              .Get<WearableWithoutDefinitionResponse>("collections/wearables/",
                                   "collections/wearables/",
                                   45,
                                   urlEncodedParams: ("collectionId", BASE_WEARABLES_COLLECTION),
                                   cancellationToken: Arg.Any<CancellationToken>());

                WearableItem firstWearable = baseWearables[0];
                Assert.AreEqual(VALID_WEARABLE_ID, firstWearable.id);
                Assert.AreEqual("description", firstWearable.description);
                Assert.AreEqual("rare", firstWearable.rarity);
                Assert.AreEqual("hash", firstWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual("thumbnail", firstWearable.thumbnail);
                Assert.AreEqual("baseurl/", firstWearable.baseUrl);
                Assert.AreEqual("https://content-assets-as-bundle.decentraland.org/", firstWearable.baseUrlBundles);
                Assert.IsNull(firstWearable.emoteDataV0);
                Assert.AreEqual(firstWearable, service.WearablesCatalog[VALID_WEARABLE_ID]);

                WearableItem secondWearable = baseWearables[1];
                Assert.AreEqual(WEARABLE_WITHOUT_THUMBNAIL, secondWearable.id);
                Assert.AreEqual("description", secondWearable.description);
                Assert.AreEqual("rare", secondWearable.rarity);
                Assert.AreEqual("hash", secondWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual(string.Empty, secondWearable.thumbnail);
                Assert.AreEqual("https://interconnected.online/content/contents/", secondWearable.baseUrl);
                Assert.AreEqual("https://content-assets-as-bundle.decentraland.org/", secondWearable.baseUrlBundles);
                Assert.IsNull(secondWearable.emoteDataV0);
                Assert.AreEqual(secondWearable, service.WearablesCatalog[WEARABLE_WITHOUT_THUMBNAIL]);
            });

        [UnityTest]
        public IEnumerator RequestFirstPageOfThirdPartyCollection() =>
            UniTask.ToCoroutine(async () =>
            {
                (IReadOnlyList<WearableItem> wearables, int totalAmount) wearablesWithAmount = await service.RequestThirdPartyWearablesByCollectionAsync(USER_ID, TPW_COLLECTION_ID,
                    0, 10, true, default(CancellationToken));

                lambdasService.Received(1)
                              .Get<WearableWithDefinitionResponse>("users/",
                                   $"users/{USER_ID}/third-party-wearables/{TPW_COLLECTION_ID}",
                                   45,
                                   3,
                                   Arg.Any<CancellationToken>(),
                                   ("pageSize", "10"),
                                   ("pageNum", "0"),
                                   ("includeDefinitions", "true"));

                WearableItem firstWearable = wearablesWithAmount.wearables[0];
                Assert.AreEqual(VALID_WEARABLE_ID, firstWearable.id);
                Assert.AreEqual("description", firstWearable.description);
                Assert.AreEqual("rare", firstWearable.rarity);
                Assert.AreEqual("hash", firstWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual("thumbnail", firstWearable.thumbnail);
                Assert.AreEqual("baseurl/", firstWearable.baseUrl);
                Assert.AreEqual("https://content-assets-as-bundle.decentraland.org/", firstWearable.baseUrlBundles);
                Assert.IsNull(firstWearable.emoteDataV0);
                Assert.AreEqual(firstWearable, service.WearablesCatalog[VALID_WEARABLE_ID]);

                WearableItem secondWearable = wearablesWithAmount.wearables[1];
                Assert.AreEqual(WEARABLE_WITHOUT_THUMBNAIL, secondWearable.id);
                Assert.AreEqual("description", secondWearable.description);
                Assert.AreEqual("rare", secondWearable.rarity);
                Assert.AreEqual("hash", secondWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual(string.Empty, secondWearable.thumbnail);
                Assert.AreEqual("https://interconnected.online/content/contents/", secondWearable.baseUrl);
                Assert.AreEqual("https://content-assets-as-bundle.decentraland.org/", secondWearable.baseUrlBundles);
                Assert.IsNull(secondWearable.emoteDataV0);
                Assert.AreEqual(secondWearable, service.WearablesCatalog[WEARABLE_WITHOUT_THUMBNAIL]);
            });

        [UnityTest]
        public IEnumerator RequestOwnedWearables() =>
            UniTask.ToCoroutine(async () =>
            {
                (IReadOnlyList<WearableItem> wearables, int totalAmount) wearables = await service.RequestOwnedWearablesAsync(USER_ID, 0, 10, true, default(CancellationToken));

                lambdasService.Received(1)
                              .Get<WearableWithDefinitionResponse>("users/",
                                   $"users/{USER_ID}/wearables",
                                   45,
                                   3,
                                   Arg.Any<CancellationToken>(),
                                   ("pageSize", "10"),
                                   ("pageNum", "0"),
                                   ("includeDefinitions", "true"));

                WearableItem firstWearable = wearables.wearables[0];
                Assert.AreEqual(VALID_WEARABLE_ID, firstWearable.id);
                Assert.AreEqual("description", firstWearable.description);
                Assert.AreEqual("rare", firstWearable.rarity);
                Assert.AreEqual("hash", firstWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual("thumbnail", firstWearable.thumbnail);
                Assert.AreEqual("baseurl/", firstWearable.baseUrl);
                Assert.AreEqual("https://content-assets-as-bundle.decentraland.org/", firstWearable.baseUrlBundles);
                Assert.IsNull(firstWearable.emoteDataV0);
                Assert.AreEqual(firstWearable, service.WearablesCatalog[VALID_WEARABLE_ID]);

                WearableItem secondWearable = wearables.wearables[1];
                Assert.AreEqual(WEARABLE_WITHOUT_THUMBNAIL, secondWearable.id);
                Assert.AreEqual("description", secondWearable.description);
                Assert.AreEqual("rare", secondWearable.rarity);
                Assert.AreEqual("hash", secondWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual(string.Empty, secondWearable.thumbnail);
                Assert.AreEqual("https://interconnected.online/content/contents/", secondWearable.baseUrl);
                Assert.AreEqual("https://content-assets-as-bundle.decentraland.org/", secondWearable.baseUrlBundles);
                Assert.IsNull(secondWearable.emoteDataV0);
                Assert.AreEqual(secondWearable, service.WearablesCatalog[WEARABLE_WITHOUT_THUMBNAIL]);
            });

        [UnityTest]
        public IEnumerator ValidateParamsWhenRequestingOwnedWearablesWithFilters() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearableWithSpecificLambdasUrl(GivenValidWearableItem(VALID_WEARABLE_ID, ""));

                (IReadOnlyList<WearableItem> wearables, int totalAmount) =
                    await service.RequestOwnedWearablesAsync(USER_ID, 0, 10, default(CancellationToken),
                        category: "upper_body",
                        rarity: NftRarity.Epic,
                        name: "woah",
                        orderBy: (NftOrderByOperation.Date, true));

                lambdasService.Received(1)
                              .GetFromSpecificUrl<WearableWithDefinitionResponse>(
                                   "https://peer-ue-2.decentraland.zone/explorer-service/backpack/:userId/wearables",
                                   $"https://peer-ue-2.decentraland.zone/explorer-service/backpack/{USER_ID}/wearables",
                                   30, 3,
                                   Arg.Any<CancellationToken>(),
                                   Arg.Is<(string paramName, string paramValue)[]>(args =>
                                       args[0].paramName == "pageNumber"
                                       && args[0].paramValue == "0"
                                       && args[1].paramName == "pageSize"
                                       && args[1].paramValue == "10"
                                       && args[2].paramName == "rarity"
                                       && args[2].paramValue == "epic"
                                       && args[3].paramName == "categories"
                                       && args[3].paramValue == "upper_body"
                                       && args[4].paramName == "name"
                                       && args[4].paramValue == "woah"
                                       && args[5].paramName == "orderBy"
                                       && args[5].paramValue == "date"
                                       && args[6].paramName == "direction"
                                       && args[6].paramValue == "ASC"
                                       && args[7].paramName == "collectionCategory"
                                       && args[7].paramValue == "third-party,base-wearable,on-chain"));
            });

        [UnityTest]
        [TestCase("urn:decentraland:off-chain:base-avatars:male",
            "base-wearable",
            new[] {"urn:decentraland:off-chain:base-avatars:male"},
            ExpectedResult = null)]
        [TestCase("urn:collections-thirdparty:woah,urn:decentraland:off-chain:base-avatars:male",
            "third-party,base-wearable",
            new[] {"urn:collections-thirdparty:woah", "urn:decentraland:off-chain:base-avatars:male"},
            ExpectedResult = null)]
        [TestCase("urn:decentraland:custom:wearables,urn:decentraland:custom2:wearables,urn:collections-thirdparty:woah,urn:decentraland:off-chain:base-avatars:male",
            "third-party,base-wearable,on-chain",
            new[] {"urn:decentraland:custom:wearables", "urn:decentraland:custom2:wearables", "urn:collections-thirdparty:woah", "urn:decentraland:off-chain:base-avatars:male"},
            ExpectedResult = null)]
        public IEnumerator ValidateCollectionIdParamsWhenRequestingOwnedWearablesWithFilters(
            string expectedCollectionIdsParamValue,
            string expectedCollectionCategoryParamValue,
            string[] collectionIds) =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearableWithSpecificLambdasUrl(GivenValidWearableItem(VALID_WEARABLE_ID, ""));

                (IReadOnlyList<WearableItem> wearables, int totalAmount) =
                    await service.RequestOwnedWearablesAsync(USER_ID, 0, 10, default(CancellationToken),
                        collectionIds: collectionIds);

                lambdasService.Received(1)
                              .GetFromSpecificUrl<WearableWithDefinitionResponse>(
                                   "https://peer-ue-2.decentraland.zone/explorer-service/backpack/:userId/wearables",
                                   $"https://peer-ue-2.decentraland.zone/explorer-service/backpack/{USER_ID}/wearables",
                                   30, 3,
                                   Arg.Any<CancellationToken>(),
                                   Arg.Is<(string paramName, string paramValue)[]>(args =>
                                       args[0].paramName == "pageNumber"
                                       && args[0].paramValue == "0"
                                       && args[1].paramName == "pageSize"
                                       && args[1].paramValue == "10"
                                       && args[2].paramName == "collectionIds"
                                       && args[2].paramValue == expectedCollectionIdsParamValue
                                       && args[3].paramName == "collectionCategory"
                                       && args[3].paramValue == expectedCollectionCategoryParamValue));
            });

        [Test]
        public void RemoveWearables()
        {
            service.AddWearablesToCatalog(new[] { GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail") });
            service.AddWearablesToCatalog(new[] { GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null) });

            service.RemoveWearablesFromCatalog(new[] { VALID_WEARABLE_ID, WEARABLE_WITHOUT_THUMBNAIL });

            Assert.IsFalse(service.WearablesCatalog.ContainsKey(VALID_WEARABLE_ID));
            Assert.IsFalse(service.WearablesCatalog.ContainsKey(WEARABLE_WITHOUT_THUMBNAIL));
        }

        [Test]
        public void RemoveWearablesInUse()
        {
            service.EmbedWearables(new[] { GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail") });
            service.AddWearablesToCatalog(new[] { GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null) });

            service.RemoveWearablesInUse(new[] { VALID_WEARABLE_ID, WEARABLE_WITHOUT_THUMBNAIL });

            Assert.IsTrue(service.WearablesCatalog.ContainsKey(VALID_WEARABLE_ID));
            Assert.IsTrue(service.IsValidWearable(VALID_WEARABLE_ID));
            Assert.IsFalse(service.WearablesCatalog.ContainsKey(WEARABLE_WITHOUT_THUMBNAIL));
            Assert.IsFalse(service.IsValidWearable(WEARABLE_WITHOUT_THUMBNAIL));
        }

        private void GivenWearableWithSpecificLambdasUrl(WearableItem wearable)
        {
            lambdasService.GetFromSpecificUrl<WearableWithDefinitionResponse>(
                               Arg.Any<string>(),
                               Arg.Any<string>(),
                               Arg.Any<int>(),
                               Arg.Any<int>(),
                               Arg.Any<CancellationToken>(),
                               Arg.Any<(string paramName, string paramValue)[]>())
                          .Returns(UniTask.FromResult<(WearableWithDefinitionResponse response, bool success)>(
                               (new WearableWithDefinitionResponse(new List<WearableDefinition>
                               {
                                   new ()
                                   {
                                       definition = wearable
                                   },
                               }, 0, 10, 1), true)));
        }

        private void GivenWearableInLambdas(string wearableID, WearableItem wearable)
        {
            lambdasService.Get<WearableWithoutDefinitionResponse>(Arg.Any<string>(),
                               Arg.Any<string>(),
                               Arg.Any<int>(),
                               Arg.Any<int>(),
                               Arg.Any<CancellationToken>(),
                               urlEncodedParams: ("wearableId", wearableID))
                          .Returns(UniTask.FromResult<(WearableWithoutDefinitionResponse response, bool success)>(
                               (new WearableWithoutDefinitionResponse
                               {
                                   wearables = new List<WearableItem> { wearable },
                               }, true)));
        }

        private void GivenCollectionInLambdas(string collectionId, List<WearableItem> wearables)
        {
            lambdasService.Get<WearableWithoutDefinitionResponse>(Arg.Any<string>(),
                               Arg.Any<string>(),
                               Arg.Any<int>(),
                               Arg.Any<int>(),
                               Arg.Any<CancellationToken>(),
                               urlEncodedParams: ("collectionId", collectionId))
                          .Returns(UniTask.FromResult<(WearableWithoutDefinitionResponse response, bool success)>(
                               (new WearableWithoutDefinitionResponse
                               {
                                   wearables = wearables,
                               }, true)));
        }

        private void GivenPaginatedCollectionInLambdas(string collectionId, List<WearableDefinition> wearables)
        {
            lambdasService.Get<WearableWithDefinitionResponse>(Arg.Any<string>(),
                               $"users/{USER_ID}/third-party-wearables/{collectionId}",
                               Arg.Any<int>(),
                               Arg.Any<int>(),
                               Arg.Any<CancellationToken>(),
                               Arg.Is<(string paramName, string paramValue)>(arg => arg.paramName == "pageSize"),
                               Arg.Is<(string paramName, string paramValue)>(arg => arg.paramName == "pageNum"),
                               ("includeDefinitions", "true"))
                          .Returns(UniTask.FromResult<(WearableWithDefinitionResponse response, bool success)>(
                               (new WearableWithDefinitionResponse
                               {
                                   elements = wearables
                               }, true)));
        }

        private void GivenPaginatedWearableInLambdas(List<WearableDefinition> wearables)
        {
            lambdasService.Get<WearableWithDefinitionResponse>(Arg.Any<string>(),
                               $"users/{USER_ID}/wearables",
                               Arg.Any<int>(),
                               Arg.Any<int>(),
                               Arg.Any<CancellationToken>(),
                               Arg.Is<(string paramName, string paramValue)>(arg => arg.paramName == "pageSize"),
                               Arg.Is<(string paramName, string paramValue)>(arg => arg.paramName == "pageNum"),
                               ("includeDefinitions", "true"))
                          .Returns(UniTask.FromResult<(WearableWithDefinitionResponse response, bool success)>(
                               (new WearableWithDefinitionResponse
                               {
                                   elements = wearables
                               }, true)));
        }

        private WearableItem GivenValidWearableItem(string id, string thumbnail)
        {
            return new WearableItem
            {
                id = id,
                data = new WearableItem.Data
                {
                    representations = new WearableItem.Representation[]
                    {
                        new WearableItem.Representation()
                        {
                            contents = new WearableItem.MappingPair[]
                            {
                                new WearableItem.MappingPair()
                                {
                                    url = "representation/url/hash"
                                }
                            }
                        }
                    }
                },
                thumbnail = thumbnail,
                description = "description",
                rarity = "rare",
            };
        }
    }
}
