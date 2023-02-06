using DCL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MainScripts.DCL.Controllers.AssetManager.AssetBundles.SceneAB
{
    public class Asset_SceneAB : Asset
    {
        private string tld;
        private SceneAbDto sceneAb;

        public void Setup(SceneAbDto dto, string tld)
        {
            sceneAb = dto;
            this.tld = tld;
        }

        public bool IsSceneConverted() =>
            sceneAb != null;

        public string GetBaseUrl() =>
            $"{SceneAssetBundles.BASE_URL}{tld}{sceneAb.version}/";

        public HashSet<string> GetConvertedFiles() =>
            sceneAb.files.ToHashSet();

        public override void Cleanup()
        {

        }
    }
}
