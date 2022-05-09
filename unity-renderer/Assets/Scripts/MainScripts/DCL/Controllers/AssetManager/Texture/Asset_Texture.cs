using System.Collections;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Cache;
using Object = UnityEngine.Object;

namespace DCL
{
    public class Asset_Texture : Asset, ITexture
    {
        public Texture2D texture { get; set; }
        public Asset_Texture dependencyAsset; // to store the default tex asset and release it accordingly
        public event System.Action OnCleanup;

        private static readonly IThrottlingCounter throttlingCounter = new SmartThrottlingCounter(2 / 1000.0);

        public IEnumerable ConfigureTexture(TextureWrapMode textureWrapMode, FilterMode textureFilterMode, bool makeNoLongerReadable = true)
        {
            if (texture == null)
                yield break;

            texture.wrapMode = textureWrapMode;
            texture.filterMode = textureFilterMode;

#if !UNITY_STANDALONE
            yield return TextureHelpers.ThrottledCompress(texture, makeNoLongerReadable, texture2D => texture = texture2D, e => throw e);
#else
            texture.Apply(textureFilterMode != FilterMode.Point, makeNoLongerReadable);
#endif
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