using UnityEngine;
using DCL;

namespace Builder
{
    public class DCLBuilderEnvironment : MonoBehaviour
    {

        const string groundGameObjectPath = "Environment/Ground/GroundVisual";

        public Material builderSkybox;
        public Material previewSkybox;

        private bool isGameObjectActive = false;
        private GameObject groundGameObject = null;

        private void Awake()
        {
            groundGameObject = InitialSceneReferences.i?.groundVisual;
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
            groundGameObject?.SetActive(isPreview);
            RenderSettings.fog = isPreview;
            Material skybox = isPreview ? previewSkybox : builderSkybox;
            RenderSettings.skybox = skybox;
            DynamicGI.UpdateEnvironment();
        }
    }
}