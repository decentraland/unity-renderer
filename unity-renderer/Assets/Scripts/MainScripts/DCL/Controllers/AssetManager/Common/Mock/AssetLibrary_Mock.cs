using System;
using System.Collections.Generic;

namespace DCL
{
    public class AssetLibrary_Mock : AssetLibrary<Asset_Mock>
    {
        public class RefCountedMockAsset
        {
            public Asset_Mock asset;
            public int referenceCount;
        }

        public Dictionary<object, RefCountedMockAsset> masterAssets = new Dictionary<object, RefCountedMockAsset>();

        public override bool Add(Asset_Mock asset)
        {
            if (!masterAssets.ContainsKey(asset.id))
                masterAssets.Add(asset.id, new RefCountedMockAsset() { referenceCount = 0, asset = asset });

            return true;
        }

        public override bool Contains(object id)
        {
            if (masterAssets == null || id == null)
                return false;

            return masterAssets.ContainsKey(id);
        }

        public override bool Contains(Asset_Mock asset)
        {
            if (masterAssets == null || asset == null)
                return false;

            return masterAssets.ContainsKey(asset.id);
        }

        public override Asset_Mock Get(object id)
        {
            if (Contains(id))
            {
                masterAssets[id].referenceCount++;
                return masterAssets[id].asset.Clone() as Asset_Mock;
            }

            return null;
        }

        public override void Release(Asset_Mock asset)
        {
            if (Contains(asset.id))
            {
                masterAssets[asset.id].referenceCount--;
                if (masterAssets[asset.id].referenceCount <= 0)
                {
                    masterAssets.Remove(asset.id);
                }
            }
        }

        public override void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}
