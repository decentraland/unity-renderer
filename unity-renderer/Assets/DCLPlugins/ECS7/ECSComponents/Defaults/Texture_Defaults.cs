using DCL.Controllers;
using Decentraland.Common;

namespace DCL.ECSComponents
{
    public static class Texture_Defaults
    {
        public static string GetTextureUrl(this TextureUnion self, IParcelScene scene)
        {
            switch (self.TexCase)
            {
                case TextureUnion.TexOneofCase.AvatarTexture:
                    return self.AvatarTexture.GetTextureUrl();
                case TextureUnion.TexOneofCase.VideoTexture:
                    return $"{scene.sceneData.sceneNumber}{self.VideoTexture.VideoPlayerEntity}";
                case TextureUnion.TexOneofCase.Texture:
                default:
                    return self.Texture.GetTextureUrl(scene);
            }
        }

        public static long GetVideoTextureId(this TextureUnion self)
        {
            switch (self.TexCase)
            {
                case TextureUnion.TexOneofCase.VideoTexture:
                    return self.VideoTexture.VideoPlayerEntity;
                default:
                    return 0;
            }
        }

        public static bool IsVideoTexture(this TextureUnion self) =>
            self.TexCase == TextureUnion.TexOneofCase.VideoTexture;

        public static UnityEngine.TextureWrapMode GetWrapMode(this TextureUnion self)
        {
            switch (self.TexCase)
            {
                case TextureUnion.TexOneofCase.AvatarTexture:
                    return self.AvatarTexture.GetWrapMode();
                case TextureUnion.TexOneofCase.VideoTexture:
                    return self.VideoTexture.GetWrapMode();
                case TextureUnion.TexOneofCase.Texture:
                default:
                    return self.Texture.GetWrapMode();
            }
        }

        public static UnityEngine.FilterMode GetFilterMode(this TextureUnion self)
        {
            switch (self.TexCase)
            {
                case TextureUnion.TexOneofCase.AvatarTexture:
                    return self.AvatarTexture.GetFilterMode();
                case TextureUnion.TexOneofCase.VideoTexture:
                    return self.VideoTexture.GetFilterMode();
                case TextureUnion.TexOneofCase.Texture:
                default:
                    return self.Texture.GetFilterMode();
            }
        }

        public static string GetTextureUrl(this Texture self, IParcelScene scene)
        {
            if (string.IsNullOrEmpty(self.Src))
                return self.Src;

            if (scene.contentProvider.TryGetContentsUrl(self.Src, out string textureUrl))
                return textureUrl;

            return string.Empty;
        }

        public static string GetTextureUrl(this AvatarTexture self)
        {
            return KernelConfig.i.Get().avatarTextureAPIBaseUrl + self.UserId;
        }

        public static UnityEngine.TextureWrapMode GetWrapMode(this Texture self)
        {
            return (UnityEngine.TextureWrapMode)(self.HasWrapMode ? self.WrapMode : TextureWrapMode.TwmClamp);
        }

        public static UnityEngine.TextureWrapMode GetWrapMode(this AvatarTexture self)
        {
            return (UnityEngine.TextureWrapMode)(self.HasWrapMode ? self.WrapMode : TextureWrapMode.TwmClamp);
        }

        public static UnityEngine.TextureWrapMode GetWrapMode(this VideoTexture self)
        {
            return (UnityEngine.TextureWrapMode)(self.HasWrapMode ? self.WrapMode : TextureWrapMode.TwmClamp);
        }

        public static UnityEngine.FilterMode GetFilterMode(this Texture self)
        {
            return (UnityEngine.FilterMode)(self.HasFilterMode ? self.FilterMode : TextureFilterMode.TfmBilinear);
        }

        public static UnityEngine.FilterMode GetFilterMode(this AvatarTexture self)
        {
            return (UnityEngine.FilterMode)(self.HasFilterMode ? self.FilterMode : TextureFilterMode.TfmBilinear);
        }

        public static UnityEngine.FilterMode GetFilterMode(this VideoTexture self)
        {
            return (UnityEngine.FilterMode)(self.HasFilterMode ? self.FilterMode : TextureFilterMode.TfmBilinear);
        }
    }
}
