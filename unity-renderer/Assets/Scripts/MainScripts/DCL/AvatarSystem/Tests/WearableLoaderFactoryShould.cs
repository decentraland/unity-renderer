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
            Assert.AreEqual(wearableItem, loader.wearable);
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
            WearableItem bodyshape = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.BODY_SHAPE } };
            WearableItem eyes = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYES } };
            WearableItem eyebrows = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYEBROWS } };
            WearableItem mouth = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.MOUTH } };
            IBodyshapeLoader loader = wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth);

            Assert.NotNull(loader);
            Assert.AreEqual(bodyshape, loader.wearable);
            Assert.AreEqual(eyes, loader.eyes);
            Assert.AreEqual(eyebrows, loader.eyebrows);
            Assert.AreEqual(mouth, loader.mouth);
        }

        [Test]
        [TestCase(WearableLiterals.Categories.HAIR)]
        [TestCase(WearableLiterals.Categories.EYES)]
        [TestCase(WearableLiterals.Categories.FEET)]
        public void ThrowsWhenRequestingBodyshapeLoaderWithNoBodyshape(string noBodyshape)
        {
            WearableItem bodyshape = new WearableItem { data = new WearableItem.Data { category = noBodyshape } };
            WearableItem eyes = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYES } };
            WearableItem eyebrows = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYEBROWS } };
            WearableItem mouth = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.MOUTH } };
            Assert.Throws<Exception>(() => wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth));
        }

        [Test]
        [TestCase(WearableLiterals.Categories.HAIR)]
        [TestCase(WearableLiterals.Categories.FEET)]
        public void ThrowsWhenRequestingBodyshapeLoaderWithWrongEyes(string wrongEyes)
        {
            WearableItem bodyshape = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.BODY_SHAPE } };
            WearableItem eyes = new WearableItem { data = new WearableItem.Data { category = wrongEyes } };
            WearableItem eyebrows = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYEBROWS } };
            WearableItem mouth = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.MOUTH } };
            Assert.Throws<Exception>(() => wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth));
        }

        [Test]
        [TestCase(WearableLiterals.Categories.HAIR)]
        [TestCase(WearableLiterals.Categories.FEET)]
        public void ThrowsWhenRequestingBodyshapeLoaderWithWrongEyebrows(string wrongEyebrows)
        {
            WearableItem bodyshape = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.BODY_SHAPE } };
            WearableItem eyes = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYES } };
            WearableItem eyebrows = new WearableItem { data = new WearableItem.Data { category = wrongEyebrows } };
            WearableItem mouth = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.MOUTH } };
            Assert.Throws<Exception>(() => wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth));
        }

        [Test]
        [TestCase(WearableLiterals.Categories.HAIR)]
        [TestCase(WearableLiterals.Categories.FEET)]
        public void ThrowsWhenRequestingBodyshapeLoaderWithWrongMouth(string wrongMouth)
        {
            WearableItem bodyshape = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.BODY_SHAPE } };
            WearableItem eyes = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYES } };
            WearableItem eyebrows = new WearableItem { data = new WearableItem.Data { category = WearableLiterals.Categories.EYEBROWS } };
            WearableItem mouth = new WearableItem { data = new WearableItem.Data { category = wrongMouth } };
            Assert.Throws<Exception>(() => wearableLoaderFactory.GetBodyshapeLoader(bodyshape, eyes, eyebrows, mouth));
        }
    }
}
