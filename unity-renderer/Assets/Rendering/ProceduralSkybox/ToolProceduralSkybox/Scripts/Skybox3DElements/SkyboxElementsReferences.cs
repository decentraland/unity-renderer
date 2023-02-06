using Cysharp.Threading.Tasks;
using DCL.Providers;
using System.Threading;
using UnityEngine;

namespace DCL.Skybox
{
    public class SkyboxElementsReferences : MonoBehaviour
    {
        private const string PREFAB = "SkyboxElements.prefab";

        public GameObject domeElementsGO;
        public GameObject satelliteElementsGO;
        public GameObject planarElementsGO;

        public static async UniTask<SkyboxElementsReferences> Create(IAddressableResourceProvider addressableResourceProvider, CancellationToken ct)
        {
            GameObject prefabToInstantiate = await addressableResourceProvider.GetAddressable<GameObject>(PREFAB, ct);
            var refernces = Instantiate(prefabToInstantiate).GetComponent<SkyboxElementsReferences>();
            return refernces;
        }
    }
}
