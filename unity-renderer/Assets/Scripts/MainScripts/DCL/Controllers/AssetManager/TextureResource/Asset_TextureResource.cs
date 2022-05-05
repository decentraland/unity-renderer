using UnityEngine;
using UnityGLTF.Cache;

namespace DCL
{
    public class Asset_TextureResource : Asset
    {
        public Texture2D texture2D;
        
        public override void Cleanup()
        {
            PersistentAssetCache.RemoveImage(texture2D);
            Object.Destroy(texture2D);
        }
    }
}