using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class Asset_AB : Asset
    {
        const string METADATA_FILENAME = "metadata.json";
        const string METRICS_FILENAME = "metrics.json";

        private AssetBundle assetBundle;
        private Dictionary<string, List<Object>> assetsByExtension;
        public AssetBundleMetrics metrics { get; private set; } = new AssetBundleMetrics { meshesEstimatedSize = 0, animationsEstimatedSize = 0 };

        public Asset_AB()
        {
            assetsByExtension = new Dictionary<string, List<Object>>();
        }

        public override object Clone() => (Asset_AB) MemberwiseClone();
        public string GetName() => assetBundle.name;

        public void CancelShow() => Cleanup();
        public bool IsValid() => assetBundle != null;

        public override void Cleanup()
        {
            assetsByExtension = null;

            if (assetBundle)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }
        }

        public List<T> GetAssetsByExtensions<T>(params string[] extensions)
            where T : Object
        {
            var goList = new List<T>();

            for (int i1 = 0; i1 < extensions.Length; i1++)
            {
                string ext = extensions[i1];
                List<Object> assets;

                if (assetsByExtension.ContainsKey(ext))
                {
                    assets = assetsByExtension[ext];
                    int glbCount = assets.Count;

                    for (int i = 0; i < glbCount; i++)
                    {
                        Object go = assets[i];

                        if (go is T)
                            goList.Add((T) go);
                    }
                }
            }

            return goList;
        }
        public void AddAssetByExtension(string ext, Object loadedAsset)
        {
            if (assetsByExtension == null)
            {
                Debug.LogWarning($"Trying to add asset of type {ext} to unloaded AB");

                return;
            }
            
            if (!assetsByExtension.ContainsKey(ext))
            {
                assetsByExtension.Add(ext, new List<Object>());
            }
            
            assetsByExtension[ext].Add(loadedAsset);
        }
        public void SetAssetBundle(AssetBundle ab)
        {
            assetBundle = ab;
        }

        public void LoadMetrics()
        {
            var metricsFile = assetBundle.LoadAsset<TextAsset>(METRICS_FILENAME);
            if (metricsFile != null)
                metrics = JsonUtility.FromJson<AssetBundleMetrics>(metricsFile.text);
        }

        public TextAsset GetMetadata()
        {
            return assetBundle.LoadAsset<TextAsset>(METADATA_FILENAME);
        }
        public AssetBundleRequest LoadAllAssetsAsync()
        {
            return assetBundle.LoadAllAssetsAsync();
        }
    }
}