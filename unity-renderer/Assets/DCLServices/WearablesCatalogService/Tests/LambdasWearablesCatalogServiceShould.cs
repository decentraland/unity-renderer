using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.Lambdas;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine.TestTools;
using static DCLServices.WearablesCatalogService.LambdasWearablesCatalogService;
using static DCLServices.WearablesCatalogService.WearableWithEntityResponseDto.ElementDto;
using static DCLServices.WearablesCatalogService.WearableWithEntityResponseDto.ElementDto.EntityDto;

namespace DCLServices.WearablesCatalogService
{
    public class LambdasWearablesCatalogServiceShould
    {
        private const string ASSET_BUNDLES_URL_ORG = "https://content-assets-as-bundle.decentraland.org/";
        private const string VALID_WEARABLE_ID = "validWearable";
        private const string WEARABLE_WITHOUT_THUMBNAIL = "wearableWithoutThumbnail";
        private const string BASE_WEARABLES_COLLECTION = "urn:decentraland:off-chain:base-avatars";
        private const string TPW_COLLECTION_ID = "tpwCollection";
        private const string CONTENT_URL = "http://catalyst.url/content/";
        private const string LAMBDAS_URL = "http://catalyst.url/lambdas/";
        private const string EXPLORER_URL = "http://catalyst.url/explorer/";
        private const string USER_ID = "userId";

        private LambdasWearablesCatalogService service;
        private ILambdasService lambdasService;
        private BaseDictionary<string, WearableItem> initialCatalog;
        private IServiceProviders serviceProviders;

        [SetUp]
        public void SetUp()
        {
            lambdasService = Substitute.For<ILambdasService>();

            GivenWearableInLambdas(GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail"));

            GivenWearableInLambdas(GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null));

            GivenCollectionInLambdas(BASE_WEARABLES_COLLECTION,
                new List<WearableItem>
                {
                    GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail"),
                    GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null),
                });

            GivenPaginatedCollectionInLambdas(TPW_COLLECTION_ID,
                new List<WearableElementV1Dto>
                {
                    new () { definition = GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail"), },
                    new () { definition = GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null) },
                });

            GivenPaginatedWearableInLambdas(new List<WearableElementV1Dto>
            {
                new () { definition = GivenValidWearableItem(VALID_WEARABLE_ID, "baseurl/thumbnail") },
                new () { definition = GivenValidWearableItem(WEARABLE_WITHOUT_THUMBNAIL, null) },
            });

            initialCatalog = new BaseDictionary<string, WearableItem>();
            serviceProviders = Substitute.For<IServiceProviders>();
            ICatalyst catalyst = Substitute.For<ICatalyst>();
            catalyst.contentUrl.Returns(CONTENT_URL);
            catalyst.lambdasUrl.Returns(LAMBDAS_URL);
            catalyst.GetLambdaUrl(CancellationToken.None).Returns(UniTask.FromResult(LAMBDAS_URL));
            serviceProviders.catalyst.Returns(catalyst);

            BaseVariable<FeatureFlag> featureFlags = new BaseVariable<FeatureFlag>();
            featureFlags.Set(new FeatureFlag());
            service = new LambdasWearablesCatalogService(initialCatalog, lambdasService, serviceProviders, featureFlags);
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
                              .PostFromSpecificUrl<EntityDto[], WearableRequest>(
                                   Arg.Any<string>(),
                                   Arg.Any<string>(),
                                   Arg.Is<WearableRequest>(wr => wr.pointers.Contains(VALID_WEARABLE_ID)),
                                   cancellationToken: Arg.Any<CancellationToken>());

                Assert.AreEqual(VALID_WEARABLE_ID, resultantWearable.id);
                Assert.AreEqual("description", resultantWearable.description);
                Assert.AreEqual("rare", resultantWearable.rarity);
                Assert.AreEqual("hash", resultantWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual("thumbnail", resultantWearable.thumbnail);
                Assert.AreEqual("baseurl/", resultantWearable.baseUrl);
                Assert.AreEqual(ASSET_BUNDLES_URL_ORG, resultantWearable.baseUrlBundles);
                Assert.IsNull(resultantWearable.emoteDataV0);
                Assert.AreEqual(resultantWearable, service.WearablesCatalog[VALID_WEARABLE_ID]);
            });

