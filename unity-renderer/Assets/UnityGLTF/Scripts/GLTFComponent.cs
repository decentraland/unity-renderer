using System;
using System.Collections.Generic;
using System.Threading;
using DCL;
using UnityEngine;
using UnityGLTF.Loader;
using UnityGLTF.Scripts;
using Cysharp.Threading.Tasks;

namespace UnityGLTF
{
    /// <summary>
    /// Component to load a GLTF scene with
    /// </summary>
    public class GLTFComponent : MonoBehaviour, IGLTFComponent
    {
        private static bool VERBOSE = false;

        public static int downloadingCount;
        public static event Action OnDownloadingProgressUpdate;

        public static int queueCount;

        private static readonly BaseVariable<int> maxSimultaneousDownloads = DataStore.i.performance.maxDownloads;
        private static readonly DownloadQueueHandler downloadQueueHandler = new DownloadQueueHandler(maxSimultaneousDownloads.Get(), () => downloadingCount);

        public class Settings
        {
            public bool? useVisualFeedback;
            public bool? initialVisibility;
            public Shader shaderOverride;
            public bool addMaterialsToPersistentCaching;
            public bool forceGPUOnlyMesh = true;
        }

        public string GLTFUri = null;
        public string idPrefix = null;

        public bool UseStream = false;
        public bool UseVisualFeedback = true;
        private bool addMaterialsToPersistentCaching = true;

        public int MaximumLod = 300;
        public int Timeout = 8;
        public Material LoadingTextureMaterial;
        public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;
        private IThrottlingCounter throttlingCounter;

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
        private Action<Exception> OnFailedLoadingAsset;

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
        private GLTFSceneImporter sceneImporter { get; set; }
        private Camera mainCamera;
        private IWebRequestController webRequestController;
        private bool prioritizeDownload = false;
        private string baseUrl = "";

        private Action<Mesh> meshCreatedCallback;
        private Action<Renderer> rendererCreatedCallback;

        private Settings settings;

        private  CancellationTokenSource ctokenSource;

        public Action OnSuccess { get { return OnFinishedLoadingAsset; } set { OnFinishedLoadingAsset = value; } }

        public Action<Exception> OnFail { get { return OnFailedLoadingAsset; } set { OnFailedLoadingAsset = value; } }

        public void Initialize( IWebRequestController webRequestController, IThrottlingCounter throttlingCounter)
        {
            this.webRequestController = webRequestController;
            this.throttlingCounter = throttlingCounter;
        }

        public void LoadAsset(string baseUrl, string incomingURI = "", string idPrefix = "", bool loadEvenIfAlreadyLoaded = false, Settings settings = null, AssetIdConverter fileToHashConverter = null)
        {
            ctokenSource = new CancellationTokenSource();

            if (alreadyLoadedAsset && !loadEvenIfAlreadyLoaded)
            {
                return;
            }

            if (!string.IsNullOrEmpty(incomingURI))
                GLTFUri = incomingURI;

            if (!string.IsNullOrEmpty(idPrefix))
                this.idPrefix = idPrefix;

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
            CancellationToken cancellationToken = ctokenSource.Token;

            Internal_LoadAsset(settings, cancellationToken)
                .Forget();
        }

        public void RegisterCallbacks(Action<Mesh> meshCreated, Action<Renderer> rendererCreated)
        {
            rendererCreatedCallback = rendererCreated;
            meshCreatedCallback = meshCreated;
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

            this.addMaterialsToPersistentCaching = settings.addMaterialsToPersistentCaching;
        }

