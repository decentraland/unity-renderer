using UnityEngine;

namespace AvatarSystem
{
    public class Visibility : IVisibility
    {
        private Renderer[] renderers = null;

        private bool explicitVisibility = true;
        private bool loadingReady = true;

        /// <summary>
        /// Bind a set of renderers, previous renderers will be left as they are 
        /// </summary>
        /// <param name="renderers"></param>
        public void Bind(Renderer[] renderers)
        {
            this.renderers = renderers;
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

        private void UpdateVisibility()
        {
            if (renderers == null)
                return;

            bool visibility = loadingReady && explicitVisibility;
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = visibility;
            }
        }

        public void Dispose()
        {
            renderers = null;
        }
    }
}