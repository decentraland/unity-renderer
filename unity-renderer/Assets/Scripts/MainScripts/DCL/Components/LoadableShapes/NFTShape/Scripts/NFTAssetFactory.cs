using System;
using DCL;
using UnityEngine;

namespace NFTShape_Internal
{
    public static class NFTAssetFactory
    {
        public static NFTGifAsset CreateGifAsset(Asset_Gif gif, NFTShapeConfig shapeConfig, GifPlayer gifPlayer)
        {
            return new NFTGifAsset(gif, shapeConfig.hqGifResolution, gifPlayer);
        }

        public static NFTImageAsset CreateImageAsset(Asset_Texture texture, NFTShapeConfig shapeConfig,
            Action<Texture2D> textureUpdateCallback)
        {
            return new NFTImageAsset(texture, shapeConfig.hqImgResolution, textureUpdateCallback);
        }
    }
}