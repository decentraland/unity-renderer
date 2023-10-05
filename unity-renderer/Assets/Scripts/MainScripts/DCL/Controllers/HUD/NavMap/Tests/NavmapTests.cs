using Cysharp.Threading.Tasks;
using DCL;
using DCL.Map;
using DCLServices.CopyPaste.Analytics;
using DCLServices.MapRendererV2;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NavmapTests : IntegrationTestSuite_Legacy
    {
        private const string INITIAL_SCENE_NAME = "INITIAL_SCENE";

        private MinimapHUDController controller;
        private NavmapView navmapView;
        private IPlacesAPIService placesAPIService;
        private WebInterfaceMinimapApiBridgeMock minimapApiBridge;

        protected override List<GameObject> SetUp_LegacySystems()
        {
            List<GameObject> result = new List<GameObject>();
            minimapApiBridge = new GameObject("WebInterfaceMinimapApiBridge").AddComponent<WebInterfaceMinimapApiBridgeMock>();
            minimapApiBridge.ScenesInformationResult = new []
            {
                new MinimapMetadata.MinimapSceneInfo
                {
                    name = INITIAL_SCENE_NAME,
                    parcels = new List<Vector2Int>
                    {
                        Vector2Int.zero,
                    },
                }
            };
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

            placesAPIService = Substitute.For<IPlacesAPIService>();
            placesAPIService.Configure()
                            .GetPlace(Arg.Any<Vector2Int>(), Arg.Any<CancellationToken>())
                            .Returns(x => new UniTask<IHotScenesController.PlaceInfo>(new IHotScenesController.PlaceInfo()));

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

        private class WebInterfaceMinimapApiBridgeMock : WebInterfaceMinimapApiBridge
        {
            public MinimapMetadata.MinimapSceneInfo[] ScenesInformationResult { get; set; }

            public async override UniTask<MinimapMetadata.MinimapSceneInfo[]> GetScenesInformationAroundParcel(Vector2Int coordinate, int areaSize, CancellationToken cancellationToken)
            {
                foreach (MinimapMetadata.MinimapSceneInfo sceneInfo in ScenesInformationResult)
                    MinimapMetadata.GetMetadata().AddSceneInfo(sceneInfo);

                return ScenesInformationResult;
            }
        }
    }
}
