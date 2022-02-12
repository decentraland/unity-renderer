using System;
using DCL;
using UnityEngine;

namespace NFTShape_Internal
{
    public static class NFTAssetFactory
    {
        public static NFTGifAsset CreateGifAsset(Asset_Gif gif)
        {
            return new NFTGifAsset(gif);
        }

        public static NFTImageAsset CreateImageAsset(Asset_Texture texture)
        {
            return new NFTImageAsset(texture);
        }
    }
}