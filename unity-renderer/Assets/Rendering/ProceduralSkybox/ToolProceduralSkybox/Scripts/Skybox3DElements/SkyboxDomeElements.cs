using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Skybox
{
    public class DomeReferences
    {
        public GameObject domeGO;
        public Material domeMat;
        public bool domeInUse;
    }

    public class SkyboxDomeElements
    {
        private const string DOME_RESOURCES_PATH = "SkyboxPrefabs/Dome";

        private readonly GameObject domeElements;
        private GameObject domePrefab;
        private FollowBehaviour followBehavior;
        private MaterialReferenceContainer materialReferenceContainer;

        Queue<DomeReferences> domeObjects = new Queue<DomeReferences>();
        Queue<DomeReferences> activeDomeObjects = new Queue<DomeReferences>();

        public SkyboxDomeElements(GameObject domeElements, MaterialReferenceContainer materialReferenceContainer)
        {
            this.materialReferenceContainer = materialReferenceContainer;
            this.domeElements = domeElements;
            Initialize();
        }

        private void Initialize()
        {
            if (domeElements == null)
            {
                Debug.LogError("Dome Elements Container is null");
                return;
            }

            followBehavior = domeElements.GetOrCreateComponent<FollowBehaviour>();
            followBehavior.followPos = true;

            for (int i = 0; i < domeElements.transform.childCount; i++)
            {
                DomeReferences dome = new DomeReferences();
                dome.domeGO = domeElements.transform.GetChild(i).gameObject;
                dome.domeGO.GetComponent<Renderer>().material = Object.Instantiate(materialReferenceContainer.domeMat);
                dome.domeMat = dome.domeGO.GetComponent<Renderer>().sharedMaterial;
                dome.domeGO.SetActive(false);
                domeObjects.Enqueue(dome);
            }
        }

        public void ResetObjects()
        {
            while (activeDomeObjects.Count > 0)
            {
                DomeReferences dome = activeDomeObjects.Dequeue();
                dome.domeGO.SetActive(false);
                domeObjects.Enqueue(dome);
            }
        }

        private DomeReferences GetDomeReference()
        {
            DomeReferences dome = null;
            if (domeObjects.Count <= 0)
            {
                dome = InstantiateNewDome();
            }
            else
            {
                dome = domeObjects.Dequeue();
            }

            dome.domeGO.SetActive(true);

            // Add to active objects
            activeDomeObjects.Enqueue(dome);
            return dome;
        }

        private DomeReferences InstantiateNewDome()
        {
            if (domePrefab == null)
            {
                domePrefab = Resources.Load<GameObject>(DOME_RESOURCES_PATH);
            }

            GameObject obj = GameObject.Instantiate<GameObject>(domePrefab);
            obj.layer = LayerMask.NameToLayer("Skybox");
            obj.name = "Dome";
            obj.transform.parent = domeElements.transform;
            obj.transform.localPosition = new Vector3(0, -0.7f, 0);
            obj.GetComponent<Renderer>().material = Object.Instantiate<Material>(materialReferenceContainer.domeMat);

            DomeReferences dome = new DomeReferences();
            dome.domeGO = obj;
            dome.domeMat = obj.GetComponent<Renderer>().sharedMaterial;

            return dome;
        }

        private List<DomeReferences> GetOrderedGameobjectList(List<Config3DDome> configList)
        {
            ResetObjects();
            List<DomeReferences> orderedList = new List<DomeReferences>();
            // make a list of domes for each active dome config

            for (int i = 0; i < configList.Count; i++)
            {
                if (!configList[i].enabled)
                {
                    continue;
                }

                DomeReferences dome = GetDomeReference();

                orderedList.Insert(0, dome);
            }

            return orderedList;
        }

        public void ApplyDomeConfigurations(SkyboxConfiguration config, float dayTime, float normalizedDayTime, Light directionalLightGO = null, float cycleTime = 24)
        {
            List<DomeReferences> domeReferences = GetOrderedGameobjectList(config.additional3Dconfig);

            float percentage = normalizedDayTime * 100;
            int domeCount = 0;

            for (int i = 0; i < config.additional3Dconfig.Count; i++)
            {
                if (!config.additional3Dconfig[i].enabled)
                {
                    // Change all texture layer rendering to NotRendering
                    continue;
                }

                // If dome is not active due to time, Increment dome number, close dome GO and continue
                if (!config.additional3Dconfig[i].IsConfigActive(dayTime, cycleTime))
                {
                    domeReferences[domeCount].domeGO.SetActive(false);
                    domeCount++;
                    continue;
                }

                domeReferences[domeCount].domeGO.SetActive(true);

                // resize
                domeReferences[domeCount].domeGO.transform.localScale = config.additional3Dconfig[i].domeRadius * Vector3.one;

                domeReferences[domeCount].domeGO.name = config.additional3Dconfig[i].layers.nameInEditor;

                //Apply config
                //General Values
                domeReferences[domeCount].domeMat.SetColor(SkyboxShaderUtils.LightTint, config.directionalLightLayer.tintColor.Evaluate(normalizedDayTime));
                domeReferences[domeCount].domeMat.SetVector(SkyboxShaderUtils.LightDirection, directionalLightGO.transform.rotation.eulerAngles);


                TextureLayerFunctionality.ApplyTextureLayer(domeReferences[domeCount].domeMat, dayTime, normalizedDayTime, 0, config.additional3Dconfig[i].layers, cycleTime);
                domeCount++;
            }
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
