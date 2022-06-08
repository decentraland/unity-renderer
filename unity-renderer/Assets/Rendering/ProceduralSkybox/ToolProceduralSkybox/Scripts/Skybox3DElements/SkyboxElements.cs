using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace DCL.Skybox
{
    public class SkyboxElements : IDisposable
    {
        public SkyboxDomeElements domeElements;
        public SkyboxSatelliteElements satelliteElements;
        public SkyboxPlanarElements planarElements;
        public SkyboxElementsReferences references;

        public SkyboxElements()
        {
            references = SkyboxElementsReferences.Create();

            domeElements = new SkyboxDomeElements(references.domeElementsGO);
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