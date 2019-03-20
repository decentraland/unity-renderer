using DCL.Components;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF;

namespace DCL
{
    public class AssetManager_GLTF : AssetManager<AssetManager_GLTF.AssetInfo, GameObject, GLTFComponent>
    {
        public static AssetManager_GLTF i { get; private set; }

        public class AssetInfo : DCL.AssetInfo
        {
            public GameObject cachedContainer;
            public string name;

            public void UpdateContainerName()
            {
                if ( cachedContainer != null && !string.IsNullOrEmpty(name) )
                   cachedContainer.name = $"refs {referenceCount} -- Cached GLTF: {name}";
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

        private void Awake()
        {
            if (i == null)
                i = this;

            assetLibrary = new Dictionary<object, AssetInfo>();
        }

        public override void ClearLibrary()
        {
            foreach (var asset in assetLibrary)
            {
                if (asset.Value.cachedContainer != null)
                    Destroy(asset.Value.cachedContainer);
            }

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

        protected override GameObject GetCachedAsset(object id, Transform parent)
        {
            GameObject cachedAsset = DuplicateGLTF(assetLibrary[id].cachedContainer);
            StartCoroutine(ShowObject(cachedAsset, Configuration.ParcelSettings.VISUAL_LOADING_ENABLED));

            cachedAsset.transform.parent = parent;
            cachedAsset.transform.ResetLocalTRS();

            return cachedAsset;
        }

        protected override void CleanCachedAsset(object id)
        {
            Destroy(assetLibrary[id].cachedContainer);
        }

        protected override IEnumerator AddToLibrary(object id, string url, GameObject container)
        {
            MaterialTransitionController[] matTransitions = container.GetComponentsInChildren<MaterialTransitionController>(true);

            //NOTE(Brian): Wait for the MaterialTransition to finish before copying the object to the library
            yield return new WaitUntil(
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

            BaseShape.ConfigureColliders(container, true, true);

            GameObject containerCopy = DuplicateGLTF(container);


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

        // Loadable methods
        protected override GLTFComponent GetLoadable(GameObject container)
        {
            return container.AddComponent<GLTFComponent>();
        }

        protected override void StartLoading(GLTFComponent loadable, string url)
        {
            loadable.UseVisualFeedback = Configuration.ParcelSettings.VISUAL_LOADING_ENABLED;
            loadable.Multithreaded = false;
            loadable.LoadingTextureMaterial = Utils.EnsureResourcesMaterial("Materials/LoadingTextureMaterial");
            loadable.LoadAsset(url, true);
        }

        IEnumerator ShowObject(GameObject go, bool useMaterialTransition)
        {
            float delay = Random.Range(0, 1f);
            yield return new WaitForSeconds(delay);

            // NOTE(Brian): This GameObject can be removed by distance after the delay
            if (go != null)
            {
                go.SetActive(true);

                const float MIN_DISTANCE_TO_USE_MATERIAL_TRANSITION = 50;
                var character = DCLCharacterController.i;

                if (character == null || Vector3.Distance(go.transform.position, character.transform.position) < MIN_DISTANCE_TO_USE_MATERIAL_TRANSITION)
                {
                    if (useMaterialTransition)
                    {
                        MaterialTransitionController.ApplyToLoadedObject(go, false);
                    }
                    else
                    {
                        MaterialTransitionController.ApplyToLoadedObjectFast(go);
                    }
                }
                else
                {
                    MaterialTransitionController.ApplyToLoadedObjectFast(go);
                }
            }
        }

        GameObject DuplicateGLTF(GameObject original)
        {
            GameObject GLTFCopy;
            InstantiatedGLTFObject gltfDuplicator = original.GetComponentInChildren<InstantiatedGLTFObject>(true);

            if (gltfDuplicator != null)
            {
                GLTFCopy = gltfDuplicator.Duplicate().gameObject;
            }
            else
            {
                GLTFCopy = Instantiate(original);
            }

            return GLTFCopy;
        }
    }
}
