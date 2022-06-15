using GLTF;
using GLTF.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
#if !WINDOWS_UWP
using System.Threading;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityGLTF.Cache;
using UnityGLTF.Extensions;
using UnityGLTF.Loader;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using WaitUntil = UnityEngine.WaitUntil;
#if !WINDOWS_UWP
using ThreadPriority = System.Threading.ThreadPriority;
#endif
using WrapMode = UnityEngine.WrapMode;

namespace UnityGLTF
{
    public struct MeshConstructionData
    {
        public MeshPrimitive Primitive { get; set; }
        public Dictionary<string, AttributeAccessor> MeshAttributes { get; set; }
    }

    public class UnityMeshData
    {
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public Vector2[] Uv1;
        public Vector2[] Uv2;
        public Vector2[] Uv3;
        public Vector2[] Uv4;
        public Color[] Colors;
        public int[] Triangles;
        public Vector4[] Tangents;
        public BoneWeight[] BoneWeights;
    }

    public struct TextureCreationSettings
    {
        public FilterMode filterMode;
        public TextureWrapMode wrapMode;
        public bool generateMipmaps;
        public bool uploadToGpu;
        public bool linear;
    }

    /// <summary>
    /// Converts gltf animation data to unity
    /// </summary>
    public delegate float[] ValuesConvertion(NumericArray data, int frame);

    public class GLTFSceneImporter : IDisposable
    {
        public static bool VERBOSE = false;
        public static bool PROFILING_ENABLED = false;

        public enum ColliderType
        {
            None,
            Box,
            Mesh,
            MeshConvex
        }

        /// <summary>
        /// Maximum LOD
        /// </summary>
        public int maximumLod = 300;

        /// <summary>
        /// The parent transform for the created GameObject
        /// </summary>
        public Transform SceneParent { get; set; }

        /// <summary>
        /// The last created object
        /// </summary>
        public GameObject CreatedObject { get; private set; }

        /// <summary>
        /// Adds colliders to primitive objects when created
        /// </summary>
        public ColliderType Collider { get; set; }

        /// <summary>
        /// Override for the shader to use on created materials
        /// </summary>
        public string CustomShaderName { get; set; }

        /// <summary>
        /// Material to be applied while textures are being loaded
        /// </summary>
        public Material LoadingTextureMaterial { get; set; }

        /// <summary>
        /// Initial state of the meshes
        /// </summary>
        public bool initialVisibility { get; set; }

        private static bool renderingIsDisabled => !CommonScriptableObjects.rendererState.Get();

        public bool forceGPUOnlyMesh = true;
        public bool forceGPUOnlyTex = true;
        
        // this setting forces coroutines to be ran in a single call
        public bool forceSyncCoroutines = false;

        private bool useMaterialTransitionValue = true;

        public bool importSkeleton = true;

        public bool useMaterialTransition { get => useMaterialTransitionValue && !renderingIsDisabled; set => useMaterialTransitionValue = value; }

        public int maxTextureSize = 512;
        private const float SAME_KEYFRAME_TIME_DELTA = 0.0001f;

        protected struct GLBStream
        {
            public Stream Stream;
            public long StartPosition;
        }

        private SkipFrameIfDepletedTimeBudget skipFrameIfDepletedTimeBudget;

        public bool addImagesToPersistentCaching = true;
        public bool addMaterialsToPersistentCaching = true;

        protected GameObject _lastLoadedScene;
        protected readonly GLTFMaterial DefaultMaterial = new GLTFMaterial();
        protected MaterialCacheData _defaultLoadedMaterial = null;

        protected string _gltfFileName;
        private readonly IThrottlingCounter throttlingCounter;
        protected GLBStream _gltfStream;
        protected GLTFRoot _gltfRoot;
        protected AssetCache _assetCache;
        protected ILoader _loader;
        protected bool _isRunning = false;
        private bool _isCompleted = false;
        public bool IsRunning => _isRunning;
        public bool IsCompleted => _isCompleted;

        public string id;

        struct NodeId_Like
        {
            public int Id;
            public Node Value;
        }

        List<NodeId_Like> nodesWithMeshes = new List<NodeId_Like>();

        //NOTE(Brian): Using primitives in Dictionaries would produce unneeded boxing. Improve later.
        Dictionary<int, int> nodeToParent = new Dictionary<int, int>();

        //NOTE(Brian): If a material is reused between many primitives, this prevents that the ref count
        //             goes above one, as this would prevent the material to unload properly.

        //             This is because the GLTF cache cleanup setup expects ref owners to be the entire GLTF object.
        HashSet<Material> usedMaterials = new HashSet<Material>();

        const int KEYFRAME_SIZE = 32;
        public long meshesEstimatedSize { get; private set; }
        public long animationsEstimatedSize { get; private set; }

        /// <summary>
        /// Creates a GLTFSceneBuilder object which will be able to construct a scene based off a url
        /// </summary>
        /// <param name="gltfFileName">glTF file relative to data loader path</param>
        /// <param name="externalDataLoader">Loader to load external data references</param>
        /// <param name="throttlingCounter">Used to determine if we should throttle some Main-thread only processes</param>
        public GLTFSceneImporter(string id, string gltfFileName, ILoader externalDataLoader, IThrottlingCounter throttlingCounter) : this(externalDataLoader)
        {
            _gltfFileName = gltfFileName;
            this.throttlingCounter = throttlingCounter;
            this.id = string.IsNullOrEmpty(id) ? gltfFileName : id;
        }

        public GLTFSceneImporter(string id, GLTFRoot rootNode, ILoader externalDataLoader, IThrottlingCounter throttlingCounter, Stream gltfStream = null) : this(externalDataLoader)
        {
            this.id = id;
            _gltfRoot = rootNode;
            _loader = externalDataLoader;
            this.throttlingCounter = throttlingCounter;

            skipFrameIfDepletedTimeBudget = new SkipFrameIfDepletedTimeBudget();

            if (gltfStream != null)
            {
                _gltfStream = new GLBStream { Stream = gltfStream, StartPosition = gltfStream.Position };
            }
        }

        private GLTFSceneImporter(ILoader externalDataLoader)
        {
            _loader = externalDataLoader;
            skipFrameIfDepletedTimeBudget = new SkipFrameIfDepletedTimeBudget();
        }

        public void Dispose()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif

            //NOTE(Brian): If the coroutine is interrupted and the local streaming list contains something,
            //             we must clean the static list or other GLTFSceneImporter instances might get stuck.
            int streamingImagesLocalListCount = streamingImagesLocalList.Count;

            for (int i = 0; i < streamingImagesLocalListCount; i++)
            {
                string s = streamingImagesLocalList[i];

                if (streamingImagesStaticList.Contains(s))
                {
                    streamingImagesStaticList.Remove(s);
                }
            }

            if (_assetCache != null)
            {
                _assetCache.Dispose();
                _assetCache = null;
            }
        }

        public GameObject lastLoadedScene { get { return _lastLoadedScene; } }

        public static Action<float> OnPerformanceFinish;
        public event Action<Mesh> OnMeshCreated;
        public event Action<Renderer> OnRendererCreated;

