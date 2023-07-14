using System.Collections.Generic;
using AvatarSystem;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Test.AvatarSystem
{
    public class VisibilityShould
    {
        private Visibility visibility;
        private Renderer combined;
        private List<Renderer> facialFeatures;

        [SetUp]
        public void SetUp()
        {
            combined = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>();
            facialFeatures = new List<Renderer>
            {
                GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>(),
                GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>(),
                GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>(),
            };

            visibility = new Visibility();

            visibility.Bind(combined, facialFeatures);
        }

        [Test]
        [TestCase("constrain1")]
        [TestCase("constrain1", "constrain2")]
        [TestCase("constrain1", "constrain2", "constrain3")]
        [TestCase("constrain1", "constrain2", "constrain3", "constrain4")]
        public void HideCombinedRendererIfAnyGlobalConstrain(params string[] constrains)
        {
            combined.enabled = true;

            visibility.globalConstrains.UnionWith(constrains);
            visibility.UpdateCombinedRendererVisibility();

            Assert.AreEqual(false, combined.enabled);
        }

        [Test]
        [TestCase("constrain1")]
        [TestCase("constrain1", "constrain2")]
        [TestCase("constrain1", "constrain2", "constrain3")]
        [TestCase("constrain1", "constrain2", "constrain3", "constrain4")]
        public void HideCombinedRendererIfAnySpecificConstrain(params string[] constrains)
        {
            combined.enabled = true;

            visibility.combinedRendererConstrains.UnionWith(constrains);
            visibility.UpdateCombinedRendererVisibility();

            Assert.AreEqual(false, combined.enabled);
        }

        [Test]
        [TestCase("constrain1")]
        [TestCase("constrain1", "constrain2")]
        [TestCase("constrain1", "constrain2", "constrain3")]
        [TestCase("constrain1", "constrain2", "constrain3", "constrain4")]
        public void HideCombinedRendererIfAnyCombinedConstrain(params string[] constrains)
        {
            combined.enabled = true;

            visibility.globalConstrains.UnionWith(constrains);
            visibility.combinedRendererConstrains.UnionWith(constrains);
            visibility.UpdateCombinedRendererVisibility();

            Assert.AreEqual(false, combined.enabled);
        }

        [Test]
        public void ShowCombinedRendererIfNoConstrains()
        {
            combined.enabled = false;

            visibility.globalConstrains.Clear();
            visibility.combinedRendererConstrains.Clear();
            visibility.UpdateCombinedRendererVisibility();

            Assert.AreEqual(true, combined.enabled);
        }

        [Test]
        [TestCase("constrain1")]
        [TestCase("constrain1", "constrain2")]
        [TestCase("constrain1", "constrain2", "constrain3")]
        [TestCase("constrain1", "constrain2", "constrain3", "constrain4")]
        public void HideFacialFeaturesIfAnyGlobalConstrain(params string[] constrains)
        {
            foreach (Renderer facialFeature in facialFeatures)
            {
                facialFeature.enabled = true;
            }

            visibility.globalConstrains.UnionWith(constrains);
            visibility.UpdateFacialFeatureVisibility();

            foreach (Renderer facialFeature in facialFeatures)
            {
                Assert.AreEqual(false, facialFeature.enabled);
            }
        }

        [Test]
        [TestCase("constrain1")]
        [TestCase("constrain1", "constrain2")]
        [TestCase("constrain1", "constrain2", "constrain3")]
        [TestCase("constrain1", "constrain2", "constrain3", "constrain4")]
        public void HideFacialFeaturesIfAnySpecificConstrain(params string[] constrains)
        {
            foreach (Renderer facialFeature in facialFeatures)
            {
                facialFeature.enabled = true;
            }

            visibility.facialFeaturesConstrains.UnionWith(constrains);
            visibility.UpdateFacialFeatureVisibility();

            foreach (Renderer facialFeature in facialFeatures)
            {
                Assert.AreEqual(false, facialFeature.enabled);
            }
        }

        [Test]
        [TestCase("constrain1")]
        [TestCase("constrain1", "constrain2")]
        [TestCase("constrain1", "constrain2", "constrain3")]
        [TestCase("constrain1", "constrain2", "constrain3", "constrain4")]
        public void HideFacialFeaturesIfAnyCombinedConstrain(params string[] constrains)
        {
            foreach (Renderer facialFeature in facialFeatures)
            {
                facialFeature.enabled = true;
            }

            visibility.globalConstrains.UnionWith(constrains);
            visibility.facialFeaturesConstrains.UnionWith(constrains);
            visibility.UpdateFacialFeatureVisibility();

            foreach (Renderer facialFeature in facialFeatures)
            {
                Assert.AreEqual(false, facialFeature.enabled);
            }
        }

        [Test]
        public void ShowFacialFeaturesIfNoConstrains()
        {
            foreach (Renderer facialFeature in facialFeatures)
            {
                facialFeature.enabled = true;
            }

            visibility.globalConstrains.Clear();
            visibility.facialFeaturesConstrains.Clear();
            visibility.UpdateFacialFeatureVisibility();

            foreach (Renderer facialFeature in facialFeatures)
            {
                Assert.AreEqual(true, facialFeature.enabled);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Utils.SafeDestroy(combined.gameObject);
            foreach (Renderer facialFeature in facialFeatures)
            {
                Utils.SafeDestroy(facialFeature.gameObject);
            }
        }
    }

}
