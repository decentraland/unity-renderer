using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Loader;
using UnityGLTF.Scripts;

namespace MainScripts.DCL.GLTF
{
    public class GLTFComponentAsync : MonoBehaviour, IGLTFComponent
    {
        private static bool VERBOSE = false;

        private static int maxSimultaneousDownloads = 10;

        private static int downloadingCount;
        public static event Action OnDownloadingProgressUpdate;

        private static int totalDownloadedCount;
        private static int queueCount;

        private static DownloadQueueHandler downloadQueueHandler = new DownloadQueueHandler(maxSimultaneousDownloads, () => downloadingCount);

        public string GLTFUri = null;
        public string idPrefix = null;

        public bool UseStream = false;
        public bool UseVisualFeedback = true;
        private bool addMaterialsToPersistentCaching = true;

        public int MaximumLod = 300;
        public int Timeout = 8;
        public Material LoadingTextureMaterial;
        public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;
        private GLTFThrottlingCounter throttlingCounter;

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
        public Action OnFinishedLoadingAsset;
        public Action<Exception> OnFailedLoadingAsset;

        [HideInInspector] public bool alreadyLoadedAsset = false;
        [HideInInspector] public GameObject loadedAssetRootGameObject;

        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool MaterialsOnly = false;
        [SerializeField] private int RetryCount = 10;
        [SerializeField] private float RetryTimeout = 2.0f;
        [SerializeField] public Shader shaderOverride = null;
        private bool initialVisibility = true;
        private AssetIdConverter fileToHashConverter;

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
        private GLTFSceneImporterAsync sceneImporter { get; set; }
        private Camera mainCamera;
        private IWebRequestController webRequestController;
        private bool prioritizeDownload = false;
        private string baseUrl = "";
        
        private Action<Mesh> meshCreatedCallback;
        private Action<Renderer> rendererCreatedCallback;

        public Action OnSuccess { get { return OnFinishedLoadingAsset; } set { OnFinishedLoadingAsset = value; } }

        public Action<Exception> OnFail { get { return OnFailedLoadingAsset; } set { OnFailedLoadingAsset = value; } }

        public void Initialize(IWebRequestController webRequestController, GLTFThrottlingCounter throttlingCounter)
        {
            this.webRequestController = webRequestController;
            this.throttlingCounter = throttlingCounter;
        }

        public void LoadAsset(string baseUrl, string incomingURI = "", string idPrefix = "", bool loadEvenIfAlreadyLoaded = false, GLTFComponent.Settings settings = null, AssetIdConverter fileToHashConverter = null)
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
                Debug.LogError($"ERROR: trying to load GLTF when is already loading {idPrefix}");
                return;
            }

            alreadyDecrementedRefCount = false;
            state = State.NONE;
            mainCamera = Camera.main;
            this.baseUrl = baseUrl ?? "";

            if (settings != null)
            {
                ApplySettings(settings);
            }

            this.fileToHashConverter = fileToHashConverter;
            this.settings = settings;
            
