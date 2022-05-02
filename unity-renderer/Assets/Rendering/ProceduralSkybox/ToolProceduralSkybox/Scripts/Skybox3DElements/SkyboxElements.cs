using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    /// <summary>
    /// This class Initialize and maintain 3D elements for Procedural skybox
    /// </summary>
    public class SkyboxElements
    {
        private GameObject skyboxElementGO;
        private Planar3DElements planarElements;

        public SkyboxElements()
        {
            // Get or instantiate Skybox elements GameObject
            GetOrInstantiateSkyboxElements();

            // Initialize Planar 3D Layer Object
            planarElements = new Planar3DElements(skyboxElementGO);
        }

        private void GetOrInstantiateSkyboxElements()
        {
            skyboxElementGO = GameObject.Find("Skybox Elements");

            if (skyboxElementGO != null)
            {
                return;
            }

            skyboxElementGO = new GameObject("Skybox Elements");
            skyboxElementGO.layer = LayerMask.NameToLayer("Skybox");
        }

        public void ApplySkyboxElements(SkyboxConfiguration config, float timeOfTheDay, float cycleTime, bool isEditor) { planarElements.ApplyConfig(config.planarLayers, timeOfTheDay, cycleTime, isEditor); }
    }
}