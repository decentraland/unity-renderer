using UnityEngine;

namespace DCL
{
    public class ABDetectorMaterialsHolder : MonoBehaviour
    {
        [SerializeField] private Material abMaterial;
        [SerializeField] private Material gltfMaterial;
        [SerializeField] private Material parametrizedShapeMaterial;

        public Material ABMaterial => abMaterial;
        public Material GLTFMaterial => gltfMaterial;
        public Material ParametrizedShapeMaterial => parametrizedShapeMaterial;
    }
}
