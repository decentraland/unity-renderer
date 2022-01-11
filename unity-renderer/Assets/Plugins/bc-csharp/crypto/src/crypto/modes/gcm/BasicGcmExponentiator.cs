using System;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm
{
    public class BasicGcmExponentiator
        : IGcmExponentiator
    {
        private ulong[] x;

        public void Init(byte[] x)
        {
            this.x = GcmUtilities.AsUlongs(x);
        }

        public void ExponentiateX(long pow, byte[] output)
        {
            // Initial value is little-endian 1
            ulong[] y = GcmUtilities.OneAsUlongs();

            if (pow > 0)
            {
                ulong[] powX = Arrays.Clone(x);
                do
                {
                    if ((pow & 1L) != 0)
                    {
                        GcmUtilities.Multiply(y, powX);
                    }
                    GcmUtilities.Square(powX, powX);
                    pow >>= 1;
                }
                while (pow > 0);
            }

            GcmUtilities.AsBytes(y, output);
        }
    }
}
