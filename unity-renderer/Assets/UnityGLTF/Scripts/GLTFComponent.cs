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

        public static int maxSimultaneousDownloads = 10;
        public static float nearestDistance = float.MaxValue;
        public static GLTFComponent nearestGLTFComponent;

        public static int downloadingCount;
        public static event Action OnDownloadingProgressUpdate;

        public static int totalDownloadedCount;
        public static int queueCount;

        public class Settings
        {
            public bool? useVisualFeedback;
            public bool? initialVisibility;
            public Shader shaderOverride;
            public bool addMaterialsToPersistentCaching;
            public WebRequestLoader.WebRequestLoaderEventAction OnWebRequestStartEvent;
        }

        public string GLTFUri = null;
        public string idPrefix = null;

        public bool Multithreaded = false;
        public bool UseStream = false;
        public bool UseVisualFeedback = true;
        private bool addMaterialsToPersistentCaching = true;

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
                    sceneImporter.initialVisibility = value;
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
        [SerializeField] public Shader shaderOverride = null;
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
        public Coroutine loadingRoutine = null;
        private GLTFSceneImporter sceneImporter;
        private Camera mainCamera;
        private DCL.IWebRequestController webRequestController;

        public WebRequestLoader.WebRequestLoaderEventAction OnWebRequestStartEvent;

        public Action OnSuccess { get { return OnFinishedLoadingAsset; } set { OnFinishedLoadingAsset = value; } }

        public Action OnFail { get { return OnFailedLoadingAsset; } set { OnFailedLoadingAsset = value; } }

        public void Initialize(DCL.IWebRequestController webRequestController) { this.webRequestController = webRequestController; }

        public void LoadAsset(string incomingURI = "", string idPrefix = "", bool loadEvenIfAlreadyLoaded = false, Settings settings = null)
        {
            if (alreadyLoadedAsset && !loadEvenIfAlreadyLoaded)
            {
                return;
            }

            if (!string.IsNullOrEmpty(incomingURI))
                GLTFUri = incomingURI;

            if (!string.IsNullOrEmpty(idPrefix))
                this.idPrefix = idPrefix;

            if (loadingRoutine != null)
            {
                //StopCoroutine(loadingRoutine);
                Debug.LogError($"PATO: load when already loading {idPrefix}");
                return;
            }

            alreadyDecrementedRefCount = false;
            state = State.NONE;
            mainCamera = Camera.main;

            if (settings != null)
            {
                ApplySettings(settings);
            }

            loadingRoutine = DCL.CoroutineHelpers.StartThrowingCoroutine(this, LoadAssetCoroutine(), OnFail_Internal);
        }

        void ApplySettings(Settings settings)
        {
            if (settings.initialVisibility.HasValue)
            {
                this.InitialVisibility = settings.initialVisibility.Value;
            }

            if (settings.useVisualFeedback.HasValue)
            {
                this.UseVisualFeedback = settings.useVisualFeedback.Value;
            }

            if (settings.shaderOverride != null)
            {
                this.shaderOverride = settings.shaderOverride;
            }

            if (settings.OnWebRequestStartEvent != null)
            {
                OnWebRequestStartEvent = settings.OnWebRequestStartEvent;
            }

            this.addMaterialsToPersistentCaching = settings.addMaterialsToPersistentCaching;
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

            if (obj != null)
            {
                if (obj is IndexOutOfRangeException)
                {
                    Destroy(gameObject);
                }


                Debug.Log($"GLTF Failure {obj} ... url = {this.GLTFUri}\n{obj.StackTrace}");
            }
        }

        private void IncrementDownloadCount()
        {
            downloadingCount++;
            OnDownloadingProgressUpdate?.Invoke();

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
                OnDownloadingProgressUpdate?.Invoke();
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
                            null,
                            Path.GetFileName(GLTFUri),
                            loader,
                            asyncCoroutineHelper
                        );
                    }
                    else
                    {
                        loader = new WebRequestLoader("", webRequestController);

                        string id = string.IsNullOrEmpty(idPrefix) ? GLTFUri : idPrefix;

                        if (OnWebRequestStartEvent != null)
                            (loader as WebRequestLoader).OnLoadStreamStart += OnWebRequestStartEvent;

                        sceneImporter = new GLTFSceneImporter(
                            id,
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
                    sceneImporter.maximumLod = MaximumLod;
                    sceneImporter.Timeout = Timeout;
                    sceneImporter.isMultithreaded = Multithreaded;
                    sceneImporter.useMaterialTransition = UseVisualFeedback;
                    sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;
                    sceneImporter.LoadingTextureMaterial = LoadingTextureMaterial;
                    sceneImporter.initialVisibility = initialVisibility;
                    sceneImporter.addMaterialsToPersistentCaching = addMaterialsToPersistentCaching;

                    float time = Time.realtimeSinceStartup;

                    queueCount++;

                    state = State.QUEUED;

                    Func<bool> funcTestDistance = () => TestDistance();
                    yield return new WaitUntil(funcTestDistance);

                    queueCount--;
                    totalDownloadedCount++;

                    if (this == null)
                        yield break;

                    IncrementDownloadCount();

                    Debug.Log($"PATO: start loading {idPrefix} process...");
                    state = State.DOWNLOADING;

                    if (transform != null)
                    {
                        Debug.Log($"PATO: start loading {idPrefix} do start...");
                        yield return sceneImporter.LoadScene(-1);
                        Debug.Log($"PATO: ended loading {idPrefix}");

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

                    state = State.COMPLETED;
                    Debug.Log($"PATO: finish loading {idPrefix}");

                    DecrementDownloadCount();
                }
                finally
                {
                    Debug.Log($"PATO: finally loading {idPrefix}");
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

                    alreadyLoadedAsset = true;

                    CoroutineStarter.Stop(loadingRoutine);
                    loadingRoutine = null;
                    Debug.Log($"PATO: loading routine finished {idPrefix}");

                    OnFinishedLoadingAsset?.Invoke();

                    Destroy(loadingPlaceholder);
                    Destroy(this);
                }
            }
            else
            {
                Debug.Log("couldn't load GLTF because url is empty");
            }
        }

        private bool TestDistance()
        {
            if (mainCamera == null || this == null)
                return true;

            if (ignoreDistanceTest)
                return true;

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

        public void Load(string url) { throw new NotImplementedException(); }

        private bool DEBUG_cancelled = false;
        private bool ignoreDistanceTest = false;

        public void CancelIfQueued()
        {
            Debug.Log($"PATO: try-cancel {idPrefix} state = {state}");
            DEBUG_cancelled = true;
            if (state == State.QUEUED || state == State.NONE)
            {
                CoroutineStarter.Stop(loadingRoutine);
                loadingRoutine = null;
                OnFail_Internal(null);
                Debug.Log($"PATO: cancelled {idPrefix}");
            }
        }

        public void SetIgnoreDistanceTest() { ignoreDistanceTest = true; }

#if UNITY_EDITOR
        // In production it will always be false
        private bool isQuitting = false;

        // We need to check if application is quitting in editor
        // to prevent the pool from releasing objects that are
        // being destroyed 
        void Awake() { Application.quitting += OnIsQuitting; }

        void OnIsQuitting()
        {
            Application.quitting -= OnIsQuitting;
            isQuitting = true;
        }
#endif
        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (isQuitting)
                return;
#endif
            if (sceneImporter != null)
            {
                sceneImporter.Dispose();
            }

            if (loadingRoutine != null)
            {
                Debug.LogError($"PATO: destroyed while loading {name}");
            }

            if (!alreadyLoadedAsset && loadingRoutine != null)
            {
                CoroutineStarter.Stop(loadingRoutine);
                OnFail_Internal(null);
                return;
            }

            DecrementDownloadCount();
        }
    }
}