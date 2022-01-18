using AvatarSystem;
using NUnit.Framework;
using UnityEngine;

namespace Test.AvatarSystem
{
    public class VisibilityShould
    {
        private Visibility visibility;
        private Renderer combined;
        private Renderer[] facialFeatures;

        [SetUp]
        public void SetUp()
        {
            combined = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>();
            facialFeatures = new []
            {
                GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>(),
                GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>(),
                GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>(),
            };

            visibility = new Visibility();

            visibility.Bind(combined, facialFeatures);
        }

        [Test]
        [TestCase(false, false, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, true, false, false)]
        [TestCase(false, false, true, false)]
        [TestCase(true, true, true, true)]
        public void SetVisibilityProperlyForCombinedRenderer(bool loadingReady, bool explicitVisibility, bool combinedRendererVisibility, bool expected)
        {
            visibility.SetLoadingReady(loadingReady);
            visibility.SetExplicitVisibility(explicitVisibility);
            visibility.SetCombinedRendererVisibility(combinedRendererVisibility);

            Assert.AreEqual(expected, combined.enabled);
        }

        [Test]
        [TestCase(false, false, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, true, false, false)]
        [TestCase(false, false, true, false)]
        [TestCase(true, true, true, true)]
        public void SetVisibilityProperlyForFacialFeatures(bool loadingReady, bool explicitVisibility, bool facialFeaturesVisibility, bool expected)
        {
            visibility.SetLoadingReady(loadingReady);
            visibility.SetExplicitVisibility(explicitVisibility);
            visibility.SetFacialFeaturesVisibility(facialFeaturesVisibility);

            Assert.AreEqual(expected, combined.enabled);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(combined.gameObject);
            foreach (Renderer facialFeature in facialFeatures)
            {
                Object.Destroy(facialFeature.gameObject);
            }
        }
    }

}