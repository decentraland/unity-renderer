using DCL;
using DCL.Controllers.Gif;

namespace NFTShape_Internal
{
    public static class NFTAssetFactory
    {
        public static INFTAsset CreateAsset(ITexture asset, NFTShapeConfig shapeConfig)
        {
            if (asset == null)
            {
                return null;
            }

            if (asset is Asset_Gif gif)
            {
                return new NFTGifAsset(gif, shapeConfig.hqGifResolution);
            }

            return new NFTImageAsset(asset, shapeConfig.hqImgResolution);
        }
    }
}