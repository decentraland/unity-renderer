#if UNITY_2017_1_OR_NEWER
using GLTF;
using GLTF.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityGLTF.Loader;
using UnityGLTF.Cache;
using Object = UnityEngine.Object;

// using System.Threading.Tasks;

namespace UnityGLTF
{

    // Disabled because we are using gltfast now
    //[ScriptedImporter(1, new[] { "glb", "gltf" })]
    public class GLTFImporter : ScriptedImporter
    {
        [SerializeField] private bool _removeEmptyRootObjects = true;
        [SerializeField] private float _scaleFactor = 1.0f;
        [SerializeField] private int _maximumLod = 300;
        [SerializeField] private bool _readWriteEnabled = true;
        [SerializeField] private bool _generateColliders = false;
        [SerializeField] private bool _swapUvs = false;
        [SerializeField] private GLTFImporterNormals _importNormals = GLTFImporterNormals.Import;
        [SerializeField] private bool _importMaterials = true;
        [SerializeField] private bool _useJpgTextures = false;

        public bool _importTextures = true;

        static int delayCallsCount = 0;

        public static bool finishedImporting { get { return delayCallsCount == 0; } }

        public List<Material> SimplifyMaterials(Renderer[] renderers)
        {
            Dictionary<string, Material> matByCrc = new Dictionary<string, Material>();
            List<Material> materials = new List<Material>();

            foreach (var rend in renderers)
            {
                var matList = new List<Material>(1);

                foreach (var mat in rend.sharedMaterials)
                {
                    if (rend.sharedMaterials.Length == 0)
                        break;

                    if (mat == null)
                        continue;

                    string crc = mat.ComputeCRC() + mat.name;

                    if (!matByCrc.ContainsKey(crc))
                    {
                        matByCrc.Add(crc, mat);
                        materials.Add(mat);
                    }

                    matList.Add(matByCrc[crc]);
                }

                rend.sharedMaterials = matList.ToArray();
            }

            return materials;
        }

        public static bool ShouldWaitForPreloadedGLTF = false;
        public static Dictionary<string, GameObject> PreloadedGLTFObjects = new Dictionary<string, GameObject>();

        public override void OnImportAsset(AssetImportContext ctx)
        {
            GameObject preloadedGltfObject = null;
            if (ShouldWaitForPreloadedGLTF)
            {
                delayCallsCount++;

                if (!PreloadedGLTFObjects.ContainsKey(ctx.assetPath))
                {
                    delayCallsCount--;
                    return;
                }

                preloadedGltfObject = PreloadedGLTFObjects[ctx.assetPath];

                if (!preloadedGltfObject)
                {
                    delayCallsCount--;
                    return;
                }
            }
            else
            {
                char ps = Path.DirectorySeparatorChar;

                string path = ctx.assetPath;

                path = path.Replace('/', ps);
                path = path.Replace('\\', ps);

                preloadedGltfObject = CreateGLTFScene(path);
            }
            
            ImportAsset(ctx, preloadedGltfObject);
        }
        
        private GameObject CreateGLTFScene(string projectFilePath)
        {
            ILoader fileLoader = new GLTFFileLoader(Path.GetDirectoryName(projectFilePath));
            using var stream = File.OpenRead(projectFilePath);

            GLTFRoot gLTFRoot;
            GLTFParser.ParseJson(stream, out gLTFRoot);

            var loader = new GLTFSceneImporter(Path.GetFullPath(projectFilePath), gLTFRoot, fileLoader, null, stream);
            loader.addImagesToPersistentCaching = false;
            loader.addMaterialsToPersistentCaching = false;
            loader.initialVisibility = true;
            loader.maxTextureSize = 512;
            loader.maximumLod = _maximumLod;
            loader.forceGPUOnlyMesh = false;
            loader.forceGPUOnlyTex = false;
            loader.forceSyncCoroutines = true;
            loader.ignoreMaterials = !_importMaterials;

            Task task = loader.LoadScene(CancellationToken.None);
            bool result = task.Wait(TimeSpan.FromSeconds(30));
                
            switch (result)
            {
                case false when task.Exception == null:
                    throw new TimeoutException($"Importing {projectFilePath}");
                case false when task.Exception != null:
                    throw task.Exception;
            }

            stream.Dispose();
            return loader.lastLoadedScene;

        }

