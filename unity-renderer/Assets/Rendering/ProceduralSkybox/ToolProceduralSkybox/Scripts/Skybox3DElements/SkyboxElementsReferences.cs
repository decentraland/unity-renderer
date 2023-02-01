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
        private const string PREFAB = "SkyboxElements.prefab";

        public GameObject domeElementsGO;
        public GameObject satelliteElementsGO;
        public GameObject planarElementsGO;

        public static async Task<SkyboxElementsReferences> Create(IAddressableResourceProvider addressableResourceProvider)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(15));
            GameObject prefabToInstantiate = await addressableResourceProvider.GetAddressable<GameObject>(PREFAB, cts.Token);
            var refernces = Instantiate(prefabToInstantiate).GetComponent<SkyboxElementsReferences>();
            return refernces;
        }
    }
}
