using UnityEngine;

namespace DCL
{
    public class DebugView : MonoBehaviour
    {
        [Header("Debug Tools")] [SerializeField]
        private GameObject fpsPanel;

        [Header("Debug Panel")] [SerializeField]
        private GameObject engineDebugPanel;

        [SerializeField] private InfoPanel infoPanel;

        private HUDCanvasCameraModeController hudCanvasCameraModeController;

        private void Awake()
        {
            infoPanel.SetVisible(false);
            hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);
        }

        private void OnDestroy() { hudCanvasCameraModeController?.Dispose(); }

        public void ShowFPSPanel()
        {
            fpsPanel.SetActive(true);
        }

        public void HideFPSPanel()
        {
            fpsPanel.SetActive(false);
        }

        public void SetSceneDebugPanel()
        {
            engineDebugPanel.SetActive(false);
        }

        public void SetEngineDebugPanel()
        {
            engineDebugPanel.SetActive(true);
        }

        public void ShowInfoPanel(string network, string realm)
        {
            infoPanel.Setup(network, realm);
            infoPanel.SetVisible(true);
        }

        public void HideInfoPanel()
        {
            infoPanel.SetVisible(false);
        }

        public void SetRealm(string realm)
        {
            infoPanel.SetRealm(realm);
        }
    }
}