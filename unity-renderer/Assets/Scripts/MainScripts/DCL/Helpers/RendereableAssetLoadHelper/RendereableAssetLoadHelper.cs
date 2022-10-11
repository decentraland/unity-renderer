using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Components
{
    public class RendereableAssetLoadHelper
    {
        public enum LoadingType
        {
            ASSET_BUNDLE_WITH_GLTF_FALLBACK,
            ASSET_BUNDLE_ONLY,
            GLTF_ONLY,
            DEFAULT
        }

        public static bool VERBOSE = false;

        public static bool useCustomContentServerUrl = false;
        public static string customContentServerUrl;

        public static LoadingType defaultLoadingType = LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK;

        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();

        public Rendereable loadedAsset { get; protected set; }

        public bool isFinished
        {
            get
            {
                if (gltfPromise != null)
                    return gltfPromise.state == AssetPromiseState.FINISHED;

                if (abPromise != null)
                    return abPromise.state == AssetPromiseState.FINISHED;

                return true;
            }
        }

        string bundlesContentUrl;
        ContentProvider contentProvider;

        AssetPromise_GLTF gltfPromise;
        AssetPromise_AB_GameObject abPromise;

#if UNITY_EDITOR
        public override string ToString()
        {
            float loadTime = Mathf.Min(loadFinishTime, Time.realtimeSinceStartup) - loadStartTime;

            string result = "not loading";

            if (gltfPromise != null)
            {
                result = $"GLTF -> promise state = {gltfPromise.state} ({loadTime} load time)... waiting promises = {AssetPromiseKeeper_GLTF.i.waitingPromisesCount}";

                if (gltfPromise.state == AssetPromiseState.WAITING)
                {
                    result += $"\nmaster promise state... is blocked... {AssetPromiseKeeper_GLTF.i.GetMasterState(gltfPromise)}";
                }
            }

            if (abPromise != null)
            {
                result = $"ASSET BUNDLE -> promise state = {abPromise.ToString()} ({loadTime} load time)... waiting promises = {AssetPromiseKeeper_AB.i.waitingPromisesCount}";
            }

            return result;
        }

        float loadStartTime = 0;
        float loadFinishTime = float.MaxValue;
#endif

        public RendereableAssetLoadHelper(ContentProvider contentProvider, string bundlesContentUrl)
        {
            this.contentProvider = contentProvider;
            this.bundlesContentUrl = bundlesContentUrl;
        }

        public event Action<Rendereable> OnSuccessEvent;
        public event Action<Exception> OnFailEvent;

        public void Load(string targetUrl, LoadingType forcedLoadingType = LoadingType.DEFAULT)
        {
            Assert.IsFalse(string.IsNullOrEmpty(targetUrl), "url is null!!");
#if UNITY_EDITOR
            loadStartTime = Time.realtimeSinceStartup;
#endif

            LoadingType finalLoadingType = forcedLoadingType == LoadingType.DEFAULT ? defaultLoadingType : forcedLoadingType;
            switch (finalLoadingType)
            {
                case LoadingType.ASSET_BUNDLE_ONLY:
                    LoadAssetBundle(targetUrl, OnSuccessEvent, OnFailEvent);
                    break;
                case LoadingType.GLTF_ONLY:
                    LoadGltf(targetUrl, OnSuccessEvent, OnFailEvent);
                    break;
                case LoadingType.DEFAULT:
                case LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK:
                    LoadAssetBundle(targetUrl, OnSuccessEvent, exception => LoadGltf(targetUrl, OnSuccessEvent, OnFailEvent));
                    break;
            }
        }

        public void Unload()
        {
            UnloadAB();
            UnloadGLTF();
        }

        void UnloadAB()
        {
            if ( abPromise != null )
            {
                AssetPromiseKeeper_AB_GameObject.i.Forget(abPromise);
            }
        }

        void UnloadGLTF()
        {
            if ( gltfPromise != null )
            {
                AssetPromiseKeeper_GLTF.i.Forget(gltfPromise);
            }
        }

        private const string AB_GO_NAME_PREFIX = "AB:";
        private const string GLTF_GO_NAME_PREFIX = "GLTF:";

        void LoadAssetBundle(string targetUrl, Action<Rendereable> OnSuccess, Action<Exception> OnFail)
        {
            if (abPromise != null)
            {
                UnloadAB();
                if (VERBOSE)
                    Debug.Log("Forgetting not null promise..." + targetUrl);
            }

            string bundlesBaseUrl = useCustomContentServerUrl ? customContentServerUrl : bundlesContentUrl;

            if (string.IsNullOrEmpty(bundlesBaseUrl))
            {
                OnFailWrapper(OnFail, new Exception("bundlesBaseUrl is null"));
                return;
            }

            if (!contentProvider.TryGetContentsUrl_Raw(targetUrl, out string hash))
            {
                OnFailWrapper(OnFail, new Exception($"Content url does not contains {targetUrl}"));
                return;
            }

            abPromise = new AssetPromise_AB_GameObject(bundlesBaseUrl, hash);
            abPromise.settings = this.settings;

            abPromise.OnSuccessEvent += (x) =>
            {
#if UNITY_EDITOR
                x.container.name = AB_GO_NAME_PREFIX + x.container.name; 
#endif
                var r = new Rendereable()
                {
                    container = x.container,
                    totalTriangleCount = x.totalTriangleCount,
                    meshes = x.meshes,
                    renderers = x.renderers,
                    materials = x.materials,
                    textures = x.textures,
                    meshToTriangleCount = x.meshToTriangleCount,
                    animationClipSize = x.animationClipSize,
                    animationClips = x.animationClips,
                    meshDataSize = x.meshDataSize
                };

                var temp = UnityEngine.Debug.unityLogger.logEnabled;
                UnityEngine.Debug.unityLogger.logEnabled = true;
                Debug.LogError($"Logging materials for {x.container.name}");

                try
                {
                       //var urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
                       var materialsCloneList = new List<Material>();
                       foreach (var mat in r.materials)
                       {
                           var matText = "";

                           matText += $"ENTITY_{r.ownerId} " + mat.name + " ";
                           
                           // if (x.container.transform.AnyAncestorNameContains("253440"))
                           // {
                           //     matText +="\n ENTITY_253440 (beam) " + mat.name;
                           // }
                           //
                           // if (x.container.transform.AnyAncestorNameContains("214016"))
                           // {
                           //     matText +="\n ENTITY_214016 (tube1) " + mat.name;
                           // }
                           //
                           // if (x.container.transform.AnyAncestorNameContains("215040"))
                           // {
                           //     matText +="\n ENTITY_215040 (tube2) " + mat.name;
                           // }
                           //
                           // if (x.container.transform.AnyAncestorNameContains("35840"))
                           // {
                           //     matText +="\n ENTITY_35840 (mika) " + mat.name;
                           // }
                           
                           
                           matText += "\n" + mat.ToText();
                    
                           Debug.LogError(matText);
                           // Debug.LogError(matText);
                           // var newMaterial = new Material(mat.shader);
                           //
                           // var propertiesOfNewMaterial = newMaterial.GetTexturePropertyNames();
                           // var propertiesOfOldMaterial = mat.GetTexturePropertyNames();
                           //
                           // var oldNamesMissingInNewMaterial = new List<string>();
                           // var newNamesMissingInOldMaterial = new List<string>();
                           // var overlapNames = new List<string>();
                           // foreach (var oldPorpertyName in propertiesOfOldMaterial)
                           // {
                           //     if (propertiesOfNewMaterial.Contains(oldPorpertyName) == false)
                           //     {
                           //         oldNamesMissingInNewMaterial.Add(oldPorpertyName);
                           //     }
                           //     else
                           //     {
                           //         overlapNames.Add(oldPorpertyName);
                           //     }
                           // }
                           //
                           // foreach (var newPropertyName in propertiesOfNewMaterial)
                           // {
                           //     if (propertiesOfOldMaterial.Contains(newPropertyName) == false)
                           //     {
                           //         newNamesMissingInOldMaterial.Add(newPropertyName);
                           //     }
                           // }
                           //
                           // string log = "";
                           //
                           // var renderPipelineTagValue = mat.GetTag("RenderPipeline", false);
                           // var renderTypeTagValue = mat.GetTag("RenderType", false);
                           // var newPipelineTagValue = newMaterial.GetTag("RenderPipeline", false);
                           // var newTypeTagValue = newMaterial.GetTag("RenderType", false);
                           //
                           // log += $"Old tags RenderPipeline={renderPipelineTagValue} RenderType={renderTypeTagValue} ";
                           // log += $"\n New tags RenderPipeline={newPipelineTagValue} RenderType={newTypeTagValue} ";
                           //
                           // log += "\n Old properties missing in new mat: ";
                           // foreach (var oldName in oldNamesMissingInNewMaterial)
                           // {
                           //     log += oldName + " ";
                           // }
                           // log += "\n New properties missing in old mat: ";
                           // foreach (var newName in newNamesMissingInOldMaterial)
                           // {
                           //     log += newName + " ";
                           // }
                           // log += "\n Overlapped properties: ";
                           // foreach (var overlapName in overlapNames)
                           // {
                           //     log += overlapName + " ";
                           // }
                           //
                           // if (mat.shaderKeywords != null)
                           // {
                           //     if (mat.shaderKeywords.Length > 0)
                           //     {
                           //         log += "\n Old material keywords: ";
                           //         foreach (var keyword in mat.shaderKeywords)
                           //         {
                           //             log += keyword + " ";
                           //         }
                           //     }
                           // }
                           //
                           // if(mat.shaderKeywords != null)
                           //      newMaterial.shaderKeywords = mat.shaderKeywords.ToArray();
                           //
                           // // newMaterial.shaderKeywords = new string[4]
                           // // {
                           // //     "_ALPHATEST_ON", "_EMISSION", "_METALLICSPECGLOSSMAP", "_NORMALMAP"
                           // // };
                           //
                           // if (newMaterial.shaderKeywords != null)
                           // {
                           //     if (newMaterial.shaderKeywords.Length > 0)
                           //     {
                           //         log += "\n New material keywords: ";
                           //         foreach (var keyword in newMaterial.shaderKeywords)
                           //         {
                           //             log += keyword + " ";
                           //         }
                           //     }
                           // }
                           //
                           // log += "\n Passes enabled: ";
                           // for (int i = 0; i < mat.passCount; i++)
                           // {
                           //     var passName = mat.GetPassName(i);
                           //     if (mat.GetShaderPassEnabled(passName))
                           //     {
                           //         log += passName + " ";
                           //     }
                           // }
                           //
                           // if (mat.name.Contains("Mika"))
                           // {
                           //     log +="\n (Mika Material)";
                           // }
                           //
                           // if (mat.name.Contains("beam_static"))
                           // {
                           //     log +="\n (beam_static Material)";
                           // }
                           //
                           // UnityEngine.Debug.LogError(log);
                           //
                           // newMaterial.CopyPropertiesFromMaterial(mat);
                           // materialsCloneList.Add(newMaterial);
                       }

                       // r.materials.Clear();
                       //  foreach (var newMat in materialsCloneList)
                       //      r.materials.Add(newMat);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
                
                UnityEngine.Debug.unityLogger.logEnabled = temp;

                OnSuccessWrapper(r, OnSuccess);
            };

            abPromise.OnFailEvent += (x, exception) => OnFailWrapper(OnFail, exception);

            AssetPromiseKeeper_AB_GameObject.i.Keep(abPromise);
        }

        void LoadGltf(string targetUrl, Action<Rendereable> OnSuccess, Action<Exception> OnFail)
        {
            if (gltfPromise != null)
            {
                UnloadGLTF();

                if (VERBOSE)
                    Debug.Log("Forgetting not null promise... " + targetUrl);
            }

            if (!contentProvider.TryGetContentsUrl_Raw(targetUrl, out string hash))
            {
                OnFailWrapper(OnFail, new Exception($"Content provider does not contains url {targetUrl}"));
                return;
            }

            gltfPromise = new AssetPromise_GLTF(contentProvider, targetUrl, hash);
            gltfPromise.settings = settings;

            gltfPromise.OnSuccessEvent += (Asset_GLTF x) =>
            {
#if UNITY_EDITOR
                x.container.name = GLTF_GO_NAME_PREFIX + x.container.name;
#endif
                var r = new Rendereable
                {
                    container = x.container,
                    totalTriangleCount = x.totalTriangleCount,
                    meshes = x.meshes,
                    renderers = x.renderers,
                    materials = x.materials,
                    textures = x.textures,
                    meshToTriangleCount = x.meshToTriangleCount,
                    animationClipSize = x.animationClipSize,
                    meshDataSize = x.meshDataSize,
                    animationClips = x.animationClips
                };

                foreach (var mat in r.materials)
                {
                    var matText = "";

                    matText += $"ENTITY_{r.ownerId} " + mat.name + " ";
                           
                    // if (x.container.transform.AnyAncestorNameContains("253440"))
                    // {
                    //     matText +="\n ENTITY_253440 (beam) " + mat.name;
                    // }
                    //
                    // if (x.container.transform.AnyAncestorNameContains("214016"))
                    // {
                    //     matText +="\n ENTITY_214016 (tube1) " + mat.name;
                    // }
                    //
                    // if (x.container.transform.AnyAncestorNameContains("215040"))
                    // {
                    //     matText +="\n ENTITY_215040 (tube2) " + mat.name;
                    // }
                    //
                    // if (x.container.transform.AnyAncestorNameContains("35840"))
                    // {
                    //     matText +="\n ENTITY_35840 (mika) " + mat.name;
                    // }
                           
                    matText += "\n" + mat.ToText();
                    
                    Debug.LogError(matText);
                }

                OnSuccessWrapper(r, OnSuccess);
            };
            gltfPromise.OnFailEvent += (asset, exception) => OnFailWrapper(OnFail, exception);

            AssetPromiseKeeper_GLTF.i.Keep(gltfPromise);
        }

        private void OnFailWrapper(Action<Exception> OnFail, Exception exception)
        {
#if UNITY_EDITOR
            loadFinishTime = Time.realtimeSinceStartup;
#endif

            OnFail?.Invoke(exception);
            ClearEvents();
        }

        private void OnSuccessWrapper(Rendereable loadedAsset, Action<Rendereable> OnSuccess)
        {
            
#if UNITY_EDITOR
            loadFinishTime = Time.realtimeSinceStartup;
#endif
            if (VERBOSE)
            {
                if (gltfPromise != null)
                    Debug.Log($"GLTF Load(): target URL -> {gltfPromise.GetId()}. Success!");
                else
                    Debug.Log($"AB Load(): target URL -> {abPromise.hash}. Success!");
            }

            this.loadedAsset = loadedAsset;
            OnSuccess?.Invoke(loadedAsset);
            ClearEvents();
        }

        public void ClearEvents()
        {
            OnSuccessEvent = null;
            OnFailEvent = null;
        }
    }
}