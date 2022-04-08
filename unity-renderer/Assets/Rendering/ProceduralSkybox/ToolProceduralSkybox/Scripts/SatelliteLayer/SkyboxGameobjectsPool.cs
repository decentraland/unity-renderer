using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public class DomeReferences
    {
        public GameObject domeGO;
        public Material domeMat;
        public bool domeInUse;
    }

    public class SatelliteReferences
    {
        public GameObject satelliteParent;
        public GameObject orbitGO;
        public GameObject satelliteGO;
        public GameObject satellitePrefab;
        public SatelliteLayerBehavior satelliteBehavior;
    }

    public class SkyboxGameobjectsPool
    {
        const float domeDefaultSize = 50;
        const string domeResourcesPath = "SkyboxPrefabs/Dome";
        const string satelliteParentResourcePath = "SkyboxPrefabs/Satellite Parent";

        private GameObject skyboxElements;
        private GameObject satelliteElements;
        private GameObject satelliteParentPrefab;

        SatelliteReferences satelliteReference;

        public void Initialize3DObjects(SkyboxConfiguration configuration)
        {
            skyboxElements = GameObject.Find("Skybox Elements");

            if (skyboxElements != null)
            {
                Object.DestroyImmediate(skyboxElements);
            }

            skyboxElements = new GameObject("Skybox Elements");
            skyboxElements.layer = LayerMask.NameToLayer("Skybox");

            MakeElementObject();
        }

        void MakeElementObject()
        {
            satelliteElements = new GameObject("Satellite Elements");
            satelliteElements.layer = LayerMask.NameToLayer("Skybox");
            satelliteElements.transform.parent = skyboxElements.transform;

            FollowBehavior followObj = satelliteElements.AddComponent<FollowBehavior>();
            followObj.followPos = true;
        }

        public void ResetObjects()
        {
            if (satelliteReference != null)
            {
                satelliteReference.satelliteParent.SetActive(false);
            }
        }

        #region Satellite

        public SatelliteReferences GetSatelliteObject(SkyboxConfiguration config)
        {
            ResetObjects();

            if (config.satelliteLayer.satellite == null)
            {
                if (satelliteReference != null)
                {
                    satelliteReference.satelliteParent.SetActive(false);
                }
                return null;
            }

            if (satelliteReference == null)
            {
                satelliteReference = InstantiateNewSatelliteReference(config);
            }
            satelliteReference.satelliteParent.SetActive(true);

            return satelliteReference;
        }

        SatelliteReferences InstantiateNewSatelliteReference(SkyboxConfiguration config)
        {
            if (satelliteParentPrefab == null)
            {
                satelliteParentPrefab = Resources.Load<GameObject>(satelliteParentResourcePath);
            }

            GameObject obj = GameObject.Instantiate<GameObject>(satelliteParentPrefab);
            obj.layer = LayerMask.NameToLayer("Skybox");
            obj.name = "Satellite Parent";
            obj.transform.parent = satelliteElements.transform;
            obj.transform.localPosition = Vector3.zero;

            GameObject orbit = obj.transform.GetChild(0).gameObject;
            GameObject satelliteObj = GameObject.Instantiate(config.satelliteLayer.satellite);
            satelliteObj.transform.parent = orbit.transform;

            // Get satellite behavior and assign satellite 
            SatelliteLayerBehavior satelliteBehavior = obj.GetComponent<SatelliteLayerBehavior>();
            satelliteBehavior.satellite = satelliteObj;

            SatelliteReferences satellite = new SatelliteReferences();
            satellite.satelliteParent = obj;
            satellite.orbitGO = orbit;
            satellite.satelliteGO = satelliteObj;
            satellite.satellitePrefab = config.satelliteLayer.satellite;
            satellite.satelliteBehavior = satelliteBehavior;

            return satellite;
        }

        internal void ResolveCameraDependency()
        {
            if (satelliteElements != null)
            {
                satelliteElements.GetComponent<FollowBehavior>().target = Camera.main.gameObject;
            }
        }

        #endregion

    }
}