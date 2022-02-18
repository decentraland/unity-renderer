﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Test.AvatarSystem
{
    public class WearableLoaderShould
    {
        private const string GLASSES_WEARABLE_ID = "urn:decentraland:matic:collections-v2:0x7c688630370a2900960f5ffd7573d2f66f179733:0";
        private const string HOODIE_ID = "urn:decentraland:off-chain:base-avatars:green_hoodie";

        private WearableLoader loader;
        private IWearableRetriever retriever;
        private GameObject container = null;
        private List<Material> materialsToBeDisposed = new List<Material>();

        [SetUp]
        public void SetUp()
        {
            container = new GameObject("Container");
            PrepareCatalog();
            retriever = Substitute.For<IWearableRetriever>();
        }

        private void PrepareCatalog()
        {
            //This is really, really ugly. There's no other way to solve it until the catalog is in our service locator
            container.AddComponent<CatalogController>();
            AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        }

        [UnityTest]
        public IEnumerator LoadWearable() => UniTask.ToCoroutine(async () =>
        {
            //Arrange
            loader = new WearableLoader(retriever, CatalogController.wearableCatalog[GLASSES_WEARABLE_ID]);

            var normalRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "ThisMaterialWontBeModified");
            var hairRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "hair");
            var skinRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "skin");
            Rendereable rendereable = new Rendereable
            {
                container = container,
                renderers = new HashSet<Renderer> { normalRenderer, hairRenderer, skinRenderer },
            };
            retriever.rendereable.Returns(rendereable);

            //Act
            await loader.Load(container, new AvatarSettings
            {
                bodyshapeId = WearableLiterals.BodyShapes.MALE,
                hairColor = Color.red,
                skinColor = Color.blue
            });

            //Assert
            Assert.AreEqual(IWearableLoader.Status.Succeeded, loader.status);
            Assert.AreEqual(Color.gray, normalRenderer.material.color);
            Assert.AreEqual(Color.red, hairRenderer.material.color);
            Assert.AreEqual(Color.blue, skinRenderer.material.color);
        });

        [UnityTest]
        public IEnumerator FallbackIfFailsWithRequiredCategory() => UniTask.ToCoroutine(async () =>
        {
            //Arrange
            WearableItem wearable = CatalogController.wearableCatalog[HOODIE_ID]; //Use a wearable with required category
            loader = new WearableLoader(retriever, wearable);

            WearableLoader.defaultWearablesResolver = Substitute.For<IWearableItemResolver>();
            WearableLoader.defaultWearablesResolver.Configure().Resolve(Arg.Any<string>()).Returns(new UniTask<WearableItem>(wearable));

            Renderer normalRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "ThisMaterialWontBeModified");
            Renderer hairRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "hair");
            Renderer skinRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "skin");
            Rendereable rendereable = new Rendereable
            {
                container = container,
                renderers = new HashSet<Renderer> { normalRenderer, hairRenderer, skinRenderer },
            };

            retriever.Configure()
                .Retrieve(Arg.Any<GameObject>(), Arg.Any<ContentProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(
                    x => // First call configures everything for null, mocking a failing wearable retrieval
                    {
                        retriever.rendereable.Returns(x => null);
                        return new UniTask<Rendereable>(null);
                    },
                    x => // Second call configures everything for the prepared rendereable, mocking a successfull fallback retrieval
                    {
                        retriever.rendereable.Returns(x => rendereable);
                        return new UniTask<Rendereable>(rendereable);
                    }
                );

            //Act
            await loader.Load(container, new AvatarSettings
            {
                bodyshapeId = WearableLiterals.BodyShapes.MALE,
                hairColor = Color.red,
                skinColor = Color.blue
            });

            //Assert
            Assert.AreEqual(IWearableLoader.Status.Defaulted, loader.status);
            retriever.Received(2).Retrieve(Arg.Any<GameObject>(), Arg.Any<ContentProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
            Assert.AreEqual(Color.gray, normalRenderer.material.color);
            Assert.AreEqual(Color.red, hairRenderer.material.color);
            Assert.AreEqual(Color.blue, skinRenderer.material.color);
        });

        [UnityTest]
        public IEnumerator FallbackIfThrowsWithRequiredCategory() => UniTask.ToCoroutine(async () =>
        {
            //Arrange
            WearableItem wearable = CatalogController.wearableCatalog[HOODIE_ID]; //Use a wearable with required category
            loader = new WearableLoader(retriever, wearable);

            WearableLoader.defaultWearablesResolver = Substitute.For<IWearableItemResolver>();
            WearableLoader.defaultWearablesResolver.Configure().Resolve(Arg.Any<string>()).Returns(new UniTask<WearableItem>(wearable));

            Renderer normalRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "ThisMaterialWontBeModified");
            Renderer hairRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "hair");
            Renderer skinRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "skin");
            Rendereable rendereable = new Rendereable
            {
                container = container,
                renderers = new HashSet<Renderer> { normalRenderer, hairRenderer, skinRenderer },
            };

            retriever.Configure()
                .Retrieve(Arg.Any<GameObject>(), Arg.Any<ContentProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(
                    x => // First call configures everything for null, mocking the wearable retrieval
                    {
                        throw new Exception();
                    },
                    x => // Second call configures everything for the prepared rendereable, mocking the fallback retrieval
                    {
                        retriever.rendereable.Returns(x => rendereable);
                        return new UniTask<Rendereable>(rendereable);
                    }
                );

            //Act
            await loader.Load(container, new AvatarSettings
            {
                bodyshapeId = WearableLiterals.BodyShapes.MALE,
                hairColor = Color.red,
                skinColor = Color.blue
            });

            //Assert
            Assert.AreEqual(IWearableLoader.Status.Defaulted, loader.status);
            retriever.Received(2).Retrieve(Arg.Any<GameObject>(), Arg.Any<ContentProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
            Assert.AreEqual(Color.gray, normalRenderer.material.color);
            Assert.AreEqual(Color.red, hairRenderer.material.color);
            Assert.AreEqual(Color.blue, skinRenderer.material.color);
        });

        [UnityTest]
        public IEnumerator NotFallbackIfFailsWithNoRequiredCategory() => UniTask.ToCoroutine(async () =>
        {
            //Arrange
            //Use a wearable with no required category
            loader = new WearableLoader(retriever, CatalogController.wearableCatalog[GLASSES_WEARABLE_ID]);

            retriever.rendereable.Returns(x => null);

            //Act
            await loader.Load(container, new AvatarSettings
            {
                bodyshapeId = WearableLiterals.BodyShapes.MALE,
                hairColor = Color.red,
                skinColor = Color.blue
            });

            //Assert
            retriever.Received(1).Retrieve(Arg.Any<GameObject>(), Arg.Any<ContentProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
            Assert.AreEqual(IWearableLoader.Status.Failed, loader.status);
        });

        [UnityTest]
        public IEnumerator NotFallbackIfThrowsWithNoRequiredCategory() => UniTask.ToCoroutine(async () =>
        {
            //Arrange
            //Use a wearable with no required category
            loader = new WearableLoader(retriever, CatalogController.wearableCatalog[GLASSES_WEARABLE_ID]);

            retriever.Configure()
                .Retrieve(Arg.Any<GameObject>(), Arg.Any<ContentProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(x => throw new Exception("Failing on purpose"));

            //Act
            await loader.Load(container, new AvatarSettings
            {
                bodyshapeId = WearableLiterals.BodyShapes.MALE,
                hairColor = Color.red,
                skinColor = Color.blue
            });

            //Assert
            retriever.Received(1).Retrieve(Arg.Any<GameObject>(), Arg.Any<ContentProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
            Assert.AreEqual(IWearableLoader.Status.Failed, loader.status);
        });

        [UnityTest]
        public IEnumerator CancelLoadIfACancelledTokenProvided() => UniTask.ToCoroutine(async () =>
        {
            //Arrange
            loader = new WearableLoader(retriever, CatalogController.wearableCatalog[GLASSES_WEARABLE_ID]);
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            retriever.ClearReceivedCalls();
            await TestUtils.ThrowsAsync<OperationCanceledException>(loader.Load(container, new AvatarSettings { bodyshapeId = WearableLiterals.BodyShapes.MALE }, cts.Token));
        });

        [UnityTest]
        public IEnumerator DisposeWhenCancellingOnRetrieving() => UniTask.ToCoroutine(async () =>
        {
            //Arrange
            loader = new WearableLoader(retriever, CatalogController.wearableCatalog[GLASSES_WEARABLE_ID]);
            CancellationTokenSource cts = new CancellationTokenSource();

            retriever.Configure()
                .Retrieve(Arg.Any<GameObject>(), Arg.Any<ContentProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(x => throw new OperationCanceledException());
            retriever.ClearReceivedCalls();

            await TestUtils.ThrowsAsync<OperationCanceledException>(loader.Load(container, new AvatarSettings { bodyshapeId = WearableLiterals.BodyShapes.MALE }, cts.Token));
            retriever.Received().Dispose();
        });

        private Renderer GetPrimitiveWithAvatarMaterial(Transform parent, string materialName)
        {
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            primitive.name = materialName;
            if (primitive.TryGetComponent(out Collider collider))
                Object.Destroy(collider);
            primitive.transform.parent = parent;

            Material material = new Material(Shader.Find("DCL/Universal Render Pipeline/Lit"))
            {
                name = materialName
            };
            materialsToBeDisposed.Add(material);

            Renderer renderer = primitive.GetComponent<Renderer>();
            renderer.material = material;
            return renderer;
        }

        [TearDown]
        public void TearDown()
        {
            loader?.Dispose();
            CatalogController.Clear();
            if (container != null)
                Object.Destroy(container);

            if (CatalogController.i != null)
                Object.Destroy(CatalogController.i);

            for (int i = 0; i < materialsToBeDisposed.Count; i++)
            {
                Material material = materialsToBeDisposed[i];
                if (material != null)
                    Object.Destroy(material);
            }
        }
    }
}