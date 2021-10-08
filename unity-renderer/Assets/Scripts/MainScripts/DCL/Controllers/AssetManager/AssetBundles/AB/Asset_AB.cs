using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class Asset_AB : Asset
    {
        public AssetBundle ownerAssetBundle;
        public string assetBundleAssetName;

        public Dictionary<string, List<Object>> assetsByExtension = new Dictionary<string, List<Object>>();

        public Asset_AB()
        {
            assetsByExtension = new Dictionary<string, List<Object>>();
        }

        public override object Clone() => (Asset_AB) MemberwiseClone();

        public void CancelShow()
        {
            Cleanup();
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