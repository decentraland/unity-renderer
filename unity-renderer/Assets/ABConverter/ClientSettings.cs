using System.IO;

namespace DCL.ABConverter
{
    public class ClientSettings
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
            /// If set to true, the GLTF _Downloads folder and the Asset Bundles folder will be deleted at the beginning of the conversion
            /// </summary>
            public bool clearDirectoriesOnStart = true;

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

            public ClientSettings Clone() { return this.MemberwiseClone() as ClientSettings; }

            public ClientSettings(ContentServerUtils.ApiTLD tld = ContentServerUtils.ApiTLD.ORG)
            {
                this.tld = tld;
                this.baseUrl = ContentServerUtils.GetContentAPIUrlBase(tld);
            }
        }
}