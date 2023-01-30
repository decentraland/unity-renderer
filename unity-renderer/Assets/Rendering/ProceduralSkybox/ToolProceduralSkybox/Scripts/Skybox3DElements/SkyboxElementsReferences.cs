using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Threading;
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
            Debug.Log($"STARTING TO LOAD {PREFAB}");
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(5));
            GameObject prefabToInstantiate = await addresableResolver.Ref.GetAddressable<GameObject>(PREFAB, cts.Token);
            Debug.Log($"LOADED {PREFAB}");
            var refernces = Instantiate(prefabToInstantiate).GetComponent<SkyboxElementsReferences>();
            refernces.gameObject.name = "_Skybox Elements";
            return refernces;
        }
    }
}
