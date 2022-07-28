using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public class Visibility : IVisibility
    {
        internal readonly HashSet<string> globalConstrains = new HashSet<string>();
        internal readonly HashSet<string> combinedRendererConstrains = new HashSet<string>();
        internal readonly HashSet<string> facialFeaturesConstrains = new HashSet<string>();

        private Renderer combinedRenderer = null;
        private List<Renderer> facialFeatures = null;

        /// <summary>
        /// Bind a set of renderers, previous renderers enabled wont be modified
        /// </summary>
        /// <param name="combinedRenderer"></param>
        /// <param name="facialFeatures"></param>
        public void Bind(Renderer combinedRenderer, List<Renderer> facialFeatures)
        {
            this.combinedRenderer = combinedRenderer;
            this.facialFeatures = facialFeatures;
            UpdateCombinedRendererVisibility();
            UpdateFacialFeatureVisibility();
        }

        public void AddGlobalConstrain(string key)
        {
            globalConstrains.Add(key);
            UpdateCombinedRendererVisibility();
            UpdateFacialFeatureVisibility();
        }

        public void RemoveGlobalConstrain(string key)
        {
            globalConstrains.Remove(key);
            UpdateCombinedRendererVisibility();
            UpdateFacialFeatureVisibility();
        }

        public void AddCombinedRendererConstrain(string key)
        {
            combinedRendererConstrains.Add(key);
            UpdateCombinedRendererVisibility();
        }

        public void RemoveCombinedRendererConstrain(string key)
        {
            combinedRendererConstrains.Remove(key);
            UpdateCombinedRendererVisibility();
        }

        public void AddFacialFeaturesConstrain(string key)
        {
            facialFeaturesConstrains.Add(key);
            UpdateFacialFeatureVisibility();
        }

        public void RemoveFacialFeaturesConstrain(string key)
        {
            facialFeaturesConstrains.Remove(key);
            UpdateFacialFeatureVisibility();
        }
        
        public bool IsGloballyVisible()
        {
            return AreFacialFeaturesVisible() && IsMainRenderVisible();
        }
        
        public bool AreFacialFeaturesVisible()
        {
            return globalConstrains.Count == 0 && facialFeaturesConstrains.Count == 0;
        }
        
        public bool IsMainRenderVisible()
        {
            return globalConstrains.Count == 0 && combinedRendererConstrains.Count == 0;
        }

        internal void UpdateCombinedRendererVisibility()
        {
            if (combinedRenderer == null)
                return;

            combinedRenderer.enabled = globalConstrains.Count == 0 && combinedRendererConstrains.Count == 0;
        }

        internal void UpdateFacialFeatureVisibility()
        {
            if (facialFeatures == null)
                return;

            bool facialFeaturesVisibility = globalConstrains.Count == 0 && facialFeaturesConstrains.Count == 0;
            for (int i = 0; i < facialFeatures.Count; i++)
            {
                facialFeatures[i].enabled = facialFeaturesVisibility;
            }
        }

        public void Dispose()
        {
            globalConstrains.Clear();
            combinedRendererConstrains.Clear();
            facialFeaturesConstrains.Clear();

            combinedRenderer = null;
            facialFeatures = null;
        }
    }
}