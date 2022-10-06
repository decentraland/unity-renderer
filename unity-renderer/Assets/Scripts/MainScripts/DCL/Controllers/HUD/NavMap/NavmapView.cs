using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapView : MonoBehaviour
    {
        [SerializeField] internal Button closeButton;
        [SerializeField] internal ScrollRect scrollRect;

        [Space]
        [SerializeField] internal TextMeshProUGUI currentSceneNameText;
        [SerializeField] internal TextMeshProUGUI currentSceneCoordsText;

        [Space]
        [SerializeField] internal NavmapToastView toastView;

        private BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;
        private BaseVariable<Transform> configureMapInFullscreenMenu => DataStore.i.exploreV2.configureMapInFullscreenMenu;

        private void OnEnable()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            configureMapInFullscreenMenu.OnChange += HideCloseButton;

            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            CommonScriptableObjects.playerCoords.OnChange += UpdateCurrentSceneData;
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            configureMapInFullscreenMenu.OnChange -= HideCloseButton;
            
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            CommonScriptableObjects.playerCoords.OnChange -= UpdateCurrentSceneData;
        }

        private void OnCloseButtonClicked() => navmapVisible.Set(false);

        private void HideCloseButton(Transform currentParentTransform, Transform _)
        {
            if (currentParentTransform != null)
                closeButton.gameObject.SetActive(false);
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