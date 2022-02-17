using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
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
        
        public static WearableCollectionsAPIData.Collection[] EnsureWearableCollections()
        {
            if (wearableCollections != null && wearableCollections.Length > 0)
                return wearableCollections;

            UnityWebRequest w = UnityWebRequest.Get(WearablesFetchingHelper.COLLECTIONS_FETCH_URL);
            w.SendWebRequest();

            while (!w.isDone) { }

            if (!w.WebRequestSucceded())
            {
                log.Error($"Request error! Wearable collections at '{WearablesFetchingHelper.COLLECTIONS_FETCH_URL}' couldn't be fetched! -- {w.error}");
                return null;
            }

            var collectionsApiData = JsonUtility.FromJson<WearableCollectionsAPIData>(w.downloadHandler.text);

            return wearableCollections = collectionsApiData.data;
        }

        public static string BuildWearableCollectionFetchingURL(string targetCollectionId)
        {
            return WearablesFetchingHelper.WEARABLES_FETCH_URL + "collectionId=" + targetCollectionId;
        }
        
        /// <summary>
        /// Wearables collection conversion batch-mode entry point
        /// </summary>
        // TODO: Fix this CLI pipeline to finish converting the assets and return the correct exit code
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

                if (Utils.ParseOption(commandLineArgs, Config.CLI_ALWAYS_BUILD_SYNTAX, 0, out _))
                    settings.skipAlreadyBuiltBundles = false;

                if (Utils.ParseOption(commandLineArgs, Config.CLI_KEEP_BUNDLES_SYNTAX, 0, out _))
                    settings.deleteDownloadPathAfterFinished = false;

                if (Utils.ParseOption(commandLineArgs, Config.CLI_BUILD_WEARABLES_COLLECTION_SYNTAX, 1, out string[] collectionId))
                {
                    if (collectionId == null || string.IsNullOrEmpty(collectionId[0]))
                    {
                        throw new ArgumentException("Invalid wearablesCollectionUrnId argument! Please use -wearablesCollectionUrnId <id> to establish the desired collection id to process.");
                    }

                    DumpSingleWearablesCollection(collectionId[0], settings);

                    return;
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
            settings.skipAlreadyBuiltBundles = false;
            settings.deleteDownloadPathAfterFinished = false;
            settings.clearDirectoriesOnStart = false;
            var abConverterCoreController = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);

            abConverterCoreController.InitializeDirectoryPaths(true, true);
            DumpWearableQueue(abConverterCoreController, itemQueue, GLTFImporter_OnBodyWearableLoad);
        }
        
        public static void DumpSingleWearablesCollection(string collectionId, ClientSettings settings = null)
        {
            EnsureEnvironment();
            
            EnsureWearableCollections();

            log.Info("Starting wearables dumping for collection: " + collectionId.Length);

            if (settings == null)
            {
                settings = new ClientSettings();
                settings.skipAlreadyBuiltBundles = false;
                settings.deleteDownloadPathAfterFinished = false;
                settings.clearDirectoriesOnStart = false;
            }
            
            var abConverterCoreController = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);
            
            DumpWearablesCollection(abConverterCoreController, collectionId, (x) =>
            {
                log.Info($"Finished single wearables collection dumping");
            });
        }

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
            
            // By manipulating these variables we control which collections are converted to batch manually
            int initialCollectionIndex = 0;
            int lastCollectionIndex = 50;
            DumpWearablesCollectionRange(abConverterCoreController, initialCollectionIndex, lastCollectionIndex);
        }

        private static void DumpWearablesCollectionRange(ABConverter.Core abConverterCoreController, int currentCollectionIndex, int lastCollectionIndex)
        {
            if (currentCollectionIndex >= wearableCollections.Length)
                return;

            string collectionId = wearableCollections[currentCollectionIndex].urn;
            
            log.Info($"Dumping... current collection: {currentCollectionIndex}, last collection: {lastCollectionIndex}");
            
            DumpWearablesCollection(abConverterCoreController, collectionId, (x) =>
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
                
                DumpWearablesCollectionRange(abConverterCoreController, currentCollectionIndex, lastCollectionIndex);
            });
        }

        private static void DumpWearablesCollection(ABConverter.Core abConverterCoreController, string collectionId, Action<Core.ErrorCodes> OnConversionFinish = null)
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
            
            Queue<WearableItem> itemQueue = new Queue<WearableItem>(avatarItemList);

            DumpWearableQueue(abConverterCoreController, itemQueue, GLTFImporter_OnNonBodyWearableLoad, OnConversionFinish);
        }

        /// <summary>
        /// Given a list of WearableItems, each one is downloaded along with its dependencies and converted to ABs recursively
        /// (to avoid mixing same-name dependencies between wearables)
        /// </summary>
        /// <param name="abConverterCoreController">an instance of the ABCore</param>
        /// <param name="items">an already-populated list of WearableItems</param>
        /// <param name="OnWearableLoad">an action to be bind to the OnWearableLoad event on each wearable</param>
        private static void DumpWearableQueue(ABConverter.Core abConverterCoreController, Queue<WearableItem> items, System.Action<UnityGLTF.GLTFSceneImporter> OnWearableLoad, Action<Core.ErrorCodes> OnConversionFinish = null)
        {
            // We toggle the core's ABs generation off so that we execute that conversion here when there is no more items left.
            abConverterCoreController.generateAssetBundles = false;

            if (items.Count == 0)
            {
                abConverterCoreController.ConvertDumpedAssets(OnConversionFinish);

                return;
            }

            var pairs = ExtractMappingPairs(new List<WearableItem>() { items.Dequeue() });
            
            abConverterCoreController.OnGLTFWillLoad += OnWearableLoad;
            
            abConverterCoreController.Convert(pairs.ToArray(),
                (err) =>
                {
                    abConverterCoreController.OnGLTFWillLoad -= OnWearableLoad;
                    abConverterCoreController.CleanupWorkingFolders();
                    DumpWearableQueue(abConverterCoreController, items, OnWearableLoad, OnConversionFinish);
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

            var wearablesApiData = JsonUtility.FromJson<WearablesAPIData>(w.downloadHandler.text);
            var resultList = wearablesApiData.GetWearableItems();

            // Since the wearables deployments response returns only a batch of elements, we need to fetch all the
            // batches sequentially
            if (!string.IsNullOrEmpty(wearablesApiData.pagination.next))
            {
                var nextPageResults = GetWearableItems(WearablesFetchingHelper.WEARABLES_FETCH_URL + wearablesApiData.pagination.next);
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
