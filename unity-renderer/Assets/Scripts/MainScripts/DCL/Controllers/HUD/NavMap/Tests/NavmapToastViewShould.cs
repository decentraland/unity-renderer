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
            yield return null;

            var navmapView = GameObject.FindObjectOfType<NavmapView>();
            navmapToastView = navmapView.toastView;

            if (!NavmapView.isOpen)
                navmapView.ToggleNavMap();
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

            Assert.IsTrue(navmapToastView.sceneLocationText.transform.parent.gameObject.activeInHierarchy);
            Assert.AreEqual("10, 11", navmapToastView.sceneLocationText.text);

            Assert.IsTrue(navmapToastView.sceneTitleText.transform.parent.gameObject.activeInHierarchy);
            Assert.IsFalse(navmapToastView.sceneOwnerText.transform.parent.gameObject.activeInHierarchy);
            Assert.IsFalse(navmapToastView.sceneDescriptionText.transform.parent.gameObject.activeInHierarchy);
            Assert.IsFalse(navmapToastView.scenePreviewImage.gameObject.activeInHierarchy);
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
