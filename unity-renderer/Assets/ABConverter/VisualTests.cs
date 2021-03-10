using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL.Helpers;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace DCL.ABConverter
{
    public static class VisualTests
    {
        static readonly string abPath = Application.dataPath + "/../AssetBundles/";
        static readonly string baselinePath = VisualTestHelpers.baselineImagesPath;
        static readonly string testImagesPath = VisualTestHelpers.testImagesPath;
        static readonly float snapshotCamOffset = 3;

        public static IEnumerator TestConvertedAssets(Environment env = null, Action<int> OnFinish = null)
        {
            EditorSceneManager.OpenScene($"Assets/ABConverter/VisualTestScene.unity", OpenSceneMode.Single);

            VisualTestHelpers.baselineImagesPath += "ABConverter/";
            VisualTestHelpers.testImagesPath += "ABConverter/";

            var gltfs = LoadAndInstantiateAllGltfAssets();

            VisualTestHelpers.generateBaseline = true;

            foreach (GameObject go in gltfs)
            {
                go.SetActive(false);
            }

            foreach (GameObject go in gltfs)
            {
                go.SetActive(true);

                // unify all child renderer bounds and use that to position the camera
                var mergedBounds = Helpers.Utils.BuildMergedBounds(go.GetComponentsInChildren<Renderer>());
                Vector3 cameraPosition = new Vector3(mergedBounds.min.x - snapshotCamOffset, mergedBounds.max.y + snapshotCamOffset, mergedBounds.min.z - snapshotCamOffset);

                yield return VisualTestHelpers.TakeSnapshot($"ABConverter_{go.name}.png", Camera.main, cameraPosition, mergedBounds.center);
                go.SetActive(false);
            }

            VisualTestHelpers.generateBaseline = false;

            var abs = LoadAndInstantiateAllAssetBundles();

            foreach (GameObject go in abs)
            {
                go.SetActive(false);
            }

            int skippedAssets = 0;

            foreach (GameObject go in abs)
            {
                string testName = $"ABConverter_{go.name}.png";

                go.SetActive(true);

                // unify all child renderer bounds and use that to position the camera
                var mergedBounds = Helpers.Utils.BuildMergedBounds(go.GetComponentsInChildren<Renderer>());
                Vector3 cameraPosition = new Vector3(mergedBounds.min.x - snapshotCamOffset, mergedBounds.max.y + snapshotCamOffset, mergedBounds.min.z - snapshotCamOffset);

                yield return VisualTestHelpers.TakeSnapshot(testName, Camera.main, cameraPosition, mergedBounds.center);

                bool result = VisualTestHelpers.TestSnapshot(
                    VisualTestHelpers.baselineImagesPath + testName,
                    VisualTestHelpers.testImagesPath + testName,
                    95,
                    false);

                // Delete failed AB files to avoid uploading them
                if (!result && env != null)
                {
                    string filePath = abPath + go.name;
                    if (env.file.Exists(filePath))
                    {
                        env.file.Delete(filePath);

                        string depMapPath = filePath + ".depmap";
                        env.file.Delete(depMapPath);
                    }

                    skippedAssets++;

                    // TODO: Notify some metrics API or something to let know that this asset has conversion problems and we should manually take a look
                    Debug.Log("Visual Test Detection: FAILED converting asset -> " + go.name);
                }

                go.SetActive(false);
            }

            VisualTestHelpers.baselineImagesPath = baselinePath;
            VisualTestHelpers.testImagesPath = testImagesPath;

            OnFinish?.Invoke(skippedAssets);
            yield break;
        }

        public static GameObject[] LoadAndInstantiateAllGltfAssets()
        {
            var assets = AssetDatabase.FindAssets($"t:GameObject", new[] {"Assets/_Downloaded"});

            List<GameObject> importedGLTFs = new List<GameObject>();

            foreach (var guid in assets)
            {
                GameObject gltf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                var importedGLTF = Object.Instantiate(gltf);
                importedGLTF.name = importedGLTF.name.Replace("(Clone)", "");
                importedGLTFs.Add(importedGLTF);
            }

            return importedGLTFs.ToArray();
        }

        public static GameObject[] LoadAndInstantiateAllAssetBundles()
        {
            Caching.ClearCache();

            string workingFolderName = "_Downloaded";

            var pathList = Directory.GetDirectories(Application.dataPath + "/" + workingFolderName);

            List<string> dependencyAbs = new List<string>();
            List<string> mainAbs = new List<string>();

            foreach (var paths in pathList)
            {
                var hash = new DirectoryInfo(paths).Name;
                var path = "Assets/" + workingFolderName + "/" + hash;
                var guids = AssetDatabase.FindAssets("t:GameObject", new[] {path});

                // NOTE(Brian): If no gameObjects are found, we assume they are dependency assets (textures, etc).
                if (guids.Length == 0)
                {
                    dependencyAbs.Add(hash);
                }
                else
                {
                    // Otherwise we assume they are gltfs.
                    mainAbs.Add(hash);
                }
            }

            // NOTE(Brian): We need to store the asset bundles so they can be unloaded later.
            List<AssetBundle> loadedAbs = new List<AssetBundle>();

            foreach (var hash in dependencyAbs)
            {
                string path = abPath + hash;
                var req = UnityWebRequestAssetBundle.GetAssetBundle(path);

                if(SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                    req.url = req.url.Replace("http://localhost", "file:///");

                req.SendWebRequest();

                while (!req.isDone)
                {
                }

                if (req.isHttpError || req.isNetworkError)
                {
                    Debug.Log("Visual Test Detection: Failed to instantiate AB, missing source file for : " + hash);
                    continue;
                }

                var assetBundle = DownloadHandlerAssetBundle.GetContent(req);
                assetBundle.LoadAllAssets();
                loadedAbs.Add(assetBundle);
            }

            List<GameObject> results = new List<GameObject>();

            foreach (var hash in mainAbs)
            {
                string path = abPath + hash;
                var req = UnityWebRequestAssetBundle.GetAssetBundle(path);

                if(SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                    req.url = req.url.Replace("http://localhost", "file:///");

                req.SendWebRequest();

                while (!req.isDone)
                {
                }

                if (req.isHttpError || req.isNetworkError)
                {
                    Debug.Log("Visual Test Detection: Failed to instantiate AB, missing source file for : " + hash);
                    continue;
                }

                var assetBundle = DownloadHandlerAssetBundle.GetContent(req);
                Object[] assets = assetBundle.LoadAllAssets();

                foreach (Object asset in assets)
                {
                    if (asset is Material material)
                    {
                        material.shader = Shader.Find("DCL/LWRP/Lit");
                    }

                    if (asset is GameObject assetAsGameObject)
                    {
                        GameObject instance = Object.Instantiate(assetAsGameObject);
                        results.Add(instance);
                        instance.name = instance.name.Replace("(Clone)", "");
                    }
                }

                loadedAbs.Add(assetBundle);
            }

            foreach (var ab in loadedAbs)
            {
                ab.Unload(false);
            }

            return results.ToArray();
        }
    }
}