using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{

    public class NavmapView : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] internal ScrollRect scrollRect;
        [SerializeField] private Transform scrollRectContentTransform;

        [Header("TEXT")]
        [SerializeField] internal TextMeshProUGUI currentSceneNameText;
        [SerializeField] internal TextMeshProUGUI currentSceneCoordsText;
        
        [Space]
        [SerializeField] internal NavmapToastView toastView;
        [SerializeField] private NavmapZoom zoom;
        
        internal NavmapVisibilityBehaviour navmapVisibilityBehaviour;
        private NavmapCloseButtonBehaviour closeButtonBehaviour;
        
        private RectTransform rectTransform;


        private RectTransform RectTransform => rectTransform ??= transform as RectTransform;
        private BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;
        private BaseVariable<Transform> configureMapInFullscreenMenu => DataStore.i.exploreV2.configureMapInFullscreenMenu;

        void Start()
        {
            closeButtonBehaviour = new NavmapCloseButtonBehaviour(closeButton, DataStore.i.HUDs.navmapVisible, DataStore.i.exploreV2.configureMapInFullscreenMenu);
            navmapVisibilityBehaviour = new NavmapVisibilityBehaviour(DataStore.i.HUDs.navmapVisible, scrollRect, scrollRectContentTransform, zoom, toastView);
            
            ConfigureMapInFullscreenMenuChanged(configureMapInFullscreenMenu.Get(), null);
            
            scrollRect.gameObject.SetActive(false);
            DataStore.i.HUDs.isNavMapInitialized.Set(true);
        }
        
        private void OnEnable()
        {
            configureMapInFullscreenMenu.OnChange += ConfigureMapInFullscreenMenuChanged;

            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            CommonScriptableObjects.playerCoords.OnChange += UpdateCurrentSceneData;
        }

        private void OnDisable()
        {
            configureMapInFullscreenMenu.OnChange -= ConfigureMapInFullscreenMenuChanged;

            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            CommonScriptableObjects.playerCoords.OnChange -= UpdateCurrentSceneData;
        }
        
        private void OnDestroy()
        {
            closeButtonBehaviour.Dispose();
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
        
        private void OnScrollValueChanged(Vector2 _)
        {
            if (!navmapVisible.Get())
                return;

            MapRenderer.i.atlas.UpdateCulling();
            toastView.Close();
        }

        private void UpdateCurrentSceneData(Vector2Int current, Vector2Int _)
        {
            const string format = "{0},{1}";
            currentSceneCoordsText.text = string.Format(format, current.x, current.y);
            currentSceneNameText.text = MinimapMetadata.GetMetadata().GetSceneInfo(current.x, current.y)?.name ?? "Unnamed";
        }
    }
}