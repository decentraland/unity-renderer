using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DCL.ABConverter
{
    public static class SceneClient
    {
        private static Logger log = new Logger("ABConverter.SceneClient");
        public static Environment env;

        public static Environment EnsureEnvironment()
        {
            if (env == null)
                env = Environment.CreateWithDefaultImplementations();

            return env;
        }

        /// <summary>
        /// Scenes conversion batch-mode entry point
        /// </summary>
        public static void ExportSceneToAssetBundles()
        {
            //NOTE(Brian): This should make the logs cleaner
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
            
            EnsureEnvironment();
            ExportSceneToAssetBundles(System.Environment.GetCommandLineArgs());
        }

        /// <summary>
        /// Start the scene conversion process with the given commandLineArgs.
        /// </summary>
        /// <param name="commandLineArgs">An array with the command line arguments.</param>
        /// <exception cref="ArgumentException">When an invalid argument is passed</exception>
        public static void ExportSceneToAssetBundles(string[] commandLineArgs)
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

                if (Utils.ParseOption(commandLineArgs, Config.CLI_BUILD_SCENE_SYNTAX, 1, out string[] sceneCid))
                {
                    if (sceneCid == null || string.IsNullOrEmpty(sceneCid[0]))
                    {
                        throw new ArgumentException("Invalid sceneCid argument! Please use -sceneCid <id> to establish the desired id to process.");
                    }

                    DumpScene(sceneCid[0], settings);
                    return;
                }

                if (Utils.ParseOption(commandLineArgs, Config.CLI_BUILD_PARCELS_RANGE_SYNTAX, 4, out string[] xywh))
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

                    DumpArea(new Vector2Int(x, y), new Vector2Int(w, h), settings);
                    return;
                }

                throw new ArgumentException("Invalid arguments! You must pass -parcelsXYWH or -sceneCid for dump to work!");
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        /// <summary>
        /// This will start the asset bundle conversion for a given scene list, given a scene cids list.
        /// </summary>
        /// <param name="sceneCidsList">The cid list for the scenes to gather from the catalyst's content server</param>
        /// <param name="settings">Any conversion settings object, if its null, a new one will be created</param>
        /// <returns>A state context object useful for tracking the conversion progress</returns>
        public static Core.State ConvertScenesToAssetBundles(List<string> sceneCidsList, ClientSettings settings = null)
        {
            if (sceneCidsList == null || sceneCidsList.Count == 0)
            {
                log.Error("Scene list is null or count == 0! Maybe this sector lacks scenes or content requests failed?");
                return new Core.State() { lastErrorCode = Core.ErrorCodes.SCENE_LIST_NULL };
            }

            log.Info($"Building {sceneCidsList.Count} scenes...");

            List<ContentServerUtils.MappingPair> rawContents = new List<ContentServerUtils.MappingPair>();

            EnsureEnvironment();

            if (settings == null)
                settings = new ClientSettings();

            foreach (var sceneCid in sceneCidsList)
            {
                ContentServerUtils.MappingsAPIData parcelInfoApiData = ABConverter.Utils.GetSceneMappingsData(env.webRequest, settings.tld, sceneCid);
                rawContents.AddRange(parcelInfoApiData.data[0].content.contents);
            }

            var core = new ABConverter.Core(env, settings);
            core.Convert(rawContents.ToArray());

            return core.state;
        }

        /// <summary>
        /// This will start the asset bundle conversion for a single asset
        /// </summary>
        /// <param name="assetHash">The asset's content server hash</param>
        /// <param name="assetFilename">The asset's content server file name</param>
        /// <param name="sceneCid">The asset scene ID</param>
        /// <param name="settings">Any conversion settings object, if its null, a new one will be created</param>
        /// <returns>A state context object useful for tracking the conversion progress</returns>
        public static Core.State ConvertAssetToAssetBundle(string assetHash, string assetFilename, string sceneCid, ClientSettings settings = null)
        {
            if (string.IsNullOrEmpty(assetHash))
            {
                log.Error("Missing asset hash for ConvertAssetToAssetBundle()");
                return new Core.State() { lastErrorCode = Core.ErrorCodes.UNDEFINED };
            }

            if (string.IsNullOrEmpty(assetFilename))
            {
                log.Error("Missing asset file name for ConvertAssetToAssetBundle()");
                return new Core.State() { lastErrorCode = Core.ErrorCodes.UNDEFINED };
            }

            log.Info($"Building {assetHash} asset...");

            EnsureEnvironment();

            if (settings == null)
            {
                settings = new ClientSettings()
                {
                    skipAlreadyBuiltBundles = false
                };
            }

            var core = new ABConverter.Core(env, settings);

            List<ContentServerUtils.MappingPair> rawContents = new List<ContentServerUtils.MappingPair>();
            rawContents.Add(new ContentServerUtils.MappingPair
            {
                file = assetFilename,
                hash = assetHash
            });

            // If the asset is a GLTF we add the dependencies to the rawContents to be downloaded
            if (assetFilename.ToLower().EndsWith(".glb") || assetFilename.ToLower().EndsWith(".gltf"))
            {
                core.GetAssetDependenciesMappingPairs(assetHash, assetFilename, sceneCid, ref rawContents);
            }

            core.Convert(rawContents.ToArray(), null);

            return core.state;
        }

        /// <summary>
        /// Dump a world area given coords and size. The zone is a rectangle with a center pivot.
        /// </summary>
        /// <param name="coords">Coords as parcel coordinates</param>
        /// <param name="size">Size as radius</param>
        /// <param name="settings">Conversion settings</param>
        /// <returns>A state context object useful for tracking the conversion progress</returns>
        public static Core.State DumpArea(Vector2Int coords, Vector2Int size, ClientSettings settings = null)
        {
            EnsureEnvironment();

            if (settings == null)
                settings = new ClientSettings();

            HashSet<string> sceneCids = ABConverter.Utils.GetSceneCids(env.webRequest, settings.tld, coords, size);
            List<string> sceneCidsList = sceneCids.ToList();
            return ConvertScenesToAssetBundles(sceneCidsList, settings);
        }

        /// <summary>
        /// Dump a world area given a parcel coords array.
        /// </summary>
        /// <param name="coords">A list with the parcels coordinates wanted to be converted</param>
        /// <param name="settings">Conversion settings</param>
        /// <returns>A state context object useful for tracking the conversion progress</returns>
        public static Core.State DumpArea(List<Vector2Int> coords, ClientSettings settings = null)
        {
            EnsureEnvironment();

            if (settings == null)
                settings = new ClientSettings();

            HashSet<string> sceneCids = Utils.GetScenesCids(env.webRequest, settings.tld, coords);

            List<string> sceneCidsList = sceneCids.ToList();
            return ConvertScenesToAssetBundles(sceneCidsList, settings);
        }

        /// <summary>
        /// Dump a single world scene given a scene cid.
        /// </summary>
        /// <param name="cid">The scene cid in the multi-hash format (i.e. Qm...etc)</param>
        /// <param name="settings">Conversion settings</param>
        /// <returns>A state context object useful for tracking the conversion progress</returns>
        public static Core.State DumpScene(string cid, ClientSettings settings = null)
        {
            EnsureEnvironment();

            if (settings == null)
                settings = new ClientSettings();

            return ConvertScenesToAssetBundles(new List<string> { cid }, settings);
        }

        /// <summary>
        /// Dump a single asset (and its dependencies) given an asset hash and scene Cid
        /// </summary>
        /// <param name="assetHash">The asset's content server hash</param>
        /// <param name="assetFilename">The asset's content server file name</param>
        /// <param name="sceneCid">The asset scene ID</param>
        /// <param name="settings">Conversion settings</param>
        /// <returns>A state context object useful for tracking the conversion progress</returns>
        public static Core.State DumpAsset(string assetHash, string assetFilename, string sceneCid, ClientSettings settings = null)
        {
            EnsureEnvironment();

            if (settings == null)
                settings = new ClientSettings();

            return ConvertAssetToAssetBundle(assetHash, assetFilename, sceneCid, settings);
        }
    }
}