        [UnityTest]
        public IEnumerator ForceBaseUrlWhenNoThumbnail() =>
            UniTask.ToCoroutine(async () =>
            {
                await service.RequestWearableAsync(WEARABLE_WITHOUT_THUMBNAIL, default(CancellationToken));

                lambdasService.Received(1)
                              .PostFromSpecificUrl<EntityDto[], WearableRequest>(
                                   Arg.Any<string>(),
                                   Arg.Any<string>(),
                                   Arg.Is<WearableRequest>(wr => wr.pointers.Contains(WEARABLE_WITHOUT_THUMBNAIL)),
                                   cancellationToken: Arg.Any<CancellationToken>());

                WearableItem resultantWearable = service.WearablesCatalog[WEARABLE_WITHOUT_THUMBNAIL];
                Assert.AreEqual($"{CONTENT_URL}contents/", resultantWearable.baseUrl);
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
                await service.RequestBaseWearablesAsync(default(CancellationToken));

                var url = $"{CONTENT_URL}entities/active/collections/{BASE_WEARABLES_COLLECTION}";

                lambdasService.Received(1)
                              .GetFromSpecificUrl<WearableCollectionResponse>(url, url, cancellationToken: Arg.Any<CancellationToken>());

                WearableItem firstWearable = service.WearablesCatalog[VALID_WEARABLE_ID];
                Assert.AreEqual(VALID_WEARABLE_ID, firstWearable.id);
                Assert.AreEqual("description", firstWearable.description);
                Assert.AreEqual("rare", firstWearable.rarity);
                Assert.AreEqual("hash", firstWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual("thumbnail", firstWearable.thumbnail);
                Assert.AreEqual("baseurl/", firstWearable.baseUrl);
                Assert.AreEqual(ASSET_BUNDLES_URL_ORG, firstWearable.baseUrlBundles);
                Assert.IsNull(firstWearable.emoteDataV0);

                WearableItem secondWearable = service.WearablesCatalog[WEARABLE_WITHOUT_THUMBNAIL];
                Assert.AreEqual(WEARABLE_WITHOUT_THUMBNAIL, secondWearable.id);
                Assert.AreEqual("description", secondWearable.description);
                Assert.AreEqual("rare", secondWearable.rarity);
                Assert.AreEqual("hash", secondWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual(string.Empty, secondWearable.thumbnail);
                Assert.AreEqual($"{CONTENT_URL}contents/", secondWearable.baseUrl);
                Assert.AreEqual(ASSET_BUNDLES_URL_ORG, secondWearable.baseUrlBundles);
                Assert.IsNull(secondWearable.emoteDataV0);
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
                Assert.AreEqual(ASSET_BUNDLES_URL_ORG, firstWearable.baseUrlBundles);
                Assert.IsNull(firstWearable.emoteDataV0);
                Assert.AreEqual(firstWearable, service.WearablesCatalog[VALID_WEARABLE_ID]);

                WearableItem secondWearable = wearablesWithAmount.wearables[1];
                Assert.AreEqual(WEARABLE_WITHOUT_THUMBNAIL, secondWearable.id);
                Assert.AreEqual("description", secondWearable.description);
                Assert.AreEqual("rare", secondWearable.rarity);
                Assert.AreEqual("hash", secondWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual(string.Empty, secondWearable.thumbnail);
                Assert.AreEqual($"{CONTENT_URL}contents/", secondWearable.baseUrl);
                Assert.AreEqual(ASSET_BUNDLES_URL_ORG, secondWearable.baseUrlBundles);
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
                Assert.AreEqual(ASSET_BUNDLES_URL_ORG, firstWearable.baseUrlBundles);
                Assert.IsNull(firstWearable.emoteDataV0);
                Assert.AreEqual(firstWearable, service.WearablesCatalog[VALID_WEARABLE_ID]);

                WearableItem secondWearable = wearables.wearables[1];
                Assert.AreEqual(WEARABLE_WITHOUT_THUMBNAIL, secondWearable.id);
                Assert.AreEqual("description", secondWearable.description);
                Assert.AreEqual("rare", secondWearable.rarity);
                Assert.AreEqual("hash", secondWearable.data.representations[0].contents[0].hash);
                Assert.AreEqual(string.Empty, secondWearable.thumbnail);
                Assert.AreEqual($"{CONTENT_URL}contents/", secondWearable.baseUrl);
                Assert.AreEqual(ASSET_BUNDLES_URL_ORG, secondWearable.baseUrlBundles);
                Assert.IsNull(secondWearable.emoteDataV0);
                Assert.AreEqual(secondWearable, service.WearablesCatalog[WEARABLE_WITHOUT_THUMBNAIL]);
            });

        [UnityTest]
        public IEnumerator ValidateParamsWhenRequestingOwnedWearablesWithFilters() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearableEntityWithSpecificLambdasUrl(GivenValidWearableEntity(VALID_WEARABLE_ID));

                (IReadOnlyList<WearableItem> wearables, int totalAmount) =
                    await service.RequestOwnedWearablesAsync(USER_ID, 0, 10, default(CancellationToken),
                        category: "upper_body",
                        rarity: NftRarity.Epic,
                        name: "woah",
                        orderBy: (NftOrderByOperation.Date, true));

                lambdasService.Received(1)
                              .GetFromSpecificUrl<WearableWithEntityResponseDto>(
                                   $"{EXPLORER_URL}/:userId/wearables",
                                   $"{EXPLORER_URL}/{USER_ID}/wearables",
                                   30, 3,
                                   Arg.Any<CancellationToken>(),
                                   Arg.Is<(string paramName, string paramValue)[]>(args =>
                                       args[0].paramName == "pageNum"
                                       && args[0].paramValue == "0"
                                       && args[1].paramName == "pageSize"
                                       && args[1].paramValue == "10"
                                       && args[2].paramName == "includeEntities"
                                       && args[2].paramValue == "true"
                                       && args[3].paramName == "rarity"
                                       && args[3].paramValue == "epic"
                                       && args[4].paramName == "category"
                                       && args[4].paramValue == "upper_body"
                                       && args[5].paramName == "name"
                                       && args[5].paramValue == "woah"
                                       && args[6].paramName == "orderBy"
                                       && args[6].paramValue == "date"
                                       && args[7].paramName == "direction"
                                       && args[7].paramValue == "ASC"
                                       && args[8].paramName == "collectionType"
                                       && args[8].paramValue == "base-wearable"
                                       && args[9].paramName == "collectionType"
                                       && args[9].paramValue == "on-chain"
                                       && args[10].paramName == "collectionType"
                                       && args[10].paramValue == "third-party"));
            });

        [UnityTest]
        [TestCase(NftCollectionType.Base, ExpectedResult = null)]
        [TestCase(NftCollectionType.OnChain, ExpectedResult = null)]
        public IEnumerator ValidateCollectionIdParamsWhenRequestingOwnedWearablesWithoutThirdPartyFilters(NftCollectionType collectionType) =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearableEntityWithSpecificLambdasUrl(GivenValidWearableEntity(VALID_WEARABLE_ID));

                (IReadOnlyList<WearableItem> wearables, int totalAmount) =
                    await service.RequestOwnedWearablesAsync(USER_ID, 0, 10, default(CancellationToken),
                        collectionTypeMask: collectionType);

                lambdasService.Received(1)
                              .GetFromSpecificUrl<WearableWithEntityResponseDto>(
                                   $"{EXPLORER_URL}/:userId/wearables",
                                   $"{EXPLORER_URL}/{USER_ID}/wearables",
                                   30, 3,
                                   Arg.Any<CancellationToken>(),
                                   Arg.Is<(string paramName, string paramValue)[]>(args =>
                                       args[0].paramName == "pageNum"
                                       && args[0].paramValue == "0"
                                       && args[1].paramName == "pageSize"
                                       && args[1].paramValue == "10"
                                       && args[2].paramName == "includeEntities"
                                       && args[2].paramValue == "true"
                                       && args[3].paramName == "collectionType"
                                       && args[3].paramValue == (collectionType == NftCollectionType.Base ? "base-wearable" : "on-chain")));
            });

        [UnityTest]
        public IEnumerator ValidateCollectionIdParamsWhenRequestingOwnedWearablesWithThirdPartyFilters() =>
            UniTask.ToCoroutine(async () =>
            {
                var thirdPartyCollectionId = "testThirdPartyCollectionId";

                GivenWearableEntityWithSpecificLambdasUrl(GivenValidWearableEntity(VALID_WEARABLE_ID));

                (IReadOnlyList<WearableItem> wearables, int totalAmount) =
                    await service.RequestOwnedWearablesAsync(USER_ID, 0, 10, default(CancellationToken),
                        thirdPartyCollectionIds: new List<string> { thirdPartyCollectionId },
                        collectionTypeMask: NftCollectionType.ThirdParty);

                lambdasService.Received(1)
                              .GetFromSpecificUrl<WearableWithEntityResponseDto>(
                                   $"{EXPLORER_URL}/:userId/wearables",
                                   $"{EXPLORER_URL}/{USER_ID}/wearables",
                                   30, 3,
                                   Arg.Any<CancellationToken>(),
                                   Arg.Is<(string paramName, string paramValue)[]>(args =>
                                       args[0].paramName == "pageNum"
                                       && args[0].paramValue == "0"
                                       && args[1].paramName == "pageSize"
                                       && args[1].paramValue == "10"
                                       && args[2].paramName == "includeEntities"
                                       && args[2].paramValue == "true"
                                       && args[3].paramName == "collectionType"
                                       && args[3].paramValue == "third-party"
                                       && args[4].paramName == "thirdPartyCollectionId"
                                       && args[4].paramValue == thirdPartyCollectionId));
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

        private void GivenWearableDefinitionWithSpecificLambdasUrl(WearableItem wearable)
        {
            lambdasService.GetFromSpecificUrl<WearableWithDefinitionResponse>(
                               Arg.Any<string>(),
                               Arg.Any<string>(),
                               Arg.Any<int>(),
                               Arg.Any<int>(),
                               Arg.Any<CancellationToken>(),
                               Arg.Any<(string paramName, string paramValue)[]>())
                          .Returns(UniTask.FromResult<(WearableWithDefinitionResponse response, bool success)>(
                               (new WearableWithDefinitionResponse(new List<WearableElementV1Dto>
                               {
                                   new ()
                                   {
                                       definition = wearable
                                   },
                               }, 0, 10, 1), true)));
        }

        private void GivenWearableEntityWithSpecificLambdasUrl(EntityDto entity)
        {
            lambdasService.GetFromSpecificUrl<WearableWithEntityResponseDto>(
                               Arg.Any<string>(),
                               Arg.Any<string>(),
                               Arg.Any<int>(),
                               Arg.Any<int>(),
                               Arg.Any<CancellationToken>(),
                               Arg.Any<(string paramName, string paramValue)[]>())
                          .Returns(UniTask.FromResult<(WearableWithEntityResponseDto response, bool success)>(
                               (new WearableWithEntityResponseDto(new List<WearableWithEntityResponseDto.ElementDto>
                               {
                                   new ()
                                   {
                                       entity = entity,
                                   },
                               }, 0, 10, 1), true)));
        }

        private void GivenWearableInLambdas(WearableItem wearable)
        {
            lambdasService.PostFromSpecificUrl<EntityDto[], WearableRequest>(
                               Arg.Any<string>(),
                               Arg.Any<string>(),
                               Arg.Is<WearableRequest>(wr => wr.pointers.Contains(wearable.id)),
                               cancellationToken: Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult<(EntityDto[] response, bool success)>((new[] { WearableItemToEntityDto(wearable) }, true)));
        }

        private void GivenCollectionInLambdas(string collectionId, List<WearableItem> wearables)
        {
            var url = $"{CONTENT_URL}entities/active/collections/{collectionId}";
            var entities = wearables.Select(WearableItemToEntityDto).ToArray();

            lambdasService.GetFromSpecificUrl<WearableCollectionResponse>(url, url, cancellationToken: Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult<(WearableCollectionResponse response, bool success)>((new WearableCollectionResponse(entities: entities), true)));
        }

        private void GivenPaginatedCollectionInLambdas(string collectionId, List<WearableElementV1Dto> wearables)
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

        private void GivenPaginatedWearableInLambdas(List<WearableElementV1Dto> wearables)
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
                    representations = new[]
                    {
                        new WearableItem.Representation()
                        {
                            contents = new[]
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

        private EntityDto GivenValidWearableEntity(string id)
        {
            return new EntityDto
            {
                content = new ContentDto[]
                {
                    new ()
                    {
                        file = "thumbnail.png",
                        hash = "thumbnailhash",
                    },
                    new ()
                    {
                        file = "model.glb",
                        hash = "modelhash",
                    },
                },
                metadata = new MetadataDto
                {
                    thumbnail = "thumbnail.png",
                    id = id,
                    data = new MetadataDto.DataDto
                    {
                        representations = new MetadataDto.Representation[]
                        {
                            new ()
                            {
                                contents = new[]
                                {
                                    "model.glb",
                                },
                                mainFile = "model.glb",
                            },
                        },
                    },
                    rarity = "rare",
                    i18n = new i18n[]
                    {
                        new ()
                        {
                            code = "en",
                            text = id,
                        }
                    }
                },
            };
        }

        private static EntityDto WearableItemToEntityDto(WearableItem wearable)
        {
            return new EntityDto
            {
                id = wearable.id,
                content = new[]
                {
                    new ContentDto()
                    {
                        file = "representation/url/hash",
                        hash = "representation/url/hash",
                    },
                    new ContentDto()
                    {
                        file = wearable.thumbnail,
                        hash = wearable.thumbnail,
                    },
                },
                metadata = new MetadataDto()
                {
                    id = wearable.id,
                    rarity = wearable.rarity,
                    thumbnail = wearable.thumbnail,
                    description = wearable.description,
                    data = new MetadataDto.DataDto()
                    {
                        representations = new[]
                        {
                            new MetadataDto.Representation()
                            {
                                contents = new[] { "representation/url/hash" },
                            },
                        },
                    },
                },
            };
        }
    }
}
