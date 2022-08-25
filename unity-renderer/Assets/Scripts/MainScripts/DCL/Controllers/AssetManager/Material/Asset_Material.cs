using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class Asset_Material : Asset
    {
        public Material material;

        internal AssetPromise_Texture emissiveTexturePromise;
        internal AssetPromise_Texture alphaTexturetPromise;
        internal AssetPromise_Texture albedoTexturePromise;
        internal AssetPromise_Texture bumpTexturePormise;

        public override void Cleanup()
        {
            if (material != null)
            {
                Utils.SafeDestroy(material);
            }
            AssetPromiseKeeper_Texture.i.Forget(emissiveTexturePromise);
            AssetPromiseKeeper_Texture.i.Forget(alphaTexturetPromise);
            AssetPromiseKeeper_Texture.i.Forget(albedoTexturePromise);
            AssetPromiseKeeper_Texture.i.Forget(bumpTexturePormise);
        }
    }
}