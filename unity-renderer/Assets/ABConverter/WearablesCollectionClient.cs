using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.ABConverter
{
    public static class WearablesCollectionClient
    {
        private static WearableCollectionsAPIData.Collection[] wearableCollections;
        private static Environment env;
        private static Logger log = new Logger("ABConverter.WearablesCollectionClient");
        private static double wearablesCollectionDumpStartTime;

        public static Environment EnsureEnvironment()
        {
            if (env == null)
                env = Environment.CreateWithDefaultImplementations();

            return env;
        }
        
        /// <summary>
        /// Fetch the newest 1000 collections (max amount that can be fetched)
        /// </summary>
        /// <returns>A list of the fetched wearable collections</returns>
        public static WearableCollectionsAPIData.Collection[] EnsureWearableCollections()
        {
            if (wearableCollections != null && wearableCollections.Length > 0)
                return wearableCollections;

            UnityWebRequest w = UnityWebRequest.Get(WearablesFetchingHelper.GetCollectionsFetchURL());
            w.SendWebRequest();

            while (!w.isDone) { }

            if (!w.WebRequestSucceded())
            {
                log.Error($"Request error! Wearable collections at '{WearablesFetchingHelper.GetCollectionsFetchURL()}' couldn't be fetched! -- {w.error}");
                return null;
            }

            var collectionsApiData = JsonUtility.FromJson<WearableCollectionsAPIData>(w.downloadHandler.text);

            return wearableCollections = collectionsApiData.data;
        }

        public static string BuildWearableCollectionFetchingURL(string targetCollectionId)
        {
            return WearablesFetchingHelper.GetWearablesFetchURL() + "collectionId=" + targetCollectionId;
        }
        
        /// <summary>
        /// Wearables collection conversion batch-mode entry point
        /// </summary>
        public static void ExportWearablesCollectionToAssetBundles()
        {
            //NOTE(Brian): This should make the logs cleaner
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
            
            EnsureEnvironment();
            ExportWearablesCollectionToAssetBundles(System.Environment.GetCommandLineArgs());
        }
        
        /// <summary>
        /// Start the wearables collection conversion process with the given commandLineArgs.
        /// </summary>
        /// <param name="commandLineArgs">An array with the command line arguments.</param>
        /// <exception cref="ArgumentException">When an invalid argument is passed</exception>
        public static void ExportWearablesCollectionToAssetBundles(string[] commandLineArgs)
        {
            ClientSettings settings = new ClientSettings();
            try
            {
                if (Utils.ParseOption(commandLineArgs, Config.CLI_SET_CUSTOM_OUTPUT_ROOT_PATH, 1, out string[] outputPath))
                {
                    settings.finalAssetBundlePath = outputPath[0] + "/";
                }

                if (Utils.ParseOption(commandLineArgs, Config.CLI_SET_CUSTOM_BASE_URL, 1, out string[] customBaseUrl))
                    settings.baseUrl = customBaseUrl[0];

                if (Utils.ParseOption(commandLineArgs, Config.CLI_VERBOSE, 0, out _))
                    settings.verbose = true;

                if (Utils.ParseOption(commandLineArgs, Config.CLI_BUILD_WEARABLES_COLLECTION_SYNTAX, 1, out string[] collectionId))
                {
                    if (collectionId == null || string.IsNullOrEmpty(collectionId[0]))
                    {
                        throw new ArgumentException("Invalid wearablesCollectionUrnId argument! Please use -wearablesCollectionUrnId <id> to establish the desired collection id to process.");
                    }
                    
                    log.Info("found 'wearablesCollectionUrnId' param, will try to convert collection with id: " + collectionId[0]);

                    DumpSingleWearablesCollection(collectionId[0], settings);

                    return;
                }
                
                if (Utils.ParseOption(commandLineArgs, Config.CLI_BUILD_WEARABLES_COLLECTION_RANGE_START_SYNTAX, 1, out string[] firstCollectionIndex))
                {
                    if (firstCollectionIndex == null || string.IsNullOrEmpty(firstCollectionIndex[0]))
                    {
                        throw new ArgumentException("Invalid firstCollectionIndex argument! Please use -firstCollectionIndex <index> to define the first collection to convert in the batch");
                    }
                    int firstCollectionIndexInt = Int32.Parse(firstCollectionIndex[0]);

                    if (Utils.ParseOption(commandLineArgs, Config.CLI_BUILD_WEARABLES_COLLECTION_RANGE_END_SYNTAX, 1, out string[] lastCollectionIndex))
                    {
                        if (lastCollectionIndex == null || string.IsNullOrEmpty(lastCollectionIndex[0]))
                        {
                            throw new ArgumentException("Invalid wearablesLastCollectionIndex argument! Please use -wearablesLastCollectionIndex <index> to define the last collection to convert in the batch");
                        }

                        int lastCollectionIndexInt = Int32.Parse(lastCollectionIndex[0]);

                        if (lastCollectionIndexInt < firstCollectionIndexInt)
                        {
                            throw new ArgumentException("Invalid wearablesLastCollectionIndex argument! Please use a wearablesLastCollectionIndex that's equal or higher than the first collection index");
                        }
                        
                        DumpWearablesCollectionRange(firstCollectionIndexInt, lastCollectionIndexInt, settings);

                        return;
                    }
                }

                throw new ArgumentException("Invalid arguments! You must pass -wearablesCollectionUrnId for dump to work!");
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }
        
        /// <summary>
        /// Dump all bodyshape wearables normally, including their imported skeleton 
        /// </summary>
        public static void DumpAllBodyshapeWearables()
        {   
            EnsureEnvironment();
            
            List<WearableItem> avatarItemList = GetWearableItems(BuildWearableCollectionFetchingURL(WearablesFetchingHelper.BASE_WEARABLES_COLLECTION_ID))
                .Where(x => x.data.category == WearableLiterals.Categories.BODY_SHAPE)
                .ToList();

            Queue<WearableItem> itemQueue = new Queue<WearableItem>(avatarItemList);
            var settings = new ClientSettings();
            SetupClientSettingsForWearablesConversion(settings);
            var abConverterCoreController = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);

            abConverterCoreController.InitializeDirectoryPaths(true, true);
            DumpWearableQueue(abConverterCoreController, itemQueue, GLTFImporter_OnBodyWearableLoad);
        }
        
        public static void DumpSingleWearablesCollection(string collectionId, ClientSettings settings = null)
        {
            EnsureEnvironment();

            log.Info("Starting wearables dumping for collection: " + collectionId);

            settings ??= new ClientSettings();
            SetupClientSettingsForWearablesConversion(settings);
            
            var abConverterCoreController = new ABConverter.Core(env, settings);
            
            DumpWearablesCollection(abConverterCoreController, collectionId, (x) =>
            {
                log.Info($"Finished single wearables collection dumping");
            });
        }
        
        public static void DumpWearablesCollectionRange(int firstCollectionIndex, int lastCollectionIndex, ClientSettings settings = null)
        {
            wearablesCollectionDumpStartTime = EditorApplication.timeSinceStartup;
            
            EnsureEnvironment();
            
            EnsureWearableCollections();

            log.Info($"Starting wearables collections range dumping, total collections in range: {(1 + lastCollectionIndex - firstCollectionIndex)}");

            settings ??= new ClientSettings();
            SetupClientSettingsForWearablesConversion(settings);
            
            var abConverterCoreController = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);
            
            DumpWearablesCollectionRange(abConverterCoreController, firstCollectionIndex, lastCollectionIndex);
        }

        private static void SetupClientSettingsForWearablesConversion(ClientSettings settings)
        {
            settings.skipAlreadyBuiltBundles = false;
            settings.deleteDownloadPathAfterFinished = false;
            settings.clearDirectoriesOnStart = false;
        }

        // By manipulating these variables we control which collections are converted to batch manually
        private const int INITIAL_COLLECTION_INDEX = 0;
        private const int LAST_COLLECTION_INDEX = 10; 
        
        /// <summary>
        /// Dump all non-bodyshape wearables, optimized to remove the skeleton for the wearables ABs since that is
        /// only needed for the body shapes (and the WearablesController sets it up for non-bodyshapes in runtime).
        /// Each collection is dumped and converted sequentially as the amount of collections has grown more that we
        /// can handle in 1 massive dump-conversion.
        /// </summary>
        public static void DumpAllNonBodyshapeWearables()
        {
            wearablesCollectionDumpStartTime = EditorApplication.timeSinceStartup;
            
            EnsureEnvironment();
            
            EnsureWearableCollections();

            log.Info("Starting all non-bodyshape wearables dumping, total collections: " + wearableCollections.Length);
            
            var settings = new ClientSettings();
            settings.skipAlreadyBuiltBundles = false;
            settings.deleteDownloadPathAfterFinished = false;
            settings.clearDirectoriesOnStart = false;
            var abConverterCoreController = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);
            
            DumpWearablesCollectionRange(abConverterCoreController, INITIAL_COLLECTION_INDEX, LAST_COLLECTION_INDEX);
        }

        private static void DumpWearablesCollectionRange(ABConverter.Core core, int currentCollectionIndex, int lastCollectionIndex)
        {
            if (currentCollectionIndex >= wearableCollections.Length)
                return;

            string collectionId = wearableCollections[currentCollectionIndex].urn;
            
            log.Info($"Dumping... current collection: {currentCollectionIndex}, last collection: {lastCollectionIndex}");
            
            DumpWearablesCollection(core, collectionId, (x) =>
            {
                currentCollectionIndex++;

                if (currentCollectionIndex > lastCollectionIndex || currentCollectionIndex >= wearableCollections.Length)
                {
                    double totalTime = EditorApplication.timeSinceStartup - wearablesCollectionDumpStartTime;
                    
                    TimeSpan t = TimeSpan.FromSeconds(totalTime);
                    string formattedTotalTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds);
                    
                    log.Info($"Finished all non-bodyshape wearables dumping, total time: {formattedTotalTime}");
                    
                    return;
                }
                
                DumpWearablesCollectionRange(core, currentCollectionIndex, lastCollectionIndex);
            }, cleanAndExitOnFinish: currentCollectionIndex == lastCollectionIndex);
        }

        private static void DumpWearablesCollection(ABConverter.Core abConverterCoreController, string collectionId, Action<Core.ErrorCodes> OnConversionFinish = null, bool cleanAndExitOnFinish = true)
        {
            if (string.IsNullOrEmpty(collectionId))
                return;
            
            abConverterCoreController.InitializeDirectoryPaths(true, false);
         
            log.Info($"Dumping wearables from collection {collectionId}");
            
            List<WearableItem> avatarItemList = GetWearableItems(BuildWearableCollectionFetchingURL(collectionId))?
                                                .Where(x => x?.data?.category != WearableLiterals.Categories.BODY_SHAPE)
                                                .ToList();

            if (avatarItemList == null)
            {
                log.Info($"Can't dump wearable items of collection '{collectionId}' because it has no items!");    
                
                OnConversionFinish?.Invoke(Core.ErrorCodes.SOME_ASSET_BUNDLES_SKIPPED);
                return;
            }
            
            log.Info($"found wearable items: {avatarItemList.Count}");
            
            Queue<WearableItem> itemQueue = new Queue<WearableItem>(avatarItemList);

            DumpWearableQueue(abConverterCoreController, itemQueue, GLTFImporter_OnNonBodyWearableLoad, OnConversionFinish, cleanAndExitOnFinish);
        }

        /// <summary>
        /// Given a list of WearableItems, each one is downloaded along with its dependencies and converted to ABs recursively
        /// (to avoid mixing same-name dependencies between wearables)
        /// </summary>
        /// <param name="core">an instance of the ABCore</param>
        /// <param name="items">an already-populated list of WearableItems</param>
        /// <param name="OnWearableLoad">an action to be bind to the OnWearableLoad event on each wearable</param>
        private static void DumpWearableQueue(ABConverter.Core core, Queue<WearableItem> items, System.Action<UnityGLTF.GLTFSceneImporter> OnWearableLoad, Action<Core.ErrorCodes> OnConversionFinish = null, bool cleanAndExitOnFinish = true)
        {
            // We toggle the core's ABs generation off so that we execute that conversion here when there is no more items left.
            core.generateAssetBundles = false;
            core.cleanAndExitOnFinish = false;

            if (items.Count == 0)
            {
                core.ConvertDumpedAssets((x) =>
                {
                    OnConversionFinish?.Invoke(x);
                    
                    if(cleanAndExitOnFinish)
                        core.CleanAndExit(x);
                });

                return;
            }

            var pairs = ExtractMappingPairs(new List<WearableItem>() { items.Dequeue() });
            
            core.OnGLTFWillLoad += OnWearableLoad;
            
            log.Info($"will dump mapping pairs: {pairs.Count}");
            
            core.Convert(pairs.ToArray(),
                (err) =>
                {
                    log.Info($"finished dumping a mapping pair...");
                    core.OnGLTFWillLoad -= OnWearableLoad;
                    core.CleanupWorkingFolders();
                    DumpWearableQueue(core, items, OnWearableLoad, OnConversionFinish, cleanAndExitOnFinish);
                });
        }

        /// <summary>
        /// Given a list of WearableItems, extracts and returns a list of MappingPairs
        /// </summary>
        /// <param name="wearableItems">A list of already-populated WearableItems</param>
        /// <returns>A list of the extracted Wearables MappingPairs</returns>
        private static List<ContentServerUtils.MappingPair> ExtractMappingPairs(List<WearableItem> wearableItems)
        {
            var result = new List<ContentServerUtils.MappingPair>();

            foreach (var wearable in wearableItems)
            {
                foreach (var representation in wearable.data.representations)
                {
                    foreach (var datum in representation.contents)
                    {
                        result.Add(new ContentServerUtils.MappingPair() { file = datum.key, hash = datum.hash });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Given a base-url to fetch wearables collections, returns a list of all the WearableItems
        /// </summary>
        /// <param name="url">base-url to fetch the wearables collections</param>
        /// <returns>A list of all the WearableItems found</returns>
        private static List<WearableItem> GetWearableItems(string url)
        {
            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (!w.isDone) { }

            if (!w.WebRequestSucceded())
            {
                log.Error($"Request error! Wearable at '{url}' couldn't be fetched! -- {w.error}");
                return null;
            }

            var wearablesApiData = JsonConvert.DeserializeObject<WearablesAPIData>(w.downloadHandler.text);
            var resultList = wearablesApiData.GetWearableItems();

            // Since the wearables deployments response returns only a batch of elements, we need to fetch all the
            // batches sequentially
            if (!string.IsNullOrEmpty(wearablesApiData.pagination.next))
            {
                var nextPageResults = GetWearableItems(WearablesFetchingHelper.GetWearablesFetchURL() + wearablesApiData.pagination.next);
                resultList.AddRange(nextPageResults);
            }

            return resultList;
        }

        private static void GLTFImporter_OnNonBodyWearableLoad(UnityGLTF.GLTFSceneImporter obj)
        {
            obj.importSkeleton = false;
            obj.maxTextureSize = 512;
        }

        private static void GLTFImporter_OnBodyWearableLoad(UnityGLTF.GLTFSceneImporter obj)
        {
            obj.importSkeleton = true;
            obj.maxTextureSize = 512;
        }
    }
}
