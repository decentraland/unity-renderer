using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using GLTF;
using GLTF.Schema;
using UnityEngine;
using UnityGLTF.Loader;
using DCL.Components;

namespace UnityGLTF
{
    /// <summary>
    /// Component to load a GLTF scene with
    /// </summary>
    public class GLTFComponent : MonoBehaviour, ILoadable
    {
        const int GLTF_DOWNLOAD_THROTTLING_LIMIT = 3;
        public static bool VERBOSE = false;

        public static int downloadingCount;

        public string GLTFUri = null;
        public bool Multithreaded = true;
        public bool UseStream = false;
        public bool UseVisualFeedback = true;

        public int MaximumLod = 300;
        public int Timeout = 8;
        public Material LoadingTextureMaterial;
        public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;

        public GameObject loadingPlaceholder;
        public System.Action OnFinishedLoadingAsset;
        public System.Action OnFailedLoadingAsset;

        [HideInInspector] public bool alreadyLoadedAsset = false;
        [HideInInspector] public GameObject loadedAssetRootGameObject;

        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool MaterialsOnly = false;
        [SerializeField] private int RetryCount = 10;
        [SerializeField] private float RetryTimeout = 2.0f;
        [SerializeField] private Shader shaderOverride = null;

        private int numRetries = 0;
        private AsyncCoroutineHelper asyncCoroutineHelper;
        Coroutine loadingRoutine = null;

        public Action OnSuccess { get { return OnFinishedLoadingAsset; } set { OnFinishedLoadingAsset = value; } }
        public Action OnFail { get { return OnFailedLoadingAsset; } set { OnFailedLoadingAsset = value; } }

        bool alreadyFailed;
        bool alreadyDecrementedRefCount;

        public void LoadAsset(string incomingURI = "", bool loadEvenIfAlreadyLoaded = false)
        {
            if (alreadyLoadedAsset && !loadEvenIfAlreadyLoaded) return;

            if (!string.IsNullOrEmpty(incomingURI))
            {
                GLTFUri = incomingURI;
            }

            if (loadingRoutine != null)
            {
                StopCoroutine(loadingRoutine);
            }

            alreadyFailed = false;
            alreadyDecrementedRefCount = false;

            loadingRoutine = DCL.CoroutineHelpers.StartThrowingCoroutine(this, LoadAssetCoroutine(), OnFail_Internal);
        }

        private void OnFail_Internal(Exception obj)
        {
            if (alreadyFailed)
                return;

            alreadyFailed = true;

            if (OnFailedLoadingAsset != null)
                OnFailedLoadingAsset.Invoke();

            if (!alreadyDecrementedRefCount)
            {
                downloadingCount--;
                alreadyDecrementedRefCount = true;
                if (VERBOSE) Debug.Log($"(ERROR) downloadingCount-- = {downloadingCount}");
            }

            if (obj is IndexOutOfRangeException)
            {
                Debug.Log("Destroying...");
                Destroy(gameObject);
            }

            Debug.Log("GLTF Failure " + obj.ToString());
        }

        public IEnumerator LoadAssetCoroutine()
        {
            if (!string.IsNullOrEmpty(GLTFUri))
            {
                asyncCoroutineHelper = gameObject.GetComponent<AsyncCoroutineHelper>() ?? gameObject.AddComponent<AsyncCoroutineHelper>();

                GLTFSceneImporter sceneImporter = null;
                ILoader loader = null;

                Destroy(loadedAssetRootGameObject);

                try
                {
                    if (UseStream)
                    {
                        // Path.Combine treats paths that start with the separator character
                        // as absolute paths, ignoring the first path passed in. This removes
                        // that character to properly handle a filename written with it.
                        GLTFUri = GLTFUri.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                        string fullPath = Path.Combine(Application.streamingAssetsPath, GLTFUri);
                        string directoryPath = URIHelper.GetDirectoryName(fullPath);
                        loader = new FileLoader(directoryPath);
                        sceneImporter = new GLTFSceneImporter(
                            Path.GetFileName(GLTFUri),
                            loader,
                            asyncCoroutineHelper
                            );
                    }
                    else
                    {
                        string directoryPath = URIHelper.GetDirectoryName(GLTFUri);
                        loader = new WebRequestLoader(directoryPath);

                        sceneImporter = new GLTFSceneImporter(
                            URIHelper.GetFileFromUri(new Uri(GLTFUri)),
                            loader,
                            asyncCoroutineHelper
                            );

                    }

                    if (sceneImporter.CreatedObject != null)
                    {
                        Destroy(sceneImporter.CreatedObject);
                    }

                    sceneImporter.SceneParent = gameObject.transform;
                    sceneImporter.Collider = Collider;
                    sceneImporter.MaximumLod = MaximumLod;
                    sceneImporter.Timeout = Timeout;
                    sceneImporter.isMultithreaded = Multithreaded;
                    sceneImporter.UseMaterialTransition = UseVisualFeedback;
                    sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;
                    sceneImporter.LoadingTextureMaterial = LoadingTextureMaterial;

                    float time = Time.realtimeSinceStartup;

                    if (downloadingCount >= GLTF_DOWNLOAD_THROTTLING_LIMIT)
                    {
                        yield return new WaitUntil(() => { return downloadingCount < GLTF_DOWNLOAD_THROTTLING_LIMIT; });
                    }

                    downloadingCount++;
                    if ( VERBOSE ) Debug.Log($"downloadingCount++ = {downloadingCount}");

                    yield return sceneImporter.LoadScene(-1);
                    
                    if (!alreadyDecrementedRefCount)
                    {
                        downloadingCount--;
                        alreadyDecrementedRefCount = true;
                        if (VERBOSE) Debug.Log($"downloadingCount-- = {downloadingCount}");
                    }

                    // Override the shaders on all materials if a shader is provided
                    if (shaderOverride != null)
                    {
                        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderer in renderers)
                        {
                            renderer.sharedMaterial.shader = shaderOverride;
                        }
                    }

                }
                finally
                {
                    if (loader != null)
                    {
                        if (sceneImporter == null)
                        {
                            Debug.Log("sceneImporter is null, could be due to an invalid URI.", this);
                        }
                        else
                        {
                            loadedAssetRootGameObject = sceneImporter.CreatedObject;

                            sceneImporter?.Dispose();
                            sceneImporter = null;
                        }

                        loader = null;
                    }

                    if (OnFinishedLoadingAsset != null)
                    {
                        OnFinishedLoadingAsset.Invoke();
                    }

                    alreadyLoadedAsset = true;
                }
            }
            else
            {
                Debug.Log("couldn't load GLTF because url is empty");
            }

            loadingRoutine = null;
            Destroy(loadingPlaceholder);
            Destroy(this);
        }

        public void Load(string url, bool useVisualFeedback)
        {
            throw new NotImplementedException();
        }
    }
}
