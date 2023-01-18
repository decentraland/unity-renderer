using DCL.UIElements.Structures;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.UIElements.Image.Tests
{
    [TestFixture]
    public class DCLImageShould
    {
        private UIDocument uiDocument;
        private DCLImage image;

        private static Color color = Color.cyan;

        [SetUp]
        public void SetUp()
        {
            uiDocument = Object.Instantiate(Resources.Load<UIDocument>("ScenesUI"));
            var canvas = new VisualElement();
            uiDocument.rootVisualElement.Add(canvas);
            image = new DCLImage(canvas);
        }

        [TearDown]
        public void TearDown()
        {
            image.Dispose();
        }

        [Test]
        public void SelectColorGenerationMode()
        {
            image.Color = color;
            image.ScaleMode = DCLImageScaleMode.STRETCH;
            image.Texture = null;

            Assert.IsFalse(image.customMeshGenerationRequired);
            Assert.AreEqual(new StyleColor(color), image.canvas.style.backgroundColor);
            Assert.AreEqual(new StyleBackground(StyleKeyword.Null), image.canvas.style.backgroundImage);
        }

        [Test]
        public void SelectCenteredMode()
        {
            var tex = new Texture2D(64, 64);

            image.Color = color;
            image.ScaleMode = DCLImageScaleMode.CENTER;
            image.Texture = tex;
            image.Slices = new Vector4(0.4f, 0.1f, 0.1f, 0.5f);

            Assert.IsTrue(image.customMeshGenerationRequired);
            Assert.AreEqual(new StyleBackground(StyleKeyword.Null), image.canvas.style.backgroundImage);
            Assert.AreEqual(new StyleColor(StyleKeyword.None), image.canvas.style.backgroundColor);
        }

        [Test]
        public void SelectStretchedMode()
        {
            var tex = new Texture2D(64, 64);

            image.Color = color;
            image.ScaleMode = DCLImageScaleMode.STRETCH;
            image.Texture = tex;
            image.Slices = new Vector4(20, 5, 10, 2);

            Assert.IsTrue(image.customMeshGenerationRequired);
            Assert.AreEqual(new StyleBackground(StyleKeyword.Null), image.canvas.style.backgroundImage);
            Assert.AreEqual(new StyleColor(StyleKeyword.None), image.canvas.style.backgroundColor);
        }

        [Test]
        public void SelectBackgroundSlicedMode()
        {
            var tex = new Texture2D(64, 64);

            image.Color = color;
            image.ScaleMode = DCLImageScaleMode.NINE_SLICES;
            image.Texture = tex;
            image.Slices = new Vector4(0.125f, 0.0625f, 0.25f, 0.5f);

            Assert.IsFalse(image.customMeshGenerationRequired);
            Assert.AreEqual(new StyleBackground(tex), image.canvas.style.backgroundImage);
            Assert.AreEqual(new StyleColor(color), image.canvas.style.unityBackgroundImageTintColor);
            Assert.AreEqual(new StyleInt(8), image.canvas.style.unitySliceLeft);
            Assert.AreEqual(new StyleInt(4), image.canvas.style.unitySliceTop);
            Assert.AreEqual(new StyleInt(16), image.canvas.style.unitySliceRight);
            Assert.AreEqual(new StyleInt(32), image.canvas.style.unitySliceBottom);
        }

        [Test]
        public void FixIncorrectSlices()
        {
            var tex = new Texture2D(64, 64);

            image.Color = color;
            image.ScaleMode = DCLImageScaleMode.NINE_SLICES;
            image.Texture = tex;
            image.Slices = new Vector4(2f, 3f, 0.3f, 0.75f);

            Assert.LessOrEqual(image.Slices[0] + image.Slices[2], 1f);
            Assert.LessOrEqual(image.Slices[1] + image.Slices[3], 1f);
        }
    }
}
