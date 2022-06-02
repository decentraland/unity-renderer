using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public class PlanarRefs
    {
        public GameObject prefab;
        public GameObject planarObject;
        public bool inUse = false;
    }

    public class SkyboxPlanarElements
    {
        private const string LAYER = "Skybox";

        private readonly GameObject planarElements;
        private Transform planarCameraFollowingElements;
        private Transform planarStaticElements;
        private FollowBehaviour followObj;

        private Dictionary<GameObject, Queue<PlanarRefs>> planarReferences = new Dictionary<GameObject, Queue<PlanarRefs>>();
        private List<PlanarRefs> usedPlanes = new List<PlanarRefs>();

        public SkyboxPlanarElements(GameObject planarElements)
        {
            this.planarElements = planarElements;
            // Get or instantiate Skybox elements GameObject
            Initialize();
        }

        private void Initialize()
        {
            planarCameraFollowingElements = new GameObject("Camera Following").transform;
            planarCameraFollowingElements.parent = planarElements.transform;
            planarCameraFollowingElements.gameObject.layer = LayerMask.NameToLayer(LAYER);

            planarStaticElements = new GameObject("Static").transform;
            planarStaticElements.parent = planarElements.transform;
            planarStaticElements.gameObject.layer = LayerMask.NameToLayer(LAYER);

            // Add follow script
            followObj = planarCameraFollowingElements.gameObject.AddComponent<FollowBehaviour>();
            followObj.ignoreYAxis = true;
            followObj.followPos = true;
        }

        internal void ResolveCameraDependency(Transform cameraTransform) { followObj.target = cameraTransform.gameObject; }

        internal void ApplyConfig(List<Config3DPlanar> planarLayers, float timeOfTheDay, float cycleTime, bool isEditor)
        {
            if (isEditor || (usedPlanes == null))
            {
                ResetPlanes();
                GetAllEnabledPlanarRefs(planarLayers);
            }

            for (int i = 0; i < usedPlanes.Count; i++)
            {
                // If satellite is disabled, disable the 3D object too.
                if (!planarLayers[i].enabled)
                {
                    if (usedPlanes[i] != null)
                    {
                        usedPlanes[i].planarObject.SetActive(false);
                    }
                    continue;
                }

                if (usedPlanes[i] == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(planarLayers[i].nameInEditor + " Plane not working!, prefab not assigned");
#endif
                    continue;
                }

                ApplyParticleProperties(planarLayers[i], usedPlanes[i]);

                // Parent with the moving or static parent
                ParentPlanarLayer(planarLayers[i], usedPlanes[i]);

                // Change layer mask
                ChangeRenderingCamera(planarLayers[i], usedPlanes[i]);

                // if this layer is not active at present time, disable the object
                if (!IsLayerActiveInCurrentTime(timeOfTheDay, planarLayers[i], cycleTime))
                {
                    planarLayers[i].renderType = LayerRenderType.NotRendering;
                    usedPlanes[i].planarObject.SetActive(false);
                    continue;
                }

            }
        }

        public List<PlanarRefs> GetAllEnabledPlanarRefs(List<Config3DPlanar> planarLayers)
        {
            for (int i = 0; i < planarLayers.Count; i++)
            {
                GetPlanarRef(planarLayers[i]);
            }
            return usedPlanes;
        }

        private void ChangeRenderingCamera(Config3DPlanar config, PlanarRefs tempRef)
        {
            if (config.renderWithMainCamera)
            {
                tempRef.planarObject.layer = 0;
            }
            else
            {
                tempRef.planarObject.layer = LayerMask.NameToLayer(LAYER);
            }
        }

        private void ParentPlanarLayer(Config3DPlanar config, PlanarRefs tempRef)
        {
            if (config.followCamera)
            {
                tempRef.planarObject.transform.parent = planarCameraFollowingElements;
            }
            else
            {
                tempRef.planarObject.transform.parent = planarStaticElements;
            }
        }

        private void ApplyParticleProperties(Config3DPlanar config, PlanarRefs tempRef)
        {
            // Apply radius
            ParticleSystem particle = tempRef.planarObject.GetComponent<ParticleSystem>();
            var shape = particle.shape;
            shape.radius = config.radius;

            // Apply y-pos
            Vector3 pos = tempRef.planarObject.transform.position;
            pos.y = config.yPos;
            tempRef.planarObject.transform.position = pos;
        }

        public PlanarRefs GetPlanarRef(Config3DPlanar config)
        {
            PlanarRefs tempPlane = null;

            if (config.prefab == null)
            {
                usedPlanes.Add(tempPlane);
                return tempPlane;
            }

            // Check if GO for this prefab is already in scene, else create new
            if (planarReferences.ContainsKey(config.prefab))
            {
                // Check if there is any unused GO for the given prefab
                if (planarReferences[config.prefab].Count > 0)
                {
                    tempPlane = planarReferences[config.prefab].Dequeue();
                }
                else
                {
                    tempPlane = InstantiateNewSatelliteReference(config);
                }
            }
            else
            {
                planarReferences.Add(config.prefab, new Queue<PlanarRefs>());
                tempPlane = InstantiateNewSatelliteReference(config);
            }
            usedPlanes.Add(tempPlane);
            tempPlane.planarObject.SetActive(true);

            return tempPlane;
        }

        PlanarRefs InstantiateNewSatelliteReference(Config3DPlanar config)
        {
            GameObject obj = GameObject.Instantiate<GameObject>(config.prefab);
            obj.layer = LayerMask.NameToLayer(LAYER);
            obj.name = "Planar Layer";
            obj.transform.parent = planarElements.transform;
            obj.transform.localPosition = Vector3.zero;

            PlanarRefs planar = new PlanarRefs();
            planar.prefab = config.prefab;
            planar.planarObject = obj;

            return planar;
        }

        private void ResetPlanes()
        {
            if (usedPlanes != null)
            {
                for (int i = 0; i < usedPlanes.Count; i++)
                {
                    PlanarRefs plane = usedPlanes[i];
                    plane.inUse = false;
                    plane.planarObject.SetActive(false);
                    planarReferences[plane.prefab].Enqueue(plane);
                }
                usedPlanes.Clear();
            }
        }

        private bool IsLayerActiveInCurrentTime(float timeOfTheDay, Config3DPlanar config, float cycleTime)
        {
            // Calculate edited time for the case of out time less than in time (over the day scenario)
            float outTimeEdited = config.timeSpan_End;
            float timeOfTheDayEdited = timeOfTheDay;

            if (config.timeSpan_End < config.timeSpan_start)
            {
                outTimeEdited += cycleTime;
            }

            if (timeOfTheDay < config.timeSpan_start)
            {
                timeOfTheDayEdited += cycleTime;
            }

            if (timeOfTheDayEdited >= config.timeSpan_start && timeOfTheDayEdited <= outTimeEdited)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}