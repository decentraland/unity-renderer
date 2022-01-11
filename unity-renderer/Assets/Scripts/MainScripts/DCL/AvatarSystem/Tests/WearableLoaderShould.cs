using System.Collections;
using System.Collections.Generic;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Test.AvatarSystem
{
    public class WearableLoaderShould
    {
        private const string GLASSES_WEARABLE_ID = "urn:decentraland:matic:collections-v2:0x7c688630370a2900960f5ffd7573d2f66f179733:0";

        private WearableLoader loader;
        private IWearableRetriever retriever;
        private WearableItem wearable;
        private GameObject container = null;
        private List<Material> materialsToBeDisposed = new List<Material>();

        [SetUp]
        public void SetUp()
        {
            container = new GameObject("Container");
            PrepareCatalog();
            wearable = CatalogController.wearableCatalog[GLASSES_WEARABLE_ID];
            retriever = Substitute.For<IWearableRetriever>();
            loader = new WearableLoader(retriever, wearable);
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

            var normalRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "ThisMaterialWontBeModified");
            var hairRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "hair");
            var skinRenderer = GetPrimitiveWithAvatarMaterial(container.transform, "skin");
            Rendereable rendereable = new Rendereable
            {
                container = container,
                renderers = new List<Renderer> { normalRenderer, hairRenderer, skinRenderer },
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
            Assert.AreEqual(Color.gray, normalRenderer.material.color);
            Assert.AreEqual(Color.red, hairRenderer.material.color);
            Assert.AreEqual(Color.blue, skinRenderer.material.color);
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

            for (var i = 0; i < materialsToBeDisposed.Count; i++)
            {
                Material material = materialsToBeDisposed[i];
                if (material != null)
                    Object.Destroy(material);
            }
        }
    }
}