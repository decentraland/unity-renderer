using NUnit.Framework;
using UnityEditor;

namespace Tests
{
    public class GameViewUtilsTests
    {
        [Test]
        [Category("Visual Tests")]
        public static void SizeIsAddedAndRemovedCorrectly()
        {
            GameViewUtils.AddOrGetCustomSize(GameViewUtils.GameViewSizeType.FixedResolution, GameViewSizeGroupType.Standalone, 123, 456, "Test size");

            int idx = GameViewUtils.FindSize(GameViewSizeGroupType.Standalone, 123, 456);
            Assert.AreNotEqual(-1, idx);
            GameViewUtils.RemoveCustomSize(GameViewSizeGroupType.Standalone, idx);

            idx = GameViewUtils.FindSize(GameViewSizeGroupType.Standalone, 123, 456);
            Assert.AreEqual(-1, idx);
        }
    }
}
