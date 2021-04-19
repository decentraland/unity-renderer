using UnityEngine;

namespace DCL.Rendering
{
    /// <summary>
    /// Group of arguments for configuring the rules of a group of renderers of skinned renderers.
    /// Used by CullingControllerSettings.
    /// </summary>
    [System.Serializable]
    public class CullingControllerProfile
    {
        /// <summary>
        /// Less than this distance from camera: should be always visible, regardless other checks
        /// </summary>
        public float visibleDistanceThreshold;

        /// <summary>
        /// Less than this distance from camera: should always have shadow, regardless other checks
        /// </summary>
        public float shadowDistanceThreshold;

        /// <summary>
        /// Avatars with more than this distance from camera: should never have shadow, regardless other checks
        /// </summary>
        public float maxShadowDistanceForAvatars;

        /// <summary>
        /// Emissive and bigger than this, should be visible
        /// </summary>
        public float emissiveSizeThreshold;

        /// <summary>
        /// Opaque and bigger than this, should be visible
        /// </summary>
        public float opaqueSizeThreshold;

        /// <summary>
        /// Bigger than this size: has shadow
        /// </summary>
        public float shadowRendererSizeThreshold;

        /// <summary>
        /// Bigger than this size: has shadow
        /// </summary>
        public float shadowMapProjectionSizeThreshold; //

        /// <summary>
        /// Performs a linear interpolation between the values of two CullingControllerProfiles.
        /// Used for controlling the settings panel slider.
        /// </summary>
        /// <param name="p1">Starting profile</param>
        /// <param name="p2">Ending profile</param>
        /// <param name="t">Time value for the linear interpolation.</param>
        /// <returns>A new CullingControllerProfile with the interpolated values.</returns>
        public static CullingControllerProfile Lerp(CullingControllerProfile p1, CullingControllerProfile p2, float t)
        {
            return new CullingControllerProfile
            {
                visibleDistanceThreshold = Mathf.Lerp(p1.visibleDistanceThreshold, p2.visibleDistanceThreshold, t),
                shadowDistanceThreshold = Mathf.Lerp(p1.shadowDistanceThreshold, p2.shadowDistanceThreshold, t),
                emissiveSizeThreshold = Mathf.Lerp(p1.emissiveSizeThreshold, p2.emissiveSizeThreshold, t),
                opaqueSizeThreshold = Mathf.Lerp(p1.opaqueSizeThreshold, p2.opaqueSizeThreshold, t),
                shadowRendererSizeThreshold = Mathf.Lerp(p1.shadowRendererSizeThreshold, p2.shadowRendererSizeThreshold, t),
                shadowMapProjectionSizeThreshold = Mathf.Lerp(p1.shadowMapProjectionSizeThreshold, p2.shadowMapProjectionSizeThreshold, t),
                maxShadowDistanceForAvatars = Mathf.Lerp(p1.maxShadowDistanceForAvatars, p2.maxShadowDistanceForAvatars, t)
            };
        }

        /// <summary>
        /// Returns a clone of this object.
        /// </summary>
        /// <returns>The clone.</returns>
        public CullingControllerProfile Clone()
        {
            return this.MemberwiseClone() as CullingControllerProfile;
        }
    }
}