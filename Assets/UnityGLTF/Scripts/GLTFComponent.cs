using DCL.Components;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityGLTF.Loader;

namespace UnityGLTF
{
    /// <summary>
    /// Component to load a GLTF scene with
    /// </summary>
    public class GLTFComponent : MonoBehaviour, ILoadable
    {
        public static bool VERBOSE = false;

        public static int maxSimultaneousDownloads = 3;
        public static float nearestDistance = float.MaxValue;
        public static GLTFComponent nearestGLTFComponent;

        public static int downloadingCount;
        public static int queueCount;

        public string GLTFUri = null;
        public bool Multithreaded = true;
        public bool UseStream = false;
        public bool UseVisualFeedback = true;

        public int MaximumLod = 300;
        public int Timeout = 8;
        public Material LoadingTextureMaterial;
        public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;

        public bool InitialVisibility
        {
            get { return initialVisibility; }
            set
            {
                initialVisibility = value;
                if (sceneImporter != null)
                {
                    sceneImporter.InitialVisibility = value;
                }
            }
        }


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
        private bool initialVisibility = true;

        private enum State
        {
            NONE,
            QUEUED,
            DOWNLOADING,
            COMPLETED,
            FAILED
        }

        private State state = State.NONE;

        private bool alreadyDecrementedRefCount;
        private AsyncCoroutineHelper asyncCoroutineHelper;
        private Coroutine loadingRoutine = null;
        private GLTFSceneImporter sceneImporter;
        private Camera mainCamera;

        public WebRequestLoader.WebRequestLoaderEventAction OnWebRequestStartEvent;
        public Action OnSuccess { get { return OnFinishedLoadingAsset; } set { OnFinishedLoadingAsset = value; } }
        public Action OnFail { get { return OnFailedLoadingAsset; } set { OnFailedLoadingAsset = value; } }

        public void LoadAsset(string incomingURI = "", bool loadEvenIfAlreadyLoaded = false)
        {
            if (alreadyLoadedAsset && !loadEvenIfAlreadyLoaded)
            {
                return;
            }

            if (!string.IsNullOrEmpty(incomingURI))
            {
                GLTFUri = incomingURI;
            }

            if (loadingRoutine != null)
            {
                StopCoroutine(loadingRoutine);
            }

            alreadyDecrementedRefCount = false;
            state = State.NONE;
            mainCamera = Camera.main;

            loadingRoutine = DCL.CoroutineHelpers.StartThrowingCoroutine(this, LoadAssetCoroutine(), OnFail_Internal);
        }

        private void OnFail_Internal(Exception obj)
        {
            if (state == State.FAILED)
            {
                return;
            }

            state = State.FAILED;

            DecrementDownloadCount();

            OnFailedLoadingAsset?.Invoke();

            if (obj is IndexOutOfRangeException)
            {
                Destroy(gameObject);
            }

            Debug.Log("GLTF Failure " + obj.ToString());
        }

        private void IncrementDownloadCount()
        {
            downloadingCount++;

            if (VERBOSE)
            {
                Debug.Log($"downloadingCount++ = {downloadingCount}");
            }
        }

        private void DecrementDownloadCount()
        {
            if (!alreadyDecrementedRefCount && state != State.NONE && state != State.QUEUED)
            {
                downloadingCount--;
                alreadyDecrementedRefCount = true;
                if (VERBOSE)
                {
                    Debug.Log($"(ERROR) downloadingCount-- = {downloadingCount}");
                }
            }
        }

        public IEnumerator LoadAssetCoroutine()
        {
            if (!string.IsNullOrEmpty(GLTFUri))
            {
                if (VERBOSE)
                {
                    Debug.Log("LoadAssetCoroutine() GLTFUri ->" + GLTFUri);
                }

                asyncCoroutineHelper = gameObject.GetComponent<AsyncCoroutineHelper>() ?? gameObject.AddComponent<AsyncCoroutineHelper>();

                sceneImporter = null;
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
                        loader = new WebRequestLoader("");

                        if (OnWebRequestStartEvent != null)
                        {
                            (loader as WebRequestLoader).OnLoadStreamStart += OnWebRequestStartEvent;
                        }

                        sceneImporter = new GLTFSceneImporter(
                            GLTFUri,
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
                    sceneImporter.InitialVisibility = initialVisibility;

                    float time = Time.realtimeSinceStartup;

                    queueCount++;

                    state = State.QUEUED;

                    Func<bool> funcTestDistance = () => TestDistance();
                    yield return new WaitUntil(funcTestDistance);

                    queueCount--;

                    IncrementDownloadCount();

                    state = State.DOWNLOADING;

                    yield return sceneImporter.LoadScene(-1);

                    state = State.COMPLETED;

                    DecrementDownloadCount();

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

                        if (OnWebRequestStartEvent != null)
                        {
                            (loader as WebRequestLoader).OnLoadStreamStart -= OnWebRequestStartEvent;
                            OnWebRequestStartEvent = null;
                        }

                        loader = null;
                    }

                    OnFinishedLoadingAsset?.Invoke();
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

        private bool TestDistance()
        {
            float dist = Vector3.Distance(mainCamera.transform.position, transform.position);

            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestGLTFComponent = this;
            }

            bool result = nearestGLTFComponent == this && downloadingCount < maxSimultaneousDownloads;

            if (result)
            {
                //NOTE(Brian): Reset values so the other GLTFComponents running this coroutine compete again
                //             for distance.
                nearestGLTFComponent = null;
                nearestDistance = float.MaxValue;
            }

            return result;
        }

        public void Load(string url, bool useVisualFeedback)
        {
            throw new NotImplementedException();
        }

        private void OnDestroy()
        {
            if (!alreadyDecrementedRefCount && state != State.NONE && state != State.QUEUED)
            {
                downloadingCount--;
                alreadyDecrementedRefCount = true;

                if (VERBOSE)
                {
                    Debug.Log($"downloadingCount-- = {downloadingCount}");
                }
            }
        }
    }
}
