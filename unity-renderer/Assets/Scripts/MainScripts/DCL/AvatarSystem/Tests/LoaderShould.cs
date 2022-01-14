using AvatarSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Test.AvatarSystem
{
    public class LoaderShould
    {
        private const string FEMALE_ID = "urn:decentraland:off-chain:base-avatars:BaseFemale";
        private const string EYES_ID = "urn:decentraland:off-chain:base-avatars:eyes_00";
        private const string EYEBROWS_ID = "urn:decentraland:off-chain:base-avatars:eyebrows_00";
        private const string MOUTH_ID = "urn:decentraland:off-chain:base-avatars:mouth_00";
        private const string WEARABLE_SHADER_PATH = "DCL/Universal Render Pipeline/Lit";

        private IWearableLoaderFactory wearableLoaderFactory;
        private Loader loader;
        private GameObject container;

        [SetUp]
        public void SetUp()
        {
            container = new GameObject("Container");
            PrepareCatalog();

            wearableLoaderFactory = Substitute.For<IWearableLoaderFactory>();
            loader = new Loader(wearableLoaderFactory, container);
        }

        private void PrepareCatalog()
        {
            //This is really, really ugly. There's no other way to solve it until the catalog is in our service locator
            container.AddComponent<CatalogController>();
            AvatarAssetsTestHelpers.CreateTestCatalogLocal();
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
        }
    }
}