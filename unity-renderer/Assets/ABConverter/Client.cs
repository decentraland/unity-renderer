using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DCL.ABConverter
{
    public static class Client
    {
        public class Settings
        {
            /// <summary>
            /// if set to true, when conversion finishes, the working folder containing all downloaded assets will be deleted
            /// </summary>
            public bool deleteDownloadPathAfterFinished = false;

            /// <summary>
            /// If set to true, Asset Bundles will not be built at all, and only the asset dump will be performed.
            /// </summary>
            public bool dumpOnly = false;

            /// <summary>
            /// If set to true, Asset Bundle output folder will be checked, and existing bundles in that folder will be excluded from
            /// the conversion process.
            /// </summary>
            public bool skipAlreadyBuiltBundles = false;

            /// <summary>
            /// Log verbosity.
            /// </summary>
            public bool verbose = false;

            /// <summary>
            /// Output folder for asset bundles, by default, they will be stored in Assets/../AssetBundles.
            /// </summary>
            public string finalAssetBundlePath = Config.ASSET_BUNDLES_PATH_ROOT + Path.DirectorySeparatorChar;

            /// <summary>
            /// Target top level domain. This will define the content server url used for the conversion (org, zone, etc).
            /// </summary>
            public ContentServerUtils.ApiTLD tld = ContentServerUtils.ApiTLD.ORG;

            /// <summary>
            /// Raw baseUrl using for asset dumping.
            /// </summary>
            public string baseUrl;

            public Settings Clone()
            {
                return this.MemberwiseClone() as Settings;
            }

            public Settings(ContentServerUtils.ApiTLD tld = ContentServerUtils.ApiTLD.ORG)
            {
                this.tld = tld;
                this.baseUrl = ContentServerUtils.GetContentAPIUrlBase(tld);
            }
        }

        private static Logger log = new Logger("ABConverter.Client");

        public static Environment env;

        public static Environment EnsureEnvironment()
        {
            if (env == null)
                env = Environment.CreateWithDefaultImplementations();

            return env;
        }

        /// <summary>
        /// Batch-mode entry point
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
        /// Start the conversion process with the given commandLineArgs.
        /// </summary>
        /// <param name="commandLineArgs">An array with the command line arguments.</param>
        /// <exception cref="ArgumentException">When an invalid argument is passed</exception>
        public static void ExportSceneToAssetBundles(string[] commandLineArgs)
        {
            Settings settings = new Settings();
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
        public static Core.State ConvertScenesToAssetBundles(List<string> sceneCidsList, Settings settings = null)
        {
            if (sceneCidsList == null || sceneCidsList.Count == 0)
            {
                log.Error("Scene list is null or count == 0! Maybe this sector lacks scenes or content requests failed?");
                return new Core.State() {lastErrorCode = Core.ErrorCodes.SCENE_LIST_NULL};
            }

            log.Info($"Building {sceneCidsList.Count} scenes...");

            List<ContentServerUtils.MappingPair> rawContents = new List<ContentServerUtils.MappingPair>();

            EnsureEnvironment();

            if (settings == null)
                settings = new Settings();

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
        /// Dump a world area given coords and size. The zone is a rectangle with a center pivot.
        /// </summary>
        /// <param name="coords">Coords as parcel coordinates</param>
        /// <param name="size">Size as radius</param>
        /// <param name="settings">Conversion settings</param>
        /// <returns>A state context object useful for tracking the conversion progress</returns>
        public static Core.State DumpArea(Vector2Int coords, Vector2Int size, Settings settings = null)
        {
            EnsureEnvironment();

            if (settings == null)
                settings = new Settings();

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
        public static Core.State DumpArea(List<Vector2Int> coords, Settings settings = null)
        {
            EnsureEnvironment();

            if (settings == null)
                settings = new Settings();

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
        public static Core.State DumpScene(string cid, Settings settings = null)
        {
            EnsureEnvironment();

            if (settings == null)
                settings = new Settings();

            return ConvertScenesToAssetBundles(new List<string> {cid}, settings);
        }
    }
}