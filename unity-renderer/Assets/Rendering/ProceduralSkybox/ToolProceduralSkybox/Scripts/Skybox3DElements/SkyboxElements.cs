using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{

    public class SkyboxElements
    {
        private GameObject skyboxElementsGO;
        public SkyboxDomeElements domeElements;
        public SkyboxSatelliteElements satelliteElements;

        public SkyboxElements()
        {
            skyboxElementsGO = GameObject.Find("Skybox Elements");
            if (skyboxElementsGO == null)
            {
                skyboxElementsGO = new GameObject("Skybox Elements");
                skyboxElementsGO.layer = LayerMask.NameToLayer("Skybox");
                skyboxElementsGO.transform.position = Vector3.zero;
            }

            domeElements = new SkyboxDomeElements(skyboxElementsGO);
            satelliteElements = new SkyboxSatelliteElements(skyboxElementsGO);
        }

        public void ApplyConfigTo3DElements(SkyboxConfiguration config, float dayTime, float normalizedDayTime, Light directionalLightGO = null, float cycleTime = 24, bool isEditor = false)
        {
            domeElements.ApplyDomeConfigurations(config, dayTime, normalizedDayTime, directionalLightGO, cycleTime);
            satelliteElements.ApplySatelliteConfigurations(config, dayTime, normalizedDayTime, directionalLightGO, cycleTime);
        }

        internal void AssignCameraInstance(Transform currentTransform)
        {
            domeElements.ResolveCameraDependency(currentTransform);
            satelliteElements.ResolveCameraDependency(currentTransform);
        }
    }
}