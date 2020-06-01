using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF;
using UnityGLTF.Cache;
using MappingPair = DCL.ContentServerUtils.MappingPair;
using MappingsAPIData = DCL.ContentServerUtils.MappingsAPIData;

namespace DCL
{
    public static class AssetBundleBuilderConfig
    {
        internal const string CLI_VERBOSE = "verbose";
        internal const string CLI_ALWAYS_BUILD_SYNTAX = "alwaysBuild";
        internal const string CLI_KEEP_BUNDLES_SYNTAX = "keepBundles";
        internal const string CLI_BUILD_SCENE_SYNTAX = "sceneCid";
        internal const string CLI_BUILD_PARCELS_RANGE_SYNTAX = "parcelsXYWH";
        internal const string CLI_SET_CUSTOM_BASE_URL = "baseUrl";

        internal const string CLI_SET_CUSTOM_OUTPUT_ROOT_PATH = "output";

        internal static string ASSET_BUNDLE_FOLDER_NAME = "AssetBundles";
        internal static string DOWNLOADED_FOLDER_NAME = "_Downloaded";

        internal static string DOWNLOADED_ASSET_DB_PATH_ROOT = "Assets/" + DOWNLOADED_FOLDER_NAME;
        internal static string DOWNLOADED_PATH_ROOT = Application.dataPath + "/" + DOWNLOADED_FOLDER_NAME;
        internal static string ASSET_BUNDLES_PATH_ROOT = Application.dataPath + "/../" + ASSET_BUNDLE_FOLDER_NAME;

        internal static string[] bufferExtensions = { ".bin" };
        internal static string[] gltfExtensions = { ".glb", ".gltf" };
        internal static string[] textureExtensions = { ".jpg", ".png", ".jpeg", ".tga", ".gif", ".bmp", ".psd", ".tiff", ".iff" };
    }

    public class AssetBundleBuilder
    {
        static bool VERBOSE = false;

        public enum ErrorCodes
        {
            SUCCESS = 0,
            UNDEFINED = 1,
            SCENE_LIST_NULL = 2,
            ASSET_BUNDLE_BUILD_FAIL = 3,
        }

        private const int ASSET_REQUEST_RETRY_COUNT = 5;

        public Dictionary<string, string> hashLowercaseToHashProper = new Dictionary<string, string>();

        internal ContentServerUtils.ApiEnvironment environment = ContentServerUtils.ApiEnvironment.ORG;

        internal bool deleteDownloadPathAfterFinished = true;
        internal bool skipAlreadyBuiltBundles = true;

        internal string finalAssetBundlePath = "";
        internal string finalDownloadedPath = "";
        internal string finalDownloadedAssetDbPath = "";

        internal string customContentServerBaseUrl = "";

        float startTime;
        string logBuffer;
        int totalAssets;
        int skippedAssets;

        public AssetBundleBuilder(ContentServerUtils.ApiEnvironment environment = ContentServerUtils.ApiEnvironment.ORG)
        {
            this.environment = environment;
            finalAssetBundlePath = AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT + "/";
            finalDownloadedPath = AssetBundleBuilderConfig.DOWNLOADED_PATH_ROOT + "/";
            finalDownloadedAssetDbPath = AssetBundleBuilderConfig.DOWNLOADED_ASSET_DB_PATH_ROOT + "/";
        }

        public static void ExportSceneToAssetBundles()
        {
            ExportSceneToAssetBundles(Environment.GetCommandLineArgs());
        }

