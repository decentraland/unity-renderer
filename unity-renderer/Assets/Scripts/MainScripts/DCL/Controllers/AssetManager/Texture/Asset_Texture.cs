using UnityEngine;
using UnityGLTF.Cache;
using Object = UnityEngine.Object;

namespace DCL
{
    public class Asset_Texture : Asset, ITexture
    {
        private const string TEXTURE_COMPRESSION_FLAG_NAME = "tex_compression";
        
        public Texture2D texture { get; set; }
        public float resizingFactor = 1;
        public Asset_Texture dependencyAsset; // to store the default tex asset and release it accordingly
        public event System.Action OnCleanup;

#if UNITY_STANDALONE
        bool compressTexture = false
#else
        bool compressTexture = DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(TEXTURE_COMPRESSION_FLAG_NAME);
#endif
        
        public void ConfigureTexture(TextureWrapMode textureWrapMode, FilterMode textureFilterMode, bool makeNoLongerReadable = true)
        {
            if (texture == null)
                return;
            
            Debug.Log("Asset_Texture.ConfigureTexture() - compress? " + compressTexture);

            texture.wrapMode = textureWrapMode;
            texture.filterMode = textureFilterMode;
            
            if(compressTexture)
                texture.Compress(false);
            
            texture.Apply(textureFilterMode != FilterMode.Point, makeNoLongerReadable);
        }

        public override void Cleanup()
        {
            OnCleanup?.Invoke();
            PersistentAssetCache.RemoveImage(texture);
            Object.Destroy(texture);
        }

        public void Dispose() { Cleanup(); }

        public int width => texture.width;
        public int height => texture.height;
    }
}