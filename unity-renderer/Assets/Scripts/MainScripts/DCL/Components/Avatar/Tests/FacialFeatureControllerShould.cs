using System.Collections;
using DCL;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = DCL.WaitUntil;

namespace AvatarShape_Tests
{
    public class FacialFeatureControllerShould : IntegrationTestSuite_Legacy
    {
        private const string EYES_ID = "urn:decentraland:off-chain:base-avatars:f_eyes_01";
        private const string DRACULA_MOUTH_ID = "urn:decentraland:off-chain:base-avatars:dracula_mouth";
        private BaseDictionary<string, WearableItem> catalog;
        private IBodyShapeController bodyShapeController;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            bodyShapeController = Substitute.For<IBodyShapeController>();
            bodyShapeController.bodyShapeId.Returns(WearableLiterals.BodyShapes.FEMALE);

            catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        }

        [UnityTest]
        public IEnumerator LoadProperly()
        {
            //Arrange
            catalog.TryGetValue(EYES_ID, out WearableItem wereableItem);
            FacialFeatureController controller = new FacialFeatureController(wereableItem, new Material(Shader.Find("DCL/Toon Shader")));

            //Act
            controller.Load(bodyShapeController, Color.red);
            yield return new WaitUntil(() => controller.isReady);

            //Assert
            Assert.NotNull(controller.mainTexture);
            Assert.NotNull(controller.maskTexture);
        }

        [UnityTest]
        public IEnumerator FailsGracefully_BadURL()
        {
            //Arrange
            WearableItem fakeWearable = new WearableItem
            {
                baseUrl = "http://nothing_here.nope",
                data = new WearableItem.Data()
                {
                    category = WearableLiterals.Categories.EYES,
                    representations = new []
                    {
                        new WearableItem.Representation()
                        {
                            bodyShapes = new [] { WearableLiterals.BodyShapes.FEMALE },
                            contents = new []
                            {
                                new WearableItem.MappingPair { key = "fake.png", hash = "nope" },
                                new WearableItem.MappingPair { key = "fake_mask.png", hash = "nope2" }
                            },
                        }
                    }
                }
            };
            FacialFeatureController controller = new FacialFeatureController(fakeWearable, new Material(Shader.Find("DCL/Toon Shader")));

            //Act
            controller.Load(bodyShapeController, Color.red);
            yield return new WaitUntil(() => controller.isReady);

            //Assert
            Assert.Null(controller.mainTexture);
            Assert.Null(controller.maskTexture);
        }

        [UnityTest]
        public IEnumerator FailsGracefully_EmptyContent()
        {
            //Arrange
            WearableItem fakeWearable = new WearableItem
            {
                baseUrl = "http://nothing_here.nope",
                data = new WearableItem.Data()
                {
                    category = WearableLiterals.Categories.EYES,
                    representations = new []
                    {
                        new WearableItem.Representation
                        {
                            bodyShapes = new [] { WearableLiterals.BodyShapes.FEMALE },
                            contents = new WearableItem.MappingPair[0],
                        }
                    }
                }
            };
            FacialFeatureController controller = new FacialFeatureController(fakeWearable, new Material(Shader.Find("DCL/Toon Shader")));

            //Act
            controller.Load(bodyShapeController, Color.red);
            yield return new WaitUntil(() => controller.isReady);

            //Assert
            Assert.Null(controller.mainTexture);
            Assert.Null(controller.maskTexture);
        }

        [UnityTest]
        public IEnumerator LoadMouthWithMaskProperly()
        {
            //Arrange
            catalog.TryGetValue(DRACULA_MOUTH_ID, out WearableItem wereableItem);

            FacialFeatureController controller = new FacialFeatureController(wereableItem, new Material(Shader.Find("DCL/Unlit Cutout Tinted")));

            //Act
            controller.Load(bodyShapeController, Color.red);
            yield return new WaitUntil(() => controller.isReady);

            //Assert
            Assert.NotNull(controller.mainTexture);
            Assert.NotNull(controller.maskTexture);
        }
    }
}