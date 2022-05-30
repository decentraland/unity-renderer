using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class Asset_AB : Asset
    {
        const string METADATA_FILENAME = "metadata.json";
        
        private AssetBundle ownerAssetBundle;
        private string assetBundleAssetName;

        public Dictionary<string, List<Object>> assetsByExtension;

        public Asset_AB()
        {
            assetsByExtension = new Dictionary<string, List<Object>>();
        }
        public string GetName() => assetBundleAssetName;

        public override object Clone() => (Asset_AB) MemberwiseClone();

        public void CancelShow()
        {
            Cleanup();
        }

        public void SetAssetBundle(AssetBundle ab)
        {
            ownerAssetBundle = ab;
            assetBundleAssetName = ab.name;
        }

        public bool IsValid()
        {
            return ownerAssetBundle != null;
        }

        public TextAsset GetMetadata()
        {
            if (ownerAssetBundle == null)
            {
                throw new Exception($"Cant load asset bundle because its already destroyed. Name: {assetBundleAssetName}");
            }
            
            return ownerAssetBundle.LoadAsset<TextAsset>(METADATA_FILENAME);
        }

        public AssetBundleRequest LoadAllAssetsAsync()
        {
            if (ownerAssetBundle == null)
            {
                throw new Exception($"Cant load asset bundle because its already destroyed. Name: {assetBundleAssetName}");
            }
            
            return ownerAssetBundle.LoadAllAssetsAsync();
        }

        public override void Cleanup()
        {
            assetsByExtension = null;

            if (ownerAssetBundle)
            {
                ownerAssetBundle.Unload(true);
                ownerAssetBundle = null;
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
    }
}