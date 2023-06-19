using Cysharp.Threading.Tasks;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DCLServices.WearablesCatalogService
{
    public class WebInterfaceWearablesCatalogServiceShould
    {
        private const string USER_ID = "userId";

        private WebInterfaceWearablesCatalogService service;
        private GameObject gameObject;
        private WearablesWebInterfaceBridge bridge;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("WebInterfaceWearablesCatalogService");

            service = gameObject
               .AddComponent<WebInterfaceWearablesCatalogService>();

            bridge = Substitute.ForPartsOf<WearablesWebInterfaceBridge>();
            service.Initialize(bridge, new BaseDictionary<string, WearableItem>());
        }

        [TearDown]
        public void TearDown()
        {
            service.Dispose();
            Utils.SafeDestroy(gameObject);
        }

        [UnityTest]
        public IEnumerator RequestValidOwnedWearables() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearablesResponse($"OwnedWearables{USER_ID}", new[]
                {
                    GivenValidWearableItem("w1"),
                    GivenValidWearableItem("w2"),
                    GivenValidWearableItem("w3"),
                });

                (IReadOnlyList<WearableItem> wearables, int totalAmount) wearablesWithAmount = await service.RequestOwnedWearablesAsync(USER_ID, 0,
                    10, true, default(CancellationToken));

                ThenWearableIsValid("w1", wearablesWithAmount.wearables[0]);
                ThenWearableIsValid("w2", wearablesWithAmount.wearables[1]);
                ThenWearableIsValid("w3", wearablesWithAmount.wearables[2]);
                Assert.AreEqual(3, wearablesWithAmount.wearables.Count);
            });

        [UnityTest]
        public IEnumerator FailRequestWhenRequestingOwnedWearables() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearablesResponseError($"OwnedWearables{USER_ID}");

                try
                {
                    (IReadOnlyList<WearableItem> wearables, int totalAmount) wearables = await service.RequestOwnedWearablesAsync(USER_ID, 0,
                        10, true, default(CancellationToken));

                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Test error", e.Message);
                }
            });

        [UnityTest]
        public IEnumerator RequestValidBaseWearables() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearableCollectionResponse("BaseWearables",
                    "urn:decentraland:off-chain:base-avatars",
                    new[]
                    {
                        GivenValidWearableItem("w1"),
                        GivenValidWearableItem("w2"),
                        GivenValidWearableItem("w3"),
                    });

                await service.RequestBaseWearablesAsync(default(CancellationToken));

                ThenWearableIsValid("w1", service.WearablesCatalog["w1"]);
                ThenWearableIsValid("w2", service.WearablesCatalog["w2"]);
                ThenWearableIsValid("w3", service.WearablesCatalog["w3"]);
                Assert.AreEqual(3, service.WearablesCatalog.Count());
            });

        [UnityTest]
        public IEnumerator FailRequestWhenRequestingBaseWearables() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearableCollectionResponseError("BaseWearables", "urn:decentraland:off-chain:base-avatars");

                try
                {
                    await service.RequestBaseWearablesAsync(default(CancellationToken));

                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Test error", e.Message);
                }
            });

        [UnityTest]
        public IEnumerator RequestValidThirdPartyWearableCollection() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenThirdPartyWearableCollectionResponse("tpw-collection",
                    new[]
                    {
                        GivenValidWearableItem("w1"),
                        GivenValidWearableItem("w2"),
                        GivenValidWearableItem("w3"),
                    });

                (IReadOnlyList<WearableItem> wearables, int totalAmount) wearables = await service.RequestThirdPartyWearablesByCollectionAsync(USER_ID, "tpw-collection", 0, 20,
                    true, default(CancellationToken));

                ThenWearableIsValid("w1", wearables.wearables[0]);
                ThenWearableIsValid("w2", wearables.wearables[1]);
                ThenWearableIsValid("w3", wearables.wearables[2]);
                Assert.AreEqual(3, wearables.wearables.Count);
            });

        [UnityTest]
        public IEnumerator FailRequestWhenRequestingThirdPartyWearables() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenThirdPartyWearableCollectionResponseError("tpw-collection");

                try
                {
                    (IReadOnlyList<WearableItem> wearables, int totalAmount) wearables = await service.RequestThirdPartyWearablesByCollectionAsync(USER_ID, "tpw-collection", 0, 20,
                        true, default(CancellationToken));

                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Test error", e.Message);
                }
            });

        [UnityTest]
        public IEnumerator RequestValidWearableWhenIsNotInCache() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearablesByIdResponse("w1", GivenValidWearableItem("w1"));

                WearableItem wearable = await service.RequestWearableAsync("w1", default(CancellationToken));

                ThenWearableIsValid("w1", wearable);
            });

        [UnityTest]
        public IEnumerator FailRequestWhenRequestingWearablesById() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenWearablesByIdResponseError("w1");

                try
                {
                    WearableItem wearable = await service.RequestWearableAsync("w1", default(CancellationToken));

                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual("The request for the wearable 'w1' has failed: Test error", e.Message);
                }
            });

        [UnityTest]
        public IEnumerator RequestValidWearableWhenIsInCache() =>
            UniTask.ToCoroutine(async () =>
            {
                service.AddWearablesToCatalog(new[] { GivenValidWearableItem("w1") });

                WearableItem wearable = await service.RequestWearableAsync("w1", default(CancellationToken));

                ThenWearableIsValid("w1", wearable);
            });

        private void GivenThirdPartyWearableCollectionResponse(string collectionId, WearableItem[] wearables)
        {
            var context = $"ThirdPartyWearables_{collectionId}";

            bridge.When(b =>
                   {
                       b.RequestThirdPartyWearables(USER_ID, collectionId,
                           context);
                   })
                  .Do(info => service.AddWearablesToCatalog(JsonUtility.ToJson(new WearablesRequestResponse
                   {
                       wearables = wearables,
                       context = context,
                   })));
        }

        private void GivenThirdPartyWearableCollectionResponseError(string collectionId)
        {
            var context = $"ThirdPartyWearables_{collectionId}";

            bridge.When(b =>
                   {
                       b.RequestThirdPartyWearables(USER_ID, collectionId,
                           context);
                   })
                  .Do(info => service.WearablesRequestFailed(JsonUtility.ToJson(new WearablesRequestFailed
                   {
                       context = context,
                       error = "Test error",
                   })));
        }

        private void GivenWearableCollectionResponse(string context, string collectionId, WearableItem[] wearables)
        {
            bridge.When(b => b.RequestWearables(null, Arg.Any<string[]>(),
                       Arg.Is<string[]>(s => s[0] == collectionId && s.Length == 1),
                       context))
                  .Do(info => service.AddWearablesToCatalog(JsonUtility.ToJson(new WearablesRequestResponse
                   {
                       wearables = wearables,
                       context = context,
                   })));
        }

        private void GivenWearableCollectionResponseError(string context, string collectionId)
        {
            bridge.When(b => b.RequestWearables(null, Arg.Any<string[]>(),
                       Arg.Is<string[]>(s => s[0] == collectionId && s.Length == 1),
                       context))
                  .Do(info => service.WearablesRequestFailed(JsonUtility.ToJson(new WearablesRequestFailed
                   {
                       context = context,
                       error = "Test error",
                   })));
        }

        private void GivenWearablesResponse(string context, WearableItem[] wearables)
        {
            bridge.When(b => b.RequestWearables(USER_ID, null, null, context))
                  .Do(info => service.AddWearablesToCatalog(JsonUtility.ToJson(new WearablesRequestResponse
                   {
                       wearables = wearables,
                       context = context,
                   })));
        }

        private void GivenWearablesResponseError(string context)
        {
            bridge.When(b => b.RequestWearables(USER_ID, null, null, context))
                  .Do(info => service.WearablesRequestFailed(JsonUtility.ToJson(new WearablesRequestFailed
                   {
                       context = context,
                       error = "Test error",
                   })));
        }

        private void GivenWearablesByIdResponse(string wearableId, WearableItem wearable)
        {
            bridge.When(b => b.RequestWearables(null, Arg.Is<string[]>(s => s[0] == wearableId), Arg.Any<string[]>(), wearableId))
                  .Do(info => service.AddWearablesToCatalog(JsonUtility.ToJson(new WearablesRequestResponse
                   {
                       wearables = new[] { wearable },
                       context = wearableId,
                   })));
        }

        private void GivenWearablesByIdResponseError(string wearableId)
        {
            bridge.When(b => b.RequestWearables(null, Arg.Is<string[]>(s => s[0] == wearableId), Arg.Any<string[]>(), wearableId))
                  .Do(info => service.WearablesRequestFailed(JsonUtility.ToJson(new WearablesRequestFailed
                   {
                       context = wearableId,
                       error = "Test error",
                   })));
        }

        private void ThenWearableIsValid(string wearableId, WearableItem wearable)
        {
            Assert.AreEqual(wearableId, wearable.id);
            Assert.AreEqual("description", wearable.description);
            Assert.AreEqual("rare", wearable.rarity);
            Assert.AreEqual("representation/url/hash", wearable.data.representations[0].contents[0].url);
            Assert.AreEqual("hash", wearable.data.representations[0].contents[0].hash);
            Assert.AreEqual("key", wearable.data.representations[0].contents[0].key);
            Assert.AreEqual("shape", wearable.data.representations[0].bodyShapes[0]);
            Assert.AreEqual("head", wearable.data.representations[0].overrideHides[0]);
            Assert.AreEqual("lower_body", wearable.data.representations[0].overrideHides[1]);
            Assert.AreEqual("upper_body", wearable.data.representations[0].overrideReplaces[0]);
            Assert.AreEqual("baseurl/thumbnail", wearable.thumbnail);
            Assert.AreEqual("baseurl/", wearable.baseUrl);
            Assert.AreEqual("baseurl/bundles/", wearable.baseUrlBundles);
            Assert.AreEqual(wearable, service.WearablesCatalog[wearableId]);
        }

        private WearableItem GivenValidWearableItem(string id)
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
                                    url = "representation/url/hash",
                                    hash = "hash",
                                    key = "key"
                                }
                            },
                            mainFile = "file.gltf",
                            overrideHides = new[] { "head", "lower_body" },
                            bodyShapes = new[] { "shape" },
                            overrideReplaces = new[] { "upper_body" }
                        }
                    }
                },
                baseUrl = "baseurl/",
                thumbnail = "baseurl/thumbnail",
                description = "description",
                rarity = "rare",
                baseUrlBundles = "baseurl/bundles/",
            };
        }
    }
}
