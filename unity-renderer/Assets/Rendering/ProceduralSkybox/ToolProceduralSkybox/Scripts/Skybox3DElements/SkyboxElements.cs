using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCL.Skybox
{
    public class SkyboxElements : IDisposable
    {
        public SkyboxDomeElements domeElements;
        public SkyboxSatelliteElements satelliteElements;
        public SkyboxPlanarElements planarElements;
        public SkyboxElementsReferences references;

        public async UniTask Initialize(IAddressableResourceProvider addresableResolver, MaterialReferenceContainer materialReferenceContainer, CancellationToken ct = default)
        {
            references = await SkyboxElementsReferences.Create(addresableResolver, ct);

            domeElements = new SkyboxDomeElements(references.domeElementsGO, materialReferenceContainer);
            satelliteElements = new SkyboxSatelliteElements(references.satelliteElementsGO);
            planarElements = new SkyboxPlanarElements(references.planarElementsGO);
        }

        public void ApplyConfigTo3DElements(SkyboxConfiguration config, float dayTime, float normalizedDayTime, Light directionalLightGO = null, float cycleTime = 24, bool isEditor = false)
        {
            domeElements.ApplyDomeConfigurations(config, dayTime, normalizedDayTime, directionalLightGO, cycleTime);
            satelliteElements.ApplySatelliteConfigurations(config, dayTime, normalizedDayTime, directionalLightGO, cycleTime);
            planarElements.ApplyConfig(config.planarLayers, dayTime, cycleTime, isEditor);
        }

        internal void AssignCameraInstance(Transform currentTransform)
        {
            if (currentTransform == null)
                return;

            domeElements.ResolveCameraDependency(currentTransform);
            satelliteElements.ResolveCameraDependency(currentTransform);
            planarElements.ResolveCameraDependency(currentTransform);
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(references.gameObject);
#else
            UnityEngine.Object.Destroy(references.gameObject);
#endif
            references = null;
        }


    }
}
