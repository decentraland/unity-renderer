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

    public class SkyboxElements
    {
        const float domeDefaultSize = 50;
        const string domeResourcesPath = "SkyboxPrefabs/Dome";

        private GameObject skyboxElements;
        private GameObject domeElements;
        private GameObject domePrefab;

        Queue<DomeReferences> domeObjects = new Queue<DomeReferences>();
        Queue<DomeReferences> activeDomeObjects = new Queue<DomeReferences>();

        public void Initialize3DObjects(SkyboxConfiguration configuration)
        {
            if (skyboxElements == null)
            {
                skyboxElements = GameObject.Find("Skybox Elements");

                // If Skybox element doesn't exsist make new object else find dome objects 
                if (skyboxElements == null)
                {
                    skyboxElements = new GameObject("Skybox Elements");
                    skyboxElements.layer = LayerMask.NameToLayer("Skybox");
                    domeElements = new GameObject("Dome Elements");
                    domeElements.layer = LayerMask.NameToLayer("Skybox");
                    domeElements.transform.parent = skyboxElements.transform;
                }
                else
                {
                    domeElements = skyboxElements.transform.Find("Dome Elements").gameObject;
                    for (int i = 0; i < domeElements.transform.childCount; i++)
                    {
                        DomeReferences dome = new DomeReferences();
                        dome.domeGO = domeElements.transform.GetChild(i).gameObject;
                        dome.domeGO.GetComponent<Renderer>().material = Object.Instantiate(MaterialReferenceContainer.i.domeMat);
                        dome.domeMat = dome.domeGO.GetComponent<Renderer>().sharedMaterial;
                        dome.domeGO.SetActive(false);
                        domeObjects.Enqueue(dome);
                    }
                }
                skyboxElements.transform.position = Vector3.zero;
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

        #region 3D Domes

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

        DomeReferences InstantiateNewDome()
        {
            if (domePrefab == null)
            {
                domePrefab = Resources.Load<GameObject>(domeResourcesPath);
            }

            GameObject obj = GameObject.Instantiate<GameObject>(domePrefab);
            obj.layer = LayerMask.NameToLayer("Skybox");
            obj.name = "Dome";
            obj.transform.parent = domeElements.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.GetComponent<Renderer>().material = Object.Instantiate<Material>(MaterialReferenceContainer.i.domeMat);

            DomeReferences dome = new DomeReferences();
            dome.domeGO = obj;
            dome.domeMat = obj.GetComponent<Renderer>().sharedMaterial;

            return dome;
        }

        public List<DomeReferences> GetOrderedGameobjectList(List<Config3DDome> configList)
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

                // resize
                dome.domeGO.transform.localScale = domeDefaultSize * Vector3.one + Vector3.one * i;
                orderedList.Insert(0, dome);
            }

            return orderedList;
        }

        #endregion

    }
}