using DCL;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MainScripts.DCL.Controllers.AssetManager.AssetBundles.SceneAB
{
    public class Asset_SceneAB : Asset
    {
        [CanBeNull] private SceneAbDto sceneAb;
        private string contentUrl;

        public void Setup(SceneAbDto dto, string contentUrl)
        {
            this.contentUrl = contentUrl;
            sceneAb = dto;
        }

        public bool IsSceneConverted() =>
            sceneAb != null;

        public string GetBaseUrl() => $"{contentUrl}{sceneAb.version}/";

        public HashSet<string> GetConvertedFiles() => sceneAb.files.ToHashSet();

        public override void Cleanup()
        {

        }
    }
}