        private void OnFail_Internal(Exception exception)
        {
            if (state == State.FAILED)
                return;

            state = State.FAILED;

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

        private async UniTaskVoid Internal_LoadAsset(Settings settings, CancellationToken token)
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif

            if (!string.IsNullOrEmpty(GLTFUri))
            {
                if (VERBOSE)
                {
                    Debug.Log("LoadAssetCoroutine() GLTFUri ->" + GLTFUri);
                }

                sceneImporter = null;
                ILoader loader = null;

                Destroy(loadedAssetRootGameObject);

                try
                {
                    loader = new WebRequestLoader(baseUrl, webRequestController, fileToHashConverter);
                    string id = string.IsNullOrEmpty(idPrefix) ? GLTFUri : idPrefix;

                    SetupSceneImporter(settings, id, loader);

                    EnqueueDownload();

                    await UniTask.WaitUntil( () => downloadQueueHandler.CanDownload(this), cancellationToken: token);
                    token.ThrowIfCancellationRequested();

                    DequeueDownload();

                    IncrementDownloadCount();
                    state = State.DOWNLOADING;

                    if (transform != null)
                    {
                        await sceneImporter.LoadScene(token);
                        token.ThrowIfCancellationRequested();

                        // Override the shaders on all materials if a shader is provided
                        if (shaderOverride != null)
                        {
                            OverrideShaders();
                        }
                    }

                    state = State.COMPLETED;
                    alreadyLoadedAsset = true;
                    DecrementDownloadCount();
                }
                catch (Exception e) when (!(e is OperationCanceledException))
                {
#if UNITY_STANDALONE || UNITY_EDITOR
                    if (DataStore.i.common.isApplicationQuitting.Get())
                        return;
#endif

                    Debug.LogException(e);
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
                            animationsEstimatedSize = sceneImporter.animationsEstimatedSize;
                            meshesEstimatedSize = sceneImporter.meshesEstimatedSize;

                            sceneImporter?.Dispose();
                            sceneImporter = null;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        if ( state == State.COMPLETED)
                            OnFinishedLoadingAsset?.Invoke();
                        else
                            OnFailedLoadingAsset?.Invoke(new Exception($"GLTF state finished as: {state}"));
                    }
                    
                    CleanUp();
                    Destroy(loadingPlaceholder);
                    Destroy(this);
                }
            }
            else
            {
                Debug.Log("couldn't load GLTF because url is empty");
            }
        }
        private void OverrideShaders()
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                renderer.sharedMaterial.shader = shaderOverride;
            }
        }

        private void SetupSceneImporter(Settings settings, string id, ILoader loader)
        {
            sceneImporter = new GLTFSceneImporter(
                id,
                GLTFUri,
                loader,
                throttlingCounter
            );

            if (sceneImporter.CreatedObject != null)
            {
                Destroy(sceneImporter.CreatedObject);
            }

            sceneImporter.SceneParent = gameObject.transform;
            sceneImporter.Collider = Collider;
            sceneImporter.maximumLod = MaximumLod;
            sceneImporter.useMaterialTransition = UseVisualFeedback;
            sceneImporter.maxTextureSize = DataStore.i.textureSize.gltfMaxSize.Get();
            sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;
            sceneImporter.LoadingTextureMaterial = LoadingTextureMaterial;
            sceneImporter.initialVisibility = initialVisibility;
            sceneImporter.addMaterialsToPersistentCaching = addMaterialsToPersistentCaching;

            sceneImporter.forceGPUOnlyMesh = false;

            sceneImporter.OnMeshCreated += meshCreatedCallback;
            sceneImporter.OnRendererCreated += rendererCreatedCallback;
        }
        private void DequeueDownload()
        {
            queueCount--;
            downloadQueueHandler.Dequeue(this);
        }
        private void EnqueueDownload()
        {
            queueCount++;
            state = State.QUEUED;
            downloadQueueHandler.Queue(this);
        }

        public void Load(string url) { throw new NotImplementedException(); }

        public void SetPrioritized() { prioritizeDownload = true; }

        private long animationsEstimatedSize;
        private long meshesEstimatedSize;
        public long GetAnimationClipMemorySize() { return animationsEstimatedSize; }

        public long GetMeshesMemorySize() { return meshesEstimatedSize; }

        private void OnDestroy()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif
            CleanUp();
            
            if (state != State.COMPLETED)
            {
                ctokenSource.Cancel();
                ctokenSource.Dispose();
            }
        }
        private void CleanUp()
        {
            if (state == State.QUEUED)
            {
                DequeueDownload();
            }

            if (state == State.DOWNLOADING)
            {
                DecrementDownloadCount();
            }

            if (!alreadyLoadedAsset)
            {
                OnFail_Internal(null);
            }

            state = State.NONE;
        }

        bool IDownloadQueueElement.ShouldPrioritizeDownload() { return prioritizeDownload; }

        bool IDownloadQueueElement.ShouldForceDownload()
        {
#if UNITY_EDITOR_OSX
            return false;
#endif
            return mainCamera == null;
        }

        float IDownloadQueueElement.GetSqrDistance()
        {
            if (mainCamera == null || transform == null)
                return float.MaxValue;

            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 gltfPosition = transform.position;
            gltfPosition.y = cameraPosition.y;

            return (gltfPosition - cameraPosition).sqrMagnitude;
        }
    }
}