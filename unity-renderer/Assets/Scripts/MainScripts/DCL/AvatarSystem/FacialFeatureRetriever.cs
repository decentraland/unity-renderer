using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;

namespace AvatarSystem
{
    public class FacialFeatureRetriever : IFacialFeatureRetriever
    {
        AssetPromise_Texture mainTexturePromise = null;
        AssetPromise_Texture maskTexturePromise = null;

        public async UniTask<(Texture main, Texture mask)> Retrieve(string mainTextureUrl, string maskTextureUrl)
        {
            AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);
            AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);

            Texture2D mainTexture = null;
            Texture2D maskTexture = null;

            mainTexturePromise = new AssetPromise_Texture(mainTextureUrl);
            mainTexturePromise.OnSuccessEvent += (x) => mainTexture = x.texture;
            mainTexturePromise.OnFailEvent += (x) => mainTexture = null;

            AssetPromiseKeeper_Texture.i.Keep(mainTexturePromise);

            if (!string.IsNullOrEmpty(maskTextureUrl))
            {
                maskTexturePromise = new AssetPromise_Texture(maskTextureUrl);
                maskTexturePromise.OnSuccessEvent += (x) => maskTexture = x.texture;
                maskTexturePromise.OnFailEvent += (x) => maskTexture = null;

                AssetPromiseKeeper_Texture.i.Keep(maskTexturePromise);
            }

            await mainTexturePromise;
            if (maskTexturePromise != null)
                await maskTexturePromise;

            return (mainTexture, maskTexture);

        }

        public void Dispose()
        {
            AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);
            AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);
        }
    }
}