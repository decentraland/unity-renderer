using UnityEngine;

namespace DCL.Rendering
{
    [CreateAssetMenu(menuName = "Create CullingControllerSettingsData", fileName = "CullingControllerSettingsData", order = 0)]
    public class CullingControllerSettingsData : ScriptableObject
    {
        [Header("Renderer Profile")] 
        public CullingControllerProfile rendererProfileMin;
        public CullingControllerProfile rendererProfileMax;

        [Header("Skinned Renderer Profile")] 
        public CullingControllerProfile skinnedRendererProfileMin;
        public CullingControllerProfile skinnedRendererProfileMax;
    }
}