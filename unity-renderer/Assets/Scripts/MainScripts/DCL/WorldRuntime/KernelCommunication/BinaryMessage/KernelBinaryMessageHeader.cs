namespace KernelCommunication
{
    public readonly struct KernelBinaryMessageHeader
    {
        public readonly int length;
        public readonly int type;

        public KernelBinaryMessageHeader(int length, int type)
        {
            this.length = length;
            this.type = type;
        }
    }
}