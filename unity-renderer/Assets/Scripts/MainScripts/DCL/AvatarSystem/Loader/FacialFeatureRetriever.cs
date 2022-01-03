using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;

namespace AvatarSystem
{
    public class FacialFeatureRetriever : IFacialFeatureRetriever
    {
        AssetPromise_Texture mainTexturePromise = null;
        AssetPromise_Texture maskTexturePromise = null;

        public async UniTask<(Texture main, Texture mask)> Retrieve( string mainTextureUrl, string maskTextureUrl, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return (null, null);
            }

            AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);
            AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);

            Texture2D mainTexture = null;
            Texture2D maskTexture = null;

            mainTexturePromise = new AssetPromise_Texture(mainTextureUrl);
            mainTexturePromise.OnSuccessEvent += (x) => mainTexture = x.texture;
            mainTexturePromise.OnFailEvent += (x, exception) =>
            {
                //TODO Handle exception
                mainTexture = null;
            };

            AssetPromiseKeeper_Texture.i.Keep(mainTexturePromise);

            if (!string.IsNullOrEmpty(maskTextureUrl))
            {
                maskTexturePromise = new AssetPromise_Texture(maskTextureUrl);
                maskTexturePromise.OnSuccessEvent += (x) => maskTexture = x.texture;
                maskTexturePromise.OnFailEvent += (x, exception) =>
                {
                    //TODO Handle exception
                    maskTexture = null;
                };

                AssetPromiseKeeper_Texture.i.Keep(maskTexturePromise);
            }

            await mainTexturePromise.WithCancellation(ct);
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return (null, null);
            }

            if (maskTexturePromise != null)
            {
                await maskTexturePromise.WithCancellation(ct);
                if (ct.IsCancellationRequested)
                {
                    Dispose();
                    return (null, null);
                }
            }

            return (mainTexture, maskTexture);
        }

        public void Dispose()
        {
            AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);
            AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);
        }
    }
}