        /// <summary>
        /// Loads a glTF Scene into the LastLoadedScene field
        /// </summary>
        /// <param name="sceneIndex">The scene to load, If the index isn't specified, we use the default index in the file. Failing that we load index 0.</param>
        /// <param name="showSceneObj"></param>
        /// <param name="onLoadComplete">Callback function for when load is completed</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public async Task LoadScene(CancellationToken token, int sceneIndex = -1, bool showSceneObj = true)
        {
            try
            {
                PerformanceAnalytics.GLTFTracker.TrackLoading();
                token.ThrowIfCancellationRequested();

                lock (this)
                {
                    if (_isRunning)
                    {
                        throw new GLTFLoadException("Cannot call LoadScene while GLTFSceneImporter is already running");
                    }

                    _isRunning = true;
                }

                if (_gltfRoot == null)
                {
                    if (VERBOSE)
                    {
                        Debug.Log("LoadScene() GLTF File Name -> " + _gltfFileName);
                    }

                    await LoadJsonStream(token);
                }

                token.ThrowIfCancellationRequested();

                float profiling = 0, frames = 0, jsonProfiling = 0;

                if (PROFILING_ENABLED)
                {
                    jsonProfiling = Time.realtimeSinceStartup;
                }

                if (_gltfRoot == null)
                {
                    await ParseJson(token);
                }

                token.ThrowIfCancellationRequested();

                if (PROFILING_ENABLED)
                {
                    jsonProfiling = ((Time.realtimeSinceStartup - jsonProfiling) * 1000f);
                }

                _assetCache ??= new AssetCache(_gltfRoot);

                if (PROFILING_ENABLED)
                {
                    profiling = Time.realtimeSinceStartup;
                    frames = Time.frameCount;
                }

                await CreateScene(sceneIndex, showSceneObj, token);

                token.ThrowIfCancellationRequested();

                if (PROFILING_ENABLED)
                {
                    if (VERBOSE)
                    {
                        Debug.Log($"{_gltfFileName} >>> Load finished in {((Time.realtimeSinceStartup - profiling) * 1000f)} ms... frames = {(Time.frameCount - frames)} (json = {jsonProfiling})");
                    }

                    OnPerformanceFinish?.Invoke(Time.realtimeSinceStartup - profiling);
                }

                MaterialTransitionController[] matTransitions = CreatedObject.GetComponentsInChildren<MaterialTransitionController>(true);

                if (matTransitions != null && matTransitions.Length > 0)
                {
                    //NOTE(Brian): Wait for the MaterialTransition to finish before copying the object to the library
                    await UniTask.WaitUntil(() => IsTransitionFinished(matTransitions), cancellationToken: token);

                }

                if (!importSkeleton)
                {
                    foreach (var skeleton in skeletonGameObjects)
                    {
                        if (Application.isPlaying)
                            Object.Destroy(skeleton);
                        else
                            Object.DestroyImmediate(skeleton);
                    }
                }
                
                PerformanceAnalytics.GLTFTracker.TrackLoaded();
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    PerformanceAnalytics.GLTFTracker.TrackCancelled();
                }
                else
                {
                    PerformanceAnalytics.GLTFTracker.TrackFailed();

                    if (Application.isPlaying)
                        throw;
                    else
                    {
                        Debug.LogException(e);

                        throw;
                    }
                }
            }
            finally
            {
                lock (this)
                {
                    _isRunning = false;
                    _isCompleted = true;

                }
            }
        }
        private static bool IsTransitionFinished(MaterialTransitionController[] matTransitions)
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

        /// <summary>
        /// Initializes the top-level created node by adding an instantiated GLTF object component to it,
        /// so that it can cleanup after itself properly when destroyed
        /// </summary>
        private void InitializeGltfTopLevelObject()
        {
            instantiatedGLTFObject = CreatedObject.AddComponent<InstantiatedGLTFObject>();

            instantiatedGLTFObject.CachedData = new RefCountedCacheData
            {
                MaterialCache = _assetCache.MaterialCache,
                TextureCache = _assetCache.TextureCache,
                MeshCache = _assetCache.MeshCache,
                animationCache = _assetCache.AnimationCache
            };

        }

        private async UniTask ConstructBufferData(Node node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            MeshId mesh = node.Mesh;

            if (mesh != null)
            {
                if (mesh.Value.Primitives != null)
                {
                    await ConstructMeshAttributes(mesh.Value, mesh, token);
                }
            }

            if (node.Children != null)
            {
                foreach (NodeId child in node.Children)
                {
                    await ConstructBufferData(child.Value, token);
                }
            }

            const string msft_LODExtName = MSFT_LODExtensionFactory.EXTENSION_NAME;
            MSFT_LODExtension lodsextension = null;

            if (_gltfRoot.ExtensionsUsed != null
                && _gltfRoot.ExtensionsUsed.Contains(msft_LODExtName)
                && node.Extensions != null
                && node.Extensions.ContainsKey(msft_LODExtName))
            {
                lodsextension = node.Extensions[msft_LODExtName] as MSFT_LODExtension;

                if (lodsextension != null && lodsextension.MeshIds.Count > 0)
                {
                    for (int i = 0; i < lodsextension.MeshIds.Count; i++)
                    {
                        int lodNodeId = lodsextension.MeshIds[i];
                        await ConstructBufferData(_gltfRoot.Nodes[lodNodeId], token);
                    }
                }
            }
        }

        private async UniTask ConstructMeshAttributes(GLTFMesh mesh, MeshId meshId, CancellationToken token)
        {
            int meshIdIndex = meshId.Id;

            if (_assetCache.MeshCache[meshIdIndex] == null)
            {
                _assetCache.MeshCache[meshIdIndex] = new MeshCacheData[mesh.Primitives.Count];
            }

            for (int i = 0; i < mesh.Primitives.Count; ++i)
            {
                MeshPrimitive primitive = mesh.Primitives[i];

                if (_assetCache.MeshCache[meshIdIndex][i] == null)
                {
                    _assetCache.MeshCache[meshIdIndex][i] = new MeshCacheData();
                }

                if (_assetCache.MeshCache[meshIdIndex][i].MeshAttributes.Count == 0)
                {
                    await ConstructMeshAttributes(primitive, meshIdIndex, i, token);

                    if (primitive.Material != null && !pendingImageBuffers.Contains(primitive.Material.Value))
                    {
                        pendingImageBuffers.Add(primitive.Material.Value);
                    }
                }
            }
        }

        HashSet<GLTFMaterial> pendingImageBuffers = new HashSet<GLTFMaterial>();

        static List<string> streamingImagesStaticList = new List<string>();
        List<string> streamingImagesLocalList = new List<string>();

        protected async UniTask ConstructImageBuffer(GLTFTexture texture, int textureIndex, bool linear, CancellationToken token)
        {
            int sourceId = GetTextureSourceId(texture);
            GLTFImage image = _gltfRoot.Images[sourceId];

            if (image.Uri != null && addImagesToPersistentCaching)
            {
                while (streamingImagesStaticList.Contains(image.Uri))
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
                }
            }

            TextureCreationSettings settings = GetTextureCreationSettingsForTexture(texture, linear);
            string imageId = GenerateImageId(image.Uri, sourceId, settings);

            if ((image.Uri == null || !PersistentAssetCache.HasImage(imageId)) && _assetCache.ImageStreamCache[sourceId] == null)
            {
                // we only load the streams if not a base64 uri, meaning the data is in the uri
                if (image.Uri != null && !URIHelper.IsBase64Uri(image.Uri))
                {
                    streamingImagesStaticList.Add(image.Uri);
                    streamingImagesLocalList.Add(image.Uri);

                    await _loader.LoadStream(image.Uri, token);
                    _assetCache.ImageStreamCache[sourceId] = _loader.LoadedStream;

                    streamingImagesStaticList.Remove(image.Uri);
                    streamingImagesLocalList.Remove(image.Uri);
                }
                else if (image.Uri == null && image.BufferView != null && _assetCache.BufferCache[image.BufferView.Value.Buffer.Id] == null)
                {
                    int bufferIndex = image.BufferView.Value.Buffer.Id;
                    await ConstructBuffer(_gltfRoot.Buffers[bufferIndex], bufferIndex, token);
                }
            }

            _assetCache.TextureCache[textureIndex] = new TextureCacheData();
        }

        protected IEnumerator WaitUntilEnum(WaitUntil waitUntil) { yield return waitUntil; }

        protected IEnumerator EmptyYieldEnum() { yield break; }

        private async UniTask LoadJsonStream(CancellationToken token)
        {

            await _loader.LoadStream(_gltfFileName, token);

            token.ThrowIfCancellationRequested();

            if (_loader.LoadedStream == null)
            {
                throw new Exception($"Failed to Load Json Stream {_gltfFileName}");
            }

            _gltfStream.Stream = _loader.LoadedStream;
            _gltfStream.StartPosition = 0;
        }

        private async UniTask ParseJson(CancellationToken cancellationToken)
        {
            if (DataStore.i.performance.multithreading.Get())
            {
                await TaskUtils.Run( () => GLTFParser.ParseJson(_gltfStream.Stream, out _gltfRoot, _gltfStream.StartPosition), cancellationToken: cancellationToken);
            }
            else
            {
                _gltfRoot ??= new GLTFRoot();

                cancellationToken.ThrowIfCancellationRequested();

                IEnumerator coroutine = GLTFParser.ParseJsonDelayed(_gltfStream.Stream, _gltfRoot, _gltfStream.StartPosition);

                if (forceSyncCoroutines)
                {
                    CoroutineUtils.RunCoroutineSync(coroutine);
                }
                else
                {
                    await TaskUtils.RunThrottledCoroutine(coroutine, exception => throw exception, throttlingCounter.EvaluateTimeBudget);
                }
            }

            if (_gltfRoot == null)
            {
                throw new Exception($"Failed to parse GLTF {_gltfFileName}");
            }
        }

        /// <summary>
        /// Creates a scene based off loaded JSON. Includes loading in binary and image data to construct the meshes required.
        /// </summary>
        /// <param name="sceneIndex">The bufferIndex of scene in gltf file to load</param>
        /// <returns></returns>
        private async UniTask CreateScene(int sceneIndex = -1, bool showSceneObj = true, CancellationToken cancellationToken = default)
        {
            GLTFScene scene;

            if (sceneIndex >= 0 && sceneIndex < _gltfRoot.Scenes.Count)
            {
                scene = _gltfRoot.Scenes[sceneIndex];
            }
            else
            {
                scene = _gltfRoot.GetDefaultScene();
            }

            if (scene == null)
            {
                throw new GLTFLoadException("No default scene in gltf file.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            CreatedObject = new GameObject(string.IsNullOrEmpty(scene.Name) ? ("GLTFScene") : scene.Name);
            _lastLoadedScene = CreatedObject;

            InitializeGltfTopLevelObject();

            await ConstructScene(scene, showSceneObj, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (SceneParent != null)
                CreatedObject.transform.SetParent(SceneParent, false);

            _lastLoadedScene = CreatedObject;
        }

        protected async UniTask ConstructBuffer(GLTFBuffer buffer, int bufferIndex, CancellationToken token)
        {
            if (buffer.Uri == null)
            {
                _assetCache.BufferCache[bufferIndex] = ConstructBufferFromGLB(bufferIndex);
            }
            else
            {
                Stream bufferDataStream = null;
                var uri = buffer.Uri;

                byte[] bufferData;
                URIHelper.TryParseBase64(uri, out bufferData);

                if (bufferData != null)
                {
                    bufferDataStream = new MemoryStream(bufferData, 0, bufferData.Length, false, true);
                }
                else
                {
                    if (PersistentAssetCache.HasBuffer(buffer.Uri, id))
                    {
                        bufferDataStream = PersistentAssetCache.GetBuffer(buffer.Uri, id).stream;
                    }
                    else
                    {
                        await _loader.LoadStream(buffer.Uri, token);
                        bufferDataStream = _loader.LoadedStream;
                        PersistentAssetCache.AddBuffer(buffer.Uri, id, bufferDataStream);
                    }
                }

                _assetCache.BufferCache[bufferIndex] = new BufferCacheData
                {
                    Stream = bufferDataStream
                };
            }
        }

        private async UniTask ConstructImage(  TextureCreationSettings settings, GLTFImage image, int imageCacheIndex, CancellationToken cancellationToken) //, bool markGpuOnly = false, bool linear = true)
        {
            if (_assetCache.ImageCache[imageCacheIndex] != null)
                return;

            if (image.Uri == null)
            {
                //NOTE(Zak): This fixes current issues of concurrent texture loading,
                //           but it's possible that it would happen again in the future.
                //           If that happens, we'll have implement some locking behavior for concurrent
                //           import calls.

                //NOTE(Brian): We can't yield between the stream creation and the stream.Read because
                //             another coroutine can modify the stream Position in the next frame.
                var bufferView = image.BufferView.Value;
                var data = new byte[bufferView.ByteLength];

                BufferCacheData bufferContents = _assetCache.BufferCache[bufferView.Buffer.Id];
                bufferContents.Stream.Position = bufferView.ByteOffset + bufferContents.ChunkOffset;

                if (forceSyncCoroutines)
                {
                    bufferContents.Stream.Read(data, 0, data.Length);
                }
                else
                {
                    await bufferContents.Stream.ReadAsync(data, 0, data.Length, cancellationToken);
                }

                ConstructUnityTexture(settings, data, imageCacheIndex);
            }
            else
            {
                string uri = image.Uri;
                byte[] bufferData = null;

                await TaskUtils.Run(() => URIHelper.TryParseBase64(uri, out bufferData), cancellationToken: cancellationToken);

                Stream stream = null;

                if (bufferData != null)
                {
                    stream = new MemoryStream(bufferData, 0, bufferData.Length, false, true);
                }
                else
                {
                    stream = _assetCache.ImageStreamCache[imageCacheIndex];
                }

                await ConstructUnityTexture(settings, stream, imageCacheIndex);
            }
        }

        protected void ConstructUnityTexture(TextureCreationSettings settings, byte[] buffer, int imageCacheIndex)
        {
            Texture2D texture = new Texture2D(0, 0, TextureFormat.ARGB32, settings.generateMipmaps, settings.linear);

            //  NOTE: the second parameter of LoadImage() marks non-readable, but we can't mark it until after we call Apply()
            texture.LoadImage(buffer, false);

            // We need to keep compressing in UNITY_EDITOR for the Asset Bundles Converter
#if !UNITY_STANDALONE || UNITY_EDITOR
            if ( Application.isPlaying )
            {
                //NOTE(Brian): This breaks importing in editor mode
                texture.Compress(false);
            }
#endif

            texture.wrapMode = settings.wrapMode;
            texture.filterMode = settings.filterMode;
            texture.Apply(settings.generateMipmaps, settings.uploadToGpu);

            // Resizing must be the last step to avoid breaking the texture when copying with Graphics.CopyTexture()
            _assetCache.ImageCache[imageCacheIndex] = TextureHelpers.ClampSize(texture, maxTextureSize, settings.linear);
        }

        protected virtual async UniTask ConstructUnityTexture(TextureCreationSettings settings, Stream stream, int imageCacheIndex)
        {
            if (stream == null)
                return;

            if (stream is MemoryStream)
            {
                using (MemoryStream memoryStream = stream as MemoryStream)
                {
                    ConstructUnityTexture(settings, memoryStream.ToArray(), imageCacheIndex);
                }
            }

            if (stream is FileStream fileStream)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    ConstructUnityTexture(settings, memoryStream.ToArray(), imageCacheIndex);
                }
            }
        }

        protected virtual async UniTask ConstructMeshAttributes(MeshPrimitive primitive, int meshID, int primitiveIndex, CancellationToken token)
        {
            if (_assetCache.MeshCache[meshID][primitiveIndex].MeshAttributes.Count == 0)
            {
                Dictionary<string, AttributeAccessor> attributeAccessors = new Dictionary<string, AttributeAccessor>(primitive.Attributes.Count + 1);

                foreach (var attributePair in primitive.Attributes)
                {
                    BufferId bufferIdPair = attributePair.Value.Value.BufferView.Value.Buffer;
                    GLTFBuffer buffer = bufferIdPair.Value;
                    int bufferId = bufferIdPair.Id;

                    // on cache miss, load the buffer
                    if (_assetCache.BufferCache[bufferId] == null)
                    {
                        await ConstructBuffer(buffer, bufferId, token);
                    }

                    AttributeAccessor attributeAccessor = new AttributeAccessor
                    {
                        AccessorId = attributePair.Value,
                        Stream = _assetCache.BufferCache[bufferId].Stream,
                        Offset = (uint) _assetCache.BufferCache[bufferId].ChunkOffset
                    };

                    attributeAccessors[attributePair.Key] = attributeAccessor;
                }

                if (primitive.Indices != null)
                {
                    int bufferId = primitive.Indices.Value.BufferView.Value.Buffer.Id;

                    if (_assetCache.BufferCache[bufferId] == null)
                    {
                        await ConstructBuffer(primitive.Indices.Value.BufferView.Value.Buffer.Value, bufferId, token);
                    }

                    AttributeAccessor indexBuilder = new AttributeAccessor
                    {
                        AccessorId = primitive.Indices,
                        Stream = _assetCache.BufferCache[bufferId].Stream,
                        Offset = (uint) _assetCache.BufferCache[bufferId].ChunkOffset
                    };

                    attributeAccessors[SemanticProperties.INDICES] = indexBuilder;
                }

                await TaskUtils.Run( () =>
                {
                    GLTFHelpers.BuildMeshAttributes(ref attributeAccessors);
                    TransformAttributes(ref attributeAccessors);
                }, cancellationToken: token);

                _assetCache.MeshCache[meshID][primitiveIndex].MeshAttributes = attributeAccessors;
            }
        }

        protected void TransformAttributes(ref Dictionary<string, AttributeAccessor> attributeAccessors)
        {
            // Flip vectors and triangles to the Unity coordinate system.
            if (attributeAccessors.ContainsKey(SemanticProperties.POSITION))
            {
                AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.POSITION];
                SchemaExtensions.ConvertVector3CoordinateSpaceFast(ref attributeAccessor);
            }

            if (attributeAccessors.ContainsKey(SemanticProperties.INDICES))
            {
                AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.INDICES];
                SchemaExtensions.FlipFaces(ref attributeAccessor);
            }

            if (attributeAccessors.ContainsKey(SemanticProperties.NORMAL))
            {
                AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.NORMAL];
                SchemaExtensions.ConvertVector3CoordinateSpaceFast(ref attributeAccessor);
            }

            // TexCoord goes from 0 to 3 to match GLTFHelpers.BuildMeshAttributes
            for (int i = 0; i < 4; i++)
            {
                if (attributeAccessors.ContainsKey(SemanticProperties.TexCoord[i]))
                {
                    AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.TexCoord[i]];
                    SchemaExtensions.FlipTexCoordArrayV(ref attributeAccessor);
                }
            }

            if (attributeAccessors.ContainsKey(SemanticProperties.TANGENT))
            {
                AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.TANGENT];
                SchemaExtensions.ConvertVector4CoordinateSpaceFast(ref attributeAccessor);
            }
        }

        #region Animation

        static string RelativePathFrom(Transform self, Transform root)
        {
            var path = new List<String>();

            for (var current = self; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return String.Join("/", path.ToArray());
                }

                path.Insert(0, current.name);
            }

            throw new Exception("no RelativePath");
        }

        protected virtual void BuildAnimationSamplers(GLTFAnimation animation, int animationId)
        {
            // look up expected data types
            var typeMap = new Dictionary<int, string>();

            foreach (var channel in animation.Channels)
            {
                typeMap[channel.Sampler.Id] = channel.Target.Path.ToString();
            }

            var samplers = _assetCache.AnimationCache[animationId].Samplers;

            var samplersByType = new Dictionary<string, List<AttributeAccessor>>
            {
                { "time", new List<AttributeAccessor>(animation.Samplers.Count) }
            };

            for (var i = 0; i < animation.Samplers.Count; i++)
            {
                // no sense generating unused samplers
                if (!typeMap.ContainsKey(i))
                {
                    continue;
                }

                var samplerDef = animation.Samplers[i];

                samplers[i].Interpolation = samplerDef.Interpolation;

                // set up input accessors
                int bufferId = samplerDef.Input.Value.BufferView.Value.Buffer.Id;
                BufferCacheData bufferCacheData = _assetCache.BufferCache[bufferId];

                AttributeAccessor attributeAccessor = new AttributeAccessor
                {
                    AccessorId = samplerDef.Input,
                    Stream = bufferCacheData.Stream,
                    Offset = bufferCacheData.ChunkOffset
                };

                samplers[i].Input = attributeAccessor;
                samplersByType["time"].Add(attributeAccessor);

                // set up output accessors
                int anotherBufferId = samplerDef.Output.Value.BufferView.Value.Buffer.Id;
                bufferCacheData = _assetCache.BufferCache[anotherBufferId];

                attributeAccessor = new AttributeAccessor
                {
                    AccessorId = samplerDef.Output,
                    Stream = bufferCacheData.Stream,
                    Offset = bufferCacheData.ChunkOffset
                };

                samplers[i].Output = attributeAccessor;

                if (!samplersByType.ContainsKey(typeMap[i]))
                {
                    samplersByType[typeMap[i]] = new List<AttributeAccessor>();
                }

                samplersByType[typeMap[i]].Add(attributeAccessor);
            }

            // populate attributeAccessors with buffer data
            GLTFHelpers.BuildAnimationSamplers(ref samplersByType);
        }

        async UniTask ProcessCurves(Transform root, GameObject[] nodes, AnimationClip clip, GLTFAnimation animation, AnimationCacheData animationCache, CancellationToken cancellationToken)
        {
            foreach (AnimationChannel channel in animation.Channels)
            {
                AnimationSamplerCacheData samplerCache = animationCache.Samplers[channel.Sampler.Id];

                if (channel.Target.Node == null)
                {
                    // If a channel doesn't have a target node, then just skip it.
                    // This is legal and is present in one of the asset generator models, but means that animation doesn't actually do anything.
                    // https://github.com/KhronosGroup/glTF-Asset-Generator/tree/master/Output/Positive/Animation_NodeMisc
                    // Model 08
                    continue;
                }

                var node = nodes[channel.Target.Node.Id];
                string relativePath = RelativePathFrom(node.transform, root);

                NumericArray input = samplerCache.Input.AccessorContent,
                             output = samplerCache.Output.AccessorContent;

                string[] propertyNames;
                Vector3 coordinateSpaceConversionScale = new Vector3(-1, 1, 1);

                switch (channel.Target.Path)
                {
                    case GLTFAnimationChannelPath.translation:
                        propertyNames = new string[] { "localPosition.x", "localPosition.y", "localPosition.z" };

                        await SetAnimationCurve(clip, relativePath, propertyNames, input, output,
                            samplerCache.Interpolation, typeof(Transform),
                            (data, frame) =>
                            {
                                var position = data.AsVec3s[frame].ToUnityVector3Convert();

                                return new float[] { position.x, position.y, position.z };
                            }, cancellationToken);

                        break;

                    case GLTFAnimationChannelPath.rotation:
                        propertyNames = new string[] { "localRotation.x", "localRotation.y", "localRotation.z", "localRotation.w" };

                        await SetAnimationCurve(clip, relativePath, propertyNames, input, output,
                            samplerCache.Interpolation, typeof(Transform),
                            (data, frame) =>
                            {
                                var rotation = data.AsVec4s[frame];

                                var quaternion = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w).ToUnityQuaternionConvert();

                                return new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
                            },
                            cancellationToken,
                            // NOTE(Brian): Unity makes some conversion to eulers on AnimationClip.SetCurve
                            // that breaks the keyframe optimization.
                            optimizeKeyframes: false);

                        break;

                    case GLTFAnimationChannelPath.scale:
                        propertyNames = new string[] { "localScale.x", "localScale.y", "localScale.z" };

                        await SetAnimationCurve(clip, relativePath, propertyNames, input, output,
                            samplerCache.Interpolation, typeof(Transform),
                            (data, frame) =>
                            {
                                var scale = data.AsVec3s[frame];

                                return new float[] { scale.x, scale.y, scale.z };
                            }, cancellationToken);

                        break;

                    case GLTFAnimationChannelPath.weights:
                        // TODO: add support for blend shapes/morph targets
                        break;

                    default:
                        Debug.LogWarning("Cannot read GLTF animation path");

                        break;
                } // switch target type
            } // foreach channel

            // This is needed to ensure certain rotations don't get messed up
            clip.EnsureQuaternionContinuity();
        }

        protected async UniTask SetAnimationCurve(AnimationClip clip,
            string relativePath,
            string[] propertyNames,
            NumericArray input,
            NumericArray output,
            InterpolationType mode,
            Type curveType,
            ValuesConvertion getConvertedValues,
            CancellationToken cancellationToken,
            bool optimizeKeyframes = true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var channelCount = propertyNames.Length;
            var frameCount = input.AsFloats.Length;

            // copy all the key frame data to cache
            Keyframe[][] keyframes = new Keyframe[channelCount][];

            for (var ci = 0; ci < channelCount; ++ci)
            {
                keyframes[ci] = new Keyframe[frameCount];
            }

            for (var i = 0; i < frameCount; ++i)
            {
                var time = input.AsFloats[i];

                float[] values = null;
                float[] inTangents = null;
                float[] outTangents = null;

                if (mode == InterpolationType.CUBICSPLINE)
                {
                    // For cubic spline, the output will contain 3 values per keyframe; inTangent, dataPoint, and outTangent.
                    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#appendix-c-spline-interpolation

                    var cubicIndex = i * 3;
                    inTangents = getConvertedValues(output, cubicIndex);
                    values = getConvertedValues(output, cubicIndex + 1);
                    outTangents = getConvertedValues(output, cubicIndex + 2);
                }
                else
                {
                    // For other interpolation types, the output will only contain one value per keyframe
                    values = getConvertedValues(output, i);
                }

                for (var ci = 0; ci < channelCount; ++ci)
                {
                    if (mode == InterpolationType.CUBICSPLINE)
                    {
                        keyframes[ci][i] = new Keyframe(time, values[ci], inTangents[ci], outTangents[ci]);
                    }
                    else
                    {
                        keyframes[ci][i] = new Keyframe(time, values[ci]);
                    }
                }
            }

            await TaskUtils.Run( () =>
            {
                for (var ci = 0; ci < channelCount; ++ci)
                {
                    // For cubic spline interpolation, the inTangents and outTangents are already explicitly defined.
                    // For the rest, set them appropriately.
                    if (mode != InterpolationType.CUBICSPLINE)
                    {
                        for (var i = 0; i < keyframes[ci].Length; i++)
                            SetTangentMode(keyframes[ci], i, mode);
                    }
                }
            }, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (optimizeKeyframes)
            {
                await TaskUtils.Run( () =>
                {
                    for (var ci = 0; ci < channelCount; ++ci)
                    {
                        keyframes[ci] = OptimizeKeyFrames(keyframes[ci]);
                    }
                }, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                // Setting the curves by the keyframe length order fixes weird index overrun issue:
                // https://forum.unity.com/threads/animationutility-seteditorcurve-cant-add-curve-with-one-keyfram.247372/
                var orderedKeyframes = keyframes.OrderBy(x => { return x.Length; }).ToArray();

                foreach (var keyframeCollection in orderedKeyframes)
                {
                    int index = Array.IndexOf(keyframes, keyframeCollection);

                    // copy all key frames data to animation curve and add it to the clip
                    AnimationCurve curve = new AnimationCurve(keyframeCollection);
                    clip.SetCurve(relativePath, curveType, propertyNames[index], curve);
                    animationsEstimatedSize += KEYFRAME_SIZE * keyframeCollection.Length;
                    animationsEstimatedSize += relativePath.Length;
                    animationsEstimatedSize += propertyNames[index].Length;
                }
            }
            else
            {
                for (var ci = 0; ci < channelCount; ++ci)
                {
                    // copy all key frames data to animation curve and add it to the clip
                    AnimationCurve curve = new AnimationCurve(keyframes[ci]);
                    clip.SetCurve(relativePath, curveType, propertyNames[ci], curve);
                    animationsEstimatedSize += KEYFRAME_SIZE * keyframes[ci].Length;
                    animationsEstimatedSize += relativePath.Length;
                    animationsEstimatedSize += propertyNames[ci].Length;
                }
            }
        }

        private static float GetDiffAngle(float a1, float a2) { return Mathf.PI - Mathf.Abs(Mathf.Abs(a1 - a2) - Mathf.PI); }

        public static Keyframe[] OptimizeKeyFrames(Keyframe[] rawKeyframes)
        {
            if (rawKeyframes.Length <= 2)
                return rawKeyframes;

            List<Keyframe> result = new List<Keyframe>(2);

            result.Add(rawKeyframes[0]);

            const float TANGENT_THRESHOLD = 0.1f;

            for (int i = 1; i < rawKeyframes.Length - 1; i++)
            {
                Keyframe nextKey = rawKeyframes[i + 1];
                Keyframe prevKey = rawKeyframes[i - 1];
                Keyframe curKey = rawKeyframes[i];

                float angCurToNext = Mathf.Atan2(nextKey.value - curKey.value, nextKey.time - curKey.time);
                float angCurToPrev = Mathf.Atan2(curKey.value - prevKey.value, curKey.time - prevKey.time);

                float curOutAngle = Mathf.Atan(curKey.outTangent);
                float curInAngle = Mathf.Atan(curKey.inTangent);

                //NOTE(Brian): Collinearity tests. Small value = more collinear.

                //NOTE(Brian): curr keyframe out tangent point against path towards the next keyframe.
                float curOutDiff = GetDiffAngle(curOutAngle, angCurToNext);

                //NOTE(Brian): curr keyframe in tangent point against path towards the prev keyframe.
                float curInDiff = GetDiffAngle(curInAngle, angCurToPrev);

                //NOTE(Brian): next keyframe in tangent point against path towards the curr keyframe.
                float nextInDiff = GetDiffAngle(Mathf.Atan(nextKey.inTangent), angCurToNext);

                //NOTE(Brian): prev keyframe out tangent point against path towards the curr keyframe.
                float prevOutDiff = GetDiffAngle(Mathf.Atan(prevKey.outTangent), angCurToPrev);

                //NOTE(Brian): test if both tangents for the current keyframe are collinear
                //             (i.e. don't cull broken curves).
                float sameDiff = GetDiffAngle(curInAngle, curOutAngle);

                float tangentDeviation = Mathf.Abs(curOutDiff + curInDiff + nextInDiff + prevOutDiff + sameDiff);

                if (tangentDeviation > TANGENT_THRESHOLD)
                    result.Add(curKey);
            }

            result.Add(rawKeyframes[rawKeyframes.Length - 1]);

            return result.ToArray();
        }

        private static void SetTangentMode(Keyframe[] keyframes, int keyframeIndex, InterpolationType interpolation)
        {
            var key = keyframes[keyframeIndex];

            switch (interpolation)
            {
                case InterpolationType.CATMULLROMSPLINE:
                    key.inTangent = 0;
                    key.outTangent = 0;

                    break;
                case InterpolationType.LINEAR:
                    key.inTangent = GetCurveKeyframeLeftLinearSlope(keyframes, keyframeIndex);
                    key.outTangent = GetCurveKeyframeLeftLinearSlope(keyframes, keyframeIndex + 1);

                    break;
                case InterpolationType.STEP:
                    key.inTangent = float.PositiveInfinity;
                    key.outTangent = float.PositiveInfinity;

                    break;
            }
        }

        private static float GetCurveKeyframeLeftLinearSlope(Keyframe[] keyframes, int keyframeIndex)
        {
            if (keyframeIndex <= 0 || keyframeIndex >= keyframes.Length)
                return 0;

            var valueDelta = keyframes[keyframeIndex].value - keyframes[keyframeIndex - 1].value;
            var timeDelta = keyframes[keyframeIndex].time - keyframes[keyframeIndex - 1].time;

            // As Unity doesn't allow us to put two keyframes in with the same time, we set the time delta to a minimum.
            if (timeDelta <= 0)
                timeDelta = SAME_KEYFRAME_TIME_DELTA;

            return valueDelta / timeDelta;
        }

        protected AnimationClip ConstructClip(int animationId, out GLTFAnimation animation, out AnimationCacheData animationCache)
        {
            animation = _gltfRoot.Animations[animationId];

            animationCache = _assetCache.AnimationCache[animationId];

            if (animationCache == null)
            {
                animationCache = new AnimationCacheData(animation.Samplers.Count);
                _assetCache.AnimationCache[animationId] = animationCache;
            }
            else if (animationCache.LoadedAnimationClip != null)
            {
                return animationCache.LoadedAnimationClip;
            }

            // unpack accessors
            BuildAnimationSamplers(animation, animationId);

            // init clip
            AnimationClip clip = new AnimationClip
            {
                name = animation.Name ?? string.Format("animation:{0}", animationId)
            };

            _assetCache.AnimationCache[animationId].LoadedAnimationClip = clip;

            // Animation instance memory overhead
            animationsEstimatedSize += 20;

            // needed because Animator component is unavailable at runtime
            clip.legacy = true;

            return clip;
        }

        #endregion

        protected virtual async UniTask ConstructScene(GLTFScene scene, bool showSceneObj, CancellationToken token)
        {
            CreatedObject.SetActive(showSceneObj);
            CreatedObject.transform.SetParent(SceneParent, false);
            CreatedObject.transform.localPosition = Vector3.zero;
            CreatedObject.transform.localRotation = Quaternion.identity;
            CreatedObject.transform.localScale = Vector3.one;

            if (scene?.Nodes != null)
            {
                for (int i = 0; i < scene.Nodes.Count; ++i)
                {
                    token.ThrowIfCancellationRequested();

                    NodeId node = scene.Nodes[i];

                    Node nodeToLoad = _gltfRoot.Nodes[node.Id];

                    await ConstructBufferData(nodeToLoad, token);
                    await ConstructNode(nodeToLoad, node.Id, token, CreatedObject.transform);
                }
            }

            foreach (var gltfMaterial in pendingImageBuffers)
            {
                token.ThrowIfCancellationRequested();

                await ConstructMaterialImageBuffers(gltfMaterial, token);
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < nodesWithMeshes.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                NodeId_Like nodeId = nodesWithMeshes[i];
                Node node = nodeId.Value;

                await ConstructMesh(mesh: node.Mesh.Value,
                    parent: _assetCache.NodeCache[nodeId.Id].transform,
                    meshId: node.Mesh.Id,
                    skin: node.Skin != null ? node.Skin.Value : null, token);

                if ( stopWatch.ElapsedMilliseconds > 5 && !forceSyncCoroutines)
                {
                    await UniTask.Yield();
                    stopWatch.Restart();
                }
            }

            stopWatch.Stop();
            token.ThrowIfCancellationRequested();

            if (_gltfRoot.Animations != null && _gltfRoot.Animations.Count > 0)
            {
                // create the AnimationClip that will contain animation data
                // NOTE (Pravs): Khronos GLTFLoader sets the animationComponent as 'enabled = false' but we don't do that so that we can find the component when needed.
                Animation animation = CreatedObject.AddComponent<Animation>();
                animation.playAutomatically = true;
                animation.cullingType = AnimationCullingType.AlwaysAnimate;

                int animationsCount = _gltfRoot.Animations.Count;

                for (int i = 0; i < animationsCount; ++i)
                {
                    token.ThrowIfCancellationRequested();

                    GLTFAnimation gltfAnimation = null;
                    AnimationCacheData animationCache = null;

                    await LoadAnimationBufferData(_gltfRoot.Animations[i], i, token);

                    AnimationClip clip = ConstructClip(i, out gltfAnimation, out animationCache);

                    await ProcessCurves(CreatedObject.transform, _assetCache.NodeCache, clip, gltfAnimation, animationCache, token);

                    clip.wrapMode = WrapMode.Loop;

                    animation.AddClip(clip, clip.name);

                    if (i == 0)
                    {
                        animation.clip = clip;
                    }
                }
            }
        }

        async UniTask LoadAnimationBufferData(GLTFAnimation animation, int animationId, CancellationToken token)
        {
            var typeMap = new Dictionary<int, string>();

            foreach (var channel in animation.Channels)
            {
                typeMap[channel.Sampler.Id] = channel.Target.Path.ToString();
            }

            for (var i = 0; i < animation.Samplers.Count; i++)
            {
                // no sense generating unused samplers
                if (!typeMap.ContainsKey(i))
                {
                    continue;
                }

                var samplerDef = animation.Samplers[i];

                // set up input accessors
                int bufferId = samplerDef.Input.Value.BufferView.Value.Buffer.Id;
                BufferCacheData bufferCacheData = _assetCache.BufferCache[bufferId];

                if (bufferCacheData == null)
                {
                    await ConstructBuffer(_gltfRoot.Buffers[bufferId], bufferId, token);
                    bufferCacheData = _assetCache.BufferCache[bufferId];

                    if (VERBOSE)
                    {
                        Debug.Log("A GLTF Animation buffer cache data is null, skipping animation sampler for " + animation.Name);
                    }
                }

                // set up output accessors
                int anotherBufferId = samplerDef.Output.Value.BufferView.Value.Buffer.Id;
                bufferCacheData = _assetCache.BufferCache[anotherBufferId];

                if (bufferCacheData == null)
                {
                    await ConstructBuffer(_gltfRoot.Buffers[anotherBufferId], anotherBufferId, token);
                    bufferCacheData = _assetCache.BufferCache[anotherBufferId];

                    if (VERBOSE)
                    {
                        Debug.Log("A GLTF Animation buffer cache data is null, skipping animation sampler for " + animation.Name);
                    }
                }
            }
        }

        protected virtual async UniTask ConstructNode(Node node, int nodeIndex, CancellationToken cancellationToken, Transform parent = null)
        {
            if (_assetCache.NodeCache[nodeIndex] != null)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var nodeObj = new GameObject(string.IsNullOrEmpty(node.Name) ? ("GLTFNode" + nodeIndex) : node.Name);

            Vector3 position, scale;
            Quaternion rotation;

            node.GetUnityTRSProperties(out position, out rotation, out scale);

            nodeObj.transform.localPosition = position;
            nodeObj.transform.localRotation = rotation;
            nodeObj.transform.localScale = scale;

            _assetCache.NodeCache[nodeIndex] = nodeObj;

            if (node.Children != null)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    NodeId child = node.Children[i];

                    if (!nodeToParent.ContainsKey(child.Id))
                    {
                        nodeToParent.Add(child.Id, nodeIndex);
                    }

                    // todo blgross: replace with an iterartive solution
                    await ConstructNode(child.Value, child.Id, cancellationToken, nodeObj.transform);
                }
            }

            Transform finalParent = parent ?? SceneParent;
            nodeObj.transform.SetParent(finalParent, false);

            const string msft_LODExtName = MSFT_LODExtensionFactory.EXTENSION_NAME;
            MSFT_LODExtension lodsextension = null;

            if (_gltfRoot.ExtensionsUsed != null
                && _gltfRoot.ExtensionsUsed.Contains(msft_LODExtName)
                && node.Extensions != null
                && node.Extensions.ContainsKey(msft_LODExtName))
            {
                lodsextension = node.Extensions[msft_LODExtName] as MSFT_LODExtension;

                if (lodsextension != null && lodsextension.MeshIds.Count > 0)
                {
                    LOD[] lods = new LOD[lodsextension.MeshIds.Count + 1];
                    List<double> lodCoverage = lodsextension.GetLODCoverage(node);

                    cancellationToken.ThrowIfCancellationRequested();
                    var lodGroupNodeObj = new GameObject(string.IsNullOrEmpty(node.Name) ? ("GLTFNode_LODGroup" + nodeIndex) : node.Name);

                    lodGroupNodeObj.SetActive(false);

                    nodeObj.transform.SetParent(lodGroupNodeObj.transform, false);

                    MeshRenderer[] childRenders = nodeObj.GetComponentsInChildren<MeshRenderer>();
                    lods[0] = new LOD(GetLodCoverage(lodCoverage, 0), childRenders);

                    LODGroup lodGroup = lodGroupNodeObj.AddComponent<LODGroup>();

                    for (int i = 0; i < lodsextension.MeshIds.Count; i++)
                    {
                        int lodNodeId = lodsextension.MeshIds[i];

                        await ConstructNode(_gltfRoot.Nodes[lodNodeId], lodNodeId, cancellationToken, lodGroupNodeObj.transform);

                        int lodIndex = i + 1;
                        GameObject lodNodeObj = _assetCache.NodeCache[lodNodeId];
                        lodNodeObj.transform.SetParent(lodGroupNodeObj.transform, false);

                        childRenders = lodNodeObj.GetComponentsInChildren<MeshRenderer>();
                        lods[lodIndex] = new LOD(GetLodCoverage(lodCoverage, lodIndex), childRenders);
                    }

                    lodGroup.SetLODs(lods);
                    lodGroup.RecalculateBounds();
                    lodGroupNodeObj.SetActive(true);
                    _assetCache.NodeCache[nodeIndex] = lodGroupNodeObj;
                }
            }

            if (node.Mesh != null)
            {
                nodesWithMeshes.Add(new NodeId_Like { Id = nodeIndex, Value = node });
            }
        }

        float GetLodCoverage(List<double> lodcoverageExtras, int lodIndex)
        {
            if (lodcoverageExtras != null && lodIndex < lodcoverageExtras.Count)
            {
                return (float) lodcoverageExtras[lodIndex];
            }
            else
            {
                return 1.0f / (lodIndex + 2);
            }
        }

        private bool NeedsSkinnedMeshRenderer(MeshPrimitive primitive, Skin skin) { return HasBones(skin) || HasBlendShapes(primitive); }

        private bool HasBones(Skin skin) { return skin != null; }

        private bool HasBlendShapes(MeshPrimitive primitive) { return primitive.Targets != null; }

        async UniTask FindSkeleton(int nodeId, Action<int> found)
        {
            if (nodeToParent.ContainsKey(nodeId))
            {
                await FindSkeleton(nodeToParent[nodeId], found);
            }

            found.Invoke(nodeId);
        }

        protected virtual async UniTask SetupBones(Skin skin, MeshPrimitive primitive, SkinnedMeshRenderer renderer, GameObject primitiveObj, Mesh curMesh, CancellationToken token)
        {
            var boneCount = skin.Joints.Count;
            Transform[] bones = new Transform[boneCount];

            int bufferId = skin.InverseBindMatrices.Value.BufferView.Value.Buffer.Id;

            // on cache miss, load the buffer
            if (_assetCache.BufferCache[bufferId] == null)
            {
                await ConstructBuffer(_gltfRoot.Buffers[bufferId], bufferId, token);
            }

            AttributeAccessor attributeAccessor = new AttributeAccessor
            {
                AccessorId = skin.InverseBindMatrices,
                Stream = _assetCache.BufferCache[bufferId].Stream,
                Offset = _assetCache.BufferCache[bufferId].ChunkOffset
            };

            GLTFHelpers.BuildBindPoseSamplers(ref attributeAccessor);

            Matrix4x4[] gltfBindPoses = attributeAccessor.AccessorContent.AsMatrix4x4s;
            Matrix4x4[] bindPoses = new Matrix4x4[skin.Joints.Count];

            int skeletonId = 0;

            if (skin.Skeleton != null)
            {
                skeletonId = skin.Skeleton.Id;
            }
            else
            {
                await FindSkeleton(skin.Joints[0].Id, (id) => skeletonId = id);
            }

            for (int i = 0; i < boneCount; i++)
            {
                bones[i] = _assetCache.NodeCache[skin.Joints[i].Id].transform;
                bindPoses[i] = gltfBindPoses[i].ToMatrix4x4Convert();
            }

            curMesh.bindposes = bindPoses;
            renderer.bones = bones;
            renderer.rootBone = _assetCache.NodeCache[skeletonId].transform;

            if (!skeletonGameObjects.Contains(renderer.rootBone.gameObject))
                skeletonGameObjects.Add(renderer.rootBone.gameObject);
        }

        HashSet<GameObject> skeletonGameObjects = new HashSet<GameObject>();
        private InstantiatedGLTFObject instantiatedGLTFObject;

        private BoneWeight[] CreateBoneWeightArray(Vector4[] joints, Vector4[] weights, int vertCount)
        {
            NormalizeBoneWeightArray(weights);

            BoneWeight[] boneWeights = new BoneWeight[vertCount];

            for (int i = 0; i < vertCount; i++)
            {
                boneWeights[i].boneIndex0 = (int) joints[i].x;
                boneWeights[i].boneIndex1 = (int) joints[i].y;
                boneWeights[i].boneIndex2 = (int) joints[i].z;
                boneWeights[i].boneIndex3 = (int) joints[i].w;

                boneWeights[i].weight0 = weights[i].x;
                boneWeights[i].weight1 = weights[i].y;
                boneWeights[i].weight2 = weights[i].z;
                boneWeights[i].weight3 = weights[i].w;
            }

            return boneWeights;
        }

        /// <summary>
        /// Ensures each bone weight influences applied to the vertices add up to 1
        /// </summary>
        /// <param name="weights">Bone weight array</param>
        private void NormalizeBoneWeightArray(Vector4[] weights)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                var weightSum = (weights[i].x + weights[i].y + weights[i].z + weights[i].w);

                if (weightSum > 0.001f || weightSum < -0.001f)
                {
                    weights[i] /= weightSum;
                }
            }
        }

        protected virtual async UniTask ConstructPrimitiveMaterials(  GLTFMesh mesh, int meshId, int primitiveIndex, CancellationToken cancellationToken)
        {
            var primitive = mesh.Primitives[primitiveIndex];
            int materialIndex = primitive.Material?.Id ?? -1;

            GameObject primitiveObj = _assetCache.MeshCache[meshId][primitiveIndex].PrimitiveGO;

            Renderer renderer = primitiveObj.GetComponent<Renderer>();

            cancellationToken.ThrowIfCancellationRequested();

            //// NOTE(Brian): Texture loading
            if (useMaterialTransition && initialVisibility)
            {
                var matController = primitiveObj.AddComponent<MaterialTransitionController>();
                await DownloadAndConstructMaterial(primitive, materialIndex, renderer, matController, cancellationToken);
            }
            else
            {
                if (LoadingTextureMaterial != null)
                {
                    renderer.sharedMaterial = LoadingTextureMaterial;
                }

                await DownloadAndConstructMaterial(primitive, materialIndex, renderer, null, cancellationToken);

                if (LoadingTextureMaterial == null)
                {
                    primitiveObj.SetActive(true);
                }
            }
        }

        protected virtual async UniTask ConstructMesh(GLTFMesh mesh, Transform parent, int meshId, Skin skin, CancellationToken cancellationToken)
        {
            bool isColliderMesh = parent.name.Contains("_collider");

            _assetCache.MeshCache[meshId] ??= new MeshCacheData[mesh.Primitives.Count];

            for (int i = 0; i < mesh.Primitives.Count; ++i)
            {
                var primitive = mesh.Primitives[i];

                await ConstructMeshPrimitive(primitive, meshId, i, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                var primitiveObj = new GameObject("Primitive");
                primitiveObj.transform.SetParent(parent, false);
                primitiveObj.SetActive(useMaterialTransition || LoadingTextureMaterial != null);

                _assetCache.MeshCache[meshId][i].PrimitiveGO = primitiveObj;

                Mesh curMesh = _assetCache.MeshCache[meshId][i].LoadedMesh;
                MeshFilter meshFilter = primitiveObj.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = curMesh;

                if (!isColliderMesh)
                {
                    Renderer renderer;

                    if (NeedsSkinnedMeshRenderer(primitive, skin))
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = primitiveObj.AddComponent<SkinnedMeshRenderer>();
                        skinnedMeshRenderer.sharedMesh = curMesh;
                        skinnedMeshRenderer.quality = SkinQuality.Auto;
                        renderer = skinnedMeshRenderer;
                        renderer.enabled = initialVisibility;

                        if (HasBones(skin))
                        {
                            await SetupBones(skin, primitive, skinnedMeshRenderer, primitiveObj, curMesh, cancellationToken);
                        }

                        OnRendererCreated?.Invoke(skinnedMeshRenderer);
                    }
                    else
                    {
                        MeshRenderer meshRenderer = primitiveObj.AddComponent<MeshRenderer>();
                        renderer = meshRenderer;
                        renderer.enabled = initialVisibility;

                        OnRendererCreated?.Invoke(meshRenderer);
                    }

                    await ConstructPrimitiveMaterials(mesh, meshId, i, cancellationToken);
                }
                else
                {
                    primitiveObj.SetActive(true);
                }

                switch (Collider)
                {
                    case ColliderType.Box:
                        var boxCollider = primitiveObj.AddComponent<BoxCollider>();
                        boxCollider.center = curMesh.bounds.center;
                        boxCollider.size = curMesh.bounds.size;

                        break;
                    case ColliderType.Mesh:
                        var meshCollider = primitiveObj.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = curMesh;

                        break;
                    case ColliderType.MeshConvex:
                        var meshConvexCollider = primitiveObj.AddComponent<MeshCollider>();
                        meshConvexCollider.sharedMesh = curMesh;
                        meshConvexCollider.convex = true;

                        break;
                }
            }
        }

        async UniTask DownloadAndConstructMaterial(MeshPrimitive primitive, int materialIndex, Renderer renderer, MaterialTransitionController matController, CancellationToken cancellationToken)
        {
            bool shouldUseDefaultMaterial = primitive.Material == null;

            GLTFMaterial materialToLoad = shouldUseDefaultMaterial ? DefaultMaterial : primitive.Material.Value;

            if ((shouldUseDefaultMaterial && _defaultLoadedMaterial == null) ||
                (!shouldUseDefaultMaterial && _assetCache.MaterialCache[materialIndex] == null))
            {
                await ConstructMaterial(materialToLoad, shouldUseDefaultMaterial ? -1 : materialIndex, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            MaterialCacheData materialCacheData =
                materialIndex >= 0 ? _assetCache.MaterialCache[materialIndex] : _defaultLoadedMaterial;

            Material material = materialCacheData.GetContents();

            bool alreadyUsedMaterial = usedMaterials.Contains(material);

            if (!alreadyUsedMaterial)
            {
                usedMaterials.Add(material);
            }

            SRPBatchingHelper.OptimizeMaterial(material);

            if (matController != null)
            {
                matController.OnDidFinishLoading(material);
            }
            else
            {
                renderer.sharedMaterial = material;
            }
        }

        protected virtual async UniTask ConstructMeshPrimitive(MeshPrimitive primitive, int meshID, int primitiveIndex, CancellationToken cancellationToken)
        {
            _assetCache.MeshCache[meshID][primitiveIndex] ??= new MeshCacheData();

            if (_assetCache.MeshCache[meshID][primitiveIndex].LoadedMesh == null)
            {
                var meshAttributes = _assetCache.MeshCache[meshID][primitiveIndex].MeshAttributes;

                var meshConstructionData = new MeshConstructionData
                {
                    Primitive = primitive,
                    MeshAttributes = meshAttributes
                };

                UnityMeshData unityMeshData = ConvertAccessorsToUnityTypes(meshConstructionData);

                cancellationToken.ThrowIfCancellationRequested();

                IEnumerator coroutine = ConstructUnityMesh(meshConstructionData, meshID, primitiveIndex, unityMeshData);

                if (forceSyncCoroutines)
                {
                    CoroutineUtils.RunCoroutineSync(coroutine);
                }
                else
                {
                    await TaskUtils.RunThrottledCoroutine(coroutine, exception => throw exception, throttlingCounter.EvaluateTimeBudget)
                                   .AttachExternalCancellation(cancellationToken);
                }
            }
        }

        private UnityMeshData ConvertAccessorsToUnityTypes(MeshConstructionData meshConstructionData)
        {
            // todo optimize: There are multiple copies being performed to turn the buffer data into mesh data. Look into reducing them
            MeshPrimitive primitive = meshConstructionData.Primitive;
            Dictionary<string, AttributeAccessor> meshAttributes = meshConstructionData.MeshAttributes;

            int vertexCount = (int) primitive.Attributes[SemanticProperties.POSITION].Value.Count;

            return new UnityMeshData
            {
                Vertices = primitive.Attributes.ContainsKey(SemanticProperties.POSITION)
                    ? meshAttributes[SemanticProperties.POSITION].AccessorContent.AsVertices
                    : null,

                Normals = primitive.Attributes.ContainsKey(SemanticProperties.NORMAL)
                    ? meshAttributes[SemanticProperties.NORMAL].AccessorContent.AsNormals
                    : null,

                Uv1 = primitive.Attributes.ContainsKey(SemanticProperties.TEXCOORD_0)
                    ? meshAttributes[SemanticProperties.TEXCOORD_0].AccessorContent.AsTexcoords
                    : null,

                Uv2 = primitive.Attributes.ContainsKey(SemanticProperties.TEXCOORD_1)
                    ? meshAttributes[SemanticProperties.TEXCOORD_1].AccessorContent.AsTexcoords
                    : null,

                Uv3 = primitive.Attributes.ContainsKey(SemanticProperties.TEXCOORD_2)
                    ? meshAttributes[SemanticProperties.TEXCOORD_2].AccessorContent.AsTexcoords
                    : null,

                Uv4 = primitive.Attributes.ContainsKey(SemanticProperties.TEXCOORD_3)
                    ? meshAttributes[SemanticProperties.TEXCOORD_3].AccessorContent.AsTexcoords
                    : null,

                Colors = primitive.Attributes.ContainsKey(SemanticProperties.COLOR_0)
                    ? meshAttributes[SemanticProperties.COLOR_0].AccessorContent.AsColors
                    : null,

                Triangles = primitive.Indices != null
                    ? meshAttributes[SemanticProperties.INDICES].AccessorContent.AsInts
                    : MeshPrimitive.GenerateIndices(vertexCount),

                Tangents = primitive.Attributes.ContainsKey(SemanticProperties.TANGENT)
                    ? meshAttributes[SemanticProperties.TANGENT].AccessorContent.AsTangents
                    : null,

                BoneWeights = meshAttributes.ContainsKey(SemanticProperties.WEIGHTS_0) && meshAttributes.ContainsKey(SemanticProperties.JOINTS_0)
                    ? CreateBoneWeightArray(meshAttributes[SemanticProperties.JOINTS_0].AccessorContent.AsVec4s,
                        meshAttributes[SemanticProperties.WEIGHTS_0].AccessorContent.AsVec4s, vertexCount)
                    : null
            };
        }

        protected virtual async UniTask ConstructMaterialImageBuffers(GLTFMaterial def, CancellationToken token)
        {
            var tasks = new List<UniTask>(8);

            if (def.PbrMetallicRoughness != null)
            {
                var pbr = def.PbrMetallicRoughness;

                if (pbr.BaseColorTexture != null)
                {
                    var textureId = pbr.BaseColorTexture.Index;
                    tasks.Add(ConstructImageBuffer(textureId.Value, textureId.Id, false, token));
                }

                if (pbr.MetallicRoughnessTexture != null)
                {
                    var textureId = pbr.MetallicRoughnessTexture.Index;
                    tasks.Add(ConstructImageBuffer(textureId.Value, textureId.Id, true, token));
                }
            }

            if (def.CommonConstant != null)
            {
                if (def.CommonConstant.LightmapTexture != null)
                {
                    var textureId = def.CommonConstant.LightmapTexture.Index;
                    tasks.Add(ConstructImageBuffer(textureId.Value, textureId.Id, true, token));
                }
            }

            if (def.NormalTexture != null)
            {
                var textureId = def.NormalTexture.Index;
                tasks.Add(ConstructImageBuffer(textureId.Value, textureId.Id, true, token));
            }

            if (def.OcclusionTexture != null)
            {
                var textureId = def.OcclusionTexture.Index;

                if (!(def.PbrMetallicRoughness != null
                      && def.PbrMetallicRoughness.MetallicRoughnessTexture != null
                      && def.PbrMetallicRoughness.MetallicRoughnessTexture.Index.Id == textureId.Id))
                {
                    tasks.Add(ConstructImageBuffer(textureId.Value, textureId.Id, true, token));
                }
            }

            if (def.EmissiveTexture != null)
            {
                var textureId = def.EmissiveTexture.Index;
                tasks.Add(ConstructImageBuffer(textureId.Value, textureId.Id, false, token));
            }

            // pbr_spec_gloss extension
            const string specGlossExtName = KHR_materials_pbrSpecularGlossinessExtensionFactory.EXTENSION_NAME;

            if (def.Extensions != null && def.Extensions.ContainsKey(specGlossExtName))
            {
                var specGlossDef = (KHR_materials_pbrSpecularGlossinessExtension) def.Extensions[specGlossExtName];

                if (specGlossDef.DiffuseTexture != null)
                {
                    var textureId = specGlossDef.DiffuseTexture.Index;
                    tasks.Add(ConstructImageBuffer(textureId.Value, textureId.Id, true, token));
                }

                if (specGlossDef.SpecularGlossinessTexture != null)
                {
                    var textureId = specGlossDef.SpecularGlossinessTexture.Index;
                    tasks.Add(ConstructImageBuffer(textureId.Value, textureId.Id, true, token));
                }
            }

            await UniTask.WhenAll(tasks);
        }

        protected IEnumerator ConstructUnityMesh(MeshConstructionData meshConstructionData, int meshId, int primitiveIndex, UnityMeshData unityMeshData)
        {
            MeshPrimitive primitive = meshConstructionData.Primitive;
            int vertexCount = (int) primitive.Attributes[SemanticProperties.POSITION].Value.Count;
            bool hasNormals = unityMeshData.Normals != null;

            yield return skipFrameIfDepletedTimeBudget;

            Mesh mesh = new Mesh
            {
#if UNITY_2017_3_OR_NEWER
                indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16,
#endif
            };

            _assetCache.MeshCache[meshId][primitiveIndex].LoadedMesh = mesh;

            meshesEstimatedSize += GLTFSceneImporterUtils.ComputeEstimatedMeshSize(unityMeshData);

            mesh.vertices = unityMeshData.Vertices;

            yield return skipFrameIfDepletedTimeBudget;

            mesh.normals = unityMeshData.Normals;

            yield return skipFrameIfDepletedTimeBudget;

            mesh.uv = unityMeshData.Uv1;

            yield return skipFrameIfDepletedTimeBudget;

            mesh.uv2 = unityMeshData.Uv2;

            yield return skipFrameIfDepletedTimeBudget;

            mesh.uv3 = unityMeshData.Uv3;

            yield return skipFrameIfDepletedTimeBudget;

            mesh.uv4 = unityMeshData.Uv4;

            yield return skipFrameIfDepletedTimeBudget;

            mesh.colors = unityMeshData.Colors;

            if ( !Application.isPlaying )
            {
                if (AreMeshTrianglesValid(unityMeshData.Triangles, vertexCount)) // Some scenes contain broken meshes that can trigger a fatal error
                {
                    mesh.triangles = unityMeshData.Triangles;
                }
                else
                {
                    Debug.Log("GLTFSceneImporter - ERROR - ConstructUnityMesh - Couldn't assign triangles to mesh as there are indices pointing to vertices out of bounds");
                }
            }
            else
            {
                mesh.triangles = unityMeshData.Triangles;

                yield return skipFrameIfDepletedTimeBudget;
            }

            mesh.tangents = unityMeshData.Tangents;

            yield return skipFrameIfDepletedTimeBudget;

            mesh.boneWeights = unityMeshData.BoneWeights;

            yield return skipFrameIfDepletedTimeBudget;

            if (!hasNormals)
                mesh.RecalculateNormals();

            if ( !Application.isPlaying )
                mesh.Optimize();

            OnMeshCreated?.Invoke(mesh);

            if (forceGPUOnlyMesh)
            {
                Physics.BakeMesh(mesh.GetInstanceID(), false);
                mesh.UploadMeshData(true);
            }
        }

        // This check is to avoid broken meshes fatal error "Failed setting triangles. Some indices are referencing out of bounds vertices."
        private bool AreMeshTrianglesValid(int[] triangles, int vertexCount)
        {
            bool areValid = true;

            for (var i = 0; i < triangles.Length; i++)
            {
                if (triangles[i] > vertexCount)
                {
                    areValid = false;

                    break;
                }
            }

            return areValid;
        }

        protected virtual async UniTask ConstructMaterial( GLTFMaterial def, int materialIndex, CancellationToken cancellationToken)
        {
            IUniformMap mapper;
            const string specGlossExtName = KHR_materials_pbrSpecularGlossinessExtensionFactory.EXTENSION_NAME;

            if (_gltfRoot.ExtensionsUsed != null
                && _gltfRoot.ExtensionsUsed.Contains(specGlossExtName)
                && def.Extensions != null
                && def.Extensions.ContainsKey(specGlossExtName))
            {
                if (string.IsNullOrEmpty(CustomShaderName))
                    mapper = new SpecGlossMap(maximumLod);
                else
                    mapper = new SpecGlossMap(CustomShaderName, maximumLod);
            }
            else
            {
                if (string.IsNullOrEmpty(CustomShaderName))
                    mapper = new MetalRoughMap(maximumLod);
                else
                    mapper = new MetalRoughMap(CustomShaderName, maximumLod);
            }

            mapper.Material.name = def.Name;

            MaterialCacheData materialWrapper = new MaterialCacheData
            {
                GLTFMaterial = def
            };

            // Add the material before-hand so it gets freed if the importing is cancelled.
            string materialCRC = mapper.Material.ComputeCRC().ToString();
            materialWrapper.CachedMaterial = new RefCountedMaterialData(materialCRC, mapper.Material);
            materialWrapper.CachedMaterial.IncreaseRefCount();

            if (materialIndex >= 0)
            {
                _assetCache.MaterialCache[materialIndex] = materialWrapper;
            }
            else
            {
                _defaultLoadedMaterial = materialWrapper;
            }

            mapper.AlphaMode = def.AlphaMode;
            mapper.DoubleSided = def.DoubleSided;

            cancellationToken.ThrowIfCancellationRequested();

            if (def.PbrMetallicRoughness != null && mapper is IMetalRoughUniformMap mrMapper)
            {
                var pbr = def.PbrMetallicRoughness;

                mrMapper.BaseColorFactor = pbr.BaseColorFactor;

                if (pbr.BaseColorTexture != null)
                {
                    TextureId textureId = pbr.BaseColorTexture.Index;
                    await ConstructTexture(textureId.Value, textureId.Id, false, cancellationToken);
                    mrMapper.BaseColorTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;

                    mrMapper.BaseColorTexCoord = pbr.BaseColorTexture.TexCoord;
                    ExtTextureTransformExtension ext = GetTextureTransform(pbr.BaseColorTexture);

                    if (ext != null)
                    {
                        mrMapper.BaseColorXOffset = ext.Offset;
                        mrMapper.BaseColorXScale = ext.Scale;
                        //TODO: Implement UVs multichannel for ext.TexCoord;
                    }
                }

                mrMapper.MetallicFactor = pbr.MetallicFactor;

                if (pbr.MetallicRoughnessTexture != null)
                {
                    TextureId textureId = pbr.MetallicRoughnessTexture.Index;
                    await ConstructTexture(textureId.Value, textureId.Id, true, cancellationToken);
                    mrMapper.MetallicRoughnessTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                    mrMapper.MetallicRoughnessTexCoord = pbr.MetallicRoughnessTexture.TexCoord;
                    mrMapper.RoughnessFactor = 0;
                }
                else
                {
                    mrMapper.RoughnessFactor = pbr.RoughnessFactor;
                }
            }

            if (mapper is ISpecGlossUniformMap sgMapper)
            {
                var specGloss = def.Extensions[specGlossExtName] as KHR_materials_pbrSpecularGlossinessExtension;

                sgMapper.DiffuseFactor = specGloss.DiffuseFactor;

                if (specGloss.DiffuseTexture != null)
                {
                    TextureId textureId = specGloss.DiffuseTexture.Index;
                    await ConstructTexture(textureId.Value, textureId.Id, true, cancellationToken);
                    sgMapper.DiffuseTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                    sgMapper.DiffuseTexCoord = specGloss.DiffuseTexture.TexCoord;
                    ExtTextureTransformExtension ext = GetTextureTransform(specGloss.DiffuseTexture);

                    if (ext != null)
                    {
                        sgMapper.DiffuseXOffset = ext.Offset;
                        sgMapper.DiffuseXScale = ext.Scale;
                        //TODO: Implement UVs multichannel for ext.TexCoord;
                    }
                }

                sgMapper.SpecularFactor = specGloss.SpecularFactor;
                sgMapper.GlossinessFactor = specGloss.GlossinessFactor;

                if (specGloss.SpecularGlossinessTexture != null)
                {
                    TextureId textureId = specGloss.SpecularGlossinessTexture.Index;
                    await ConstructTexture(textureId.Value, textureId.Id, true, cancellationToken);
                    sgMapper.SpecularGlossinessTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                }
            }

            if (def.NormalTexture != null)
            {
                TextureId textureId = def.NormalTexture.Index;
                await ConstructTexture(textureId.Value, textureId.Id, true, cancellationToken);
                mapper.NormalTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                mapper.NormalTexCoord = def.NormalTexture.TexCoord;
                mapper.NormalTexScale = def.NormalTexture.Scale;
            }

            if (def.OcclusionTexture != null)
            {
                mapper.OcclusionTexStrength = def.OcclusionTexture.Strength;
                TextureId textureId = def.OcclusionTexture.Index;
                await ConstructTexture(textureId.Value, textureId.Id, true, cancellationToken);
                mapper.OcclusionTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
            }

            if (def.EmissiveTexture != null)
            {
                TextureId textureId = def.EmissiveTexture.Index;
                await ConstructTexture(textureId.Value, textureId.Id, false, cancellationToken);
                mapper.EmissiveTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                mapper.EmissiveTexCoord = def.EmissiveTexture.TexCoord;
            }

            mapper.EmissiveFactor = def.EmissiveFactor;

            materialCRC = mapper.Material.ComputeCRC().ToString();

            if ( !addMaterialsToPersistentCaching )
            {
                materialWrapper.CachedMaterial = new RefCountedMaterialData(materialCRC, mapper.Material);
                materialWrapper.CachedMaterial.IncreaseRefCount();

                return;
            }

            if (!PersistentAssetCache.MaterialCacheByCRC.ContainsKey(materialCRC))
            {
                materialWrapper.CachedMaterial = new RefCountedMaterialData(materialCRC, mapper.Material);
                materialWrapper.CachedMaterial.IncreaseRefCount();
                PersistentAssetCache.MaterialCacheByCRC.Add(materialCRC, materialWrapper.CachedMaterial);
            }
            else
            {
                Object.Destroy( mapper.Material );
                materialWrapper.CachedMaterial = PersistentAssetCache.MaterialCacheByCRC[materialCRC];
                materialWrapper.CachedMaterial.IncreaseRefCount();
            }
        }

        protected virtual int GetTextureSourceId(GLTFTexture texture) { return texture.Source.Id; }

        TextureCreationSettings GetTextureCreationSettingsForTexture(GLTFTexture texture, bool linear)
        {
            TextureCreationSettings settings = new TextureCreationSettings();
            var desiredFilterMode = FilterMode.Trilinear;
            var desiredWrapMode = TextureWrapMode.Repeat;

            if (texture.Sampler != null)
            {
                var sampler = texture.Sampler.Value;

                switch (sampler.MinFilter)
                {
                    case MinFilterMode.Nearest:
                    case MinFilterMode.NearestMipmapNearest:
                    case MinFilterMode.NearestMipmapLinear:
                        desiredFilterMode = FilterMode.Point;

                        break;
                    case MinFilterMode.Linear:
                    case MinFilterMode.LinearMipmapNearest:
                    case MinFilterMode.LinearMipmapLinear:
                        desiredFilterMode = FilterMode.Bilinear;

                        break;
                    default:
                        Debug.LogWarning("Unsupported Sampler.MinFilter: " + sampler.MinFilter);

                        break;
                }

                switch (sampler.WrapS)
                {
                    case GLTF.Schema.WrapMode.ClampToEdge:
                        desiredWrapMode = TextureWrapMode.Clamp;

                        break;
                    case GLTF.Schema.WrapMode.Repeat:
                    default:
                        desiredWrapMode = TextureWrapMode.Repeat;

                        break;
                }
            }

            settings.filterMode = desiredFilterMode;
            settings.wrapMode = desiredWrapMode;
            settings.uploadToGpu = forceGPUOnlyTex;
            settings.generateMipmaps = true;
            settings.linear = linear;

            return settings;
        }

        protected virtual async UniTask ConstructTexture(  GLTFTexture texture, int textureIndex, bool linear, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!forceSyncCoroutines)
            {
                await UniTask.WaitUntil(() => _assetCache.TextureCache[textureIndex] != null, cancellationToken: cancellationToken);
            }

            if (_assetCache.TextureCache[textureIndex].CachedTexture != null)
                return;

            TextureCreationSettings settings = GetTextureCreationSettingsForTexture(texture, linear);

            int sourceId = GetTextureSourceId(texture);
            GLTFImage image = _gltfRoot.Images[sourceId];
            string imageId = GenerateImageId(image.Uri, sourceId, settings);
            RefCountedTextureData source = null;

            if (PersistentAssetCache.HasImage(imageId))
            {
                source = PersistentAssetCache.GetImage(imageId);
                source.IncreaseRefCount();

                if (source.linear != linear)
                {
                    Debug.LogError($"GLTF IMPORTER WARNING: using same texture as linear and srgb will lead to visual artifacts. If '{image.Uri}' is being used as a normal map or metallic map, make sure it's only used in those material properties on every model.");
                }

                _assetCache.ImageCache[sourceId] = source.Texture;

                if (_assetCache.ImageCache[sourceId] == null)
                {
                    Debug.Log($"GLTFSceneImporter - ConstructTexture - null tex detected for {image.Uri} / {id}, applying invalid-tex texture...");
                    _assetCache.ImageCache[sourceId] = Texture2D.redTexture;
                }
            }
            else
            {
                await ConstructImage(settings, image, sourceId, cancellationToken);

                if (_assetCache.ImageCache[sourceId] == null)
                {
                    Debug.Log($"GLTFSceneImporter - ConstructTexture - null tex detected for {image.Uri} / {id}, applying invalid-tex texture...");
                    _assetCache.ImageCache[sourceId] = Texture2D.redTexture;
                }

                if (addImagesToPersistentCaching)
                {
                    source = PersistentAssetCache.AddImage(imageId, _assetCache.ImageCache[sourceId], linear);
                }
                else
                {
                    source = new RefCountedTextureData(imageId, _assetCache.ImageCache[sourceId], linear);
                }

                source.IncreaseRefCount();
            }

            _assetCache.TextureCache[textureIndex].CachedTexture = source;

            cancellationToken.ThrowIfCancellationRequested();
        }

        protected virtual void ConstructImageFromGLB(GLTFImage image, int imageCacheIndex)
        {
            var texture = new Texture2D(0, 0);
            var bufferView = image.BufferView.Value;
            var data = new byte[bufferView.ByteLength];

            var bufferContents = _assetCache.BufferCache[bufferView.Buffer.Id];
            bufferContents.Stream.Position = bufferView.ByteOffset + bufferContents.ChunkOffset;
            bufferContents.Stream.Read(data, 0, data.Length);

            texture.LoadImage(data, true);

            _assetCache.ImageCache[imageCacheIndex] = texture;
        }

        protected virtual BufferCacheData ConstructBufferFromGLB(int bufferIndex)
        {
            GLTFParser.SeekToBinaryChunk(_gltfStream.Stream, bufferIndex, _gltfStream.StartPosition); // sets stream to correct start position

            return new BufferCacheData
            {
                Stream = _gltfStream.Stream,
                ChunkOffset = (uint) _gltfStream.Stream.Position
            };
        }

        protected virtual ExtTextureTransformExtension GetTextureTransform(TextureInfo def)
        {
            IExtension extension;

            if (_gltfRoot.ExtensionsUsed != null &&
                _gltfRoot.ExtensionsUsed.Contains(ExtTextureTransformExtensionFactory.EXTENSION_NAME) &&
                def.Extensions != null &&
                def.Extensions.TryGetValue(ExtTextureTransformExtensionFactory.EXTENSION_NAME, out extension))
            {
                return (ExtTextureTransformExtension) extension;
            }

            return null;
        }

        /// <summary>
        ///  Get the absolute path to a gltf uri reference.
        /// </summary>
        /// <param name="gltfPath">The path to the gltf file</param>
        /// <returns>A path without the filename or extension</returns>
        protected static string AbsoluteUriPath(string gltfPath)
        {
            var uri = new Uri(gltfPath);
            var partialPath = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Query.Length - uri.Segments[uri.Segments.Length - 1].Length);

            return partialPath;
        }

        /// <summary>
        /// Get the absolute path a gltf file directory
        /// </summary>
        /// <param name="gltfPath">The path to the gltf file</param>
        /// <returns>A path without the filename or extension</returns>
        protected static string AbsoluteFilePath(string gltfPath)
        {
            var fileName = Path.GetFileName(gltfPath);
            var lastIndex = gltfPath.IndexOf(fileName, StringComparison.Ordinal);
            var partialPath = gltfPath.Substring(0, lastIndex);

            return partialPath;
        }

        public static Vector2 GLTFOffsetToUnitySpace(Vector2 offset, float textureYScale) { return new Vector2(offset.x, 1 - textureYScale - offset.y); }

        string TextureSettingsToId(TextureCreationSettings textureSettings) { return "W" + textureSettings.wrapMode; }

        private string GenerateImageId(string uri, int sourceId, TextureCreationSettings textureSettings)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return PersistentAssetCache.GetCacheId($"embedded{sourceId}{TextureSettingsToId(textureSettings)}", id);
            }

            if (GetAssetContentHash(uri, out string imageHash))
            {
                return imageHash;
            }

            return PersistentAssetCache.GetCacheId(uri, id);
        }

        private bool GetAssetContentHash(string uri, out string hash)
        {
            if (_loader.assetIdConverter != null)
            {
                return _loader.assetIdConverter(uri, out hash);
            }

            hash = uri;

            return false;
        }
    }
}