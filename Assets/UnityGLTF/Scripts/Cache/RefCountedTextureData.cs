using System;
using UnityEngine;

namespace UnityGLTF.Cache
{
    public class RefCountedTextureData : RefCountedBase
    {
        public Texture2D Texture;
        public RefCountedTextureData(Texture2D texture)
        {
            Texture = texture;
        }

        protected override void OnDestroyCachedData()
        {
            UnityEngine.Object.Destroy(Texture);
        }
    }
}