        private void ImportAsset(AssetImportContext ctx, GameObject gltfScene)
        {
            string sceneName = null;
            UnityEngine.Mesh[] meshes = null;
            try
            {
                // Remove empty roots
                if (_removeEmptyRootObjects)
                {
                    var t = gltfScene.transform;

                    while (
                        gltfScene.transform.childCount == 1 &&
                        gltfScene.GetComponents<Component>().Length == 1)
                    {
                        var parent = gltfScene;
                        gltfScene = gltfScene.transform.GetChild(0).gameObject;
                        t = gltfScene.transform;
                        t.parent = null; // To keep transform information in the new parent
                        Object.DestroyImmediate(parent); // Get rid of the parent
                    }
                }

                // Ensure there are no hide flags present (will cause problems when saving)
                gltfScene.hideFlags &= ~(HideFlags.HideAndDontSave);

                foreach (Transform child in gltfScene.transform)
                {
                    child.gameObject.hideFlags &= ~(HideFlags.HideAndDontSave);
                }

                // Zero position
                gltfScene.transform.position = Vector3.zero;

                Animation animation = gltfScene.GetComponentInChildren<Animation>();
                HashSet<AnimationClip> animationClips = new HashSet<AnimationClip>();

                if (animation != null)
                {
                    foreach (AnimationState animationState in animation)
                    {
                        if (!animationClips.Contains(animationState.clip))
                        {
                            animationClips.Add(animationState.clip);
                        }
                    }
                }

                // Get meshes
                var meshNames = new List<string>();
                var meshHash = new HashSet<UnityEngine.Mesh>();
                var meshFilters = gltfScene.GetComponentsInChildren<MeshFilter>();
                var vertexBuffer = new List<Vector3>();

                meshes = meshFilters.Where(mf => mf.sharedMesh != null)
                                    .Select(mf =>
                                    {
                                        var mesh = mf.sharedMesh;

                                        vertexBuffer.Clear();
                                        mesh.GetVertices(vertexBuffer);

                                        for (var i = 0; i < vertexBuffer.Count; ++i)
                                        {
                                            vertexBuffer[i] *= _scaleFactor;
                                        }

                                        mesh.SetVertices(vertexBuffer);

                                        if (_swapUvs)
                                        {
                                            var uv = mesh.uv;
                                            var uv2 = mesh.uv2;
                                            mesh.uv = uv2;
                                            mesh.uv2 = uv2;
                                        }

                                        if (_importNormals == GLTFImporterNormals.None)
                                        {
                                            mesh.normals = new Vector3[0];
                                        }

                                        if (_importNormals == GLTFImporterNormals.Calculate)
                                        {
                                            mesh.RecalculateNormals();
                                        }

                                        mesh.UploadMeshData(!_readWriteEnabled);

                                        if (_generateColliders)
                                        {
                                            var collider = mf.gameObject.AddComponent<MeshCollider>();
                                            collider.sharedMesh = mesh;
                                        }

                                        if (meshHash.Add(mesh))
                                        {
                                            var meshName = string.IsNullOrEmpty(mesh.name) ? mf.gameObject.name : mesh.name;
                                            mesh.name = ObjectNames.GetUniqueName(meshNames.ToArray(), meshName);
                                            meshNames.Add(mesh.name);
                                        }

                                        return mesh;
                                    })
                                    .ToArray();

                var renderers = gltfScene.GetComponentsInChildren<Renderer>();

                if (animationClips.Count > 0)
                {
                    var folderName = Path.GetDirectoryName(ctx.assetPath);
                    var animationsRoot = string.Concat(folderName, "/", "Animations/");
                    Directory.CreateDirectory(animationsRoot);

                    foreach (AnimationClip clip in animationClips)
                    {
                        string fileName = PatchInvalidFileNameChars(clip.name);

                        AssetDatabase.CreateAsset(clip, animationsRoot + fileName + ".anim");
                        var importer = AssetImporter.GetAtPath(animationsRoot + fileName + ".anim");
                    }
                }

                if (_importMaterials)
                {
                    var materials = SimplifyMaterials(renderers);
                    // Get materials
                    List<string> materialNames = new List<string>();

                    foreach (var mat in materials)
                    {
                        var matName = string.IsNullOrEmpty(mat.name) ? mat.shader.name : mat.name;

                        if (matName == mat.shader.name)
                        {
                            matName = matName.Substring(Mathf.Min(matName.LastIndexOf("/") + 1, matName.Length - 1));
                        }

                        matName = PatchInvalidFileNameChars(matName);

                        // Ensure name is unique
                        matName = ObjectNames.NicifyVariableName(matName);
                        matName = ObjectNames.GetUniqueName(materialNames.ToArray(), matName);

                        mat.name = matName;
                        materialNames.Add(matName);
                    }

                    List<Texture2D> textures = new List<Texture2D>();
                    var texMaterialMap = new Dictionary<Texture2D, List<TexMaterialMap>>();

                    HashSet<Texture2D> baseColor = new HashSet<Texture2D>();
                    HashSet<Texture2D> normals = new HashSet<Texture2D>();
                    HashSet<Texture2D> metallics = new HashSet<Texture2D>();

                    if (_importTextures)
                    {
                        // Get textures
                        var textureNames = new List<string>();
                        var textureHash = new HashSet<Texture2D>();

                        textures = materials.SelectMany(mat =>
                                            {
                                                var shader = mat.shader;

                                                if (!shader)
                                                {
                                                    return Enumerable.Empty<Texture2D>();
                                                }

                                                var matTextures = new List<Texture2D>();

                                                for (var i = 0; i < ShaderUtil.GetPropertyCount(shader); ++i)
                                                {
                                                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                                                    {
                                                        var propertyName = ShaderUtil.GetPropertyName(shader, i);
                                                        var tex = mat.GetTexture(propertyName) as Texture2D;

                                                        if (!tex)
                                                            continue;

                                                        if (textureHash.Add(tex))
                                                        {
                                                            var texName = tex.name;

                                                            if (string.IsNullOrEmpty(texName))
                                                            {
                                                                if (propertyName.StartsWith("_"))
                                                                {
                                                                    texName = propertyName.Substring(Mathf.Min(1, propertyName.Length - 1));
                                                                }
                                                            }

                                                            // Ensure name is unique
                                                            texName = ObjectNames.NicifyVariableName(texName);
                                                            texName = ObjectNames.GetUniqueName(textureNames.ToArray(), texName);

                                                            tex.name = texName;
                                                            textureNames.Add(texName);
                                                            matTextures.Add(tex);
                                                        }

                                                        List<TexMaterialMap> materialMaps;

                                                        if (!texMaterialMap.TryGetValue(tex, out materialMaps))
                                                        {
                                                            materialMaps = new List<TexMaterialMap>();
                                                            texMaterialMap.Add(tex, materialMaps);
                                                        }

                                                        materialMaps.Add(new TexMaterialMap(mat, propertyName, propertyName == "_BumpMap"));

                                                        if (propertyName == "_BaseMap")
                                                        {
                                                            baseColor.Add(tex);
                                                        }

                                                        if (propertyName == "_BumpMap")
                                                        {
                                                            normals.Add(tex);
                                                        }
                                                        else if (propertyName == "_MetallicGlossMap")
                                                        {
                                                            metallics.Add(tex);
                                                        }
                                                    }
                                                }

                                                return matTextures;
                                            })
                                            .ToList();

                        var folderName = Path.GetDirectoryName(ctx.assetPath);

                        // Save textures as separate assets and rewrite refs
                        // TODO: Support for other texture types
                        if (textures.Count > 0)
                        {
                            var texturesRoot = string.Concat(folderName, "/", "Textures/");

                            if (!Directory.Exists(texturesRoot))
                                Directory.CreateDirectory(texturesRoot);

                            Texture2D[] cachedTextures = PersistentAssetCache.ImageCacheByUri.Values.Select((x) => { return x.Texture; }).ToArray();

                            foreach (var tex in textures)
                            {
                                var ext = _useJpgTextures ? ".jpg" : ".png";
                                var texPath = string.Concat(texturesRoot, tex.name, ext);
                                var absolutePath = Application.dataPath + "/../" + texPath;

                                if (File.Exists(absolutePath) || cachedTextures.Contains(tex))
                                    continue;

                                File.WriteAllBytes(texPath, _useJpgTextures ? tex.EncodeToJPG() : tex.EncodeToPNG());
                                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                            }

                            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
                        }
                    }

                    List<Material> materialCopies = new List<Material>(materials);

                    // Save materials as separate assets and rewrite refs
                    if (materials.Count > 0)
                    {
                        var folderName = Path.GetDirectoryName(ctx.assetPath);
                        var materialRoot = string.Concat(folderName, "/", "Materials/");
                        Directory.CreateDirectory(materialRoot);

                        for (var matIndex = 0; matIndex < materials.Count; matIndex++)
                        {
                            var mat = materials[matIndex];
                            var materialPath = string.Concat(materialRoot, mat.name, ".mat");

                            CopyOrNew(mat, materialPath, m =>
                            {
                                materialCopies[matIndex] = m;

                                foreach (var r in renderers)
                                {
                                    var sharedMaterials = r.sharedMaterials;

                                    for (var i = 0; i < sharedMaterials.Length; ++i)
                                    {
                                        var sharedMaterial = sharedMaterials[i];

                                        if (sharedMaterial.name == mat.name)
                                        {
                                            sharedMaterials[i] = m;
                                            EditorUtility.SetDirty(m);
                                        }
                                    }

                                    sharedMaterials = sharedMaterials.Where(sm => sm).ToArray();
                                    r.sharedMaterials = sharedMaterials;
                                }
                            });
                        }

                        // Fix textures
                        // HACK: This needs to be a delayed call.
                        // Unity needs a frame to kick off the texture import so we can rewrite the ref
                        if (textures.Count > 0)
                        {
                            delayCallsCount++;

                            EditorApplication.delayCall += () =>
                            {
                                Texture2D[] cachedTextures = PersistentAssetCache.ImageCacheByUri.Values.Select((x) => { return x.Texture; }).ToArray();

                                delayCallsCount--;

                                for (var i = 0; i < textures.Count; ++i)
                                {
                                    var tex = textures[i];
                                    var materialMaps = texMaterialMap[tex];
                                    bool isExternal = cachedTextures.Contains(tex);

                                    string texPath;

                                    if (isExternal)
                                    {
                                        texPath = AssetDatabase.GetAssetPath(tex);
                                    }
                                    else
                                    {
                                        var texturesRoot = string.Concat(folderName, "/", "Textures/");
                                        var ext = _useJpgTextures ? ".jpg" : ".png";
                                        texPath = string.Concat(texturesRoot, tex.name, ext);
                                    }

                                    var importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                                    var importer = (TextureImporter) TextureImporter.GetAtPath(texPath);

                                    if (importer != null)
                                    {
                                        importer.isReadable = false;
                                        var isNormalMap = true;

                                        for (var matIndex = 0; matIndex < materials.Count; matIndex++)
                                        {
                                            var originalMaterial = materials[matIndex];

                                            foreach (var materialMap in materialMaps)
                                            {
                                                if (materialMap.Material == originalMaterial)
                                                {
                                                    //NOTE(Brian): Only set as normal map if is exclusively
                                                    //             used for that.
                                                    //             We don't want DXTnm in color textures.
                                                    if (!materialMap.IsNormalMap)
                                                        isNormalMap = false;

                                                    materialCopies[matIndex].SetTexture(materialMap.Property, importedTex);
                                                }
                                            }
                                        }

                                        if (isExternal)
                                        {
                                            isNormalMap = !baseColor.Contains(tex) && normals.Contains(tex);
                                        }

                                        if (isNormalMap)
                                        {
                                            // Try to auto-detect normal maps
                                            importer.textureType = TextureImporterType.NormalMap;
                                        }
                                        else if (importer.textureType == TextureImporterType.Sprite)
                                        {
                                            // Force disable sprite mode, even for 2D projects
                                            importer.textureType = TextureImporterType.Default;
                                        }

                                        importer.crunchedCompression = true;
                                        importer.sRGBTexture = !metallics.Contains(tex);
                                        importer.compressionQuality = 100;
                                        importer.textureCompression = TextureImporterCompression.CompressedHQ;
                                        importer.SaveAndReimport();
                                    }
                                    else
                                    {
                                        Debug.LogWarning(string.Format("GLTFImporter: Unable to import texture at path: {0}", texPath));
                                    }

                                    if (delayCallsCount == 0)
                                    {
                                        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
                                        AssetDatabase.SaveAssets();
                                    }
                                }
                            };
                        }

                        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        var temp = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        temp.SetActive(false);
                        var defaultMat = new[] { temp.GetComponent<Renderer>().sharedMaterial };
                        DestroyImmediate(temp);

                        foreach (var rend in renderers)
                        {
                            rend.sharedMaterials = defaultMat;
                        }
                    }


                }
                
                var rootObject = gltfScene.GetComponentInChildren<InstantiatedGLTFObject>();

                if (rootObject != null)
                    DestroyImmediate(rootObject);
            }
            catch (Exception e)
            {
                if (gltfScene)
                {
                    DestroyImmediate(gltfScene);
                }

                throw new Exception(e.Message + "\n" + e.StackTrace, e);
            }
            finally
            {
                delayCallsCount--;
            }

            // Set main asset
            ctx.AddObjectToAsset("main asset", gltfScene);

            // Add meshes
            foreach (var mesh in meshes)
            {
                try
                {
                    ctx.AddObjectToAsset("mesh " + mesh.name, mesh);
                }
                catch (System.InvalidOperationException e)
                {
                    Debug.LogWarning(e.ToString(), mesh);
                }
            }

            ctx.SetMainObject(gltfScene);
        }

        private string PatchInvalidFileNameChars(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            fileName = fileName.Replace(":", "_");
            fileName = fileName.Replace("|", "_");

            return fileName;
        }

        private void CopyOrNew<T>(T asset, string assetPath, Action<T> replaceReferences) where T : Object
        {
            var existingAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (existingAsset)
            {
                EditorUtility.CopySerialized(asset, existingAsset);
                replaceReferences(existingAsset);

                return;
            }

            AssetDatabase.CreateAsset(asset, assetPath);
        }

        private class TexMaterialMap
        {
            public UnityEngine.Material Material { get; set; }
            public string Property { get; set; }
            public bool IsNormalMap { get; set; }

            public TexMaterialMap(UnityEngine.Material material, string property, bool isNormalMap)
            {
                Material = material;
                Property = property;
                IsNormalMap = isNormalMap;
            }
        }
    }
}
#endif
