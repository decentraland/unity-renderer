using DCL.Controllers;

namespace DCL.ECSComponents
{
    public static class Texture_Defaults
    {
        public static string GetTextureUrl(this ECSComponents.Texture self, IParcelScene scene)
        {
            switch (self.TextureCase)
            {
                case ECSComponents.Texture.TextureOneofCase.AvatarTexture:
                    return self.AvatarTexture.GetTextureUrl();
                case ECSComponents.Texture.TextureOneofCase.SrcTexture:
                default:
                    return self.SrcTexture.GetTextureUrl(scene);
            }
        }
        
        public static UnityEngine.TextureWrapMode GetWrapMode(this ECSComponents.Texture self)
        {
            switch (self.TextureCase)
            {
                case ECSComponents.Texture.TextureOneofCase.AvatarTexture:
                    return self.AvatarTexture.GetWrapMode();
                case ECSComponents.Texture.TextureOneofCase.SrcTexture:
                default:
                    return self.SrcTexture.GetWrapMode();
            }
        }
        
        public static UnityEngine.FilterMode GetFilterMode(this ECSComponents.Texture self)
        {
            switch (self.TextureCase)
            {
                case ECSComponents.Texture.TextureOneofCase.AvatarTexture:
                    return self.AvatarTexture.GetFilterMode();
                case ECSComponents.Texture.TextureOneofCase.SrcTexture:
                default:
                    return self.SrcTexture.GetFilterMode();
            }
        }
        
        public static string GetTextureUrl(this SRCTexture self, IParcelScene scene)
        {
            scene.contentProvider.TryGetContentsUrl(self.Src, out string textureUrl);

            return textureUrl;
        }
        
        public static string GetTextureUrl(this AvatarTexture self)
        {
            return KernelConfig.i.Get().avatarTextureAPIBaseUrl + self.UserId;
        }
        
        public static UnityEngine.TextureWrapMode GetWrapMode(this SRCTexture self)
        {
            return (UnityEngine.TextureWrapMode)(self.HasWrapMode ? self.WrapMode : TextureWrapMode.TwmClamp);
        }
        
        public static UnityEngine.TextureWrapMode GetWrapMode(this AvatarTexture self)
        {
            return (UnityEngine.TextureWrapMode)(self.HasWrapMode ? self.WrapMode : TextureWrapMode.TwmClamp);
        }

        public static UnityEngine.FilterMode GetFilterMode(this SRCTexture self)
        {
            return (UnityEngine.FilterMode)(self.HasFilterMode ? self.FilterMode : TextureFilterMode.TfmBilinear);
        }
        
        public static UnityEngine.FilterMode GetFilterMode(this AvatarTexture self)
        {
            return (UnityEngine.FilterMode)(self.HasFilterMode ? self.FilterMode : TextureFilterMode.TfmBilinear);
        }
    }
}