        public static void ExportSceneToAssetBundles(string[] commandLineArgs)
        {
            AssetBundleBuilder builder = new AssetBundleBuilder();
            builder.skipAlreadyBuiltBundles = true;
            builder.deleteDownloadPathAfterFinished = true;

            try
            {
                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleBuilderConfig.CLI_SET_CUSTOM_OUTPUT_ROOT_PATH, 1, out string[] outputPath))
                {
                    builder.finalAssetBundlePath = outputPath[0] + "/";
                }

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleBuilderConfig.CLI_SET_CUSTOM_BASE_URL, 1, out string[] customBaseUrl))
                {
                    ContentServerUtils.customBaseUrl = customBaseUrl[0];
                    builder.environment = ContentServerUtils.ApiEnvironment.NONE;
                }

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleBuilderConfig.CLI_VERBOSE, 0, out _))
                    VERBOSE = true;

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleBuilderConfig.CLI_ALWAYS_BUILD_SYNTAX, 0, out _))
                    builder.skipAlreadyBuiltBundles = false;

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleBuilderConfig.CLI_KEEP_BUNDLES_SYNTAX, 0, out _))
                    builder.deleteDownloadPathAfterFinished = false;

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleBuilderConfig.CLI_BUILD_SCENE_SYNTAX, 1, out string[] sceneCid))
                {
                    if (sceneCid == null || string.IsNullOrEmpty(sceneCid[0]))
                    {
                        throw new ArgumentException("Invalid sceneCid argument! Please use -sceneCid <id> to establish the desired id to process.");
                    }

                    builder.DumpScene(sceneCid[0]);
                    return;
                }
                else
                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleBuilderConfig.CLI_BUILD_PARCELS_RANGE_SYNTAX, 4, out string[] xywh))
                {
                    if (xywh == null)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please use -parcelsXYWH x y w h to establish the desired parcels range to process.");
                    }

                    int x, y, w, h;
                    bool parseSuccess = false;

                    parseSuccess |= int.TryParse(xywh[0], out x);
                    parseSuccess |= int.TryParse(xywh[1], out y);
                    parseSuccess |= int.TryParse(xywh[2], out w);
                    parseSuccess |= int.TryParse(xywh[3], out h);

                    if (!parseSuccess)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please use -parcelsXYWH x y w h to establish the desired parcels range to process.");
                    }

                    if (w > 10 || h > 10 || w < 0 || h < 0)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please don't use negative width/height values, and ensure any given width/height doesn't exceed 10.");
                    }

                    builder.DumpArea(new Vector2Int(x, y), new Vector2Int(w, h));
                    return;
                }
                else
                {
                    throw new ArgumentException("Invalid arguments! You must pass -parcelsXYWH or -sceneCid for dump to work!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                builder.CleanAndExit(ErrorCodes.UNDEFINED);
            }
        }

        private void CleanAndExit(ErrorCodes errorCode)
        {
            float conversionTime = Time.realtimeSinceStartup - startTime;
            string log = $"Conversion finished!. error code = {errorCode}";

            log += "\n";
            log += $"Converted {totalAssets - skippedAssets} of {totalAssets}. (Skipped {skippedAssets})\n";
            log += $"Total time: {conversionTime}";

            if (totalAssets > 0)
            {
                log += $"... Time per asset: {conversionTime / totalAssets}\n";
            }

            log += "\n";
            log += logBuffer;

            Debug.Log(log);

            CleanupWorkingFolders();
            AssetBundleBuilderUtils.Exit((int)errorCode);
        }


        private void DumpSceneTextures(MappingPair[] rawContents)
        {
            var hashToTexturePair = AssetBundleBuilderUtils.FilterExtensions(rawContents, AssetBundleBuilderConfig.textureExtensions);

            Dictionary<string, string> pathsToTag = new Dictionary<string, string>();

            //NOTE(Brian): Prepare textures. We should prepare all the dependencies in this phase.
            foreach (var kvp in hashToTexturePair)
            {
                string hash = kvp.Key;
                string file = kvp.Value.file;

                //NOTE(Brian): try to get an AB before getting the original texture, so we bind the dependencies correctly
                string fullPathToTag = DownloadAsset(kvp.Value.file, hash, hash + "/");

                string fileExt = Path.GetExtension(file);
                string assetPath = hash + "/" + hash + fileExt;

                AssetDatabase.ImportAsset(finalDownloadedAssetDbPath + assetPath, ImportAssetOptions.ForceUpdate);

                AssetDatabase.SaveAssets();

                string metaPath = finalDownloadedPath + assetPath + ".meta";

                AssetDatabase.ReleaseCachedFileHandles();

                //NOTE(Brian): in asset bundles, all dependencies are resolved by their guid (and not the AB hash nor CRC)
                //             So to ensure dependencies are being kept in subsequent editor runs we normalize the asset guid using
                //             the CID.
                string metaContent = File.ReadAllText(metaPath);
                string guid = AssetBundleBuilderUtils.CidToGuid(hash);
                string result = Regex.Replace(metaContent, @"guid: \w+?\n", $"guid: {guid}\n");

                //NOTE(Brian): We must do this hack in order to the new guid to be added to the AssetDatabase
                //             in windows, an AssetImporter.SaveAndReimport call makes the trick, but this won't work
                //             in Unix based OSes for some reason.
                File.Delete(metaPath);

                File.Copy(finalDownloadedPath + assetPath, finalDownloadedPath + "tmp");
                AssetDatabase.DeleteAsset(finalDownloadedAssetDbPath + assetPath);
                File.Delete(finalDownloadedPath + assetPath);

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                File.Copy(finalDownloadedPath + "tmp", finalDownloadedPath + assetPath);
                File.WriteAllText(metaPath, result);
                File.Delete(finalDownloadedPath + "tmp");

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                if (VERBOSE)
                {
                    Debug.Log($"content = {File.ReadAllText(metaPath)}");

                    Debug.Log("guid should be " + guid);
                    Debug.Log("guid is " + AssetDatabase.AssetPathToGUID(finalDownloadedAssetDbPath + assetPath));
                }

                if (fullPathToTag != null)
                {
                    pathsToTag.Add(fullPathToTag, hash);
                }
                else
                {
                    throw new Exception("Failed to get texture dependencies! failing asset: " + hash);
                }
            }

            foreach (var kvp in pathsToTag)
            {
                AssetBundleBuilderUtils.MarkForAssetBundleBuild(kvp.Key, kvp.Value);
            }
        }



        private bool DumpAssets(MappingPair[] rawContents)
        {
            var hashToGltfPair = AssetBundleBuilderUtils.FilterExtensions(rawContents, AssetBundleBuilderConfig.gltfExtensions);
            var hashToBufferPair = AssetBundleBuilderUtils.FilterExtensions(rawContents, AssetBundleBuilderConfig.bufferExtensions);

            Dictionary<string, string> pathsToTag = new Dictionary<string, string>();

            bool shouldAbortBecauseAllBundlesExist = true;

            totalAssets += hashToGltfPair.Count;

            if (skipAlreadyBuiltBundles)
            {
                int gltfCount = hashToGltfPair.Count;
                hashToGltfPair = hashToGltfPair.Where((kvp) => !File.Exists(finalAssetBundlePath + kvp.Key)).ToDictionary(x => x.Key, x => x.Value);
                int skippedCount = gltfCount - hashToGltfPair.Count;
                skippedAssets += skippedCount;
                shouldAbortBecauseAllBundlesExist = hashToGltfPair.Count == 0;
            }
            else
            {
                shouldAbortBecauseAllBundlesExist = false;
            }

            if (shouldAbortBecauseAllBundlesExist)
            {
                if (VERBOSE)
                    Debug.Log("All assets in this scene were already generated!. Skipping.");

                return false;
            }

            DumpSceneTextures(rawContents);

            //NOTE(Brian): Prepare buffers. We should prepare all the dependencies in this phase.
            foreach (var kvp in hashToBufferPair)
            {
                string hash = kvp.Key;

                var result = DownloadAsset(kvp.Value.file, hash, hash + "/");

                if (result == null)
                {
                    throw new Exception("Failed to get buffer dependencies! failing asset: " + hash);
                }
            }

            GLTFImporter.OnGLTFRootIsConstructed -= AssetBundleBuilderUtils.FixGltfRootInvalidUriCharacters;
            GLTFImporter.OnGLTFRootIsConstructed += AssetBundleBuilderUtils.FixGltfRootInvalidUriCharacters;

            List<Stream> streamsToDispose = new List<Stream>();

            //NOTE(Brian): Prepare gltfs gathering its dependencies first and filling the importer's static cache.
            foreach (var kvp in hashToGltfPair)
            {
                string gltfHash = kvp.Key;
                string gltfFilePath = kvp.Value.file;

                PersistentAssetCache.ImageCacheByUri.Clear();
                PersistentAssetCache.StreamCacheByUri.Clear();

                foreach (var contentPair in rawContents)
                {
                    string contentFilePathLower = contentPair.file.ToLowerInvariant();
                    string contentFilePath = contentPair.file;

                    bool endsWithTextureExtensions = AssetBundleBuilderConfig.textureExtensions.Any((x) => contentFilePathLower.EndsWith(x));

                    if (endsWithTextureExtensions)
                    {
                        RetrieveAndInjectTexture(hashToGltfPair, gltfHash, contentPair);
                    }

                    bool endsWithBufferExtensions = AssetBundleBuilderConfig.bufferExtensions.Any((x) => contentFilePathLower.EndsWith(x));

                    if (endsWithBufferExtensions)
                    {
                        RetrieveAndInjectBuffer(gltfFilePath, contentPair, contentFilePath);
                    }
                }

                //NOTE(Brian): Finally, load the gLTF. The GLTFImporter will use the PersistentAssetCache to resolve the external dependencies.
                string path = DownloadAsset(gltfFilePath, gltfHash, gltfHash + "/");

                if (path != null)
                {
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                    pathsToTag.Add(path, gltfHash);
                }

                foreach (var streamDataKvp in PersistentAssetCache.StreamCacheByUri)
                {
                    if (streamDataKvp.Value.stream != null)
                        streamsToDispose.Add(streamDataKvp.Value.stream);
                }
            }

            foreach (var kvp in pathsToTag)
            {
                AssetBundleBuilderUtils.MarkForAssetBundleBuild(kvp.Key, kvp.Value);
            }

            foreach (var s in streamsToDispose)
            {
                s.Dispose();
            }

            return true;
        }


        internal bool BuildAssetBundles(out AssetBundleManifest manifest)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            AssetDatabase.SaveAssets();

            AssetDatabase.MoveAsset(finalDownloadedAssetDbPath, AssetBundleBuilderConfig.DOWNLOADED_ASSET_DB_PATH_ROOT);

            manifest = BuildPipeline.BuildAssetBundles(finalAssetBundlePath, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);

            if (manifest == null)
            {
                Debug.LogError("Error generating asset bundle!");
                return false;
            }

            DependencyMapBuilder.Generate(finalAssetBundlePath, hashLowercaseToHashProper, manifest);
            logBuffer += $"Generating asset bundles at path: {finalAssetBundlePath}\n";

            string[] assetBundles = manifest.GetAllAssetBundles();

            logBuffer += $"Total generated asset bundles: {assetBundles.Length}\n";

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                logBuffer += $"#{i} Generated asset bundle name: {assetBundles[i]}\n";
            }

            return true;
        }

        public bool DownloadAndConvertAssets(MappingPair[] rawContents, System.Action<ErrorCodes> OnFinish = null)
        {
            if (OnFinish == null)
                OnFinish = CleanAndExit;

            startTime = Time.realtimeSinceStartup;

            InitializeDirectoryPaths(true);
            PopulateLowercaseMappings(rawContents);

            float timer = Time.realtimeSinceStartup;
            bool shouldGenerateAssetBundles = true;
            bool assetsAlreadyDumped = false;

            EditorApplication.CallbackFunction updateLoop = null;

            updateLoop = () =>
            {
                try
                {
                    //NOTE(Brian): We have to check this because the ImportAsset for GLTFs is not synchronous, and must execute some delayed calls
                    //             after the import asset finished. Therefore, we have to make sure those calls finished before continuing.
                    if (!GLTFImporter.finishedImporting && Time.realtimeSinceStartup - timer < 60)
                        return;

                    AssetDatabase.Refresh();

                    if (!assetsAlreadyDumped)
                    {
                        shouldGenerateAssetBundles |= DumpAssets(rawContents);
                        assetsAlreadyDumped = true;
                        timer = Time.realtimeSinceStartup;

                        //NOTE(Brian): return in order to wait for GLTFImporter.finishedImporting flag, as it will set asynchronously.
                        return;
                    }

                    EditorApplication.update -= updateLoop;

                    if (shouldGenerateAssetBundles)
                    {
                        AssetBundleManifest manifest;

                        if (BuildAssetBundles(out manifest))
                        {
                            CleanAssetBundleFolder(manifest.GetAllAssetBundles());
                            OnFinish?.Invoke(ErrorCodes.SUCCESS);
                        }
                        else
                        {
                            OnFinish?.Invoke(ErrorCodes.ASSET_BUNDLE_BUILD_FAIL);
                        }
                    }
                    else
                    {
                        OnFinish?.Invoke(ErrorCodes.SUCCESS);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    OnFinish?.Invoke(ErrorCodes.UNDEFINED);
                    EditorApplication.update -= updateLoop;
                }
            };

            EditorApplication.update += updateLoop;
            return true;
        }

        void ConvertScenesToAssetBundles(List<string> sceneCidsList, System.Action<ErrorCodes> OnFinish = null)
        {
            if (OnFinish == null)
                OnFinish = CleanAndExit;

            if (sceneCidsList == null || sceneCidsList.Count == 0)
            {
                Debug.LogError("Scene list is null or count == 0! Maybe this sector lacks scenes or content requests failed?");
                OnFinish?.Invoke(ErrorCodes.SCENE_LIST_NULL);
                return;
            }

            Debug.Log($"Building {sceneCidsList.Count} scenes...");

            List<MappingPair> rawContents = new List<MappingPair>();

            foreach (var sceneCid in sceneCidsList)
            {
                MappingsAPIData parcelInfoApiData = AssetBundleBuilderUtils.GetSceneMappingsData(environment, sceneCid);
                rawContents.AddRange(parcelInfoApiData.data[0].content.contents);
            }

            DownloadAndConvertAssets(rawContents.ToArray(), OnFinish);
        }

        private void CleanAssetBundleFolder(string[] assetBundles)
        {
            AssetBundleBuilderUtils.CleanAssetBundleFolder(finalAssetBundlePath, assetBundles, hashLowercaseToHashProper);
        }



        internal void PopulateLowercaseMappings(MappingPair[] rawContents)
        {
            //NOTE(Brian): Prepare gltfs gathering its dependencies first and filling the importer's static cache.
            foreach (var content in rawContents)
            {
                string hashLower = content.hash.ToLowerInvariant();

                if (!hashLowercaseToHashProper.ContainsKey(hashLower))
                    hashLowercaseToHashProper.Add(hashLower, content.hash);
            }
        }

        private void RetrieveAndInjectTexture(Dictionary<string, MappingPair> hashToGltfPair, string gltfHash, MappingPair textureMappingPair)
        {
            string fileExt = Path.GetExtension(textureMappingPair.file);
            string realOutputPath = finalDownloadedPath + textureMappingPair.hash + "/" + textureMappingPair.hash + fileExt;
            Texture2D t2d = null;

            if (File.Exists(realOutputPath))
            {
                string assetDBOutputPath = finalDownloadedAssetDbPath + textureMappingPair.hash + "/" + textureMappingPair.hash + fileExt;
                t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(assetDBOutputPath);
            }

            if (t2d != null)
            {
                string relativePath = AssetBundleBuilderUtils.GetRelativePathTo(hashToGltfPair[gltfHash].file, textureMappingPair.file);
                //NOTE(Brian): This cache will be used by the GLTF importer when seeking textures. This way the importer will
                //             consume the asset bundle dependencies instead of trying to create new textures.
                PersistentAssetCache.ImageCacheByUri[relativePath] = new RefCountedTextureData(relativePath, t2d);
            }
        }

        private void RetrieveAndInjectBuffer(string gltfFilePath, MappingPair bufferMappingPair, string contentFilePath)
        {
            string fileExt = Path.GetExtension(bufferMappingPair.file);
            string realOutputPath = finalDownloadedPath + bufferMappingPair.hash + "/" + bufferMappingPair.hash + fileExt;

            if (File.Exists(realOutputPath))
            {
                Stream stream = File.OpenRead(realOutputPath);
                string relativePath = AssetBundleBuilderUtils.GetRelativePathTo(gltfFilePath, contentFilePath);

                // NOTE(Brian): This cache will be used by the GLTF importer when seeking streams. This way the importer will
                //              consume the asset bundle dependencies instead of trying to create new streams.
                PersistentAssetCache.StreamCacheByUri[relativePath] = new RefCountedStreamData(relativePath, stream);
            }
        }

        private string DownloadAsset(string fileName, string hash, string additionalPath = "")
        {
            string baseUrl = ContentServerUtils.GetContentAPIUrlBase(environment);

            string fileExt = Path.GetExtension(fileName);

            string outputPath = finalDownloadedPath + additionalPath + hash + fileExt;
            string outputPathDir = Path.GetDirectoryName(outputPath);

            string finalUrl = baseUrl + hash;

            if (VERBOSE)
                Debug.Log("checking against " + outputPath);

            if (File.Exists(outputPath))
            {
                if (VERBOSE)
                    Debug.Log("Skipping already generated asset: " + outputPath);

                return finalDownloadedPath + additionalPath;
            }

            UnityWebRequest req;

            int retryCount = ASSET_REQUEST_RETRY_COUNT;

            do
            {
                req = UnityWebRequest.Get(finalUrl);
                req.SendWebRequest();
                while (req.isDone == false) { }

                retryCount--;

                if (retryCount == 0)
                    return null;
            }
            while (!req.WebRequestSucceded());

            if (VERBOSE)
            {
                Debug.Log($"Downloaded asset = {finalUrl} to {outputPathDir}");
            }

            if (!Directory.Exists(outputPathDir))
                Directory.CreateDirectory(outputPathDir);

            File.WriteAllBytes(outputPath, req.downloadHandler.data);

            return finalDownloadedPath + additionalPath;
        }


        internal void DumpArea(Vector2Int coords, Vector2Int size, Action<ErrorCodes> OnFinish = null)
        {
            HashSet<string> sceneCids = AssetBundleBuilderUtils.GetSceneCids(environment, coords, size);

            List<string> sceneCidsList = sceneCids.ToList();
            ConvertScenesToAssetBundles(sceneCidsList, OnFinish);
        }

        internal void DumpArea(List<Vector2Int> coords, Action<ErrorCodes> OnFinish = null)
        {
            HashSet<string> sceneCids = AssetBundleBuilderUtils.GetScenesCids(environment, coords);

            List<string> sceneCidsList = sceneCids.ToList();
            ConvertScenesToAssetBundles(sceneCidsList, OnFinish);
        }

        internal void DumpScene(string cid, Action<ErrorCodes> OnFinish = null)
        {
            ConvertScenesToAssetBundles(new List<string> { cid }, OnFinish);
        }

        internal void InitializeDirectoryPaths(bool deleteIfExists)
        {
            AssetBundleBuilderUtils.InitializeDirectory(finalDownloadedPath, deleteIfExists);
            AssetBundleBuilderUtils.InitializeDirectory(finalAssetBundlePath, deleteIfExists);
        }

        internal void CleanupWorkingFolders()
        {
            AssetBundleBuilderUtils.DeleteFile(finalAssetBundlePath + AssetBundleBuilderConfig.ASSET_BUNDLE_FOLDER_NAME);
            AssetBundleBuilderUtils.DeleteFile(finalAssetBundlePath + AssetBundleBuilderConfig.ASSET_BUNDLE_FOLDER_NAME + ".manifest");

            if (deleteDownloadPathAfterFinished)
            {
                AssetBundleBuilderUtils.DeleteDirectory(finalDownloadedPath);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }
    }
}
