using TMPro;

namespace MainScripts.DCL.Controllers.AssetManager.Font
{
    public readonly struct FontSuccessResponse
    {
        public readonly TMP_FontAsset Font;

        public FontSuccessResponse(TMP_FontAsset font)
        {
            Font = font;
        }
    }
}