            /*if ( throttlingCounter != null )
            {
                loadingRoutine = this.StartThrottledCoroutine(
                    enumerator: LoadAssetCoroutine(settings),
                    onException: OnFail_Internal,
                    timeBudgetCounter: throttlingCounter.EvaluateTimeBudget);
            }
            else
            {
                loadingRoutine = this.StartThrowingCoroutine(
                    enumerator: LoadAssetCoroutine(settings),
                    onException: OnFail_Internal);
            }*/
        }

        private async void Start()
        {
            await LoadAssetCoroutine(settings);
        }
        public void RegisterCallbacks(Action<Mesh> meshCreated, Action<Renderer> rendererCreated)
        {
            rendererCreatedCallback = rendererCreated;
            meshCreatedCallback = meshCreated;
        }

        void ApplySettings(GLTFComponent.Settings settings)
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

            this.addMaterialsToPersistentCaching = settings.addMaterialsToPersistentCaching;
        }

        private void OnFail_Internal(Exception exception)
        {
            if (state == State.FAILED)
                return;

            state = State.FAILED;
            
            CoroutineStarter.Stop(loadingRoutine);
            loadingRoutine = null;
            
            DecrementDownloadCount();

            OnFailedLoadingAsset?.Invoke(exception);

            if (exception != null)
            {
                if (exception is IndexOutOfRangeException)
                {
                    Destroy(gameObject);
                }

                Debug.Log($"GLTF Failure {exception} ... url = {this.GLTFUri}\n{exception.StackTrace}");
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

        private async UniTask LoadAssetCoroutine(GLTFComponent.Settings settings)
        {
            if (!string.IsNullOrEmpty(GLTFUri))
            {
                if (VERBOSE)
                {
                    Debug.Log("LoadAssetCoroutine() GLTFUri ->" + GLTFUri);
                }

                sceneImporter = null;
                ILoaderAsync loader = null;

                Destroy(loadedAssetRootGameObject);

                try
                {
                    if (UseStream)
                    {
                        throw new NotImplementedException();
                        /*// Path.Combine treats paths that start with the separator character
                        // as absolute paths, ignoring the first path passed in. This removes
                        // that character to properly handle a filename written with it.
                        GLTFUri = GLTFUri.TrimStart(new[]
                            {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar});
                        string fullPath = Path.Combine(Application.streamingAssetsPath, GLTFUri);
                        string directoryPath = URIHelper.GetDirectoryName(fullPath);
                        loader = new GLTFFileLoader(directoryPath);
                        sceneImporter = new GLTFSceneImporterAsync(
                            null,
                            Path.GetFileName(GLTFUri),
                            loader
                        );*/
                    }
                    else
                    {
                        loader = new WebRequestLoaderAsync(baseUrl, webRequestController, fileToHashConverter);

                        string id = string.IsNullOrEmpty(idPrefix) ? GLTFUri : idPrefix;

                        sceneImporter = new GLTFSceneImporterAsync(
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
                    sceneImporter.isMultithreaded = DataStore.i.multithreading.enabled.Get();
                    sceneImporter.useMaterialTransition = UseVisualFeedback;
                    sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;
                    sceneImporter.LoadingTextureMaterial = LoadingTextureMaterial;
                    sceneImporter.initialVisibility = initialVisibility;
                    sceneImporter.addMaterialsToPersistentCaching = addMaterialsToPersistentCaching;
                    sceneImporter.forceGPUOnlyMesh = settings.forceGPUOnlyMesh && DataStore.i.featureFlags.flags.Get()
                        .IsFeatureEnabled(FeatureFlag.GPU_ONLY_MESHES);
                    
                    sceneImporter.OnMeshCreated += meshCreatedCallback;
                    sceneImporter.OnRendererCreated += rendererCreatedCallback;

                    float time = Time.realtimeSinceStartup;

                    queueCount++;

                    state = State.QUEUED;
                    downloadQueueHandler.Queue(this);

                    await WaitUntil( () => downloadQueueHandler.CanDownload(this));

                    queueCount--;
                    totalDownloadedCount++;

                    IncrementDownloadCount();

                    state = State.DOWNLOADING;
                    downloadQueueHandler.Dequeue(this);

                    if (transform != null)
                    {
                        await sceneImporter.LoadScene(-1);

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
                    DecrementDownloadCount();
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

                    alreadyLoadedAsset = true;

                    CoroutineStarter.Stop(loadingRoutine);
                    loadingRoutine = null;

                    if ( state == State.COMPLETED )
                        OnFinishedLoadingAsset?.Invoke();
                    else
                        OnFailedLoadingAsset?.Invoke(new Exception($"GLTF state finished as: {state}"));

                    Destroy(loadingPlaceholder);
                    Destroy(this);
                }
            }
            else
            {
                Debug.Log("couldn't load GLTF because url is empty");
            }
        }
        
        private async UniTask WaitUntil(Func<bool> condition)
        {
            async UniTask action()
            {
                while (!condition())
                {
                    await UniTask.Delay(5);
                }
            }

            await action();
        }

        public void Load(string url) { throw new NotImplementedException(); }

        public void SetPrioritized() { prioritizeDownload = true; }
        private GLTFComponent.Settings settings;

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

            if (state == State.QUEUED)
            {
                queueCount--;
            }

            downloadQueueHandler.Dequeue(this);

            if (!alreadyLoadedAsset && loadingRoutine != null)
            {
                OnFail_Internal(null);
                return;
            }

            DecrementDownloadCount();
        }

        bool IDownloadQueueElement.ShouldPrioritizeDownload() { return prioritizeDownload; }

        bool IDownloadQueueElement.ShouldForceDownload() { return mainCamera == null; }

        float IDownloadQueueElement.GetSqrDistance()
        {
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 gltfPosition = transform.position;
            gltfPosition.y = cameraPosition.y;

            return (gltfPosition - cameraPosition).sqrMagnitude;
        }
    }
}