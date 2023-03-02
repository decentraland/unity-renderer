using Cysharp.Threading.Tasks;
using DCL.ABConverter;
using DCL.EditorEnvironment;
using MainScripts.DCL.AssetsEmbedment.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Progress = UnityEditor.Progress;

namespace MainScripts.DCL.AssetsEmbedment.Editor
{
    /// <summary>
    /// Fetches Asset Bundles and thumbnails and saves them to the streaming assets folder
    /// </summary>
    public static class WearablesEmbedment
    {
        private const string ASSET_BUNDLES_URL_ORG = "https://content-assets-as-bundle.decentraland.org/v23/";
        private const string TEXTURES_URL_ORG = "https://interconnected.online/content/contents/";

        private static bool inProgress;

        [MenuItem("Decentraland/Embed Base Wearables", priority = 2)]
        public static void EmbedBaseWearablesMenuItem()
        {
            EmbedBaseWearablesAsync().Forget();
        }

        public static async UniTask EmbedBaseWearablesAsync()
        {
            if (inProgress)
            {
                Progress.ShowDetails();
                return;
            }

            inProgress = true;

            try
            {
                using var env = EditorEnvironmentSetup.Execute();
                var wearables = WearablesCollectionClient.GetBaseWearableCollections();

                await UniTask.WhenAll(DownloadAndEmbedThumbnailsAsync(wearables), DownloadAndEmbedAssetBundlesAsync(wearables));

                AssetDatabase.Refresh();
            }
            finally
            {
                inProgress = false;
            }
        }

        private static async UniTask DownloadAndEmbedThumbnailsAsync(IReadOnlyList<WearableItem> wearables)
        {
            var thumbnails = wearables.Select(w => w.thumbnail).ToList();
            var thumbnailsCount = thumbnails.Count;
            var progressId = Progress.Start("Download Thumbnails");
            Progress.ShowDetails(false);

            try
            {
                var results = new Dictionary<string, Texture2D>();

                for (var i = 0; i < thumbnailsCount; i++)
                {
                    string hash = thumbnails[i].Split('/').Last();

                    Progress.Report(progressId, i / (float)thumbnailsCount, $"Downloading {hash}");
                    await EditorTexturesDownloader.DownloadTexture2DAsync(TEXTURES_URL_ORG, hash, results);
                }

                await TextureEmbedder.EmbedAsync(results, "");
            }
            finally { Progress.Remove(progressId); }
        }

        private static async UniTask DownloadAndEmbedAssetBundlesAsync(IReadOnlyList<WearableItem> wearables)
        {
            var mainFiles = wearables.SelectMany(w => w.data.representations
                                                       .Select(r => r.contents.FirstOrDefault(c => c.key == r.mainFile)))
                                     .Where(p => p != null)
                                     .ToList();

            var mainFilesCount = mainFiles.Count;

            var loadedData = new Dictionary<string, byte[]>();

            var progressId = Progress.Start("Download Asset Bundles");
            Progress.ShowDetails(false);

            try
            {
                for (var i = 0; i < mainFilesCount; i++)
                {
                    var currentProgress = i / (float)mainFilesCount;
                    var mainFile = mainFiles[i];
                    Progress.Report(progressId, currentProgress, $"Downloading {mainFile.key} {mainFile.hash}");

                    await EditorAssetBundlesDownloader.DownloadAssetBundleWithDependenciesAsync(ASSET_BUNDLES_URL_ORG, mainFile.hash, loadedData);

                    // don't need to handle 'success': such situation should not be: `GetAwaiter` throws `UnityWebRequestException` on error
                    await AssetBundleEmbedder.EmbedAsync(loadedData, EmbeddedWearablesPath.VALUE);
                    await UniTask.SwitchToMainThread();
                }
            }
            finally { Progress.Remove(progressId); }
        }
    }
}
