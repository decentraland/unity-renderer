using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Test.AvatarSystem
{
    public class LoaderShould
    {
        private const string FEMALE_ID = "urn:decentraland:off-chain:base-avatars:BaseFemale";
        private const string EYES_ID = "urn:decentraland:off-chain:base-avatars:f_eyes_00";
        private const string EYEBROWS_ID = "urn:decentraland:off-chain:base-avatars:f_eyebrows_00";
        private const string MOUTH_ID = "urn:decentraland:off-chain:base-avatars:f_mouth_00";
        private static readonly string[] WEARABLE_IDS = new []
        {
            "urn:decentraland:off-chain:base-avatars:black_sun_glasses",
            "urn:decentraland:off-chain:base-avatars:bear_slippers",
            "urn:decentraland:off-chain:base-avatars:hair_anime_01",
            "urn:decentraland:off-chain:base-avatars:f_african_leggins",
            "urn:decentraland:off-chain:base-avatars:blue_bandana",
            "urn:decentraland:off-chain:base-avatars:bee_t_shirt"
        };

        private IWearableLoaderFactory wearableLoaderFactory;
        private Loader loader;
        private GameObject container;
        private IAvatarMeshCombinerHelper meshCombiner;
        private IWearablesCatalogService wearablesCatalogService;

        [SetUp]
        public void SetUp()
        {
            container = new GameObject("Container");
            wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();

            meshCombiner = Substitute.For<IAvatarMeshCombinerHelper>();
            wearableLoaderFactory = Substitute.For<IWearableLoaderFactory>();
            loader = new Loader(wearableLoaderFactory, container, meshCombiner);
        }

        [Test]
        public void GetNewLoadersWhenThereWerentCurrentLoaders()
        {
            //Arrange
            List<WearableItem> items = GetStandardWearableList();
            Dictionary<string, IWearableLoader> currentLoaders = new Dictionary<string, IWearableLoader>();
            IWearableLoaderFactory loaderFactory = Substitute.For<IWearableLoaderFactory>();
            loaderFactory.Configure()
                         .GetWearableLoader(Arg.Any<WearableItem>())
                         .Returns(x => GetMockedWearableLoader(x.ArgAt<WearableItem>(0)));

            //Act
            (List<IWearableLoader> notReusableLoaders, List<IWearableLoader> newLoaders) = Loader.GetNewLoaders(items, currentLoaders, loaderFactory);

            //Assert
            Assert.AreEqual(0, notReusableLoaders.Count);
            Assert.AreEqual(4, newLoaders.Count);
            Assert.AreEqual(items.Count, newLoaders.Count);
            for (int i = 0; i < newLoaders.Count; i++)
            {
                Assert.AreEqual(items[i], newLoaders[i].wearable);
            }
        }

        [Test]
        public void GetNewLoadersWhenThereWasReusableLoaders()
        {
            //Arrange
            List<WearableItem> items = GetStandardWearableList();
            Dictionary<string, IWearableLoader> currentLoaders = new Dictionary<string, IWearableLoader>()
            {
                { items[0].data.category, GetMockedWearableLoader(items[0]) },
                { items[1].data.category, GetMockedWearableLoader(items[1]) }
            };
            IWearableLoaderFactory loaderFactory = Substitute.For<IWearableLoaderFactory>();
            loaderFactory.Configure()
                         .GetWearableLoader(Arg.Any<WearableItem>())
                         .Returns(x => GetMockedWearableLoader(x.ArgAt<WearableItem>(0)));

            //Act
            (List<IWearableLoader> notReusableLoaders, List<IWearableLoader> newLoaders) = Loader.GetNewLoaders(items, currentLoaders, loaderFactory);

            //Assert
            Assert.AreEqual(0, notReusableLoaders.Count);
            Assert.AreEqual(4, newLoaders.Count);
            Assert.IsTrue(currentLoaders.ContainsValue(newLoaders[0]));
            Assert.IsTrue(currentLoaders.ContainsValue(newLoaders[1]));
            Assert.AreEqual(items[2], newLoaders[2].wearable);
            Assert.AreEqual(items[3], newLoaders[3].wearable);
        }

        [Test]
        public void GetNewLoadersAndNotReusableLoaderWhenThereWasAnObsoleteLoader()
        {
            //Arrange
            List<WearableItem> items = GetStandardWearableList();
            Dictionary<string, IWearableLoader> currentLoaders = new Dictionary<string, IWearableLoader>()
            {
                { WearableLiterals.Categories.HAIR, GetMockedWearableLoader(new WearableItem { id = "NotReusable", data = new WearableItem.Data { category = WearableLiterals.Categories.HAIR } }) },
                { items[1].data.category, GetMockedWearableLoader(items[1]) }
            };
            IWearableLoaderFactory loaderFactory = Substitute.For<IWearableLoaderFactory>();
            loaderFactory.Configure()
                         .GetWearableLoader(Arg.Any<WearableItem>())
                         .Returns(x => GetMockedWearableLoader(x.ArgAt<WearableItem>(0)));

            //Act
            (List<IWearableLoader> notReusableLoaders, List<IWearableLoader> newLoaders) = Loader.GetNewLoaders(items, currentLoaders, loaderFactory);

            //Assert
            Assert.AreEqual(1, notReusableLoaders.Count);
            Assert.AreEqual(4, newLoaders.Count);
            Assert.IsFalse(currentLoaders.ContainsValue(newLoaders[0]));
            Assert.IsTrue(currentLoaders.ContainsValue(newLoaders[1]));
            Assert.AreEqual(items[2], newLoaders[2].wearable);
            Assert.AreEqual(items[3], newLoaders[3].wearable);
            Assert.AreEqual(currentLoaders[WearableLiterals.Categories.HAIR], notReusableLoaders[0]);
        }

        [Test]
        public void ComposeStatusWithAllLoadersSucceded()
        {
            //Arrange
            WearableItem wearable0 = new WearableItem { id = "Item0", data = new WearableItem.Data { category = WearableLiterals.Categories.UPPER_BODY } };
            WearableItem wearable1 = new WearableItem { id = "Item1", data = new WearableItem.Data { category = WearableLiterals.Categories.LOWER_BODY } };
            WearableItem wearable2 = new WearableItem { id = "Item2", data = new WearableItem.Data { category = WearableLiterals.Categories.FEET } };
            Dictionary<string, IWearableLoader> loaders = new Dictionary<string, IWearableLoader>()
            {
                { wearable0.data.category, GetMockedWearableLoader(wearable0, IWearableLoader.Status.Succeeded) },
                { wearable1.data.category, GetMockedWearableLoader(wearable1, IWearableLoader.Status.Succeeded) },
                { wearable2.data.category, GetMockedWearableLoader(wearable2, IWearableLoader.Status.Succeeded) },
            };

            //Act
            ILoader.Status result = Loader.ComposeStatus(loaders);

            //Assert
            Assert.AreEqual(ILoader.Status.Succeeded, result);
        }

        [Test]
        public void ComposeStatusWithNonRequiredLoadersFailed()
        {
            //Arrange
            WearableItem wearable0 = new WearableItem { id = "Item0", data = new WearableItem.Data { category = WearableLiterals.Categories.UPPER_BODY } };
            WearableItem wearable1 = new WearableItem { id = "Item1", data = new WearableItem.Data { category = WearableLiterals.Categories.LOWER_BODY } };
            WearableItem wearable2 = new WearableItem { id = "Item2", data = new WearableItem.Data { category = WearableLiterals.Categories.FEET } };
            Dictionary<string, IWearableLoader> loaders = new Dictionary<string, IWearableLoader>()
            {
                { wearable0.data.category, GetMockedWearableLoader(wearable0, IWearableLoader.Status.Failed) },
                { wearable1.data.category, GetMockedWearableLoader(wearable1, IWearableLoader.Status.Succeeded) },
                { wearable2.data.category, GetMockedWearableLoader(wearable2, IWearableLoader.Status.Succeeded) },
            };

            //Act
            ILoader.Status result = Loader.ComposeStatus(loaders);

            //Assert
            Assert.AreEqual(ILoader.Status.Failed_Minor, result);
        }

        [Test]
        public void ComposeStatusWithRequiredLoadersFailed()
        {
            //Arrange
            WearableItem wearable0 = new WearableItem { id = "Item0", data = new WearableItem.Data { category = WearableLiterals.Categories.EYES } };
            WearableItem wearable1 = new WearableItem { id = "Item1", data = new WearableItem.Data { category = WearableLiterals.Categories.EYEBROWS } };
            WearableItem wearable2 = new WearableItem { id = "Item2", data = new WearableItem.Data { category = WearableLiterals.Categories.MOUTH } };
            Dictionary<string, IWearableLoader> loaders = new Dictionary<string, IWearableLoader>()
            {
                { wearable0.data.category, GetMockedWearableLoader(wearable0, IWearableLoader.Status.Failed) },
                { wearable1.data.category, GetMockedWearableLoader(wearable1, IWearableLoader.Status.Succeeded) },
                { wearable2.data.category, GetMockedWearableLoader(wearable2, IWearableLoader.Status.Succeeded) },
            };

            //Act
            ILoader.Status result = Loader.ComposeStatus(loaders);

            //Assert
            Assert.AreEqual(ILoader.Status.Failed_Major, result);
        }

        [UnityTest]
        public IEnumerator LoadCorrectly() => UniTask.ToCoroutine(async () =>
        {
            MockBodyShapeLoader(IWearableLoader.Status.Succeeded);
            MockWearableLoaderFactory();
            MockCombinesMesh();

            SkinnedMeshRenderer bonesContainerReceivedByCombiner = null;
            SkinnedMeshRenderer[] renderersReceivedByCombiner = null;
            meshCombiner.Combine(
                            Arg.Do<SkinnedMeshRenderer>(x => bonesContainerReceivedByCombiner = x),
                            Arg.Do<SkinnedMeshRenderer[]>(x => renderersReceivedByCombiner = x))
                        .Returns(true);

            await loader.Load(
                wearablesCatalogService.WearablesCatalog[FEMALE_ID],
                wearablesCatalogService.WearablesCatalog[EYES_ID],
                wearablesCatalogService.WearablesCatalog[EYEBROWS_ID],
                wearablesCatalogService.WearablesCatalog[MOUTH_ID],
                IdsToWearables(WEARABLE_IDS),
                new AvatarSettings()
                {
                    bodyshapeId = WearableLiterals.BodyShapes.FEMALE,
                    eyesColor = Color.blue,
                    hairColor = Color.yellow,
                    skinColor = Color.green
                },
                null
            );

            Assert.AreEqual(meshCombiner.renderer, loader.combinedRenderer);
            Assert.AreEqual(ILoader.Status.Succeeded, loader.status);

            List<SkinnedMeshRenderer> allRenderers = new List<SkinnedMeshRenderer>()
            {
                loader.bodyshapeLoader.headRenderer, //The rest will be hidden by the wearables
            };
            allRenderers.AddRange(loader.loaders.Values.SelectMany(x => x.rendereable.renderers.OfType<SkinnedMeshRenderer>()));
            //Assert the mesh combiner received the proper data
            Assert.AreEqual(allRenderers.Count, renderersReceivedByCombiner.Length);
            for (var i = 0; i < allRenderers.Count; i++)
            {
                Assert.IsTrue(renderersReceivedByCombiner.Contains(allRenderers[i]));
            }
        });

        [UnityTest]
        public IEnumerator UseBonesContainerProvided() => UniTask.ToCoroutine(async () =>
        {
            MockBodyShapeLoader(IWearableLoader.Status.Succeeded);
            MockWearableLoaderFactory();
            MockCombinesMesh();
            SkinnedMeshRenderer bonesContainer = CreatePrimitive(container.transform).GetComponent<SkinnedMeshRenderer>();

            await loader.Load(
                wearablesCatalogService.WearablesCatalog[FEMALE_ID],
                wearablesCatalogService.WearablesCatalog[EYES_ID],
                wearablesCatalogService.WearablesCatalog[EYEBROWS_ID],
                wearablesCatalogService.WearablesCatalog[MOUTH_ID],
                IdsToWearables(WEARABLE_IDS),
                new AvatarSettings()
                {
                    bodyshapeId = WearableLiterals.BodyShapes.FEMALE,
                    eyesColor = Color.blue,
                    hairColor = Color.yellow,
                    skinColor = Color.green
                },
                bonesContainer
            );

            foreach (KeyValuePair<string, IWearableLoader> load in loader.loaders)
            {
                foreach (SkinnedMeshRenderer skinned in load.Value.rendereable.meshes.OfType<SkinnedMeshRenderer>())
                {
                    Assert.AreEqual(bonesContainer.rootBone, skinned.rootBone);
                    for (int i = 0; i < skinned.bones.Length; i++)
                    {
                        Assert.AreEqual(bonesContainer.bones[i], skinned.bones[i]);
                    }
                }
            }

        });

        [UnityTest]
        public IEnumerator UseUpperBodyContainer() => UniTask.ToCoroutine(async () =>
        {
            MockBodyShapeLoader(IWearableLoader.Status.Succeeded);
            MockWearableLoaderFactory();
            MockCombinesMesh();

            await loader.Load(
                wearablesCatalogService.WearablesCatalog[FEMALE_ID],
                wearablesCatalogService.WearablesCatalog[EYES_ID],
                wearablesCatalogService.WearablesCatalog[EYEBROWS_ID],
                wearablesCatalogService.WearablesCatalog[MOUTH_ID],
                IdsToWearables(WEARABLE_IDS),
                new AvatarSettings()
                {
                    bodyshapeId = WearableLiterals.BodyShapes.FEMALE,
                    eyesColor = Color.blue,
                    hairColor = Color.yellow,
                    skinColor = Color.green
                },
                null
            );

            foreach (KeyValuePair<string, IWearableLoader> load in loader.loaders)
            {
                foreach (SkinnedMeshRenderer skinned in load.Value.rendereable.meshes.OfType<SkinnedMeshRenderer>())
                {
                    Assert.AreEqual(loader.bodyshapeLoader.upperBodyRenderer.rootBone, skinned.rootBone);
                    for (int i = 0; i < skinned.bones.Length; i++)
                    {
                        Assert.AreEqual(loader.bodyshapeLoader.upperBodyRenderer.bones[i], skinned.bones[i]);
                    }
                }
            }

        });

        [UnityTest]
        public IEnumerator DisablesFacialWhenHeadIsHidden() => UniTask.ToCoroutine(async () =>
        {
            WearableItem wearableItem = wearablesCatalogService.WearablesCatalog[WEARABLE_IDS[0]];
            wearableItem.data.hides = new string[] { WearableLiterals.Misc.HEAD };
            var bodyShapeLoader = Substitute.For<IBodyshapeLoader>();
            wearableLoaderFactory.Configure()
                                 .GetBodyshapeLoader(Arg.Any<WearableItem>(), Arg.Any<WearableItem>(), Arg.Any<WearableItem>(), Arg.Any<WearableItem>())
                                 .Returns(x => bodyShapeLoader);

            wearableLoaderFactory.Configure()
                                 .GetWearableLoader(Arg.Any<WearableItem>())
                                 .Returns(x => GetMockedWearableLoaderWithPrimitive(wearableItem, container));

            MockCombinesMesh();

            await loader.Load(
                wearablesCatalogService.WearablesCatalog[FEMALE_ID],
                wearablesCatalogService.WearablesCatalog[EYES_ID],
                wearablesCatalogService.WearablesCatalog[EYEBROWS_ID],
                wearablesCatalogService.WearablesCatalog[MOUTH_ID],
                new List<WearableItem>() { wearableItem },
                new AvatarSettings()
                {
                    bodyshapeId = WearableLiterals.BodyShapes.FEMALE,
                    eyesColor = Color.blue,
                    hairColor = Color.yellow,
                    skinColor = Color.green
                },
                meshCombiner.renderer
            );
            bodyShapeLoader.Received().DisableFacialRenderers();
        });

        [UnityTest]
        public IEnumerator ThrowWithFailedBodyshape() => UniTask.ToCoroutine(async () =>
        {
            MockBodyShapeLoader(IWearableLoader.Status.Failed);

            TestUtils.ThrowsAsync<Exception>(
                loader.Load(
                    wearablesCatalogService.WearablesCatalog[FEMALE_ID],
                    wearablesCatalogService.WearablesCatalog[EYES_ID],
                    wearablesCatalogService.WearablesCatalog[EYEBROWS_ID],
                    wearablesCatalogService.WearablesCatalog[MOUTH_ID],
                    IdsToWearables(WEARABLE_IDS),
                    new AvatarSettings()
                    {
                        bodyshapeId = WearableLiterals.BodyShapes.FEMALE,
                        eyesColor = Color.blue,
                        hairColor = Color.yellow,
                        skinColor = Color.green
                    },
                    null
                ));
        });

        [UnityTest]
        public IEnumerator LoadCorrectlyWithFailedNotRequiredWearables() => UniTask.ToCoroutine(async () =>
        {
            MockBodyShapeLoader(IWearableLoader.Status.Succeeded);

            wearableLoaderFactory.Configure()
                                 .GetWearableLoader(Arg.Any<WearableItem>())
                                 .Returns(x => GetMockedWearableLoaderWithPrimitive(
                                     x.ArgAt<WearableItem>(0),
                                     container,
                                     WearableLiterals.Categories.REQUIRED_CATEGORIES.Contains(x.ArgAt<WearableItem>(0).data.category) ? IWearableLoader.Status.Succeeded : IWearableLoader.Status.Failed
                                 ));

            MockCombinesMesh();

            SkinnedMeshRenderer bonesContainerReceivedByCombiner = null;
            SkinnedMeshRenderer[] renderersReceivedByCombiner = null;
            meshCombiner.Combine(
                            Arg.Do<SkinnedMeshRenderer>(x => bonesContainerReceivedByCombiner = x),
                            Arg.Do<SkinnedMeshRenderer[]>(x => renderersReceivedByCombiner = x))
                        .Returns(true);

            await loader.Load(
                wearablesCatalogService.WearablesCatalog[FEMALE_ID],
                wearablesCatalogService.WearablesCatalog[EYES_ID],
                wearablesCatalogService.WearablesCatalog[EYEBROWS_ID],
                wearablesCatalogService.WearablesCatalog[MOUTH_ID],
                IdsToWearables(WEARABLE_IDS),
                new AvatarSettings()
                {
                    bodyshapeId = WearableLiterals.BodyShapes.FEMALE,
                    eyesColor = Color.blue,
                    hairColor = Color.yellow,
                    skinColor = Color.green
                },
                null
            );

            Assert.AreEqual(meshCombiner.renderer, loader.combinedRenderer);
            Assert.AreEqual(ILoader.Status.Failed_Minor, loader.status);

            List<SkinnedMeshRenderer> allRenderers = new List<SkinnedMeshRenderer>()
            {
                loader.bodyshapeLoader.headRenderer, //The rest will be hidden by the wearables
            };
            allRenderers.AddRange(loader.loaders.Values.SelectMany(x => x.rendereable.renderers.OfType<SkinnedMeshRenderer>()));
            //Assert the mesh combiner received the proper data
            Assert.AreEqual(allRenderers.Count, renderersReceivedByCombiner.Length);
            for (var i = 0; i < allRenderers.Count; i++)
            {
                Assert.IsTrue(renderersReceivedByCombiner.Contains(allRenderers[i]));
            }
        });

        [UnityTest]
        public IEnumerator ThrowIfFailedRequiredWearable() => UniTask.ToCoroutine(async () =>
        {
            MockBodyShapeLoader(IWearableLoader.Status.Succeeded);

            wearableLoaderFactory.Configure()
                                 .GetWearableLoader(Arg.Any<WearableItem>())
                                 .Returns(x => GetMockedWearableLoaderWithPrimitive(
                                     x.ArgAt<WearableItem>(0),
                                     container,
                                     WearableLiterals.Categories.REQUIRED_CATEGORIES.Contains(x.ArgAt<WearableItem>(0).data.category) ? IWearableLoader.Status.Failed : IWearableLoader.Status.Succeeded
                                 ));

            TestUtils.ThrowsAsync<Exception>(
                loader.Load(
                    wearablesCatalogService.WearablesCatalog[FEMALE_ID],
                    wearablesCatalogService.WearablesCatalog[EYES_ID],
                    wearablesCatalogService.WearablesCatalog[EYEBROWS_ID],
                    wearablesCatalogService.WearablesCatalog[MOUTH_ID],
                    IdsToWearables(WEARABLE_IDS),
                    new AvatarSettings()
                    {
                        bodyshapeId = WearableLiterals.BodyShapes.FEMALE,
                        eyesColor = Color.blue,
                        hairColor = Color.yellow,
                        skinColor = Color.green
                    },
                    null
                ));
        });

        private List<WearableItem> GetStandardWearableList()
        {
            return new List<WearableItem>()
            {
                new WearableItem() { id = "Item0", data = new WearableItem.Data { category = WearableLiterals.Categories.FEET } },
                new WearableItem() { id = "Item1", data = new WearableItem.Data { category = WearableLiterals.Categories.UPPER_BODY } },
                new WearableItem() { id = "Item2", data = new WearableItem.Data { category = WearableLiterals.Categories.LOWER_BODY } },
                new WearableItem() { id = "Item3", data = new WearableItem.Data { category = WearableLiterals.Categories.HAIR } },
            };
        }

        private void MockCombinesMesh()
        {
            var combined = CreatePrimitive(container.transform, "CombinedRenderer");
            meshCombiner.container.Returns(combined);
            meshCombiner.renderer.Returns(combined.GetComponent<SkinnedMeshRenderer>());
            meshCombiner.Configure().Combine(Arg.Any<SkinnedMeshRenderer>(), Arg.Any<SkinnedMeshRenderer[]>(), Arg.Any<Material>()).Returns(true);
            meshCombiner.Configure().Combine(Arg.Any<SkinnedMeshRenderer>(), Arg.Any<SkinnedMeshRenderer[]>()).Returns(true);
        }

        private void MockBodyShapeLoader(IWearableLoader.Status status)
        {
            wearableLoaderFactory.Configure()
                                     .GetBodyshapeLoader(Arg.Any<WearableItem>(), Arg.Any<WearableItem>(), Arg.Any<WearableItem>(), Arg.Any<WearableItem>())
                                     .Returns(x => GetMockedBodyshapeLoaderWithPrimitives(
                                         x.ArgAt<WearableItem>(0),
                                         x.ArgAt<WearableItem>(1),
                                         x.ArgAt<WearableItem>(2),
                                         x.ArgAt<WearableItem>(3),
                                         container,
                                         status
                                     ));
        }

        private void MockWearableLoaderFactory()
        {
            wearableLoaderFactory.Configure()
                                     .GetWearableLoader(Arg.Any<WearableItem>())
                                     .Returns(x => GetMockedWearableLoaderWithPrimitive(
                                         x.ArgAt<WearableItem>(0),
                                         container,
                                         IWearableLoader.Status.Succeeded
                                     ));
        }

        private IWearableLoader GetMockedWearableLoader(WearableItem wearable, IWearableLoader.Status status = IWearableLoader.Status.Succeeded)
        {
            IWearableLoader loader = Substitute.For<IWearableLoader>();
            loader.wearable.Returns(wearable);
            loader.status.Returns(status);
            return loader;
        }

        private IWearableLoader GetMockedWearableLoaderWithPrimitive(WearableItem wearable, GameObject container, IWearableLoader.Status status = IWearableLoader.Status.Succeeded)
        {
            IWearableLoader loader = Substitute.For<IWearableLoader>();
            loader.wearable.Returns(wearable);
            loader.status.Returns(status);
            Rendereable rendereable = Rendereable.CreateFromGameObject(CreatePrimitive(container.transform, wearable.id));
            loader.rendereable.Returns(rendereable);
            return loader;
        }

        private IBodyshapeLoader GetMockedBodyshapeLoaderWithPrimitives(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, GameObject container, IWearableLoader.Status status = IWearableLoader.Status.Succeeded)
        {
            var bodyshapeHolder = new GameObject("bodyshape");
            bodyshapeHolder.transform.SetParent(container.transform);

            var headPrimitive = CreatePrimitive(bodyshapeHolder.transform, "head");
            var ubodyPrimitive = CreatePrimitive(bodyshapeHolder.transform, "ubody");
            var lbodyPrimitive = CreatePrimitive(bodyshapeHolder.transform, "lbody");
            var feetPrimitive = CreatePrimitive(bodyshapeHolder.transform, "feet");
            var eyesPrimitive = CreatePrimitive(bodyshapeHolder.transform, "eyes");
            var eyebrowsPrimitive = CreatePrimitive(bodyshapeHolder.transform, "eyebrows");
            var mouthPrimitive = CreatePrimitive(bodyshapeHolder.transform, "mouth");

            IBodyshapeLoader loader = Substitute.For<IBodyshapeLoader>();
            loader.wearable.Returns(bodyshape);
            loader.eyes.Returns(eyes);
            loader.eyebrows.Returns(eyebrows);
            loader.mouth.Returns(mouth);

            loader.headRenderer.Returns(headPrimitive.GetComponent<SkinnedMeshRenderer>());
            loader.upperBodyRenderer.Returns(ubodyPrimitive.GetComponent<SkinnedMeshRenderer>());
            loader.lowerBodyRenderer.Returns(lbodyPrimitive.GetComponent<SkinnedMeshRenderer>());
            loader.feetRenderer.Returns(feetPrimitive.GetComponent<SkinnedMeshRenderer>());
            loader.eyesRenderer.Returns(eyesPrimitive.GetComponent<SkinnedMeshRenderer>());
            loader.eyebrowsRenderer.Returns(eyebrowsPrimitive.GetComponent<SkinnedMeshRenderer>());
            loader.mouthRenderer.Returns(mouthPrimitive.GetComponent<SkinnedMeshRenderer>());

            loader.rendereable.Returns(Rendereable.CreateFromGameObject(bodyshapeHolder));
            loader.status.Returns(status);

            return loader;
        }

        private List<WearableItem> IdsToWearables(IEnumerable<string> wearablesIds) { return wearablesIds.Where(x => wearablesCatalogService.WearablesCatalog.ContainsKey(x)).Select(x => wearablesCatalogService.WearablesCatalog[x]).ToList(); }

        private GameObject CreatePrimitive(Transform parent, string gameObjectName = "Name")
        {
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (primitive.TryGetComponent(out Collider collider))
                Object.Destroy(collider);
            primitive.transform.parent = parent;
            primitive.name = gameObjectName;

            Renderer renderer = primitive.GetComponent<Renderer>();
            SkinnedMeshRenderer skr = primitive.AddComponent<SkinnedMeshRenderer>();
            AddMockBones(skr);
            skr.sharedMesh = primitive.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(renderer);

            return primitive;
        }

        private void AddMockBones(SkinnedMeshRenderer renderer)
        {
            GameObject hips = new GameObject("hips");
            GameObject head = new GameObject("head");
            GameObject neck = new GameObject("neck");

            neck.transform.parent = head.transform;
            head.transform.parent = hips.transform;
            hips.transform.parent = renderer.transform;
            renderer.rootBone = hips.transform;
            renderer.bones = new Transform[] { hips.transform, head.transform, neck.transform};
        }

        [TearDown]
        public void TearDown()
        {
            loader?.Dispose();
            if (container != null)
                Object.Destroy(container);

            wearablesCatalogService.Dispose();
        }
    }
}
