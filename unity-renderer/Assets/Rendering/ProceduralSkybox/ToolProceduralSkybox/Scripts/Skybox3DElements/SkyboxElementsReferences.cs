using DCL.Providers;
using System.Threading.Tasks;
using UnityEngine;

namespace DCL.Skybox
{
    public class SkyboxElementsReferences : MonoBehaviour
    {
        private const string PREFAB = "Skybox Elements.prefab";

        public GameObject domeElementsGO;
        public GameObject satelliteElementsGO;
        public GameObject planarElementsGO;

        private static Service<IAddressableResourceProvider> addresableResolver;

        public static async Task<SkyboxElementsReferences> Create()
        {
            GameObject prefabToInstantiate = await addresableResolver.Ref.GetAddressable<GameObject>(PREFAB);
            var refernces = Instantiate(prefabToInstantiate).GetComponent<SkyboxElementsReferences>();
            refernces.gameObject.name = "_Skybox Elements";
            return refernces;
        }
    }
}
