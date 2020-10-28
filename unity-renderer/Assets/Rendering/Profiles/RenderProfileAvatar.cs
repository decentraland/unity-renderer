using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    /// <summary>
    /// This ScriptableObject is used to control toon shader and avatar rendering values
    /// assigned to any RenderProfileWorld.
    ///
    /// Values can change depending if we are in avatar editor mode or in-game world.
    /// </summary>
    [CreateAssetMenu(menuName = "DCL/Rendering/Create Avatar Profile", fileName = "RenderProfileAvatar", order = 0)]
    public class RenderProfileAvatar : ScriptableObject
    {
        [System.Serializable]
        public class MaterialProfile
        {
            [SerializeField] internal Color tintColor;
            [SerializeField] internal Color lightColor;
            [SerializeField] internal Vector3 lightDirection;
        }

        [NonSerialized] public MaterialProfile currentProfile;

        public MaterialProfile avatarEditor;

        public MaterialProfile inWorld;

        /// <summary>
        /// Applies the material profile.
        /// </summary>
        /// <param name="targetMaterial">If target material is null, the properties will be applied globally.</param>
        public void Apply(Material targetMaterial = null)
        {
            if (currentProfile == null)
                currentProfile = inWorld;

            if (targetMaterial != null)
            {
                targetMaterial.SetVector(ShaderUtils.LightDir, currentProfile.lightDirection);
                targetMaterial.SetColor(ShaderUtils.LightColor, currentProfile.lightColor);
                targetMaterial.SetColor(ShaderUtils.TintColor, currentProfile.tintColor);
                return;
            }

            Shader.SetGlobalVector(ShaderUtils.LightDir, currentProfile.lightDirection);
            Shader.SetGlobalColor(ShaderUtils.LightColor, currentProfile.lightColor);
            Shader.SetGlobalColor(ShaderUtils.TintColor, currentProfile.tintColor);
        }
    }
}