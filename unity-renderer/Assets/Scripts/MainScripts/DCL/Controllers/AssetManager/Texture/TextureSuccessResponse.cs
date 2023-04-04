using UnityEngine;

namespace MainScripts.DCL.Controllers.AssetManager.Texture
{
    public readonly struct TextureSuccessResponse
    {
        public readonly Texture2D Texture;
        public readonly float ResizingFactor;

        public TextureSuccessResponse(Texture2D texture, float resizingFactor)
        {
            Texture = texture;
            ResizingFactor = resizingFactor;
        }
    }
}