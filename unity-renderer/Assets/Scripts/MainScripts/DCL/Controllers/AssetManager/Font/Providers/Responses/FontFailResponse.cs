using System;

namespace MainScripts.DCL.Controllers.AssetManager.Font
{
    public readonly struct FontFailResponse
    {
        public readonly Exception Exception;

        public FontFailResponse(Exception exception)
        {
            Exception = exception;
        }
    }
}
