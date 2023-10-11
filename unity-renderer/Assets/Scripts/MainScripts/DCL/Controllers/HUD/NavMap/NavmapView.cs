using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Map;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class NavmapView : MonoBehaviour
    {
        [SerializeField] internal NavmapSearchComponentView searchView;
        [SerializeField] internal NavmapFilterComponentView filterView;
        [SerializeField] internal GameObject placeCardModalParent;

        [Space]
        [SerializeField] internal NavmapToastView toastView;
        [SerializeField] private NavMapLocationControlsView locationControlsView;
        [SerializeField] private NavmapZoomView zoomView;
        [SerializeField] private NavMapChunksLayersView chunksLayersView;

        [Space]
        [SerializeField] private NavmapRendererConfiguration navmapRendererConfiguration;

        private IPlaceCardComponentView placeCardModal;

        internal NavmapVisibilityBehaviour navmapVisibilityBehaviour;

        private RectTransform rectTransform;
        private CancellationTokenSource updateSceneNameCancellationToken = new ();

        private RectTransform RectTransform => rectTransform ??= transform as RectTransform;
        private BaseVariable<Transform> configureMapInFullscreenMenu => DataStore.i.exploreV2.configureMapInFullscreenMenu;
        private NavmapFilterComponentController navmapFilterComponentController;

        private void Start()
        {
            placeCardModal = placeCardModalParent.GetComponent<IPlaceCardComponentView>();

            var exploreV2Analytics = new ExploreV2Analytics.ExploreV2Analytics();

            navmapVisibilityBehaviour = new NavmapVisibilityBehaviour(
                DataStore.i.featureFlags.flags,
                DataStore.i.HUDs.navmapVisible,
                zoomView,
                toastView,
                searchView,
                locationControlsView,
                chunksLayersView,
                navmapRendererConfiguration,
                Environment.i.platform.serviceLocator.Get<IPlacesAPIService>(),
                new PlacesAnalytics(),
                placeCardModal,
                exploreV2Analytics,
                new WebInterfaceBrowserBridge());
            navmapFilterComponentController = new NavmapFilterComponentController(filterView, new WebInterfaceBrowserBridge(), exploreV2Analytics, new UserProfileWebInterfaceBridge(), DataStore.i);

            ConfigureMapInFullscreenMenuChanged(configureMapInFullscreenMenu.Get(), null);
            DataStore.i.HUDs.isNavMapInitialized.Set(true);
        }

        private void OnEnable()
        {
            configureMapInFullscreenMenu.OnChange += ConfigureMapInFullscreenMenuChanged;
            updateSceneNameCancellationToken = updateSceneNameCancellationToken.SafeRestart();

            //Needed due to script execution order
            DataStore.i.featureFlags.flags.OnChange += OnFeatureFlagsChanged;
        }

        private void OnFeatureFlagsChanged(FeatureFlag current, FeatureFlag previous)
        {
            //TODO Remove: Temporary to allow PR merging
            searchView.gameObject.SetActive(DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("navmap_header"));
        }

        private void OnDisable()
        {
            configureMapInFullscreenMenu.OnChange -= ConfigureMapInFullscreenMenuChanged;
            DataStore.i.featureFlags.flags.OnChange -= OnFeatureFlagsChanged;
        }

        private void OnDestroy()
        {
            navmapVisibilityBehaviour.Dispose();
        }

        private void ConfigureMapInFullscreenMenuChanged(Transform currentParentTransform, Transform _)
        {
            if (currentParentTransform == null || transform.parent == currentParentTransform)
                return;

            transform.SetParent(currentParentTransform);
            transform.localScale = Vector3.one;

            RectTransform.anchorMin = Vector2.zero;
            RectTransform.anchorMax = Vector2.one;
            RectTransform.pivot = new Vector2(0.5f, 0.5f);
            RectTransform.localPosition = Vector2.zero;
            RectTransform.offsetMax = Vector2.zero;
            RectTransform.offsetMin = Vector2.zero;
        }
    }
}
