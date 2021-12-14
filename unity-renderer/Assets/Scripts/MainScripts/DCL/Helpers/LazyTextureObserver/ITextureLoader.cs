using System;
using UnityEngine;

namespace DCL.Helpers
{
    /// <summary>
    /// Small texture loader facade. Mainly used by LazyTextureObserver to make testing easier.
    /// 
    /// If we find a better way to mock this aspect of LazyTextureObserver functionality we should remove this.
    /// </summary>
    public interface ITextureLoader
    {
        void Load(string uri);
        void Unload();

        event Action<Texture2D> OnSuccess;
        event Action<Exception> OnFail;
        Texture2D GetTexture();
    }
}