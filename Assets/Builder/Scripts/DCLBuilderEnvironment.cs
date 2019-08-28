using UnityEngine;

namespace Builder
{
    public class DCLBuilderEnvironment : MonoBehaviour
    {
        public Material builderSkybox;
        public Material previewSkybox;

        private bool isGameObjectActive = false;

        private void Awake()
        {
            OnPreviewModeChanged(false);
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            Material skybox = isPreview ? previewSkybox : builderSkybox;
            RenderSettings.skybox = skybox;
            DynamicGI.UpdateEnvironment();
        }
    }
}