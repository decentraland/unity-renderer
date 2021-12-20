using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace AvatarShape_Tests
{
    public class WearableItemsShould : IntegrationTestSuite_Legacy
    {
        private const string SUNGLASSES_ID = "dcl://base-avatars/black_sun_glasses";
        private const string BLUE_BANDANA_ID = "dcl://base-avatars/blue_bandana";

        private AvatarModel avatarModel;
        private BaseDictionary<string, WearableItem> catalog;
        private AvatarShape avatarShape;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            SetUp_SceneController();
            yield return SetUp_CharacterController();

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
                catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
                avatarShape = AvatarShapeTestHelpers.CreateAvatarShape(scene, avatarModel);

                yield return new DCL.WaitUntil(() => avatarShape.everythingIsLoaded, 20);
            }
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