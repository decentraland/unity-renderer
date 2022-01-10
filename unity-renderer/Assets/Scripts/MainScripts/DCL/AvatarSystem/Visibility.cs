using UnityEngine;

namespace AvatarSystem
{
    public class Visibility : IVisibility
    {
        private Renderer combinedRenderer = null;
        private Renderer[] facialFeatures = null;

        private bool explicitVisibility = true;
        private bool loadingReady = true;
        private bool combinedRendererVisibility = true;
        private bool facialFeaturesVisibility = true;

        /// <summary>
        /// Bind a set of renderers, previous renderers enabled wont be modified
        /// </summary>
        /// <param name="combinedRenderer"></param>
        /// <param name="facialFeatures"></param>
        public void Bind(Renderer combinedRenderer, Renderer[] facialFeatures)
        {
            this.combinedRenderer = combinedRenderer;
            this.facialFeatures = facialFeatures;
            UpdateVisibility();
        }

        public void SetExplicitVisibility(bool explicitVisibility)
        {
            this.explicitVisibility = explicitVisibility;
            UpdateVisibility();
        }

        public void SetLoadingReady(bool loadingReady)
        {
            this.loadingReady = loadingReady;
            UpdateVisibility();
        }
        public void SetCombinedRendererVisibility(bool combinedRendererVisibility) { this.combinedRendererVisibility = combinedRendererVisibility; }
        public void SetFacialFeaturesVisibility(bool facialFeaturesVisibility) { this.facialFeaturesVisibility = facialFeaturesVisibility; }

        private void UpdateVisibility()
        {
            if (combinedRenderer != null)
            {
                bool combinedRendererComposedVisibility = explicitVisibility && loadingReady && combinedRendererVisibility;
                combinedRenderer.enabled = combinedRendererComposedVisibility;
            }

            if (facialFeatures != null)
            {
                bool facialFeaturesComposedVisibility = explicitVisibility && loadingReady && facialFeaturesVisibility;
                for (int i = 0; i < facialFeatures.Length; i++)
                {
                    facialFeatures[i].enabled = facialFeaturesComposedVisibility;
                }
            }
        }

        public void Dispose()
        {
            combinedRenderer = null;
            facialFeatures = null;
        }
    }
}