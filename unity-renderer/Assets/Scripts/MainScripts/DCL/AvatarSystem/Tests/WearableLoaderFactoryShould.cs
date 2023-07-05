using System;
using AvatarSystem;
using DCL;
using NUnit.Framework;

namespace Test.AvatarSystem
{
    public class WearableLoaderFactoryShould
    {
        private WearableLoaderFactory wearableLoaderFactory;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            DCL.Environment.Setup(serviceLocator);

            wearableLoaderFactory = new WearableLoaderFactory();
        }

        [Test]
        public void ReturnLoaderForWearable()
        {
            WearableItem wearableItem = new WearableItem
            {
                data = new WearableItem.Data
                {
                    category = WearableLiterals.Categories.HAIR
                }
            };

            IWearableLoader loader = wearableLoaderFactory.GetWearableLoader(wearableItem);

            Assert.NotNull(loader);
            Assert.AreEqual(wearableItem, loader.bodyShape);
        }

        [Test]
        [TestCase(WearableLiterals.Categories.BODY_SHAPE)]
        [TestCase(WearableLiterals.Categories.EYES)]
        [TestCase(WearableLiterals.Categories.EYEBROWS)]
        [TestCase(WearableLiterals.Categories.MOUTH)]
        public void ThrowsWhenRequestingWearableLoaderWithBodyShapeOrFacialFeatures(string invalidCategory)
        {
            WearableItem wearableItem = new WearableItem { data = new WearableItem.Data { category = invalidCategory } };
            Assert.Throws<Exception>(() => wearableLoaderFactory.GetWearableLoader(wearableItem));
        }

        [Test]
        public void ReturnLoaderForBodyshape()
        {
            BodyWearables bodyWearables = new ()
            {
                bodyshape = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.BODY_SHAPE } },
                eyes = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYES } },
                eyebrows = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYEBROWS } },
                mouth = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.MOUTH } },
            };

            IBodyshapeLoader loader = wearableLoaderFactory.GetBodyShapeLoader(bodyWearables);

            Assert.NotNull(loader);
            Assert.AreEqual(bodyWearables.bodyshape, loader.bodyShape);
            Assert.AreEqual(bodyWearables.eyes, loader.eyes);
            Assert.AreEqual(bodyWearables.eyebrows, loader.eyebrows);
            Assert.AreEqual(bodyWearables.mouth, loader.mouth);
        }

        [Test]
        [TestCase(WearableLiterals.Categories.HAIR)]
        [TestCase(WearableLiterals.Categories.EYES)]
        [TestCase(WearableLiterals.Categories.FEET)]
        public void ThrowsWhenRequestingBodyshapeLoaderWithNoBodyshape(string noBodyshape)
        {
            BodyWearables bodyWearables = new ()
            {
                bodyshape = new WearableItem { data = new WearableItem.Data { category = noBodyshape } },
                eyes = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYES } },
                eyebrows = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYEBROWS } },
                mouth = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.MOUTH } },
            };

            Assert.Throws<Exception>(() => wearableLoaderFactory.GetBodyShapeLoader(bodyWearables));
        }

        [Test]
        [TestCase(WearableLiterals.Categories.HAIR)]
        [TestCase(WearableLiterals.Categories.FEET)]
        public void ThrowsWhenRequestingBodyshapeLoaderWithWrongEyes(string wrongEyes)
        {
            BodyWearables bodyWearables = new ()
            {
                bodyshape = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.BODY_SHAPE } },
                eyes = new WearableItem { data = new WearableItem.Data { category = wrongEyes } },
                eyebrows = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYEBROWS } },
                mouth = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.MOUTH } },
            };

            Assert.Throws<Exception>(() => wearableLoaderFactory.GetBodyShapeLoader(bodyWearables));
        }

        [Test]
        [TestCase(WearableLiterals.Categories.HAIR)]
        [TestCase(WearableLiterals.Categories.FEET)]
        public void ThrowsWhenRequestingBodyshapeLoaderWithWrongEyebrows(string wrongEyebrows)
        {
            BodyWearables bodyWearables = new ()
            {
                bodyshape = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.BODY_SHAPE } },
                eyes = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYES } },
                eyebrows = new WearableItem { data = new WearableItem.Data { category = wrongEyebrows } },
                mouth = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.MOUTH } },
            };

            Assert.Throws<Exception>(() => wearableLoaderFactory.GetBodyShapeLoader(bodyWearables));
        }

        [Test]
        [TestCase(WearableLiterals.Categories.HAIR)]
        [TestCase(WearableLiterals.Categories.FEET)]
        public void ThrowsWhenRequestingBodyshapeLoaderWithWrongMouth(string wrongMouth)
        {
            BodyWearables bodyWearables = new ()
            {
                bodyshape = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.BODY_SHAPE } },
                eyes = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYES } },
                eyebrows = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYEBROWS } },
                mouth = new WearableItem { data = new WearableItem.Data { category = wrongMouth } },
            };

            Assert.Throws<Exception>(() => wearableLoaderFactory.GetBodyShapeLoader(bodyWearables));
        }
    }
}
