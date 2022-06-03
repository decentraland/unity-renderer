using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Skybox
{
    public class SatelliteReferences
    {
        public GameObject satelliteParent;
        public GameObject orbitGO;
        public GameObject satelliteGO;
        public GameObject satellitePrefab;
        public SatelliteLayerBehaviour satelliteBehavior;
    }

    public class SkyboxSatelliteElements
    {
        const string satelliteParentResourcePath = "SkyboxPrefabs/Satellite Parent";

        private readonly GameObject satelliteElements;
        private GameObject satelliteParentPrefab;
        private FollowBehaviour followBehavior;

        Dictionary<GameObject, Queue<SatelliteReferences>> satelliteReferences = new Dictionary<GameObject, Queue<SatelliteReferences>>();
        List<SatelliteReferences> usedSatellites = new List<SatelliteReferences>();

        public SkyboxSatelliteElements(GameObject satelliteElements)
        {
            this.satelliteElements = satelliteElements;
            Initialize();
        }

        private void Initialize()
        {
            if (satelliteElements == null)
            {
                Debug.LogError("Satellite Elements Container is null");
                return;
            }

            followBehavior = satelliteElements.GetOrCreateComponent<FollowBehaviour>();
            followBehavior.followPos = true;
            followBehavior.ignoreYAxis = true;
        }

        internal void ApplySatelliteConfigurations(SkyboxConfiguration config, float dayTime, float normalizedDayTime, Light directionalLightGO, float cycleTime)
        {
            ResetObjects();
            List<SatelliteReferences> satelliteRefs = GetSatelliteAllActiveSatelliteRefs(config.satelliteLayers);

            if (satelliteRefs.Count != config.satelliteLayers.Count)
            {
                Debug.LogWarning("Satellite not working!, cause prefab is not assigned");
                return;
            }

            for (int i = 0; i < satelliteRefs.Count; i++)
            {
                // If satellite is disabled, disable the 3D object too.
                if (!config.satelliteLayers[i].enabled)
                {
                    if (satelliteRefs[i] != null)
                    {
                        satelliteRefs[i].satelliteParent.SetActive(false);
                        satelliteRefs[i].satelliteBehavior.ChangeRenderType(LayerRenderType.NotRendering);
                    }
                    continue;
                }

                if (satelliteRefs[i] == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(config.satelliteLayers[i].nameInEditor + " Satellite not working!, prefab not assigned");
#endif
                    continue;
                }

                satelliteRefs[i].satelliteBehavior.AssignValues(config.satelliteLayers[i], dayTime, cycleTime);
            }
        }

        private void ResetObjects()
        {
            if (usedSatellites != null)
            {
                for (int i = 0; i < usedSatellites.Count; i++)
                {
                    SatelliteReferences sat = usedSatellites[i];
                    if (sat == null)
                    {
                        continue;
                    }
                    sat.satelliteParent.SetActive(false);
                    satelliteReferences[sat.satellitePrefab].Enqueue(sat);
                }
                usedSatellites.Clear();
            }
        }

        private List<SatelliteReferences> GetSatelliteAllActiveSatelliteRefs(List<Config3DSatellite> satelliteLayers)
        {
            for (int i = 0; i < satelliteLayers.Count; i++)
            {
                GetSatelliteObject(satelliteLayers[i]);
            }
            return usedSatellites;
        }

        private SatelliteReferences GetSatelliteObject(Config3DSatellite config)
        {
            SatelliteReferences tempSatellite = null;

            if (config.satellite == null)
            {
                usedSatellites.Add(tempSatellite);
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

        private SatelliteReferences InstantiateNewSatelliteReference(Config3DSatellite config)
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
            satelliteObj.transform.parent = obj.transform;

            // Get satellite behavior and assign satellite 
            SatelliteLayerBehaviour satelliteBehavior = obj.GetComponent<SatelliteLayerBehaviour>();
            satelliteBehavior.satellite = satelliteObj;

            SatelliteReferences satellite = new SatelliteReferences();
            satellite.satelliteParent = obj;
            satellite.orbitGO = orbit;
            satellite.satelliteGO = satelliteObj;
            satellite.satellitePrefab = config.satellite;
            satellite.satelliteBehavior = satelliteBehavior;

            return satellite;
        }

        public void ResolveCameraDependency(Transform currentTransform)
        {
            if (followBehavior != null)
            {
                followBehavior.target = currentTransform.gameObject;
            }
        }
    }
}