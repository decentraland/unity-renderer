using System;
using UnityEngine;

namespace DCL.Helpers
{
    internal class TextureLoader : ITextureLoader
    {
        private const bool VERBOSE = false;
        
        private AssetPromise_Texture currentPromise;
        public event Action<Texture2D> OnSuccess;
        public event Action<Exception> OnFail;

        public Texture2D GetTexture()
        {
            return currentPromise.asset.texture;
        }

        public void Load(string uri)
        {
            if ( currentPromise != null )
                AssetPromiseKeeper_Texture.i.Forget(currentPromise);

            currentPromise = new AssetPromise_Texture(uri);

            currentPromise.OnSuccessEvent += (x) =>
            {
                OnSuccess?.Invoke(x.texture);
            };

            currentPromise.OnFailEvent += (x, e) =>
            {
                OnFail?.Invoke(e);
                if (VERBOSE)
                    Debug.Log($"Texture loading failed! {uri} {e.Message}");
            };

            AssetPromiseKeeper_Texture.i.Keep(currentPromise);
        }

        public void Unload()
        {
            if ( currentPromise != null )
                AssetPromiseKeeper_Texture.i.Forget(currentPromise);
        }
    }
}
