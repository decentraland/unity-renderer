using System;
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

        public async UniTask<(Texture main, Texture mask)> Retrieve(WearableItem facialFeature, string bodyshapeId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                (string mainTextureUrl, string maskTextureUrl) = AvatarSystemUtils.GetFacialFeatureTexturesUrls(bodyshapeId, facialFeature);

                AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);
                AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);

                Texture2D mainTexture = null;
                Texture2D maskTexture = null;
                Exception mainTextureException = null;
                mainTexturePromise = new AssetPromise_Texture(mainTextureUrl);
                mainTexturePromise.OnSuccessEvent += (x) => mainTexture = x.texture;
                mainTexturePromise.OnFailEvent += (x, e) =>
                {
                    mainTextureException = e;
                    mainTexture = null;
                };

                AssetPromiseKeeper_Texture.i.Keep(mainTexturePromise);

                Exception maskTextureException = null;
                if (!string.IsNullOrEmpty(maskTextureUrl))
                {
                    maskTexturePromise = new AssetPromise_Texture(maskTextureUrl);
                    maskTexturePromise.OnSuccessEvent += (x) => maskTexture = x.texture;
                    maskTexturePromise.OnFailEvent += (x, e) =>
                    {
                        maskTextureException = e;
                        maskTexture = null;
                    };

                    AssetPromiseKeeper_Texture.i.Keep(maskTexturePromise);
                }

                // AttachExternalCancellation is needed, otherwise the cancellation takes a frame to effect
                await mainTexturePromise.ToUniTask(cancellationToken: ct).AttachExternalCancellation(ct);
                if (mainTextureException != null)
                    throw mainTextureException;

                if (mainTexture == null)
                    throw new Exception($"Couldn't fetch main texture for {facialFeature.id} at {mainTextureUrl}");

                if (maskTexturePromise != null)
                {
                    await maskTexturePromise.ToUniTask(cancellationToken: ct).AttachExternalCancellation(ct);

                    if (maskTextureException != null)
                        throw maskTextureException;

                    if (maskTexture == null)
                        throw new Exception($"Couldn't fetch mask texture for {facialFeature.id} at {maskTextureUrl}");
                }

                return (mainTexture, maskTexture);
            }
            catch (OperationCanceledException)
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);
            AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);
        }
    }
}