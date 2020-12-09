using System.Collections;
using DCL;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = DCL.WaitUntil;

namespace AvatarShape_Tests
{
    public class FacialFeatureControllerShould : TestsBase
    {
        private const string EYES_ID = "dcl://base-avatars/f_eyes_01";
        private const string DRACULA_MOUTH_ID = "dcl://base-avatars/dracula_mouth";
        private WearableDictionary catalog;
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
            FacialFeatureController controller = new FacialFeatureController(catalog.GetOrDefault(EYES_ID), new Material(Shader.Find("DCL/Toon Shader")));

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
                category = WearableLiterals.Categories.EYES,
                baseUrl = "http://nothing_here.nope",
                representations = new []
                {
                    new WearableItem.Representation
                    {
                        bodyShapes = new [] { WearableLiterals.BodyShapes.FEMALE },
                        contents = new [] { new ContentServerUtils.MappingPair{file = "fake.png", hash = "nope"}, new ContentServerUtils.MappingPair{file = "fake_mask.png", hash = "nope2"} },
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
                category = WearableLiterals.Categories.EYES,
                baseUrl = "http://nothing_here.nope",
                representations = new []
                {
                    new WearableItem.Representation
                    {
                        bodyShapes = new [] { WearableLiterals.BodyShapes.FEMALE },
                        contents = new ContentServerUtils.MappingPair[0],
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
            FacialFeatureController controller = new FacialFeatureController(catalog.GetOrDefault(DRACULA_MOUTH_ID), new Material(Shader.Find("DCL/Unlit Cutout Tinted")));

            //Act
            controller.Load(bodyShapeController, Color.red);
            yield return new WaitUntil(() => controller.isReady);

            //Assert
            Assert.NotNull(controller.mainTexture);
            Assert.NotNull(controller.maskTexture);
        }
    }
}