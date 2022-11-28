namespace Altom.AltDriver
{
    public struct AltTextureInformation
    {
        public byte[] imageData;
        public AltVector2 scaleDifference;
        public AltVector3 textureSize;

        public AltTextureInformation(byte[] imageData, AltVector2 scaleDifference, AltVector3 textureSize)
        {
            this.imageData = imageData;
            this.scaleDifference = scaleDifference;
            this.textureSize = textureSize;
        }
    }
}