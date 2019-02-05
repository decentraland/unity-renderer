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

namespace UnityGLTF
{

    /// <summary>
    /// Component to load a GLTF scene with
    /// </summary>
    public class GLTFComponent : MonoBehaviour
    {
        public string GLTFUri = null;
        public bool Multithreaded = true;
        public bool UseStream = false;

        public int MaximumLod = 300;
        public int Timeout = 8;
        public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;

        public GameObject loadingPlaceholder;
        public System.Action OnFinishedLoadingAsset;
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

            loadingRoutine = StartCoroutine(LoadAssetCoroutine());
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
                    sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;

                    /* if (MaterialsOnly)
                    {
                        var mat = await sceneImporter.LoadMaterialAsync(0);

                        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.SetParent(gameObject.transform);
                        var renderer = cube.GetComponent<Renderer>();
                        renderer.sharedMaterial = mat;
                    }
                    else
                    {
                        await sceneImporter.LoadSceneAsync();
                    } */

                    yield return sceneImporter.LoadScene(-1);

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

            if (loadingPlaceholder != null)
            {
                loadingPlaceholder.SetActive(false);
            }

            loadingRoutine = null;
        }

        void OnDestroy()
        {
            Destroy(loadingPlaceholder);
            Destroy(loadedAssetRootGameObject);
        }
    }
}
