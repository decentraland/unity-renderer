using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NavmapToastViewShould : TestsBase
    {
        NavmapToastView navmapToastView;
        protected override bool justSceneSetUp => true;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            var navmapView = GameObject.FindObjectOfType<NavmapView>();
            navmapToastView = navmapView.toastView;

            if (!navmapView.isToggledOn)
                navmapView.ToggleNavMap();
        }

        [Test]
        public void CloseOnNull()
        {
            var dummyScene = new MinimapMetadata.MinimapSceneInfo()
            {
                parcels = new List<Vector2Int> { new Vector2Int(0, 0) }
            };

            MinimapMetadata.GetMetadata().Clear();
            MinimapMetadata.GetMetadata().AddSceneInfo(dummyScene);

            navmapToastView.Populate(new Vector2Int(0, 0), dummyScene);
            Assert.IsTrue(navmapToastView.gameObject.activeSelf);

            navmapToastView.Populate(new Vector2Int(0, 0), null);
            Assert.IsFalse(navmapToastView.gameObject.activeSelf);
        }

        [Test]
        public void CloseWhenCloseButtonIsClicked()
        {
            var sceneInfo = new MinimapMetadata.MinimapSceneInfo()
            {
                name = "foo",
                owner = null,
                description = "",
                isPOI = false,
                parcels = new List<Vector2Int>() { new Vector2Int(10, 10), new Vector2Int(10, 11), new Vector2Int(10, 12) }
            };

            MinimapMetadata.GetMetadata().Clear();
            MinimapMetadata.GetMetadata().AddSceneInfo(sceneInfo);

            navmapToastView.Populate(new Vector2Int(10, 11), sceneInfo);
            Assert.IsTrue(navmapToastView.gameObject.activeSelf);
            navmapToastView.closeButton.onClick.Invoke();
            Assert.IsFalse(navmapToastView.gameObject.activeSelf);
        }

        [Test]
        public void BePopulatedCorrectlyWithNullOrEmptyElements()
        {
            var sceneInfo = new MinimapMetadata.MinimapSceneInfo()
            {
                name = "foo",
                owner = null,
                description = "",
                isPOI = false,
                parcels = new List<Vector2Int>() { new Vector2Int(10, 10), new Vector2Int(10, 11), new Vector2Int(10, 12) }
            };

            MinimapMetadata.GetMetadata().Clear();
            MinimapMetadata.GetMetadata().AddSceneInfo(sceneInfo);

            navmapToastView.Populate(new Vector2Int(10, 11), sceneInfo);
            Assert.IsTrue(navmapToastView.gameObject.activeSelf);

            Assert.AreEqual("10, 11", navmapToastView.sceneLocationText.text);

            Assert.IsTrue(navmapToastView.sceneTitleText.gameObject.activeInHierarchy);
            Assert.IsFalse(navmapToastView.sceneOwnerText.gameObject.activeInHierarchy);
            Assert.IsFalse(navmapToastView.sceneDescriptionText.gameObject.activeInHierarchy);
        }

        [Test]
        public void BePopulatedCorrectly()
        {
            var sceneInfo = new MinimapMetadata.MinimapSceneInfo()
            {
                name = "foo",
                owner = "bar",
                description = "foobar",
                isPOI = false,
                parcels = new List<Vector2Int>() { new Vector2Int(10, 10), new Vector2Int(10, 11), new Vector2Int(10, 12) }
            };

            MinimapMetadata.GetMetadata().Clear();
            MinimapMetadata.GetMetadata().AddSceneInfo(sceneInfo);

            navmapToastView.Populate(new Vector2Int(10, 10), sceneInfo);
            Assert.IsTrue(navmapToastView.gameObject.activeSelf);

            Assert.AreEqual(sceneInfo.name, navmapToastView.sceneTitleText.text);
            Assert.AreEqual($"Created by: {sceneInfo.owner}", navmapToastView.sceneOwnerText.text);
            Assert.AreEqual("10, 10", navmapToastView.sceneLocationText.text);

            Assert.IsTrue(navmapToastView.sceneTitleText.gameObject.activeInHierarchy);
            Assert.IsTrue(navmapToastView.sceneOwnerText.gameObject.activeInHierarchy);
            Assert.IsTrue(navmapToastView.sceneDescriptionText.gameObject.activeInHierarchy);
        }
    }
}
