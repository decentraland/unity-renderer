using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCLServices.CopyPaste.Analytics;
using DCLServices.MapRendererV2;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NavmapTests : IntegrationTestSuite_Legacy
    {
        private MinimapHUDController controller;
        private NavmapView navmapView;
        private IPlacesAPIService placesAPIService;

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

            placesAPIService = Substitute.For<IPlacesAPIService>();
            controller = new MinimapHUDController(
                Substitute.For<MinimapMetadataController>(),
                Substitute.For<IHomeLocationController>(),
                DCL.Environment.i,
                placesAPIService,
                Substitute.For<IPlacesAnalytics>(),
                Substitute.For<IClipboard>(),
                Substitute.For<ICopyPasteAnalyticsService>());
            controller.Initialize();
            navmapView = Object.FindObjectOfType<NavmapView>();
        }

        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator Toggle()
        {
            InputAction_Trigger action = null;
            var inputController = GameObject.FindObjectOfType<InputController>();
            for (int i = 0; i < inputController.triggerTimeActions.Length; i++)
            {
                // Find the open nav map action used by the input controller
                if (inputController.triggerTimeActions[i].DCLAction == DCLAction_Trigger.ToggleNavMap)
                {
                    action = inputController.triggerTimeActions[i];
                    break;
                }
            }

            Assert.IsNotNull(action);

            action.RaiseOnTriggered();

            yield return null;

            action.RaiseOnTriggered();

            yield return null;
        }

        [Test]
        public void ReactToPlayerCoordsChange()
        {
            placesAPIService.Configure()
                            .GetPlace(Arg.Any<Vector2Int>(), Arg.Any<CancellationToken>())
                            .Returns(x => new UniTask<IHotScenesController.PlaceInfo>(new IHotScenesController.PlaceInfo()));
            const string sceneName = "SCENE_NAME";
            MinimapMetadata.GetMetadata()
                .AddSceneInfo(
                    new MinimapMetadata.MinimapSceneInfo
                    {
                        parcels = new List<Vector2Int>
                        {
                            new Vector2Int(-77, -77)
                        },
                        name = sceneName
                    });
            CommonScriptableObjects.playerCoords.Set(new Vector2Int(-77, -77));
            Assert.AreEqual(sceneName, navmapView.currentSceneNameText.text);
            Assert.AreEqual("-77,-77", navmapView.currentSceneCoordsText.text);
        }
    }
}
