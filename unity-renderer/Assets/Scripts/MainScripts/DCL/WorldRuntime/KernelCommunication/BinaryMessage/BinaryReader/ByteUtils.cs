namespace KernelCommunication
{
    public static class ByteUtils
    {
        public static unsafe int PointerToInt32(byte* ptr)
        {
            return (int)*ptr << 24 | (int)ptr[1] << 16 | (int)ptr[2] << 8 | (int)ptr[3];
        }
        
        public static unsafe long PointerToInt64(byte* ptr)
        {
            int num = (int)*ptr << 24 | (int)ptr[1] << 16 | (int)ptr[2] << 8 | (int)ptr[3];
            return (long)((uint)((int)ptr[4] << 24 | (int)ptr[5] << 16 | (int)ptr[6] << 8) | (uint)ptr[7]) | (long)num << 32;
        }
    }
}