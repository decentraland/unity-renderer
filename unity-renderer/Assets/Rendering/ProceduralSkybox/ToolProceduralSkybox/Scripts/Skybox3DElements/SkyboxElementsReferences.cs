using UnityEngine;

namespace DCL.Skybox
{
    public class SkyboxElementsReferences : MonoBehaviour
    {
        private const string PREFAB = "SkyboxPrefabs/Skybox Elements";

        public GameObject domeElementsGO;
        public GameObject satelliteElementsGO;
        public GameObject planarElementsGO;

        public static SkyboxElementsReferences Create()
        {
            var refernces = Instantiate(Resources.Load<GameObject>(PREFAB)).GetComponent<SkyboxElementsReferences>();
            refernces.gameObject.name = "_Skybox Elements";
            return refernces;
        }
    }
}