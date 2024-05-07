using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Map;
using DCLServices.CopyPaste.Analytics;
using DCLServices.MapRendererV2;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;

namespace Tests
{
    public class NavmapToastViewShould : IntegrationTestSuite_Legacy
    {
        NavmapToastView navmapToastView;
        private NavmapView navmapView;
        private MinimapHUDController controller;
        private WebInterfaceMinimapApiBridgeMock minimapApiBridge;

        protected override List<GameObject> SetUp_LegacySystems()
        {
            List<GameObject> result = new List<GameObject>();
            minimapApiBridge = new GameObject("WebInterfaceMinimapApiBridge").AddComponent<WebInterfaceMinimapApiBridgeMock>();
            result.Add(minimapApiBridge.gameObject);
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

            IPlacesAPIService placesAPIService = Substitute.For<IPlacesAPIService>();

            placesAPIService.GetPlace(default(Vector2Int), default, default)
                            .ReturnsForAnyArgs(UniTask.FromResult(new IHotScenesController.PlaceInfo
                             {
                                 id = "placeId",
                             }));

            placesAPIService.IsFavoritePlace(default(IHotScenesController.PlaceInfo), default, default)
                            .ReturnsForAnyArgs(UniTask.FromResult(false));

            controller = new MinimapHUDController(
                Substitute.For<MinimapMetadataController>(),
                Substitute.For<IHomeLocationController>(),
                Environment.i,
                placesAPIService,
                Substitute.For<IPlacesAnalytics>(),
                Substitute.For<IClipboard>(),
                Substitute.For<ICopyPasteAnalyticsService>(),
                DataStore.i.contentModeration,
                Substitute.For<IWorldState>());
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

        private class WebInterfaceMinimapApiBridgeMock : WebInterfaceMinimapApiBridge
        {
            public MinimapMetadata.MinimapSceneInfo[] ScenesInformationResult { get; set; } = Array.Empty<MinimapMetadata.MinimapSceneInfo>();

            public async override UniTask<MinimapMetadata.MinimapSceneInfo[]> GetScenesInformationAroundParcel(Vector2Int coordinate, int areaSize, CancellationToken cancellationToken)
            {
                foreach (MinimapMetadata.MinimapSceneInfo sceneInfo in ScenesInformationResult)
                    MinimapMetadata.GetMetadata().AddSceneInfo(sceneInfo);

                return ScenesInformationResult;
            }
        }
    }
}
