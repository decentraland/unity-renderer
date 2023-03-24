namespace ThreeDISevenZeroR.UnityGifDecoder.Model
{
    public struct GifImageDescriptor
    {
        public int left;
        public int top;

        public bool isInterlaced;
        public bool hasLocalColorTable;
        public int localColorTableSize;

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
