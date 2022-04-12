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

        Dictionary<GameObject, Queue<SatelliteReferences>> satelliteReferences = new Dictionary<GameObject, Queue<SatelliteReferences>>();
        List<SatelliteReferences> usedSatellites = new List<SatelliteReferences>();

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
            if (usedSatellites != null)
            {
                for (int i = 0; i < usedSatellites.Count; i++)
                {
                    SatelliteReferences sat = usedSatellites[i];
                    sat.satelliteParent.SetActive(false);
                    satelliteReferences[sat.satellitePrefab].Enqueue(sat);
                }
                usedSatellites.Clear();
            }
        }

        #region Satellite

        public List<SatelliteReferences> GetSatelliteAllActiveSatelliteRefs(List<Satellite3DLayer> satelliteLayers)
        {
            for (int i = 0; i < satelliteLayers.Count; i++)
            {
                GetSatelliteObject(satelliteLayers[i]);
            }
            return usedSatellites;
        }

        public SatelliteReferences GetSatelliteObject(Satellite3DLayer config)
        {
            SatelliteReferences tempSatellite = null;

            if (config.satellite == null)
            {
                return tempSatellite;
            }

            // Check if GO for this prefab is already in scene, else create new
            if (satelliteReferences.ContainsKey(config.satellite))
            {
                // Check if there is any unused GO for the given prefab
                if (satelliteReferences[config.satellite].Count > 0)
                {
                    tempSatellite = satelliteReferences[config.satellite].Dequeue();
                }
                else
                {
                    tempSatellite = InstantiateNewSatelliteReference(config);
                }
            }
            else
            {
                satelliteReferences.Add(config.satellite, new Queue<SatelliteReferences>());
                tempSatellite = InstantiateNewSatelliteReference(config);
            }

            usedSatellites.Add(tempSatellite);
            tempSatellite.satelliteParent.SetActive(true);

            return tempSatellite;
        }

        SatelliteReferences InstantiateNewSatelliteReference(Satellite3DLayer config)
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
            GameObject satelliteObj = GameObject.Instantiate(config.satellite);
            satelliteObj.transform.parent = orbit.transform;

            // Get satellite behavior and assign satellite 
            SatelliteLayerBehavior satelliteBehavior = obj.GetComponent<SatelliteLayerBehavior>();
            satelliteBehavior.satellite = satelliteObj;

            SatelliteReferences satellite = new SatelliteReferences();
            satellite.satelliteParent = obj;
            satellite.orbitGO = orbit;
            satellite.satelliteGO = satelliteObj;
            satellite.satellitePrefab = config.satellite;
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