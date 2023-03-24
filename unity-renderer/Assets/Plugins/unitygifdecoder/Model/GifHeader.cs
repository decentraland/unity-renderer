namespace ThreeDISevenZeroR.UnityGifDecoder.Model
{
    public struct GifHeader
    {
        public GifVersion version;
        public bool hasGlobalColorTable;
        public int globalColorTableSize;
        public int transparentColorIndex;
        public bool sortColors;
        public int colorResolution;
        public int pixelAspectRatio;

        private int width;
        private int height;
        public int Width
        {
            get
            {
                if (width % 4 != 0)
                {
                    return ((width / 4) + 1) * 4;
                }
                return width;
            }
            set => width = value;
        }

        public int Height
        {
            get
            {
                if (height % 4 != 0)
                {
                    return ((height / 4) + 1) * 4;
                }
                return height;
            }
            set => height = value;
        }
    }
}
