using System.Collections;
using System.Collections.Generic;
using DCL;
using DCLServices.MapRendererV2;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NavmapToastViewShould : IntegrationTestSuite_Legacy
    {
        NavmapToastView navmapToastView;
        private NavmapView navmapView;
        private MinimapHUDController controller;

        protected override List<GameObject> SetUp_LegacySystems()
        {
            List<GameObject> result = new List<GameObject>();
            result.Add(MainSceneFactory.CreateNavMap());
            return result;
        }

        protected override ServiceLocator InitializeServiceLocator()
        {
            var result = base.InitializeServiceLocator();
            result.Register<IMapRenderer>(() => Substitute.For<IMapRenderer>());
            return result;
        }

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            yield return null;

            controller = new MinimapHUDController(Substitute.For<MinimapMetadataController>(), Substitute.For<IHomeLocationController>(), DCL.Environment.i);
            controller.Initialize();
            navmapView = Object.FindObjectOfType<NavmapView>();
            navmapToastView = navmapView.toastView;

            if (!DataStore.i.HUDs.navmapVisible.Get())
                navmapView.navmapVisibilityBehaviour.SetVisible(true);
        }

        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
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

            navmapToastView.Populate(new Vector2Int(10, 11), new Vector2(10, 11), sceneInfo);
            Assert.IsTrue(navmapToastView.gameObject.activeSelf);
            navmapToastView.Close();
            Assert.IsFalse(navmapToastView.gameObject.activeSelf);
        }

        [Test]
        [Explicit("This fails only on run all for some reason")]
        [Category("Explicit")]
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

            navmapToastView.Populate(new Vector2Int(10, 11), new Vector2(10, 11), sceneInfo);
            Assert.IsTrue(navmapToastView.gameObject.activeSelf);

            Assert.IsTrue(navmapToastView.sceneLocationText.transform.parent.gameObject.activeInHierarchy);
            Assert.AreEqual("10, 11", navmapToastView.sceneLocationText.text);

            Assert.IsTrue(navmapToastView.sceneTitleText.transform.parent.gameObject.activeInHierarchy);
            Assert.IsFalse(navmapToastView.sceneOwnerText.transform.parent.gameObject.activeInHierarchy);
            Assert.IsFalse(navmapToastView.scenePreviewContainer.gameObject.activeInHierarchy);
        }

        [Test]
        [Explicit("Broke with Legacy suite refactor, fix later")]
        [Category("Explicit")]
        public void BePopulatedCorrectly()
        {
            CommonScriptableObjects.isFullscreenHUDOpen.Set(true);

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

            navmapToastView.Populate(new Vector2Int(10, 10), new Vector2(10, 11), sceneInfo);
            Assert.IsTrue(navmapToastView.gameObject.activeSelf);

            Assert.AreEqual(sceneInfo.name, navmapToastView.sceneTitleText.text);
            Assert.AreEqual($"Created by: {sceneInfo.owner}", navmapToastView.sceneOwnerText.text);
            Assert.AreEqual("10, 10", navmapToastView.sceneLocationText.text);

            Assert.IsTrue(navmapToastView.sceneTitleText.gameObject.activeInHierarchy);
            Assert.IsTrue(navmapToastView.sceneOwnerText.gameObject.activeInHierarchy);

            CommonScriptableObjects.isFullscreenHUDOpen.Set(false);
        }
    }
}
