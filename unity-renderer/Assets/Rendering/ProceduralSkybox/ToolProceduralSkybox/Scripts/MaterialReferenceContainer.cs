using UnityEngine;

namespace DCL.Skybox
{
    [CreateAssetMenu(fileName = "SkyboxMaterialData", menuName = "ScriptableObjects/SkyboxMaterialData", order = 1)]
    public class MaterialReferenceContainer : ScriptableObject
    {
        public Material skyboxMat;
        public int skyboxMatSlots = 5;
        public Material domeMat;

        public Material GetSkyboxMaterial() { return skyboxMat; }

        public Material GetDomeMaterial() { return domeMat; }

    }
}
