using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AvatarAssets_Test
{
    public class WearableItemShould
    {
        private const string BODY_SHAPE = "male";

        private readonly string[] skinImplicitHideList =
        {
            WearableLiterals.Categories.EYES,
            WearableLiterals.Categories.MOUTH,
            WearableLiterals.Categories.EYEBROWS,
            WearableLiterals.Categories.HAIR,
            WearableLiterals.Categories.UPPER_BODY,
            WearableLiterals.Categories.LOWER_BODY,
            WearableLiterals.Categories.FEET,
            WearableLiterals.Misc.HEAD,
            WearableLiterals.Categories.FACIAL_HAIR
        };

        [Test]
        public void GetHideCategoriesWhenIsSkin()
        {
            var wearable = GivenSkinWearable(null);

            var hides = wearable.GetHidesList(BODY_SHAPE);

            ThenHideListIs(skinImplicitHideList, hides);
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

            foreach (var category in skinImplicitHideList)
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

        private void ThenHideListIs(IEnumerable<string> expected, IEnumerable<string> current) => Assert.IsTrue(current.All(expected.Contains));
    }
}