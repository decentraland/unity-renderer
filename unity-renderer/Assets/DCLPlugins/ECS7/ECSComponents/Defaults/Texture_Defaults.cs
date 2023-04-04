using DCL.Controllers;
using Decentraland.Common;
using UnityEngine;
using Texture = Decentraland.Common.Texture;
using TextureWrapMode = UnityEngine.TextureWrapMode;

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

        public static TextureWrapMode GetWrapMode(this TextureUnion self)
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

        public static FilterMode GetFilterMode(this TextureUnion self)
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
            UtilsScene.TryGetMediaUrl(self.Src, scene.contentProvider,
                scene.sceneData.requiredPermissions, scene.sceneData.allowedMediaHostnames, out string url);

            return url;
        }

        public static string GetTextureUrl(this AvatarTexture self)
        {
            return KernelConfig.i.Get().avatarTextureAPIBaseUrl + self.UserId;
        }

        public static TextureWrapMode GetWrapMode(this Texture self)
        {
            return (TextureWrapMode)(self.HasWrapMode ? self.WrapMode : Decentraland.Common.TextureWrapMode.TwmClamp);
        }

        public static TextureWrapMode GetWrapMode(this AvatarTexture self)
        {
            return (TextureWrapMode)(self.HasWrapMode ? self.WrapMode : Decentraland.Common.TextureWrapMode.TwmClamp);
        }

        public static TextureWrapMode GetWrapMode(this VideoTexture self)
        {
            return (TextureWrapMode)(self.HasWrapMode ? self.WrapMode : Decentraland.Common.TextureWrapMode.TwmClamp);
        }

        public static FilterMode GetFilterMode(this Texture self)
        {
            return (FilterMode)(self.HasFilterMode ? self.FilterMode : TextureFilterMode.TfmBilinear);
        }

        public static FilterMode GetFilterMode(this AvatarTexture self)
        {
            return (FilterMode)(self.HasFilterMode ? self.FilterMode : TextureFilterMode.TfmBilinear);
        }

        public static FilterMode GetFilterMode(this VideoTexture self)
        {
            return (FilterMode)(self.HasFilterMode ? self.FilterMode : TextureFilterMode.TfmBilinear);
        }
    }
}
