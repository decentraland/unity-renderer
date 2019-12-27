using DCL.Helpers;
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

        public void ApplyAfterLoad(Transform t)
        {
            Renderer[] renderers = t.gameObject.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                renderer.enabled = visibleFlags == VisibleFlags.INVISIBLE ? false : true;
            }
        }
    }
}
