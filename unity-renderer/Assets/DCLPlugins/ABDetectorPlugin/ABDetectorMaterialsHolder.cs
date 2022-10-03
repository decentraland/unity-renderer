using UnityEngine;

namespace DCL
{
    public class ABDetectorMaterialsHolder : MonoBehaviour
    {
        [SerializeField] private Material abMaterial;
        [SerializeField] private Material gltfMaterial;

        public Material ABMaterial => abMaterial;
        public Material GLTFMaterial => gltfMaterial;
    }
}