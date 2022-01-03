using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;

namespace AvatarShape_Tests
{
    public class WearableItemsShould : IntegrationTestSuite_Legacy
    {
        private const string SUNGLASSES_ID = "dcl://base-avatars/black_sun_glasses";
        private const string BLUE_BANDANA_ID = "dcl://base-avatars/blue_bandana";

        private CatalogController catalogController;
        private AvatarModel avatarModel;
        private BaseDictionary<string, WearableItem> catalog;
        private AvatarShape avatarShape;
        private ParcelScene scene;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene();

            if (avatarShape == null)
            {
                avatarModel = new AvatarModel()
                {
                    name = " test",
                    hairColor = Color.white,
                    eyeColor = Color.white,
                    skinColor = Color.white,
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>()
                        { }
                };

                catalogController = TestUtils.CreateComponentWithGameObject<CatalogController>("CatalogController");
                catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
                avatarShape = AvatarShapeTestHelpers.CreateAvatarShape(scene, avatarModel);

                yield return new DCL.WaitUntil(() => avatarShape.everythingIsLoaded, 20);
            }
        }

        protected override IEnumerator TearDown()
        {
            Object.Destroy(catalogController.gameObject);
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator BeVisibleByDefault()
        {
            yield return null;
            Assert.Fail();
            //Remember to delete this
        }
    }
}