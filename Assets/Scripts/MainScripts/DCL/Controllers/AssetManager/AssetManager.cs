using DCL.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AssetInfo
    {
        public object settings;
        public double timeStamp;
        public bool isLoadingCompleted = false;

        public virtual int referenceCount { get; set; }

        public System.Action OnSuccess;
        public System.Action OnFail;
    }

    public abstract class AssetManager<AssetInfoClass, AssetContainerClass, Loadable> : MonoBehaviour
        where Loadable : ILoadable
        where AssetInfoClass : AssetInfo, new()
    {

        public Dictionary<object, AssetInfoClass> assetLibrary;

        public virtual void ClearLibrary()
        {
            assetLibrary.Clear();
        }

        //
        // Asset methods
        //

        protected abstract AssetContainerClass CreateEmptyAsset(Transform parent);
        protected abstract AssetContainerClass GetCachedAsset(object id, Transform parent, Action OnSuccess);
        protected abstract void CleanCachedAsset(object id);

        protected abstract IEnumerator AddToLibrary(object id, string url, AssetContainerClass resultContainer);

        //
        // Loadable methods
        //
        protected abstract Loadable GetLoadable(AssetContainerClass container);
        protected abstract void StartLoading(Loadable loadable, object id, string url, object settings);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="url"></param>
        /// <param name="resultContainer"></param>
        /// <param name="OnSuccess"></param>
        /// <returns></returns>
        private IEnumerator AddToLibrary_Internal(object id,
                                                  string url,
                                                  AssetContainerClass resultContainer,
                                                  System.Action OnSuccess)
        {
            yield return AddToLibrary(id, url, resultContainer);

            if (OnSuccess != null)
            {
                OnSuccess.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parent"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFail"></param>
        /// <param name="initialVisibility"></param>
        /// <returns></returns>
        public AssetContainerClass Get(string url,
                                       Transform parent,
                                       System.Action OnSuccess,
                                       System.Action OnFail,
                                       object settings = null)
        {
            return Get(url, url, parent, OnSuccess, OnFail, settings = null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="url"></param>
        /// <param name="parent"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFail"></param>
        /// <param name="initialVisibility"></param>
        /// <returns></returns>
        public AssetContainerClass Get(object id,
                                       string url,
                                       Transform parent,
                                       System.Action OnSuccess,
                                       System.Action OnFail,
                                       object settings = null)
        {
            AssetContainerClass resultContainer = default(AssetContainerClass);

            //NOTE(Brian): First time we load the asset
            if (!assetLibrary.ContainsKey(id))
            {
                Cleanup();

                resultContainer = CreateEmptyAsset(parent);

                Loadable loader = GetLoadable(resultContainer);

                AssetInfoClass assetInfo = new AssetInfoClass();
                assetInfo.isLoadingCompleted = false;
                assetInfo.timeStamp = Time.realtimeSinceStartup;
                assetInfo.referenceCount++;
                assetInfo.settings = settings;

                assetLibrary.Add(id, assetInfo);

                System.Action PreSuccessClosure = null;
                System.Action PreFailClosure = null;

                PreSuccessClosure =
                    () =>
                    {
                        loader.OnSuccess -= PreSuccessClosure;
                        loader.OnFail -= OnFail;

                        //NOTE(Brian): Before the loading finishes, I need to add the asset to the AssetManager's library.
                        StartCoroutine(AddToLibrary_Internal(id, url, resultContainer, OnSuccess));
                    };

                PreFailClosure =
                    () =>
                    {
                        loader.OnSuccess -= PreSuccessClosure;
                        loader.OnFail -= OnFail;

                        if (OnFail != null)
                        {
                            OnFail.Invoke();
                        }

                        if (assetInfo.OnFail != null)
                        {
                            assetInfo.OnFail.Invoke();
                            assetInfo.OnFail = null;
                        }

                        assetInfo.OnSuccess = null;
                    };

                loader.OnSuccess += PreSuccessClosure;
                loader.OnFail += PreFailClosure;

                StartLoading(loader, id, url, settings);

                return resultContainer;
            }
            else
            {
                assetLibrary[id].referenceCount++;

                if (!assetLibrary[id].isLoadingCompleted)
                {
                    //NOTE(Brian): If the asset is in the process of being loaded by another object, piggyback on that one
                    //             suscribing to the OnSuccess/OnFail event.
                    assetLibrary[id].OnSuccess += () => GetCachedAsset(id, parent, OnSuccess);
                    assetLibrary[id].OnFail += OnFail;
                }
                else
                {
                    GetCachedAsset(id, parent, OnSuccess);
                    return resultContainer;
                }
            }

            return resultContainer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void Release(object id)
        {
            if (assetLibrary.ContainsKey(id))
            {
                if (assetLibrary[id].referenceCount > 0)
                {
                    assetLibrary[id].referenceCount--;
                    Cleanup();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Cleanup()
        {
            // TODO(Brian): Stub cleanup code, improve later using memory size instead library size
            if (assetLibrary.Count <= Configuration.AssetManagerSettings.LIBRARY_CLEANUP_THRESHOLD)
            {
                return;
            }

            List<object> idsToRemove = new List<object>();

            foreach (var pair in assetLibrary)
            {
                if (pair.Value == null || pair.Value.referenceCount <= 0)
                {
                    idsToRemove.Add(pair.Key);
                }
            }

            for (int i = 0; i < idsToRemove.Count; i++)
            {
                CleanCachedAsset(idsToRemove[i]);
                assetLibrary.Remove(idsToRemove[i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneData"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetIdForAsset(ContentProvider provider, string url)
        {
            string id = string.Empty;

            if (SceneController.i.isDebugMode || SceneController.i.isWssDebugMode)
            {
                string[] tmp = url.Split('/');
                string hash = tmp[tmp.Length - 1];

                if (provider.contents != null)
                {
                    var contentMapping = provider.GetMappingForHash(hash);

                    if (contentMapping != null)
                    {
                        id = contentMapping.file;
                    }
                    else
                    {
                        //NOTE(Brian): Needed for tests
                        id = url;
                    }
                }
                else
                {
                    id = url;
                }
            }
            else
            {
                id = url;
            }

            return id;
        }
    }
}
