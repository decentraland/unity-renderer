using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AssetPromiseSettings_Rendering : IAssetPromiseSettings<Transform>
    {
        public enum VisibleFlags
        {
            INVISIBLE,
            VISIBLE_WITHOUT_TRANSITION,
            VISIBLE_WITH_TRANSITION
        }

        public VisibleFlags visibleFlags = VisibleFlags.VISIBLE_WITH_TRANSITION;
        public MaterialCachingHelper.Mode cachingFlags = MaterialCachingHelper.Mode.CACHE_EVERYTHING;
        public Shader shaderOverride;

        public Transform parent;
        public Vector3? initialLocalPosition;
        public Quaternion? initialLocalRotation;
        public Vector3? initialLocalScale;

        public bool forceNewInstance;

        public void ApplyBeforeLoad(Transform t)
        {
            if (parent != null)
            {
                t.SetParent(parent, false);
                t.ResetLocalTRS();
            }

            if (initialLocalPosition.HasValue)
            {
                t.localPosition = initialLocalPosition.Value;
            }

            if (initialLocalRotation.HasValue)
            {
                t.localRotation = initialLocalRotation.Value;
            }

            if (initialLocalScale.HasValue)
            {
                t.localScale = initialLocalScale.Value;
            }
        }

        public void ApplyAfterLoad(Transform transform)
        {
            ApplyAfterLoad(new List<Renderer>(transform.GetComponentsInChildren<Renderer>(true)));
        }

        public void ApplyAfterLoad(List<Renderer> renderers = null)
        {
            int renderersCount = renderers.Count;
            for (int i = 0; i < renderersCount; i++)
            {
                Renderer renderer = renderers[i];
                renderer.enabled = visibleFlags == VisibleFlags.INVISIBLE ? false : true;
            }
        }
    }
}
