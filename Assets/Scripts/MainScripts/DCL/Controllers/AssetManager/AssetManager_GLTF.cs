using DCL.Components;
using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF;
using Random = UnityEngine.Random;

namespace DCL
{
    public class AssetManager_GLTF : AssetManager<AssetManager_GLTF.AssetInfo, GameObject, GLTFComponent>
    {
        static bool VERBOSE = false;
        public static AssetManager_GLTF i { get; private set; }

        public class AssetInfo : DCL.AssetInfo
        {
            public GameObject cachedContainer;
            public string name;

            public void UpdateContainerName()
            {
                if (cachedContainer != null && !string.IsNullOrEmpty(name))
                {
                    cachedContainer.name = $"refs {referenceCount} -- Cached GLTF: {name}";
                }
            }

            public override int referenceCount
            {
                get
                {
                    return base.referenceCount;
                }

                set
                {
                    base.referenceCount = value;
                    UpdateContainerName();
                }
            }
        }

        private readonly PoolInstantiator_GLTF instantiator = new PoolInstantiator_GLTF();

        private void Awake()
        {
            if (i == null)
            {
                i = this;
            }

            assetLibrary = new Dictionary<object, AssetInfo>();
        }

        public override void ClearLibrary()
        {
            foreach (var id in assetLibrary.Keys)
                PoolManager.i.CleanupPool(id);

            base.ClearLibrary();
        }

        // Asset methods
        protected override GameObject CreateEmptyAsset(Transform parent)
        {
            GameObject emptyAsset = new GameObject();
            emptyAsset.name = "GLTF Container";
            emptyAsset.transform.parent = parent;
            emptyAsset.transform.ResetLocalTRS();

            return emptyAsset;
        }

        protected override GameObject GetCachedAsset(object id, Transform parent, Action OnSuccess)
        {
            // WARNING (Zak): This HACK was necessary because there's no way to know if
            // the parent will be destroyed when this method is called. It's a limitation
            // of current design - it won't be necessary when we refactor the AssetManager
            if (parent)
            {
                GameObject cachedAsset = DuplicateGLTF(id, assetLibrary[id].cachedContainer);
                StartCoroutine(ShowObject(cachedAsset, Configuration.ParcelSettings.VISUAL_LOADING_ENABLED, OnSuccess));

                cachedAsset.transform.parent = parent;
                cachedAsset.transform.ResetLocalTRS();

                return cachedAsset;
            }

            return null;
        }

        protected override void CleanCachedAsset(object id)
        {
            if (assetLibrary.ContainsKey(id) && assetLibrary[id].cachedContainer)
            {
                PoolableObject po = assetLibrary[id].cachedContainer.GetComponent<PoolableObject>();

                if (po)
                    po.Release();
                else
                    Destroy(assetLibrary[id].cachedContainer);
            }
        }

        protected override IEnumerator AddToLibrary(object id, string url, GameObject container)
        {
            MaterialTransitionController[] matTransitions =
                container.GetComponentsInChildren<MaterialTransitionController>(true);

            //NOTE(Brian): Wait for the MaterialTransition to finish before copying the object to the library
            yield return new UnityEngine.WaitUntil(
                () =>
                {
                    bool finishedTransition = true;

                    for (int i = 0; i < matTransitions.Length; i++)
                    {
                        if (matTransitions[i] != null)
                        {
                            finishedTransition = false;
                            break;
                        }
                    }

                    return finishedTransition;
                }
            );

            if (container == null)
            {
                //NOTE(Brian): This will happen if the object was destroyed before the loading was finished.
                yield break;
            }

            CollidersManager.i.ConfigureColliders(container, true, true);

            GameObject containerCopy = DuplicateGLTF(id, container);

            containerCopy.transform.parent = transform;
            containerCopy.transform.ResetLocalTRS();
            containerCopy.SetActive(false);

            if (!assetLibrary.ContainsKey(id))
            {
                AssetInfo assetInfo = new AssetInfo();
                assetInfo.isLoadingCompleted = true;
                assetInfo.cachedContainer = containerCopy;
                assetLibrary.Add(id, assetInfo);
            }
            else
            {
                assetLibrary[id].isLoadingCompleted = true;
                assetLibrary[id].cachedContainer = containerCopy;
            }

            assetLibrary[id].name = url;
            assetLibrary[id].UpdateContainerName();

            if (assetLibrary[id].OnSuccess != null)
            {
                assetLibrary[id].OnSuccess.Invoke();
                assetLibrary[id].OnSuccess = null;
            }

            assetLibrary[id].OnFail = null;
        }


        public GameObject Get(object id,
                              string url,
                              Transform parent,
                              System.Action OnSuccess,
                              System.Action OnFail,
                              GLTFComponent.Settings settings)
        {
            return base.Get(id, url, parent, OnSuccess, OnFail, settings);
        }

        protected override GLTFComponent GetLoadable(GameObject container)
        {
            return container.AddComponent<GLTFComponent>();
        }

        protected override void StartLoading(GLTFComponent loadable,
                                             object id,
                                             string url,
                                             object settings = null)
        {
            if (VERBOSE)
            {
                Debug.Log("StartLoading() url -> " + url);
            }

            loadable.LoadAsset(url, true, settings as GLTFComponent.Settings);
        }

        IEnumerator ShowObject(GameObject go, bool useMaterialTransition, Action OnSuccess)
        {
            float delay = Random.Range(0, 1f);
            yield return new WaitForSeconds(delay);

            // NOTE(Brian): This GameObject can be removed by distance after the delay
            if (go != null)
            {
                go.SetActive(true);

                const float MIN_DISTANCE_TO_USE_MATERIAL_TRANSITION = 50;
                var character = DCLCharacterController.i;

                if (character == null || Vector3.Distance(go.transform.position, character.transform.position) <
                    MIN_DISTANCE_TO_USE_MATERIAL_TRANSITION)
                {
                    if (useMaterialTransition)
                    {
                        MaterialTransitionController.ApplyToLoadedObject(go, false);
                        yield return new WaitForSeconds(1);
                    }
                }
            }

            OnSuccess?.Invoke();
        }

        GameObject DuplicateGLTF(object id, GameObject original)
        {
            return PoolManager.i.Get<PoolInstantiator_GLTF>(id, original, instantiator).gameObject;
        }
    }
}
