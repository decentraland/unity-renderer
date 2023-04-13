using System;
using TMPro;
using UnityEngine;

namespace DCL
{
    public class NavmapView : MonoBehaviour
    {
        [Header("TEXT")]
        [SerializeField] internal TextMeshProUGUI currentSceneNameText;
        [SerializeField] internal TextMeshProUGUI currentSceneCoordsText;

        [Space]
        [SerializeField] internal NavmapToastView toastView;
        [SerializeField] private NavmapZoom zoom;

        [SerializeField] private NavmapRendererConfiguration navmapRendererConfiguration;

        internal NavmapVisibilityBehaviour navmapVisibilityBehaviour;

        private RectTransform rectTransform;

        private RectTransform RectTransform => rectTransform ??= transform as RectTransform;
        private BaseVariable<Transform> configureMapInFullscreenMenu => DataStore.i.exploreV2.configureMapInFullscreenMenu;

        private void Start()
        {
            navmapVisibilityBehaviour = new NavmapVisibilityBehaviour(DataStore.i.HUDs.navmapVisible, zoom, toastView, navmapRendererConfiguration);

            ConfigureMapInFullscreenMenuChanged(configureMapInFullscreenMenu.Get(), null);
            DataStore.i.HUDs.isNavMapInitialized.Set(true);
        }

        private void OnEnable()
        {
            configureMapInFullscreenMenu.OnChange += ConfigureMapInFullscreenMenuChanged;
            CommonScriptableObjects.playerCoords.OnChange += UpdateCurrentSceneData;
        }

        private void OnDisable()
        {
            configureMapInFullscreenMenu.OnChange -= ConfigureMapInFullscreenMenuChanged;
            CommonScriptableObjects.playerCoords.OnChange -= UpdateCurrentSceneData;
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
    }
}
