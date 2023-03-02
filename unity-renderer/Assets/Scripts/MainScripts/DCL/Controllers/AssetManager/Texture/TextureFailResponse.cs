using System;

namespace MainScripts.DCL.Controllers.AssetManager.Texture
{
    public readonly struct TextureFailResponse
    {
        public readonly Exception Exception;

        public TextureFailResponse(Exception exception)
        {
            Exception = exception;
        }
    }
}