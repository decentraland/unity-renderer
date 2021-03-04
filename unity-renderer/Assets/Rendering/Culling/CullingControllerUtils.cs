using DCL.Helpers;
using UnityEngine;

namespace DCL.Rendering
{
    public static class CullingControllerUtils
    {
        /// <summary>
        /// Computes the rule used for toggling skinned meshes updateWhenOffscreen param.
        /// Skinned meshes should be always updated if near the camera to avoid false culling positives on screen edges.
        /// </summary>
        /// <param name="settings">Any settings object to use thresholds for computing the rule.</param>
        /// <param name="distanceToCamera">Mesh distance from camera used for computing the rule.</param>
        /// <returns>True if mesh should be updated when offscreen, false if otherwise.</returns>
        internal static bool TestSkinnedRendererOffscreenRule(CullingControllerSettings settings, float distanceToCamera)
        {
            bool finalValue = true;

            if (settings.enableAnimationCulling)
            {
                if (distanceToCamera > settings.enableAnimationCullingDistance)
                    finalValue = false;
            }

            return finalValue;
        }

        /// <summary>
        /// Computes the rule used for toggling renderers visibility.
        /// </summary>
        /// <param name="profile">Profile used for size and distance thresholds needed for the rule.</param>
        /// <param name="viewportSize">Diagonal viewport size of the renderer.</param>
        /// <param name="distanceToCamera">Distance to camera of the renderer.</param>
        /// <param name="boundsContainsCamera">Renderer bounds contains camera?</param>
        /// <param name="isOpaque">Renderer is opaque?</param>
        /// <param name="isEmissive">Renderer is emissive?</param>
        /// <returns>True if renderer should be visible, false if otherwise.</returns>
        internal static bool TestRendererVisibleRule(CullingControllerProfile profile, float viewportSize, float distanceToCamera, bool boundsContainsCamera, bool isOpaque, bool isEmissive)
        {
            bool shouldBeVisible = distanceToCamera < profile.visibleDistanceThreshold || boundsContainsCamera;

            if (isEmissive)
                shouldBeVisible |= viewportSize > profile.emissiveSizeThreshold;

            if (isOpaque)
                shouldBeVisible |= viewportSize > profile.opaqueSizeThreshold;

            return shouldBeVisible;
        }

        /// <summary>
        /// Computes the rule used for toggling renderer shadow casting.
        /// </summary>
        /// <param name="profile">Profile used for size and distance thresholds needed for the rule.</param>
        /// <param name="viewportSize">Diagonal viewport size of the renderer.</param>
        /// <param name="distanceToCamera">Distance from renderer to camera.</param>
        /// <param name="shadowMapTexelSize">Shadow map bounding box size in texels.</param>
        /// <returns>True if renderer should have shadow, false otherwise.</returns>
        internal static bool TestRendererShadowRule(CullingControllerProfile profile, float viewportSize, float distanceToCamera, float shadowMapTexelSize)
        {
            bool shouldHaveShadow = distanceToCamera < profile.shadowDistanceThreshold;
            shouldHaveShadow |= viewportSize > profile.shadowRendererSizeThreshold;
            shouldHaveShadow &= shadowMapTexelSize > profile.shadowMapProjectionSizeThreshold;
            return shouldHaveShadow;
        }

        /// <summary>
        /// Determines if the given renderer is going to be enqueued at the opaque section of the rendering pipeline.
        /// </summary>
        /// <param name="renderer">Renderer to be checked.</param>
        /// <returns>True if its opaque</returns>
        internal static bool IsOpaque(Renderer renderer)
        {
            Material firstMat = renderer.sharedMaterials[0];

            if (firstMat == null)
                return true;

            if (firstMat.HasProperty(ShaderUtils.ZWrite) &&
                (int) firstMat.GetFloat(ShaderUtils.ZWrite) == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the given renderer has emissive material traits.
        /// </summary>
        /// <param name="renderer">Renderer to be checked.</param>
        /// <returns>True if the renderer is emissive.</returns>
        internal static bool IsEmissive(Renderer renderer)
        {
            Material firstMat = renderer.sharedMaterials[0];

            if (firstMat == null)
                return false;

            if (firstMat.HasProperty(ShaderUtils.EmissionMap) && firstMat.GetTexture(ShaderUtils.EmissionMap) != null)
                return true;

            if (firstMat.HasProperty(ShaderUtils.EmissionColor) && firstMat.GetColor(ShaderUtils.EmissionColor) != Color.clear)
                return true;

            return false;
        }

        /// <summary>
        /// ComputeShadowMapTexelSize computes the shadow-map bounding box diagonal texel size
        /// for the given bounds size.
        /// </summary>
        /// <param name="boundsSize">Diagonal bounds size of the object</param>
        /// <param name="shadowDistance">Shadow distance as set in the quality settings</param>
        /// <param name="shadowMapRes">Shadow map resolution as set in the quality settings (128, 256, etc)</param>
        /// <returns>The computed shadow map diagonal texel size for the object.</returns>
        /// <remarks>
        /// This is calculated by doing the following:
        /// 
        /// - We get the boundsSize to a normalized viewport size.
        /// - We multiply the resulting value by the shadow map resolution.
        /// 
        /// To get the viewport size, we assume the shadow distance value is directly correlated by
        /// the orthogonal projection size used for rendering the shadow map.
        /// 
        /// We can use the bounds size and shadow distance to obtain the normalized shadow viewport
        /// value because both are expressed in world units.
        /// 
        /// After getting the normalized size, we scale it by the shadow map resolution to get the
        /// diagonal texel size of the bounds shadow.
        /// 
        /// This leaves us with:
        ///     <c>shadowTexelSize = boundsSize / shadow dist * shadow res</c>
        /// 
        /// This is a lazy approximation and most likely will need some refinement in the future.
        /// </remarks>
        internal static float ComputeShadowMapTexelSize(float boundsSize, float shadowDistance, float shadowMapRes)
        {
            return boundsSize / shadowDistance * shadowMapRes;
        }

        public static void DrawBounds(Bounds b, Color color, float delay = 0)
        {
            // bottom
            var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
            var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
            var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
            var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

            Debug.DrawLine(p1, p2, color, delay);
            Debug.DrawLine(p2, p3, color, delay);
            Debug.DrawLine(p3, p4, color, delay);
            Debug.DrawLine(p4, p1, color, delay);

            // top
            var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
            var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
            var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
            var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

            Debug.DrawLine(p5, p6, color, delay);
            Debug.DrawLine(p6, p7, color, delay);
            Debug.DrawLine(p7, p8, color, delay);
            Debug.DrawLine(p8, p5, color, delay);

            // sides
            Debug.DrawLine(p1, p5, color, delay);
            Debug.DrawLine(p2, p6, color, delay);
            Debug.DrawLine(p3, p7, color, delay);
            Debug.DrawLine(p4, p8, color, delay);
        }
    }
}