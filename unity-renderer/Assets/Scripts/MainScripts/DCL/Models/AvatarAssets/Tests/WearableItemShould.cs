using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AvatarAssets_Test
{
    public class WearableItemShould
    {
        private const string BODY_SHAPE = "male";

        [Test]
        public void WearableCantHideItself()
        {
            var wearable = new WearableItem
            {
                data = new WearableItem.Data
                {
                    category = WearableLiterals.Categories.UPPER_BODY,
                    representations = new[]
                    {
                        new WearableItem.Representation
                        {
                            bodyShapes = new[] { BODY_SHAPE },
                            overrideHides = new [] {WearableLiterals.Categories.UPPER_BODY }
                        }
                    },
                    hides = new [] {WearableLiterals.Categories.UPPER_BODY }
                }
            };

            string[] hides = wearable.GetHidesList(BODY_SHAPE);

            Assert.IsTrue(!hides.Contains(WearableLiterals.Categories.UPPER_BODY), "Wearable does not contain its own category");
        }

        [Test]
        public void GetHideCategoriesWhenIsSkin()
        {
            var wearable = GivenSkinWearable(null);

            var hides = wearable.GetHidesList(BODY_SHAPE);

            ThenHideListIs(WearableItem.SKIN_IMPLICIT_CATEGORIES, hides);
        }

        [Test]
        public void GetHideCategoriesWhenIsSkinThatAlsoHidesAccessories()
        {
            var customHides = new[] {"hat", "eyewear"};
            var wearable = GivenSkinWearable(customHides);

            var hides = wearable.GetHidesList(BODY_SHAPE);

            ThenHideListIs(hides.Concat(customHides).Distinct(), hides);
        }

        [Test]
        public void HidesAllCategoriesWhenIsSkin()
        {
            var wearable = GivenSkinWearable(null);

            foreach (string category in WearableItem.SKIN_IMPLICIT_CATEGORIES)
                Assert.IsTrue(wearable.DoesHide(category, BODY_SHAPE));
        }

        [Test]
        public void RemoveBodyshapeFromHidesWhenSanitizing()
        {
            var wearable = GivenSkinWearable(new [] { WearableLiterals.Categories.BODY_SHAPE });

            wearable.SanitizeHidesLists();

            Assert.IsFalse(wearable.data.hides.Contains(WearableLiterals.Categories.BODY_SHAPE));
            foreach (WearableItem.Representation representation in wearable.data.representations)
            {
                Assert.IsFalse(representation.overrideHides.Contains(WearableLiterals.Categories.BODY_SHAPE));
            }
        }

        private WearableItem GivenSkinWearable(string[] customHides)
        {
            var wearable = new WearableItem
            {
                data = new WearableItem.Data
                {
                    category = WearableLiterals.Categories.SKIN,
                    representations = new[]
                    {
                        new WearableItem.Representation
                        {
                            bodyShapes = new[] { BODY_SHAPE },
                            overrideHides = customHides
                        }
                    },
                    hides = customHides
                }
            };
            return wearable;
        }

        private void ThenHideListIs(IEnumerable<string> expected, IEnumerable<string> current) =>
            CollectionAssert.AreEqual(expected, current);
    }
}
