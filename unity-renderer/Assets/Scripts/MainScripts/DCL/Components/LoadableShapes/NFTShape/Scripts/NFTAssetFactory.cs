using System;
using DCL;
using UnityEngine;

namespace NFTShape_Internal
{
    public static class NFTAssetFactory
    {
        public static INFTAsset CreateAsset(ITexture asset, NFTShapeConfig shapeConfig,
            Action<Texture2D> textureUpdateCallback, GifPlayer gifPlayer)
        {
            if (asset == null)
            {
                return null;
            }

            if (asset is Asset_Gif gif)
            {
                return new NFTGifAsset(gif, shapeConfig.hqGifResolution, gifPlayer);
            }

            return new NFTImageAsset(asset, shapeConfig.hqImgResolution, textureUpdateCallback);
        }
    }
}