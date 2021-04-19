using UnityEngine;
using DCL;

namespace Builder
{
    public class DCLBuilderEnvironment : MonoBehaviour
    {
        public Material builderSkybox;
        public Material previewSkybox;

        private bool isGameObjectActive = false;
        private Renderer[] groundRenderers = null;

        private void Awake()
        {
            GameObject groundGameObject = InitialSceneReferences.i?.groundVisual;
            if (groundGameObject)
            {
                groundRenderers = groundGameObject.GetComponentsInChildren<Renderer>();
            }
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
            if (groundRenderers != null)
            {
                for (int i = 0; i < groundRenderers.Length; i++)
                {
                    groundRenderers[i].enabled = isPreview;
                }
            }
            RenderSettings.fog = isPreview;
            Material skybox = isPreview ? previewSkybox : builderSkybox;
            RenderSettings.skybox = skybox;
            DynamicGI.UpdateEnvironment();
        }
    }
}