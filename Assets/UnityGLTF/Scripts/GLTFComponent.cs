using System;
using System.Collections;
using System.IO;
using GLTF;
using GLTF.Schema;
using UnityEngine;
using UnityGLTF.Loader;

namespace UnityGLTF {

  /// <summary>
  /// Component to load a GLTF scene with
  /// </summary>
  public class GLTFComponent : MonoBehaviour {
    public string GLTFUri = "";
    public bool Multithreaded = true;
    public bool UseStream = false;

    public int MaximumLod = 300;
    public int Timeout = 8;
    public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;
    public GameObject loadingPlaceholder;

    public System.Action OnFinishedLoadingAsset;

    [HideInInspector] public bool alreadyLoadedAsset = false;
    [HideInInspector] public GameObject loadedAssetRootGameObject;

    [SerializeField] bool loadOnStart = true;
    [SerializeField] Shader shaderOverride = null;

    Coroutine loadingRoutine = null;
    GLTFSceneImporter gLTFSceneImporter;

    public void LoadAsset(string incomingURI = "", bool loadEvenIfAlreadyLoaded = false) {
      if (!alreadyLoadedAsset || loadEvenIfAlreadyLoaded) {
        if (!string.IsNullOrEmpty(incomingURI)) {
          GLTFUri = incomingURI;
        }

        if (loadingRoutine != null) {
          StopCoroutine(loadingRoutine);
        }

        loadingRoutine = StartCoroutine(LoadAssetCoroutine());
      }
    }

    public IEnumerator LoadAssetCoroutine() {
      if (!string.IsNullOrEmpty(GLTFUri)) {
        GLTFSceneImporter sceneImporter = null;
        ILoader loader = null;

        Destroy(loadedAssetRootGameObject);

        try {
          if (UseStream) {
            // Path.Combine treats paths that start with the separator character
            // as absolute paths, ignoring the first path passed in. This removes
            // that character to properly handle a filename written with it.
            GLTFUri = GLTFUri.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            string fullPath = Path.Combine(Application.streamingAssetsPath, GLTFUri);
            string directoryPath = URIHelper.GetDirectoryName(fullPath);
            loader = new FileLoader(directoryPath);
            sceneImporter = new GLTFSceneImporter(
              Path.GetFileName(GLTFUri),
              loader
              );
          } else {
            string directoryPath = URIHelper.GetDirectoryName(GLTFUri);
            loader = new WebRequestLoader(directoryPath);
            sceneImporter = new GLTFSceneImporter(
              URIHelper.GetFileFromUri(new Uri(GLTFUri)),
              loader
              );
          }

          if (sceneImporter.CreatedObject != null) {
            Destroy(sceneImporter.CreatedObject);
          }

          sceneImporter.SceneParent = gameObject.transform;
          sceneImporter.Collider = Collider;
          sceneImporter.MaximumLod = MaximumLod;
          sceneImporter.Timeout = Timeout;
          sceneImporter.isMultithreaded = Multithreaded;
          sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;

          yield return sceneImporter.LoadScene(-1);

          // Override the shaders on all materials if a shader is provided
          if (shaderOverride != null) {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
              renderer.sharedMaterial.shader = shaderOverride;
            }
          }
        } finally {
          if (loader != null) {
            if (sceneImporter == null) {
              Debug.Log("sceneImporter is null, could be due to an invalid URI.", this);
            }

            loadedAssetRootGameObject = sceneImporter.CreatedObject;

            sceneImporter.Dispose();
            sceneImporter = null;

            loader = null;
          }

          if (OnFinishedLoadingAsset != null) {
            OnFinishedLoadingAsset();
          }

          alreadyLoadedAsset = true;
        }
      } else {
        Debug.Log("couldn't load GLTF because url is empty");
      }

      if (loadingPlaceholder != null) {
        loadingPlaceholder.SetActive(false);
      }

      loadingRoutine = null;
    }

    void OnDestroy() {
      Destroy(loadingPlaceholder);

      Destroy(loadedAssetRootGameObject);
    }
  }
}
