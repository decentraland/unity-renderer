using Cysharp.Threading.Tasks;
using DCL.Map;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using System.Threading;
using TMPro;
using UnityEngine;

namespace DCL
{
    public class NavmapView : MonoBehaviour
    {
        [Header("TEXT")]
        [SerializeField] internal TextMeshProUGUI currentSceneNameText;
        [SerializeField] internal TextMeshProUGUI currentSceneCoordsText;
        [SerializeField] internal NavmapSearchComponentView searchView;
        [SerializeField] internal NavmapFilterComponentView filterView;

        [Space]
        [SerializeField] internal NavmapToastView toastView;
        [SerializeField] private NavMapLocationControlsView locationControlsView;
        [SerializeField] private NavmapZoomView zoomView;

        [SerializeField] private NavmapRendererConfiguration navmapRendererConfiguration;

        internal NavmapVisibilityBehaviour navmapVisibilityBehaviour;

        private RectTransform rectTransform;
        private CancellationTokenSource updateSceneNameCancellationToken = new ();

        private RectTransform RectTransform => rectTransform ??= transform as RectTransform;
        private BaseVariable<Transform> configureMapInFullscreenMenu => DataStore.i.exploreV2.configureMapInFullscreenMenu;
        private NavmapFilterComponentController navmapFilterComponentController;

        private void Start()
        {
            navmapVisibilityBehaviour = new NavmapVisibilityBehaviour(DataStore.i.featureFlags.flags, DataStore.i.HUDs.navmapVisible, zoomView, toastView, searchView, locationControlsView,
                navmapRendererConfiguration, Environment.i.platform.serviceLocator.Get<IPlacesAPIService>(), new PlacesAnalytics());
            navmapFilterComponentController = new NavmapFilterComponentController(filterView);

            ConfigureMapInFullscreenMenuChanged(configureMapInFullscreenMenu.Get(), null);
            DataStore.i.HUDs.isNavMapInitialized.Set(true);
        }

        private void OnEnable()
        {
            configureMapInFullscreenMenu.OnChange += ConfigureMapInFullscreenMenuChanged;
            updateSceneNameCancellationToken = updateSceneNameCancellationToken.SafeRestart();
            UpdateSceneNameAsync(CommonScriptableObjects.playerCoords.Get(), updateSceneNameCancellationToken.Token).Forget();
            CommonScriptableObjects.playerCoords.OnChange += UpdateCurrentSceneData;

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
            CommonScriptableObjects.playerCoords.OnChange -= UpdateCurrentSceneData;
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

        private void UpdateCurrentSceneData(Vector2Int current, Vector2Int _)
        {
            const string format = "{0},{1}";
            currentSceneCoordsText.text = string.Format(format, current.x, current.y);
            currentSceneNameText.text = MinimapMetadata.GetMetadata().GetSceneInfo(current.x, current.y)?.name ?? "Unnamed";
        }

        private async UniTaskVoid UpdateSceneNameAsync(Vector2Int current, CancellationToken cancellationToken)
        {
            MinimapMetadata.MinimapSceneInfo info = MinimapMetadata.GetMetadata().GetSceneInfo(current.x, current.y);

            if (info == null)
            {
                await WebInterfaceMinimapApiBridge.i.GetScenesInformationAroundParcel(current, 2, cancellationToken);
                info = MinimapMetadata.GetMetadata().GetSceneInfo(current.x, current.y);
            }

            currentSceneNameText.text = info?.name ?? "Unnamed";
        }
    }
}
