using Cysharp.Threading.Tasks;
using DCL;
using MainScripts.DCL.Controllers.AssetManager;
using System.Collections.Generic;
using UnityEngine;

namespace MainScripts.DCL.AssetsEmbedment.Editor
{
    public static class EditorTexturesDownloader
    {
        private static readonly AssetTextureWebLoader WEB_LOADER = new ();

        public static async UniTask DownloadTexture2DAsync(string baseUrl, string hash, Dictionary<string, Texture2D> results)
        {
            if (results.ContainsKey(hash))
                return;

            var url = $"{baseUrl}{hash}";

            var response = await WEB_LOADER.GetTextureAsync(url);

            if (response)
            {
                results.Add(hash, response);
                return;
            }

            throw new AssetNotFoundException(AssetSource.WEB, hash);
        }
    }
}
