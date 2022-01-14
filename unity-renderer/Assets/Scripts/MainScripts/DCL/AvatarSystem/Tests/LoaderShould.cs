using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

        [UnityTest]
        public IEnumerator LoadCorrectly() => UniTask.ToCoroutine(async () =>
        {
            await loader.Load(
                CatalogController.wearableCatalog[FEMALE_ID],
                CatalogController.wearableCatalog[EYES_ID],
                CatalogController.wearableCatalog[EYEBROWS_ID],
                CatalogController.wearableCatalog[MOUTH_ID],
                IdsToWearables(WEARABLE_IDS),
                new AvatarSettings()
                {
                    bodyshapeId = WearableLiterals.BodyShapes.FEMALE,
                    eyesColor = Color.blue,
                    hairColor = Color.yellow,
                    skinColor = Color.green
                }
            );
            Assert.Fail(); //Finish this test!
        });

        private List<WearableItem> IdsToWearables(IEnumerable<string> wearablesIds) { return wearablesIds.Where(x => CatalogController.wearableCatalog.ContainsKey(x)).Select(x => CatalogController.wearableCatalog[x]).ToList(); }
        
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