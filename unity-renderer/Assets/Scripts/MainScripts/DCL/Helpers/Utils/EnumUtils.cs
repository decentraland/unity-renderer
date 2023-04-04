using System;
using System.Runtime.CompilerServices;

namespace MainScripts.DCL.Helpers.Utils
{
    /// <summary>
    /// Contains non-allocating generic versions of enum utility functions
    /// </summary>
    public static class EnumUtils
    {
        public static unsafe bool HasFlag<T>(T x, T y) where T : unmanaged, Enum
        {
            switch (sizeof(T))
            {
                case sizeof(byte):
                    return (*(byte*) &x & *(byte*) &y) != 0;

                case sizeof(short):
                    return (*(short*) &x & *(short*) &y) != 0;

                case sizeof(int):
                    return (*(int*) &x & *(int*) &y) != 0;

                case sizeof(long):
                    return (*(long*) &x & *(long*) &y) != 0L;

                default:
                    return false;
            }
        }

        public static unsafe T Max<T>(T x, T y) where T : unmanaged, Enum
        {
            switch (sizeof(T))
            {
                case sizeof(byte):
                    var b = Math.Max(*(byte*) &x, *(byte*) &y);
                    return Unsafe.As<byte, T>(ref b);
                case sizeof(short):
                    var s = Math.Max(*(short*) &x, *(short*) &y);
                    return Unsafe.As<short, T>(ref s);
                case sizeof(int):
                    var i = Math.Max(*(int*) &x, *(int*) &y);
                    return Unsafe.As<int, T>(ref i);
                case sizeof(long):
                    var l = Math.Max(*(long*) &x, *(long*) &y);
                    return Unsafe.As<long, T>(ref l);
                default:
                    throw new NotImplementedException(sizeof(T).ToString());
            }
        }

        public static T[] Values<T>() =>
            (T[]) Enum.GetValues(typeof(T));
    }
}
