using DCL.Components;
using NUnit.Framework;
using TMPro;

namespace Tests
{
    public class TextShapeHelperTests
    {
        [Test]
        public void TestTextAlignment()
        {
            Assert.AreEqual(TextAlignmentOptions.BottomLeft, TextShape.GetAlignment("bottom", "left"));
            Assert.AreEqual(TextAlignmentOptions.BottomRight, TextShape.GetAlignment("Bottom", "RIght"));
            Assert.AreEqual(TextAlignmentOptions.Center, TextShape.GetAlignment("Center", "center"));
            Assert.AreEqual(TextAlignmentOptions.TopLeft, TextShape.GetAlignment("top", "Left"));
            Assert.AreEqual(TextAlignmentOptions.TopRight, TextShape.GetAlignment("top", "right"));
            Assert.AreEqual(TextAlignmentOptions.Top, TextShape.GetAlignment("top", "center"));
            Assert.AreEqual(TextAlignmentOptions.Right, TextShape.GetAlignment("center", "right"));
            Assert.AreEqual(TextAlignmentOptions.Bottom, TextShape.GetAlignment("Bottom", "cEnter"));
            Assert.AreEqual(TextAlignmentOptions.Left, TextShape.GetAlignment("center", "left"));
        }
    }
}
