using DCL.Helpers;
using NUnit.Framework;

namespace AvatarAssets_Test
{
    public class GeneralReplacesListShould : IntegrationTestSuite_Legacy
    {
        [Test]
        public void BeRetrievedProperly()
        {
            WearableItem wearableItem = new WearableItem
            {
                replaces = new [] { "category1", "category2", "category3" }
            };

            var replaces = wearableItem.GetReplacesList(null);

            Assert.AreEqual("category1", replaces[0]);
            Assert.AreEqual("category2", replaces[1]);
            Assert.AreEqual("category3", replaces[2]);
        }

        [Test]
        public void BeRetrievedWhenBodyShapeDoesntMatchOverride()
        {
            WearableItem wearableItem = new WearableItem
            {
                replaces = new [] { "category1", "category2", "category3" },
                representations = new []
                {
                    new WearableItem.Representation()
                    {
                        bodyShapes = new []{"bodyShape1"},
                        overrideReplaces = new [] { "override1", "override2", "override3" },
                    } 
                }
            };

            var replaces = wearableItem.GetReplacesList("not_bodyShape1");

            Assert.AreEqual("category1", replaces[0]);
            Assert.AreEqual("category2", replaces[1]);
            Assert.AreEqual("category3", replaces[2]);
        }
    }
    
    public class OverrideReplacesListShould : IntegrationTestSuite_Legacy
    {
        [Test]
        public void BeRetrievedProperly()
        {
            WearableItem wearableItem = new WearableItem
            {
                replaces = new [] { "category1", "category2", "category3" },
                representations = new []
                {
                    new WearableItem.Representation()
                    {
                        bodyShapes = new []{"bodyShape1"},
                        overrideReplaces = new [] { "override1", "override2", "override3" },
                    } 
                }
            };

            var replaces = wearableItem.GetReplacesList("bodyShape1");

            Assert.AreEqual("override1", replaces[0]);
            Assert.AreEqual("override2", replaces[1]);
            Assert.AreEqual("override3", replaces[2]);
        }
    }
    
    public class GeneralHidesListShould : IntegrationTestSuite_Legacy
    {
        [Test]
        public void BeRetrievedProperly()
        {
            WearableItem wearableItem = new WearableItem
            {
                hides = new [] { "category1", "category2", "category3" }
            };

            var hides = wearableItem.GetHidesList(null);

            Assert.AreEqual("category1", hides[0]);
            Assert.AreEqual("category2", hides[1]);
            Assert.AreEqual("category3", hides[2]);
        }

        [Test]
        public void BeRetrievedWhenBodyShapeDoesntMatchOverride()
        {
            WearableItem wearableItem = new WearableItem
            {
                hides = new [] { "category1", "category2", "category3" },
                representations = new []
                {
                    new WearableItem.Representation()
                    {
                        bodyShapes = new []{"bodyShape1"},
                        overrideHides = new [] { "override1", "override2", "override3" },
                    } 
                }
            };

            var hides = wearableItem.GetHidesList("not_bodyShape1");

            Assert.AreEqual("category1", hides[0]);
            Assert.AreEqual("category2", hides[1]);
            Assert.AreEqual("category3", hides[2]);
        }
    }
    
    public class OverrideHidesListShould : IntegrationTestSuite_Legacy
    {
        [Test]
        public void BeRetrievedProperly()
        {
            WearableItem wearableItem = new WearableItem
            {
                hides = new [] { "category1", "category2", "category3" },
                representations = new []
                {
                    new WearableItem.Representation()
                    {
                        bodyShapes = new []{"bodyShape1"},
                        overrideHides = new [] { "override1", "override2", "override3" },
                    } 
                }
            };

            var hides = wearableItem.GetHidesList("bodyShape1");

            Assert.AreEqual("override1", hides[0]);
            Assert.AreEqual("override2", hides[1]);
            Assert.AreEqual("override3", hides[2]);
        }
    }
}