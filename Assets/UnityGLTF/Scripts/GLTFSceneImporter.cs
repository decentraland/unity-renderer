using GLTF;
using GLTF.Schema;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
#if !WINDOWS_UWP
using System.Threading;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityGLTF.Cache;
using UnityGLTF.Extensions;
using UnityGLTF.Loader;
using Matrix4x4 = GLTF.Math.Matrix4x4;
using Object = UnityEngine.Object;
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

    public class GLTFSceneImporter : IDisposable
    {
        public static bool VERBOSE = false;

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
        public int MaximumLod = 300;

        /// <summary>
        /// Timeout for certain threading operations
        /// </summary>
        public int Timeout = 8;

        /// <summary>
        /// Use Multithreading or not
        /// </summary>
        public bool isMultithreaded = false;

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
        public bool InitialVisibility { get; set; }

        public static bool renderingIsDisabled = false;
        private static float budgetPerFrameInMillisecondsValue = 2f;
        public static float budgetPerFrameInMilliseconds
        {
            get => renderingIsDisabled ? float.MaxValue : budgetPerFrameInMillisecondsValue;
            set => budgetPerFrameInMillisecondsValue = value;
        }

        public bool KeepCPUCopyOfMesh = true;

        private bool useMaterialTransitionValue = true;
        public bool useMaterialTransition
        {
            get => useMaterialTransitionValue && !renderingIsDisabled;
            set => useMaterialTransitionValue = value;
        }

        public const int MAX_TEXTURE_SIZE = 1024;


        protected struct GLBStream
        {
            public Stream Stream;
            public long StartPosition;
        }

        static protected float _timeAtLastYield = 0f;
        protected AsyncCoroutineHelper _asyncCoroutineHelper;

        protected GameObject _lastLoadedScene;
        protected readonly GLTFMaterial DefaultMaterial = new GLTFMaterial();
        protected MaterialCacheData _defaultLoadedMaterial = null;

        protected string _gltfFileName;
        protected GLBStream _gltfStream;
        protected GLTFRoot _gltfRoot;
        protected AssetCache _assetCache;
        protected ILoader _loader;
        protected bool _isRunning = false;

        struct NodeId_Like
        {
            public int Id;
            public Node Value;
        }

        List<NodeId_Like> nodesWithMeshes = new List<NodeId_Like>();

        //NOTE(Brian): Using primitives in Dictionaries would produce unneeded boxing. Improve later.
        Dictionary<int, int> nodeToParent = new Dictionary<int, int>();

        /// <summary>
        /// Creates a GLTFSceneBuilder object which will be able to construct a scene based off a url
        /// </summary>
        /// <param name="gltfFileName">glTF file relative to data loader path</param>
        /// <param name="externalDataLoader">Loader to load external data references</param>
        /// <param name="asyncCoroutineHelper">Helper to load coroutines on a seperate thread</param>
        public GLTFSceneImporter(string gltfFileName, ILoader externalDataLoader, AsyncCoroutineHelper asyncCoroutineHelper) : this(externalDataLoader, asyncCoroutineHelper)
        {
            _gltfFileName = gltfFileName;
        }

        public GLTFSceneImporter(GLTFRoot rootNode, ILoader externalDataLoader, AsyncCoroutineHelper asyncCoroutineHelper, Stream gltfStream = null) : this(externalDataLoader, asyncCoroutineHelper)
        {
            _gltfRoot = rootNode;
            _loader = externalDataLoader;
            if (gltfStream != null)
            {
                _gltfStream = new GLBStream { Stream = gltfStream, StartPosition = gltfStream.Position };
            }
        }

        private GLTFSceneImporter(ILoader externalDataLoader, AsyncCoroutineHelper asyncCoroutineHelper)
        {
            _loader = externalDataLoader;
            _asyncCoroutineHelper = asyncCoroutineHelper;
        }

        public void Dispose()
        {
            if (_assetCache != null)
            {
                Cleanup();
            }
        }

        public GameObject LastLoadedScene
        {
            get { return _lastLoadedScene; }
        }

        public Transform enparentTarget;

        /// <summary>
        /// Loads a glTF Scene into the LastLoadedScene field
        /// </summary>
        /// <param name="sceneIndex">The scene to load, If the index isn't specified, we use the default index in the file. Failing that we load index 0.</param>
        /// <param name="showSceneObj"></param>
        /// <param name="onLoadComplete">Callback function for when load is completed</param>
        /// <returns></returns>
        public IEnumerator LoadScene(int sceneIndex = -1, bool showSceneObj = true, Action<GameObject, ExceptionDispatchInfo> onLoadComplete = null)
        {
            try
            {
                lock (this)
                {
                    if (_isRunning)
                    {
                        throw new GLTFLoadException("Cannot call LoadScene while GLTFSceneImporter is already running");
                    }

                    _isRunning = true;
                }

                _timeAtLastYield = Time.realtimeSinceStartup;
                if (_gltfRoot == null)
                {
                    if (VERBOSE)
                    {
                        Debug.Log("LoadScene() GLTF File Name -> " + _gltfFileName);
                    }

                    yield return LoadJson(_gltfFileName);
                }

                if (_assetCache == null)
                {
                    _assetCache = new AssetCache(_gltfRoot);
                }

                yield return _LoadScene(sceneIndex, showSceneObj);

                yield return null; // NOTE(Brian): DO NOT REMOVE, we must wait a frame in order to
                                   //              start enqueued coroutines, if this is removed
                                   //              the WaitUntil below will fail.

                yield return new WaitUntil(
                () =>
                {
                    return _asyncCoroutineHelper == null || _asyncCoroutineHelper.AllCoroutinesAreFinished();
                });
            }
            /* catch (Exception ex)
            {
                onLoadComplete?.Invoke(null, ExceptionDispatchInfo.Capture(ex));
                throw;
            } */
            finally
            {
                lock (this)
                {
                    _isRunning = false;
                }
            }

            onLoadComplete?.Invoke(LastLoadedScene, null);
        }

        /// <summary>
        /// Loads a node tree from a glTF file into the LastLoadedScene field
        /// </summary>
        /// <param name="nodeIndex">The node index to load from the glTF</param>
        /// <returns></returns>
        public IEnumerator LoadNodeAsync(int nodeIndex)
        {
            try
            {
                lock (this)
                {
                    if (_isRunning)
                    {
                        throw new GLTFLoadException("Cannot call LoadNode while GLTFSceneImporter is already running");
                    }

                    _isRunning = true;
                }

                if (_gltfRoot == null)
                {
                    yield return LoadJson(_gltfFileName);
                }

                if (_assetCache == null)
                {
                    _assetCache = new AssetCache(_gltfRoot);
                }

                _timeAtLastYield = Time.realtimeSinceStartup;
                yield return _LoadNode(nodeIndex);

                CreatedObject = _assetCache.NodeCache[nodeIndex];
                InitializeGltfTopLevelObject();
            }
            finally
            {
                lock (this)
                {
                    _isRunning = false;
                }
            }
        }

        /// <summary>
        /// Load a Material from the glTF by index
        /// </summary>
        /// <param name="materialIndex"></param>
        /// <returns></returns>
        /* public virtual IEnumerator<Material> LoadMaterialAsync(int materialIndex)
        {
            try
            {
                lock (this)
                {
                    if (_isRunning)
                    {
                        throw new GLTFLoadException("Cannot CreateTexture while GLTFSceneImporter is already running");
                    }

                    _isRunning = true;
                }

                if (_gltfRoot == null)
                {
                    yield return LoadJson(_gltfFileName);
                }

                if (materialIndex < 0 || materialIndex >= _gltfRoot.Materials.Count)
                {
                    throw new ArgumentException($"There is no material for index {materialIndex}");
                }

                if (_assetCache == null)
                {
                    _assetCache = new AssetCache(_gltfRoot);
                }

                _timeAtLastYield = Time.realtimeSinceStartup;
                if (_assetCache.MaterialCache[materialIndex] == null)
                {
                    var def = _gltfRoot.Materials[materialIndex];
                    yield return  ConstructMaterialImageBuffers(def);
                    yield return  ConstructMaterial(def, materialIndex);
                }

                yield return  _assetCache.MaterialCache[materialIndex].UnityMaterialWithVertexColor;
            }
            finally
            {
                lock (this)
                {
                    _isRunning = false;
                }
            }
        } */

        /// <summary>
        /// Initializes the top-level created node by adding an instantiated GLTF object component to it, 
        /// so that it can cleanup after itself properly when destroyed
        /// </summary>
        private void InitializeGltfTopLevelObject()
        {
            InstantiatedGLTFObject instantiatedGltfObject = CreatedObject.AddComponent<InstantiatedGLTFObject>();
            instantiatedGltfObject.CachedData = new RefCountedCacheData
            {
                MaterialCache = _assetCache.MaterialCache,
                TextureCache = _assetCache.TextureCache,
                MeshCache = _assetCache.MeshCache
            };
        }

        private IEnumerator ConstructBufferData(Node node)
        {
            MeshId mesh = node.Mesh;
            if (mesh != null)
            {
                if (mesh.Value.Primitives != null)
                {
                    yield return ConstructMeshAttributes(mesh.Value, mesh);
                }
            }

            if (node.Children != null)
            {
                foreach (NodeId child in node.Children)
                {
                    yield return ConstructBufferData(child.Value);
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
                        yield return ConstructBufferData(_gltfRoot.Nodes[lodNodeId]);
                    }
                }
            }
        }

        private IEnumerator ConstructMeshAttributes(GLTFMesh mesh, MeshId meshId)
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
                    yield return ConstructMeshAttributes(primitive, meshIdIndex, i);
                    if (primitive.Material != null)
                    {
                        yield return ConstructMaterialImageBuffers(primitive.Material.Value);
                    }
                }
            }
        }

        static List<string> isStreaming = new List<string>();

        protected IEnumerator ConstructImageBuffer(GLTFTexture texture, int textureIndex)
        {
            int sourceId = GetTextureSourceId(texture);
            GLTFImage image = _gltfRoot.Images[sourceId];

            if (image.Uri != null)
            {
                while (isStreaming.Contains(image.Uri))
                    yield return new WaitForSeconds(0.1f);
            }

            if ((image.Uri == null || !PersistentAssetCache.ImageCacheByUri.ContainsKey(image.Uri)) && _assetCache.ImageStreamCache[sourceId] == null)
            {
                // we only load the streams if not a base64 uri, meaning the data is in the uri
                if (image.Uri != null && !URIHelper.IsBase64Uri(image.Uri))
                {
                    isStreaming.Add(image.Uri);
                    yield return _loader.LoadStream(image.Uri);
                    _assetCache.ImageStreamCache[sourceId] = _loader.LoadedStream;
                    isStreaming.Remove(image.Uri);
                }
                else if (image.Uri == null && image.BufferView != null && _assetCache.BufferCache[image.BufferView.Value.Buffer.Id] == null)
                {
                    int bufferIndex = image.BufferView.Value.Buffer.Id;
                    yield return ConstructBuffer(_gltfRoot.Buffers[bufferIndex], bufferIndex);
                }
            }

            _assetCache.TextureCache[textureIndex] = new TextureCacheData
            {
                TextureDefinition = texture
            };
        }

        protected IEnumerator WaitUntilEnum(WaitUntil waitUntil)
        {
            yield return waitUntil;
        }

        protected IEnumerator EmptyYieldEnum()
        {
            yield break;
        }

        private IEnumerator LoadJson(string jsonFilePath)
        {
#if !WINDOWS_UWP
            if (isMultithreaded && _loader.HasSyncLoadMethod)
            {
                Thread loadThread = new Thread(() => _loader.LoadStreamSync(jsonFilePath));
                loadThread.Priority = ThreadPriority.Highest;
                loadThread.Start();
                RunCoroutineSync(WaitUntilEnum(new WaitUntil(() => !loadThread.IsAlive)));
            }
            else
#endif
            {
                // HACK: Force the coroutine to run synchronously in the editor
                yield return _loader.LoadStream(jsonFilePath);
            }

            _gltfStream.Stream = _loader.LoadedStream;
            _gltfStream.StartPosition = 0;

#if !WINDOWS_UWP
            if (isMultithreaded)
            {
                Thread parseJsonThread = new Thread(() => GLTFParser.ParseJson(_gltfStream.Stream, out _gltfRoot, _gltfStream.StartPosition));
                parseJsonThread.Priority = ThreadPriority.Highest;
                parseJsonThread.Start();
                RunCoroutineSync(WaitUntilEnum(new WaitUntil(() => !parseJsonThread.IsAlive)));
                if (_gltfRoot == null)
                {
                    throw new GLTFLoadException("Failed to parse glTF");
                }
            }
            else
#endif
            {
                GLTFParser.ParseJson(_gltfStream.Stream, out _gltfRoot, _gltfStream.StartPosition);
            }
        }


        public static void RunCoroutineSync(IEnumerator streamEnum)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(streamEnum);
            while (stack.Count > 0)
            {
                var enumerator = stack.Pop();
                if (enumerator.MoveNext())
                {
                    stack.Push(enumerator);
                    var subEnumerator = enumerator.Current as IEnumerator;
                    if (subEnumerator != null)
                    {
                        stack.Push(subEnumerator);
                    }
                }
            }
        }

        private IEnumerator _LoadNode(int nodeIndex, Transform parent = null)
        {
            if (nodeIndex >= _gltfRoot.Nodes.Count)
            {
                throw new ArgumentException("nodeIndex is out of range");
            }

            Node nodeToLoad = _gltfRoot.Nodes[nodeIndex];

            yield return ConstructBufferData(nodeToLoad);

            yield return ConstructNode(nodeToLoad, nodeIndex, parent);
        }

        /// <summary>
        /// Creates a scene based off loaded JSON. Includes loading in binary and image data to construct the meshes required.
        /// </summary>
        /// <param name="sceneIndex">The bufferIndex of scene in gltf file to load</param>
        /// <returns></returns>
        protected IEnumerator _LoadScene(int sceneIndex = -1, bool showSceneObj = true)
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

            yield return ConstructScene(scene, showSceneObj);

            if (SceneParent != null)
            {
                CreatedObject.transform.SetParent(SceneParent, false);
            }

            _lastLoadedScene = CreatedObject;
        }

        protected IEnumerator ConstructBuffer(GLTFBuffer buffer, int bufferIndex)
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
                    yield return _loader.LoadStream(buffer.Uri);

                    bufferDataStream = _loader.LoadedStream;
                }

                _assetCache.BufferCache[bufferIndex] = new BufferCacheData
                {
                    Stream = bufferDataStream
                };
            }
        }

        protected IEnumerator ConstructImage(GLTFImage image, int imageCacheIndex, bool markGpuOnly = false, bool linear = true)
        {
            if (_assetCache.ImageCache[imageCacheIndex] == null)
            {
                Stream stream = null;
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
                    bufferContents.Stream.Read(data, 0, data.Length);

                    if (ShouldYieldOnTimeout())
                    {
                        yield return YieldOnTimeout();
                    }

                    yield return ConstructUnityTexture(data, markGpuOnly, linear, image, imageCacheIndex);
                }
                else
                {
                    string uri = image.Uri;

                    byte[] bufferData;
                    URIHelper.TryParseBase64(uri, out bufferData);
                    if (bufferData != null)
                    {
                        stream = new MemoryStream(bufferData, 0, bufferData.Length, false, true);
                    }
                    else
                    {
                        stream = _assetCache.ImageStreamCache[imageCacheIndex];
                    }

                    if (ShouldYieldOnTimeout())
                    {
                        yield return YieldOnTimeout();
                    }

                    yield return ConstructUnityTexture(stream, markGpuOnly, linear, image, imageCacheIndex);
                }
            }
        }

        protected virtual IEnumerator ConstructUnityTexture(byte[] buffer, bool markGpuOnly, bool linear, GLTFImage image, int imageCacheIndex)
        {
            Texture2D texture = new Texture2D(0, 0, TextureFormat.RGBA32, true, linear);

            //  NOTE: the second parameter of LoadImage() marks non-readable, but we can't mark it until after we call Apply()
            texture.LoadImage(buffer, markGpuOnly);
            texture = CheckAndReduceTextureSize(texture);

            _assetCache.ImageCache[imageCacheIndex] = texture;

            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }
        }

        protected virtual IEnumerator ConstructUnityTexture(Stream stream, bool markGpuOnly, bool linear, GLTFImage image, int imageCacheIndex)
        {
            if (stream is MemoryStream)
            {
                using (MemoryStream memoryStream = stream as MemoryStream)
                {
                    yield return ConstructUnityTexture(memoryStream.ToArray(), false, linear, image, imageCacheIndex);
                }
            }
        }

        // Note that if the texture is reduced in size, the source one is destroyed
        protected Texture2D CheckAndReduceTextureSize(Texture2D source)
        {
            if (source.width > MAX_TEXTURE_SIZE || source.height > MAX_TEXTURE_SIZE)
            {
                float factor = 1.0f;
                int width = source.width;
                int height = source.height;

                if (width >= height)
                {
                    factor = (float)MAX_TEXTURE_SIZE / width;
                }
                else
                {
                    factor = (float)MAX_TEXTURE_SIZE / height;
                }

                Texture2D dstTex = TextureScale.Resize(source, (int)(width * factor), (int)(height * factor));
                Texture2D.Destroy(source);

                return dstTex;
            }

            return source;
        }

        protected virtual IEnumerator ConstructMeshAttributes(MeshPrimitive primitive, int meshID, int primitiveIndex)
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
                        yield return ConstructBuffer(buffer, bufferId);
                    }

                    AttributeAccessor attributeAccessor = new AttributeAccessor
                    {
                        AccessorId = attributePair.Value,
                        Stream = _assetCache.BufferCache[bufferId].Stream,
                        Offset = (uint)_assetCache.BufferCache[bufferId].ChunkOffset
                    };

                    attributeAccessors[attributePair.Key] = attributeAccessor;
                }

                if (primitive.Indices != null)
                {
                    int bufferId = primitive.Indices.Value.BufferView.Value.Buffer.Id;

                    if (_assetCache.BufferCache[bufferId] == null)
                    {
                        yield return ConstructBuffer(primitive.Indices.Value.BufferView.Value.Buffer.Value, bufferId);
                    }

                    AttributeAccessor indexBuilder = new AttributeAccessor
                    {
                        AccessorId = primitive.Indices,
                        Stream = _assetCache.BufferCache[bufferId].Stream,
                        Offset = (uint)_assetCache.BufferCache[bufferId].ChunkOffset
                    };

                    attributeAccessors[SemanticProperties.INDICES] = indexBuilder;
                }

                GLTFHelpers.BuildMeshAttributes(ref attributeAccessors);

                TransformAttributes(ref attributeAccessors);
                _assetCache.MeshCache[meshID][primitiveIndex].MeshAttributes = attributeAccessors;
            }
        }


        protected void TransformAttributes(ref Dictionary<string, AttributeAccessor> attributeAccessors)
        {
            // Flip vectors and triangles to the Unity coordinate system.
            if (attributeAccessors.ContainsKey(SemanticProperties.POSITION))
            {
                AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.POSITION];
                SchemaExtensions.ConvertVector3CoordinateSpace(ref attributeAccessor, SchemaExtensions.CoordinateSpaceConversionScale);
            }

            if (attributeAccessors.ContainsKey(SemanticProperties.INDICES))
            {
                AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.INDICES];
                SchemaExtensions.FlipFaces(ref attributeAccessor);
            }

            if (attributeAccessors.ContainsKey(SemanticProperties.NORMAL))
            {
                AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.NORMAL];
                SchemaExtensions.ConvertVector3CoordinateSpace(ref attributeAccessor, SchemaExtensions.CoordinateSpaceConversionScale);
            }

            // TexCoord goes from 0 to 3 to match GLTFHelpers.BuildMeshAttributes
            for (int i = 0; i < 4; i++)
            {
                if (attributeAccessors.ContainsKey(SemanticProperties.TexCoord(i)))
                {
                    AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.TexCoord(i)];
                    SchemaExtensions.FlipTexCoordArrayV(ref attributeAccessor);
                }
            }

            if (attributeAccessors.ContainsKey(SemanticProperties.TANGENT))
            {
                AttributeAccessor attributeAccessor = attributeAccessors[SemanticProperties.TANGENT];
                SchemaExtensions.ConvertVector4CoordinateSpace(ref attributeAccessor, SchemaExtensions.TangentSpaceConversionScale);
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

        IEnumerator LoadAnimationBufferData(GLTFAnimation animation, int animationId)
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
                    yield return ConstructBuffer(_gltfRoot.Buffers[bufferId], bufferId);
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
                    yield return ConstructBuffer(_gltfRoot.Buffers[anotherBufferId], anotherBufferId);
                    bufferCacheData = _assetCache.BufferCache[anotherBufferId];
                    if (VERBOSE)
                    {
                        Debug.Log("A GLTF Animation buffer cache data is null, skipping animation sampler for " + animation.Name);
                    }
                }
            }
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
                {"time", new List<AttributeAccessor>(animation.Samplers.Count)}
            };

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

        protected AnimationClip ConstructClip(Transform root, GameObject[] nodes, int animationId, out GLTFAnimation animation, out AnimationCacheData animationCache)
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

            // needed because Animator component is unavailable at runtime
            clip.legacy = true;


            return clip;
        }

        IEnumerator ProcessCurves(Transform root, GameObject[] nodes, AnimationClip clip, GLTFAnimation animation, AnimationCacheData animationCache)
        {
            foreach (AnimationChannel channel in animation.Channels)
            {
                AnimationSamplerCacheData samplerCache = animationCache.Samplers[channel.Sampler.Id];
                Transform node = nodes[channel.Target.Node.Id].transform;
                string relativePath = RelativePathFrom(node, root);
                AnimationCurve curveX = new AnimationCurve(),
                    curveY = new AnimationCurve(),
                    curveZ = new AnimationCurve(),
                    curveW = new AnimationCurve();

                if (samplerCache.Input == null)
                {
                    if (VERBOSE)
                    {
                        Debug.Log("GLTF Animation: samplerCache input is null for " + node.parent.name + ", skipping animation channel", node);
                    }

                    continue;
                }

                if (samplerCache.Output == null)
                {
                    if (VERBOSE)
                    {
                        Debug.Log("GLTF Animation: samplerCache output is null for " + node.parent.name + ", skipping animation channel", node);
                    }

                    continue;
                }

                NumericArray input = samplerCache.Input.AccessorContent,
                    output = samplerCache.Output.AccessorContent;

                switch (channel.Target.Path)
                {
                    case GLTFAnimationChannelPath.translation:
                        for (var i = 0; i < input.AsFloats.Length; ++i)
                        {
                            var time = input.AsFloats[i];
                            Vector3 position = output.AsVec3s[i].ToUnityVector3Convert();
                            curveX.AddKey(time, position.x);
                            curveY.AddKey(time, position.y);
                            curveZ.AddKey(time, position.z);
                        }

                        yield return SetCurveMode(curveX, samplerCache.Interpolation);
                        yield return SetCurveMode(curveY, samplerCache.Interpolation);
                        yield return SetCurveMode(curveZ, samplerCache.Interpolation);
                        clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", curveX);
                        clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", curveY);
                        clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", curveZ);
                        break;

                    case GLTFAnimationChannelPath.rotation:

                        for (int i = 0; i < input.AsFloats.Length; ++i)
                        {
                            var time = input.AsFloats[i];
                            var rotation = output.AsVec4s[i];

                            Quaternion rot = new GLTF.Math.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W).ToUnityQuaternionConvert();
                            curveX.AddKey(time, rot.x);
                            curveY.AddKey(time, rot.y);
                            curveZ.AddKey(time, rot.z);
                            curveW.AddKey(time, rot.w);
                        }

                        yield return SetCurveMode(curveX, samplerCache.Interpolation);
                        yield return SetCurveMode(curveY, samplerCache.Interpolation);
                        yield return SetCurveMode(curveZ, samplerCache.Interpolation);
                        yield return SetCurveMode(curveW, samplerCache.Interpolation);
                        clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", curveX);
                        clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", curveY);
                        clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", curveZ);
                        clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", curveW);
                        break;

                    case GLTFAnimationChannelPath.scale:
                        for (var i = 0; i < input.AsFloats.Length; ++i)
                        {
                            var time = input.AsFloats[i];
                            Vector3 scale = output.AsVec3s[i].ToUnityVector3Raw();
                            curveX.AddKey(time, scale.x);
                            curveY.AddKey(time, scale.y);
                            curveZ.AddKey(time, scale.z);
                        }

                        yield return SetCurveMode(curveX, samplerCache.Interpolation);
                        yield return SetCurveMode(curveY, samplerCache.Interpolation);
                        yield return SetCurveMode(curveZ, samplerCache.Interpolation);
                        clip.SetCurve(relativePath, typeof(Transform), "localScale.x", curveX);
                        clip.SetCurve(relativePath, typeof(Transform), "localScale.y", curveY);
                        clip.SetCurve(relativePath, typeof(Transform), "localScale.z", curveZ);
                        break;

                    case GLTFAnimationChannelPath.weights:
                        var primitives = channel.Target.Node.Value.Mesh.Value.Primitives;
                        var targetCount = primitives[0].Targets.Count;
                        for (int primitiveIndex = 0; primitiveIndex < primitives.Count; primitiveIndex++)
                        {
                            for (int targetIndex = 0; targetIndex < targetCount; targetIndex++)
                            {
                                // TODO: add support for blend shapes/morph targets
                                //clip.SetCurve(primitiveObjPath, typeof(SkinnedMeshRenderer), "blendShape." + targetIndex, curves[targetIndex]);
                            }
                        }
                        break;

                    default:
                        Debug.LogWarning("Cannot read GLTF animation path");
                        break;
                } // switch target type
            } // foreach channel

            clip.EnsureQuaternionContinuity();

            yield return null;
        }

        public static IEnumerator SetCurveMode(AnimationCurve curve, InterpolationType mode)
        {
            Keyframe[] curveKeys = curve.keys;

            if (mode == InterpolationType.CUBICSPLINE || mode == InterpolationType.CATMULLROMSPLINE)
            {
                Keyframe key;

                key = curve.keys[0];
                key.inTangent = 0;
                curve.MoveKey(0, key);

                key = curve.keys[curve.keys.Length - 1];
                key.outTangent = 0;
                curve.MoveKey(curve.keys.Length - 1, key);
                yield break;
            }

            for (int i = 0; i < curve.length; ++i)
            {
                float intangent = 0;
                float outtangent = 0;
                bool intangent_set = false;
                bool outtangent_set = false;
                Vector2 point1;
                Vector2 point2;
                Vector2 deltapoint;
                Keyframe key = curve[i];

                if (i == 0)
                {
                    intangent = 0; intangent_set = true;
                }

                if (i == curve.length - 1)
                {
                    outtangent = 0; outtangent_set = true;
                }

                switch (mode)
                {
                    case InterpolationType.STEP:
                        {
                            intangent = 0;
                            outtangent = float.PositiveInfinity;
                        }
                        break;
                    case InterpolationType.LINEAR:
                        {
                            if (!intangent_set)
                            {
                                point1.x = curveKeys[i - 1].time;
                                point1.y = curveKeys[i - 1].value;
                                point2.x = curveKeys[i].time;
                                point2.y = curveKeys[i].value;

                                deltapoint = point2 - point1;
                                intangent = deltapoint.y / deltapoint.x;
                            }

                            if (!outtangent_set)
                            {
                                point1.x = curveKeys[i].time;
                                point1.y = curveKeys[i].value;
                                point2.x = curveKeys[i + 1].time;
                                point2.y = curveKeys[i + 1].value;

                                deltapoint = point2 - point1;

                                outtangent = deltapoint.y / deltapoint.x;
                            }
                        }
                        break;
                    case InterpolationType.CUBICSPLINE:
                        break;
                    case InterpolationType.CATMULLROMSPLINE:
                        break;
                    default:
                        break;
                }

                key.inTangent = intangent;
                key.outTangent = outtangent;
                curve.MoveKey(i, key);

                if (ShouldYieldOnTimeout())
                {
                    yield return YieldOnTimeout();
                }
            }
        }
        #endregion

        protected virtual IEnumerator ConstructScene(GLTFScene scene, bool showSceneObj)
        {
            var sceneObj = new GameObject(string.IsNullOrEmpty(scene.Name) ? ("GLTFScene") : scene.Name);

            CreatedObject = sceneObj;

            sceneObj.SetActive(showSceneObj);
            sceneObj.transform.SetParent(SceneParent, false);
            sceneObj.transform.localPosition = Vector3.zero;
            sceneObj.transform.localRotation = Quaternion.identity;
            sceneObj.transform.localScale = Vector3.one;

            Transform[] nodeTransforms = new Transform[scene.Nodes.Count];

            for (int i = 0; i < scene.Nodes.Count; ++i)
            {
                NodeId node = scene.Nodes[i];
                yield return _LoadNode(node.Id, sceneObj.transform);
                GameObject nodeObj = _assetCache.NodeCache[node.Id];
                nodeTransforms[i] = nodeObj.transform;
            }

            for (int i = 0; i < nodesWithMeshes.Count; i++)
            {
                NodeId_Like nodeId = nodesWithMeshes[i];
                Node node = nodeId.Value;
                yield return ConstructMesh(node.Mesh.Value, _assetCache.NodeCache[nodeId.Id].transform, node.Mesh.Id, node.Skin != null ? node.Skin.Value : null);
            }

            if (_gltfRoot.Animations != null && _gltfRoot.Animations.Count > 0)
            {
                // create the AnimationClip that will contain animation data
                Animation animation = sceneObj.AddComponent<Animation>();
                for (int i = 0; i < _gltfRoot.Animations.Count; ++i)
                {
                    GLTFAnimation gltfAnimation = null;
                    AnimationCacheData animationCache = null;

                    yield return LoadAnimationBufferData(_gltfRoot.Animations[i], i);

                    AnimationClip clip = ConstructClip(sceneObj.transform, _assetCache.NodeCache, i, out gltfAnimation, out animationCache);

                    yield return ProcessCurves(sceneObj.transform, _assetCache.NodeCache, clip, gltfAnimation, animationCache);

                    clip.wrapMode = WrapMode.Loop;

                    animation.AddClip(clip, clip.name);

                    if (i == 0)
                    {
                        animation.clip = clip;
                    }
                }

                animation.enabled = false;
            }

            InitializeGltfTopLevelObject();
        }

        protected virtual IEnumerator ConstructNode(Node node, int nodeIndex, Transform parent = null)
        {
            if (_assetCache.NodeCache[nodeIndex] != null)
            {
                yield break;
            }

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
                    yield return ConstructNode(child.Value, child.Id, nodeObj.transform);
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

                    var lodGroupNodeObj = new GameObject(string.IsNullOrEmpty(node.Name) ? ("GLTFNode_LODGroup" + nodeIndex) : node.Name);

                    lodGroupNodeObj.SetActive(false);

                    nodeObj.transform.SetParent(lodGroupNodeObj.transform, false);

                    MeshRenderer[] childRenders = nodeObj.GetComponentsInChildren<MeshRenderer>();
                    lods[0] = new LOD(GetLodCoverage(lodCoverage, 0), childRenders);

                    LODGroup lodGroup = lodGroupNodeObj.AddComponent<LODGroup>();

                    for (int i = 0; i < lodsextension.MeshIds.Count; i++)
                    {
                        int lodNodeId = lodsextension.MeshIds[i];

                        yield return ConstructNode(_gltfRoot.Nodes[lodNodeId], lodNodeId, lodGroupNodeObj.transform);

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
                return (float)lodcoverageExtras[lodIndex];
            }
            else
            {
                return 1.0f / (lodIndex + 2);
            }
        }

        private bool NeedsSkinnedMeshRenderer(MeshPrimitive primitive, Skin skin)
        {
            return HasBones(skin) || HasBlendShapes(primitive);
        }

        private bool HasBones(Skin skin)
        {
            return skin != null;
        }

        private bool HasBlendShapes(MeshPrimitive primitive)
        {
            return primitive.Targets != null;
        }

        IEnumerator FindSkeleton(int nodeId, System.Action<int> found)
        {
            if (nodeToParent.ContainsKey(nodeId))
            {
                yield return FindSkeleton(nodeToParent[nodeId], found);
            }

            found.Invoke(nodeId);
        }

        protected virtual IEnumerator SetupBones(Skin skin, MeshPrimitive primitive, SkinnedMeshRenderer renderer, GameObject primitiveObj, Mesh curMesh)
        {
            var boneCount = skin.Joints.Count;
            Transform[] bones = new Transform[boneCount];

            int bufferId = skin.InverseBindMatrices.Value.BufferView.Value.Buffer.Id;

            // on cache miss, load the buffer
            if (_assetCache.BufferCache[bufferId] == null)
            {
                yield return ConstructBuffer(_gltfRoot.Buffers[bufferId], bufferId);
            }

            AttributeAccessor attributeAccessor = new AttributeAccessor
            {
                AccessorId = skin.InverseBindMatrices,
                Stream = _assetCache.BufferCache[bufferId].Stream,
                Offset = _assetCache.BufferCache[bufferId].ChunkOffset
            };

            GLTFHelpers.BuildBindPoseSamplers(ref attributeAccessor);

            Matrix4x4[] gltfBindPoses = attributeAccessor.AccessorContent.AsMatrix4x4s;
            UnityEngine.Matrix4x4[] bindPoses = new UnityEngine.Matrix4x4[skin.Joints.Count];

            int skeletonId = 0;

            if (skin.Skeleton != null)
            {
                skeletonId = skin.Skeleton.Id;
            }
            else
            {
                yield return FindSkeleton(skin.Joints[0].Id, (id) => skeletonId = id);
            }

            for (int i = 0; i < boneCount; i++)
            {
                bones[i] = _assetCache.NodeCache[skin.Joints[i].Id].transform;
                bindPoses[i] = gltfBindPoses[i].ToUnityMatrix4x4Convert();
            }

            renderer.rootBone = _assetCache.NodeCache[skeletonId].transform;
            curMesh.bindposes = bindPoses;
            renderer.bones = bones;
        }

        private BoneWeight[] CreateBoneWeightArray(Vector4[] joints, Vector4[] weights, int vertCount)
        {
            NormalizeBoneWeightArray(weights);

            BoneWeight[] boneWeights = new BoneWeight[vertCount];
            for (int i = 0; i < vertCount; i++)
            {
                boneWeights[i].boneIndex0 = (int)joints[i].x;
                boneWeights[i].boneIndex1 = (int)joints[i].y;
                boneWeights[i].boneIndex2 = (int)joints[i].z;
                boneWeights[i].boneIndex3 = (int)joints[i].w;

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

        protected virtual IEnumerator ConstructMesh(GLTFMesh mesh, Transform parent, int meshId, Skin skin)
        {
            bool isColliderMesh = parent.name.Contains("_collider");

            if (_assetCache.MeshCache[meshId] == null)
            {
                _assetCache.MeshCache[meshId] = new MeshCacheData[mesh.Primitives.Count];
            }

            for (int i = 0; i < mesh.Primitives.Count; ++i)
            {
                var primitive = mesh.Primitives[i];
                int materialIndex = primitive.Material != null ? primitive.Material.Id : -1;

                // NOTE(Brian): Submesh loading
                yield return ConstructMeshPrimitive(primitive, meshId, i, materialIndex);

                var primitiveObj = new GameObject("Primitive");
                primitiveObj.transform.SetParent(parent, false);
                primitiveObj.SetActive(useMaterialTransition || LoadingTextureMaterial != null);

                SkinnedMeshRenderer skinnedMeshRenderer = null;
                MeshRenderer meshRenderer = null;
                Renderer renderer = null;

                Mesh curMesh = _assetCache.MeshCache[meshId][i].LoadedMesh;
                MeshFilter meshFilter = primitiveObj.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = curMesh;

                if (!isColliderMesh)
                {
                    if (NeedsSkinnedMeshRenderer(primitive, skin))
                    {
                        skinnedMeshRenderer = primitiveObj.AddComponent<SkinnedMeshRenderer>();
                        skinnedMeshRenderer.sharedMesh = curMesh;
                        skinnedMeshRenderer.quality = SkinQuality.Auto;
                        renderer = skinnedMeshRenderer;
                        renderer.enabled = InitialVisibility;

                        if (HasBones(skin))
                        {
                            yield return SetupBones(skin, primitive, skinnedMeshRenderer, primitiveObj, curMesh);
                        }
                    }
                    else
                    {
                        meshRenderer = primitiveObj.AddComponent<MeshRenderer>();
                        renderer = meshRenderer;
                        renderer.enabled = InitialVisibility;
                    }

                    //// NOTE(Brian): Texture loading
                    if (useMaterialTransition && InitialVisibility)
                    {
                        var matController = primitiveObj.AddComponent<MaterialTransitionController>();
                        var coroutine = DownloadAndConstructMaterial(primitive, materialIndex, renderer, matController);

                        if (_asyncCoroutineHelper != null)
                        {
                            _asyncCoroutineHelper.RunAsTask(coroutine, "matDownload");
                        }
                        else
                        {
                            yield return coroutine;
                        }
                    }
                    else
                    {
                        if (LoadingTextureMaterial != null)
                        {
                            renderer.sharedMaterial = LoadingTextureMaterial;
                        }

                        yield return DownloadAndConstructMaterial(primitive, materialIndex, renderer, null);

                        if (LoadingTextureMaterial == null)
                        {
                            primitiveObj.SetActive(true);
                        }
                    }
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

                _assetCache.MeshCache[meshId][i].PrimitiveGO = primitiveObj;
            }
        }

        IEnumerator DownloadAndConstructMaterial(MeshPrimitive primitive, int materialIndex, Renderer renderer, MaterialTransitionController matController)
        {
            bool shouldUseDefaultMaterial = primitive.Material == null;

            GLTFMaterial materialToLoad = shouldUseDefaultMaterial ? DefaultMaterial : primitive.Material.Value;

            if ((shouldUseDefaultMaterial && _defaultLoadedMaterial == null) ||
                (!shouldUseDefaultMaterial && _assetCache.MaterialCache[materialIndex] == null))
            {
                yield return ConstructMaterial(materialToLoad, shouldUseDefaultMaterial ? -1 : materialIndex);
            }

            MaterialCacheData materialCacheData =
                materialIndex >= 0 ? _assetCache.MaterialCache[materialIndex] : _defaultLoadedMaterial;

            Material material = materialCacheData.GetContents(primitive.Attributes.ContainsKey(SemanticProperties.Color(0)));

            if (matController != null)
            {
                matController.OnDidFinishLoading(material);
            }
            else
            {
                renderer.sharedMaterial = material;
            }
        }


        protected virtual IEnumerator ConstructMeshPrimitive(MeshPrimitive primitive, int meshID, int primitiveIndex, int materialIndex)
        {
            if (_assetCache.MeshCache[meshID][primitiveIndex] == null)
            {
                _assetCache.MeshCache[meshID][primitiveIndex] = new MeshCacheData();
            }
            if (_assetCache.MeshCache[meshID][primitiveIndex].LoadedMesh == null)
            {
                var meshAttributes = _assetCache.MeshCache[meshID][primitiveIndex].MeshAttributes;
                var meshConstructionData = new MeshConstructionData
                {
                    Primitive = primitive,
                    MeshAttributes = meshAttributes
                };

                UnityMeshData unityMeshData = null;
                /* if (isMultithreaded)
                {
                    await Task.Run(() => unityMeshData = ConvertAccessorsToUnityTypes(meshConstructionData));
                }
                else
                {
                    unityMeshData = ConvertAccessorsToUnityTypes(meshConstructionData);
                } */
                unityMeshData = ConvertAccessorsToUnityTypes(meshConstructionData);

                //yield return null;
                yield return ConstructUnityMesh(meshConstructionData, meshID, primitiveIndex, unityMeshData);
            }
        }

        static protected bool ShouldYieldOnTimeout()
        {
            return ((Time.realtimeSinceStartup - _timeAtLastYield) > (budgetPerFrameInMilliseconds / 1000f / (float)GLTFComponent.downloadingCount));
        }

        static protected IEnumerator YieldOnTimeout()
        {
            yield return null;
            _timeAtLastYield = Time.realtimeSinceStartup;
        }

        protected UnityMeshData ConvertAccessorsToUnityTypes(MeshConstructionData meshConstructionData)
        {
            // todo optimize: There are multiple copies being performed to turn the buffer data into mesh data. Look into reducing them
            MeshPrimitive primitive = meshConstructionData.Primitive;
            Dictionary<string, AttributeAccessor> meshAttributes = meshConstructionData.MeshAttributes;

            int vertexCount = (int)primitive.Attributes[SemanticProperties.POSITION].Value.Count;

            return new UnityMeshData
            {
                Vertices = primitive.Attributes.ContainsKey(SemanticProperties.POSITION)
                    ? meshAttributes[SemanticProperties.POSITION].AccessorContent.AsVertices.ToUnityVector3Raw()
                    : null,

                Normals = primitive.Attributes.ContainsKey(SemanticProperties.NORMAL)
                    ? meshAttributes[SemanticProperties.NORMAL].AccessorContent.AsNormals.ToUnityVector3Raw()
                    : null,

                Uv1 = primitive.Attributes.ContainsKey(SemanticProperties.TexCoord(0))
                    ? meshAttributes[SemanticProperties.TexCoord(0)].AccessorContent.AsTexcoords.ToUnityVector2Raw()
                    : null,

                Uv2 = primitive.Attributes.ContainsKey(SemanticProperties.TexCoord(1))
                    ? meshAttributes[SemanticProperties.TexCoord(1)].AccessorContent.AsTexcoords.ToUnityVector2Raw()
                    : null,

                Uv3 = primitive.Attributes.ContainsKey(SemanticProperties.TexCoord(2))
                    ? meshAttributes[SemanticProperties.TexCoord(2)].AccessorContent.AsTexcoords.ToUnityVector2Raw()
                    : null,

                Uv4 = primitive.Attributes.ContainsKey(SemanticProperties.TexCoord(3))
                    ? meshAttributes[SemanticProperties.TexCoord(3)].AccessorContent.AsTexcoords.ToUnityVector2Raw()
                    : null,

                Colors = primitive.Attributes.ContainsKey(SemanticProperties.Color(0))
                    ? meshAttributes[SemanticProperties.Color(0)].AccessorContent.AsColors.ToUnityColorRaw()
                    : null,

                Triangles = primitive.Indices != null
                    ? meshAttributes[SemanticProperties.INDICES].AccessorContent.AsUInts.ToIntArrayRaw()
                    : MeshPrimitive.GenerateTriangles(vertexCount),

                Tangents = primitive.Attributes.ContainsKey(SemanticProperties.TANGENT)
                    ? meshAttributes[SemanticProperties.TANGENT].AccessorContent.AsTangents.ToUnityVector4Raw()
                    : null,

                BoneWeights = meshAttributes.ContainsKey(SemanticProperties.Weight(0)) && meshAttributes.ContainsKey(SemanticProperties.Joint(0))
                    ? CreateBoneWeightArray(meshAttributes[SemanticProperties.Joint(0)].AccessorContent.AsVec4s.ToUnityVector4Raw(),
                    meshAttributes[SemanticProperties.Weight(0)].AccessorContent.AsVec4s.ToUnityVector4Raw(), vertexCount)
                    : null
            };
        }

        protected virtual IEnumerator ConstructMaterialImageBuffers(GLTFMaterial def)
        {
            var tasks = new List<AsyncCoroutineHelper.CoroutineInfo>(8);

            if (def.PbrMetallicRoughness != null)
            {
                var pbr = def.PbrMetallicRoughness;

                if (pbr.BaseColorTexture != null)
                {
                    var textureId = pbr.BaseColorTexture.Index;
                    var coroutine = ConstructImageBuffer(textureId.Value, textureId.Id);

                    if (_asyncCoroutineHelper != null)
                    {
                        tasks.Add(_asyncCoroutineHelper.RunAsTask(coroutine, ""));
                    }
                    else
                    {
                        yield return coroutine;
                    }
                }

                if (pbr.MetallicRoughnessTexture != null)
                {
                    var textureId = pbr.MetallicRoughnessTexture.Index;
                    var coroutine = ConstructImageBuffer(textureId.Value, textureId.Id);

                    if (_asyncCoroutineHelper != null)
                    {
                        tasks.Add(_asyncCoroutineHelper.RunAsTask(coroutine, ""));
                    }
                    else
                    {
                        yield return coroutine;
                    }
                }
            }

            if (def.CommonConstant != null)
            {
                if (def.CommonConstant.LightmapTexture != null)
                {
                    var textureId = def.CommonConstant.LightmapTexture.Index;
                    var coroutine = ConstructImageBuffer(textureId.Value, textureId.Id);

                    if (_asyncCoroutineHelper != null)
                    {
                        tasks.Add(_asyncCoroutineHelper.RunAsTask(coroutine, ""));
                    }
                    else
                    {
                        yield return coroutine;
                    }
                }
            }

            if (def.NormalTexture != null)
            {
                var textureId = def.NormalTexture.Index;
                var coroutine = ConstructImageBuffer(textureId.Value, textureId.Id);

                if (_asyncCoroutineHelper != null)
                {
                    tasks.Add(_asyncCoroutineHelper.RunAsTask(coroutine, ""));
                }
                else
                {
                    yield return coroutine;
                }
            }

            if (def.OcclusionTexture != null)
            {
                var textureId = def.OcclusionTexture.Index;

                if (!(def.PbrMetallicRoughness != null
                        && def.PbrMetallicRoughness.MetallicRoughnessTexture != null
                        && def.PbrMetallicRoughness.MetallicRoughnessTexture.Index.Id == textureId.Id))
                {
                    var coroutine = ConstructImageBuffer(textureId.Value, textureId.Id);

                    if (_asyncCoroutineHelper != null)
                    {
                        tasks.Add(_asyncCoroutineHelper.RunAsTask(coroutine, ""));
                    }
                    else
                    {
                        yield return coroutine;
                    }
                }
            }

            if (def.EmissiveTexture != null)
            {
                var textureId = def.EmissiveTexture.Index;
                var coroutine = ConstructImageBuffer(textureId.Value, textureId.Id);

                if (_asyncCoroutineHelper != null)
                {
                    tasks.Add(_asyncCoroutineHelper.RunAsTask(coroutine, "A"));
                }
                else
                {
                    yield return coroutine;
                }
            }

            // pbr_spec_gloss extension
            const string specGlossExtName = KHR_materials_pbrSpecularGlossinessExtensionFactory.EXTENSION_NAME;

            if (def.Extensions != null && def.Extensions.ContainsKey(specGlossExtName))
            {
                var specGlossDef = (KHR_materials_pbrSpecularGlossinessExtension)def.Extensions[specGlossExtName];

                if (specGlossDef.DiffuseTexture != null)
                {
                    var textureId = specGlossDef.DiffuseTexture.Index;
                    var coroutine = ConstructImageBuffer(textureId.Value, textureId.Id);

                    if (_asyncCoroutineHelper != null)
                    {
                        tasks.Add(_asyncCoroutineHelper.RunAsTask(coroutine, "2"));
                    }
                    else
                    {
                        yield return coroutine;
                    }
                }

                if (specGlossDef.SpecularGlossinessTexture != null)
                {
                    var textureId = specGlossDef.SpecularGlossinessTexture.Index;
                    var coroutine = ConstructImageBuffer(textureId.Value, textureId.Id);

                    if (_asyncCoroutineHelper != null)
                    {
                        tasks.Add(_asyncCoroutineHelper.RunAsTask(coroutine, "3"));
                    }
                    else
                    {
                        yield return coroutine;
                    }
                }
            }

            if (_asyncCoroutineHelper == null)
                yield break;

            yield return null;

            yield return new WaitUntil(
                () =>
                    {
                        for (int i = 0; i < tasks.Count; i++)
                        {
                            if (!tasks[i].finished)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                );
        }

        protected IEnumerator ConstructUnityMesh(MeshConstructionData meshConstructionData, int meshId, int primitiveIndex, UnityMeshData unityMeshData)
        {
            MeshPrimitive primitive = meshConstructionData.Primitive;
            int vertexCount = (int)primitive.Attributes[SemanticProperties.POSITION].Value.Count;
            bool hasNormals = unityMeshData.Normals != null;

            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            Mesh mesh = new Mesh
            {

#if UNITY_2017_3_OR_NEWER
                indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16,
#endif
            };

            mesh.vertices = unityMeshData.Vertices;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            mesh.normals = unityMeshData.Normals;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            mesh.uv = unityMeshData.Uv1;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            mesh.uv2 = unityMeshData.Uv2;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            mesh.uv3 = unityMeshData.Uv3;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            mesh.uv4 = unityMeshData.Uv4;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            mesh.colors = unityMeshData.Colors;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            mesh.triangles = unityMeshData.Triangles;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            mesh.tangents = unityMeshData.Tangents;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            mesh.boneWeights = unityMeshData.BoneWeights;
            if (ShouldYieldOnTimeout())
            {
                yield return YieldOnTimeout();
            }

            if (!hasNormals)
            {
                mesh.RecalculateNormals();
            }

            if (!KeepCPUCopyOfMesh)
            {
                mesh.UploadMeshData(true);
            }

            _assetCache.MeshCache[meshId][primitiveIndex].LoadedMesh = mesh;
        }

        protected virtual IEnumerator ConstructMaterial(GLTFMaterial def, int materialIndex)
        {
            IUniformMap mapper;
            const string specGlossExtName = KHR_materials_pbrSpecularGlossinessExtensionFactory.EXTENSION_NAME;
            if (_gltfRoot.ExtensionsUsed != null && _gltfRoot.ExtensionsUsed.Contains(specGlossExtName)
                && def.Extensions != null && def.Extensions.ContainsKey(specGlossExtName))
            {
                if (!string.IsNullOrEmpty(CustomShaderName))
                {
                    mapper = new SpecGlossMap(CustomShaderName, MaximumLod);
                }
                else
                {
                    mapper = new SpecGlossMap(MaximumLod);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(CustomShaderName))
                {
                    mapper = new MetalRoughMap(CustomShaderName, MaximumLod);
                }
                else
                {
                    mapper = new MetalRoughMap(MaximumLod);
                }
            }

            mapper.Material.name = def.Name;
            mapper.AlphaMode = def.AlphaMode;
            mapper.DoubleSided = def.DoubleSided;

            var mrMapper = mapper as IMetalRoughUniformMap;
            if (def.PbrMetallicRoughness != null && mrMapper != null)
            {
                var pbr = def.PbrMetallicRoughness;

                mrMapper.BaseColorFactor = pbr.BaseColorFactor.ToUnityColorRaw();

                if (pbr.BaseColorTexture != null)
                {
                    TextureId textureId = pbr.BaseColorTexture.Index;
                    yield return ConstructTexture(textureId.Value, textureId.Id, false, false);
                    mrMapper.BaseColorTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                    _assetCache.TextureCache[textureId.Id].CachedTexture.IncreaseRefCount();

                    mrMapper.BaseColorTexCoord = pbr.BaseColorTexture.TexCoord;
                }

                mrMapper.MetallicFactor = pbr.MetallicFactor;

                if (pbr.MetallicRoughnessTexture != null)
                {
                    TextureId textureId = pbr.MetallicRoughnessTexture.Index;
                    yield return ConstructTexture(textureId.Value, textureId.Id);
                    mrMapper.MetallicRoughnessTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                    _assetCache.TextureCache[textureId.Id].CachedTexture.IncreaseRefCount();
                    mrMapper.MetallicRoughnessTexCoord = pbr.MetallicRoughnessTexture.TexCoord;
                    mrMapper.RoughnessFactor = 0;
                }
                else
                {
                    mrMapper.RoughnessFactor = pbr.RoughnessFactor;
                }

            }

            var sgMapper = mapper as ISpecGlossUniformMap;
            if (sgMapper != null)
            {
                var specGloss = def.Extensions[specGlossExtName] as KHR_materials_pbrSpecularGlossinessExtension;

                sgMapper.DiffuseFactor = specGloss.DiffuseFactor.ToUnityColorRaw();

                if (specGloss.DiffuseTexture != null)
                {
                    TextureId textureId = specGloss.DiffuseTexture.Index;
                    yield return ConstructTexture(textureId.Value, textureId.Id);
                    sgMapper.DiffuseTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                    _assetCache.TextureCache[textureId.Id].CachedTexture.IncreaseRefCount();
                    sgMapper.DiffuseTexCoord = specGloss.DiffuseTexture.TexCoord;
                }

                sgMapper.SpecularFactor = specGloss.SpecularFactor.ToUnityVector3Raw();
                sgMapper.GlossinessFactor = specGloss.GlossinessFactor;

                if (specGloss.SpecularGlossinessTexture != null)
                {
                    TextureId textureId = specGloss.SpecularGlossinessTexture.Index;
                    yield return ConstructTexture(textureId.Value, textureId.Id);
                    sgMapper.SpecularGlossinessTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                    _assetCache.TextureCache[textureId.Id].CachedTexture.IncreaseRefCount();
                }
            }

            if (def.NormalTexture != null)
            {
                TextureId textureId = def.NormalTexture.Index;
                yield return ConstructTexture(textureId.Value, textureId.Id);
                mapper.NormalTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                _assetCache.TextureCache[textureId.Id].CachedTexture.IncreaseRefCount();
                mapper.NormalTexCoord = def.NormalTexture.TexCoord;
                mapper.NormalTexScale = def.NormalTexture.Scale;
            }

            if (def.OcclusionTexture != null)
            {
                mapper.OcclusionTexStrength = def.OcclusionTexture.Strength;
                TextureId textureId = def.OcclusionTexture.Index;
                yield return ConstructTexture(textureId.Value, textureId.Id);
                mapper.OcclusionTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                _assetCache.TextureCache[textureId.Id].CachedTexture.IncreaseRefCount();
            }

            if (def.EmissiveTexture != null)
            {
                TextureId textureId = def.EmissiveTexture.Index;
                yield return ConstructTexture(textureId.Value, textureId.Id, false, false);
                mapper.EmissiveTexture = _assetCache.TextureCache[textureId.Id].CachedTexture.Texture;
                _assetCache.TextureCache[textureId.Id].CachedTexture.IncreaseRefCount();
                mapper.EmissiveTexCoord = def.EmissiveTexture.TexCoord;
            }

            mapper.EmissiveFactor = def.EmissiveFactor.ToUnityColorRaw();

            IUniformMap vertColorMapper = mapper.Clone();
            vertColorMapper.VertexColorsEnabled = true;

            Material[] material = new Material[2];

            const int MATERIAL = 0;
            const int MATERIAL_WITH_VERTEX_COLORS = 1;

            material[MATERIAL] = mapper.Material;
            material[MATERIAL_WITH_VERTEX_COLORS] = vertColorMapper.Material;

            MaterialCacheData materialWrapper = new MaterialCacheData();

            for (int i = 0; i < 2; i++)
            {
                string materialCRC = material[i].ComputeCRC().ToString() + material[i].name;

                //TODO(Brian): Remove old material here if the material won't be used. 
                //             (We can use Resources.UnloadUnusedAssets too, but I hate to rely on this)
                if (!PersistentAssetCache.MaterialCacheByCRC.ContainsKey(materialCRC))
                {
                    RefCountedMaterialData newRefCountedMaterial = new RefCountedMaterialData(materialCRC, material[i]);
                    PersistentAssetCache.MaterialCacheByCRC.Add(materialCRC, newRefCountedMaterial);
                }
                else if (material[i] != PersistentAssetCache.MaterialCacheByCRC[materialCRC].material)
                {
                    Material.Destroy(material[i]);
                }

                switch (i)
                {
                    case MATERIAL:
                        materialWrapper.CachedMaterial = PersistentAssetCache.MaterialCacheByCRC[materialCRC];
                        break;
                    case MATERIAL_WITH_VERTEX_COLORS:
                        materialWrapper.CachedMaterialWithVertexColor = PersistentAssetCache.MaterialCacheByCRC[materialCRC];
                        break;
                }
            }

            materialWrapper.GLTFMaterial = def;

            if (materialIndex >= 0)
            {
                _assetCache.MaterialCache[materialIndex] = materialWrapper;
            }
            else
            {
                _defaultLoadedMaterial = materialWrapper;
            }
        }


        protected virtual int GetTextureSourceId(GLTFTexture texture)
        {
            return texture.Source.Id;
        }

        /// <summary>
        /// Creates a texture from a glTF texture
        /// </summary>
        /// <param name="texture">The texture to load</param>
        /// <returns>The loaded unity texture</returns>
        public virtual IEnumerator LoadTextureAsync(GLTFTexture texture, int textureIndex, bool markGpuOnly = true)
        {
            try
            {
                lock (this)
                {
                    if (_isRunning)
                    {
                        throw new GLTFLoadException("Cannot CreateTexture while GLTFSceneImporter is already running");
                    }

                    _isRunning = true;
                }

                if (_gltfRoot == null)
                {
                    yield return LoadJson(_gltfFileName);
                }

                if (_assetCache == null)
                {
                    _assetCache = new AssetCache(_gltfRoot);
                }

                _timeAtLastYield = Time.realtimeSinceStartup;
                yield return ConstructImageBuffer(texture, textureIndex);
                yield return ConstructTexture(texture, textureIndex, markGpuOnly);
            }
            finally
            {
                lock (this)
                {
                    _isRunning = false;
                }
            }
        }

        /// <summary>
        /// Gets texture that has been loaded from CreateTexture
        /// </summary>
        /// <param name="textureIndex">The texture to get</param>
        /// <returns>Created texture</returns>
        public virtual Texture GetTexture(int textureIndex)
        {
            if (_assetCache == null)
            {
                throw new GLTFLoadException("Asset cache needs initialized before calling GetTexture");
            }

            if (_assetCache.TextureCache[textureIndex] == null)
            {
                return null;
            }

            return _assetCache.TextureCache[textureIndex].CachedTexture.Texture;
        }

        protected virtual IEnumerator ConstructTexture(GLTFTexture texture, int textureIndex,
            bool markGpuOnly = false, bool isLinear = true)
        {
            yield return new WaitUntil(() => _assetCache.TextureCache[textureIndex] != null);

            if (_assetCache.TextureCache[textureIndex].CachedTexture == null)
            {
                int sourceId = GetTextureSourceId(texture);
                GLTFImage image = _gltfRoot.Images[sourceId];

                RefCountedTextureData source = null;

                if (image.Uri != null && PersistentAssetCache.ImageCacheByUri.ContainsKey(image.Uri))
                {
                    source = PersistentAssetCache.ImageCacheByUri[image.Uri];
                    _assetCache.ImageCache[sourceId] = source.Texture;
                }
                else
                {
                    yield return ConstructImage(image, sourceId, markGpuOnly, isLinear);
                    source = new RefCountedTextureData(image.Uri, _assetCache.ImageCache[sourceId]);

                    if (image.Uri != null)
                        PersistentAssetCache.ImageCacheByUri[image.Uri] = source;
                }

                var desiredFilterMode = FilterMode.Bilinear;
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

                if (markGpuOnly || (source.Texture.filterMode == desiredFilterMode && source.Texture.wrapMode == desiredWrapMode))
                {
                    _assetCache.TextureCache[textureIndex].CachedTexture = source;

                    if (markGpuOnly)
                    {
                        Debug.LogWarning("Ignoring sampler");
                    }
                }
                else
                {
                    var unityTexture = Object.Instantiate(source.Texture);
                    unityTexture.filterMode = desiredFilterMode;
                    unityTexture.wrapMode = desiredWrapMode;

                    _assetCache.TextureCache[textureIndex].CachedTexture = new RefCountedTextureData(image.Uri, unityTexture);
                }
            }
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
            GLTFParser.SeekToBinaryChunk(_gltfStream.Stream, bufferIndex, _gltfStream.StartPosition);  // sets stream to correct start position
            return new BufferCacheData
            {
                Stream = _gltfStream.Stream,
                ChunkOffset = (uint)_gltfStream.Stream.Position
            };
        }

        protected virtual void ApplyTextureTransform(TextureInfo def, Material mat, string texName)
        {
            IExtension extension;
            if (_gltfRoot.ExtensionsUsed != null &&
                _gltfRoot.ExtensionsUsed.Contains(ExtTextureTransformExtensionFactory.EXTENSION_NAME) &&
                def.Extensions != null &&
                def.Extensions.TryGetValue(ExtTextureTransformExtensionFactory.EXTENSION_NAME, out extension))
            {
                ExtTextureTransformExtension ext = (ExtTextureTransformExtension)extension;

                Vector2 temp = ext.Offset.ToUnityVector2Raw();
                temp = new Vector2(temp.x, -temp.y);
                mat.SetTextureOffset(texName, temp);

                mat.SetTextureScale(texName, ext.Scale.ToUnityVector2Raw());
            }
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
            var lastIndex = gltfPath.IndexOf(fileName);
            var partialPath = gltfPath.Substring(0, lastIndex);
            return partialPath;
        }

        /// <summary>
        /// Cleans up any undisposed streams after loading a scene or a node.
        /// </summary>
        private void Cleanup()
        {
            _assetCache.Dispose();
            _assetCache = null;
        }
    }
}
