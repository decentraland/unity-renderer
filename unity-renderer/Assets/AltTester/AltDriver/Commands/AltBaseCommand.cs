using System;
using Altom.AltDriver.Logging;

namespace Altom.AltDriver.Commands
{
    public class AltBaseCommand
    {
        readonly NLog.Logger logger = DriverLogManager.Instance.GetCurrentClassLogger();

        protected IDriverCommunication CommHandler;

        public AltBaseCommand(IDriverCommunication commHandler)
        {
            this.CommHandler = commHandler;
        }

        protected void ValidateResponse(string expected, string received)
        {
            if (!expected.Equals(received, StringComparison.InvariantCulture))
            {
                throw new AltInvalidServerResponse(expected, received);
            }
        }

        protected static byte[] DecompressScreenshot(byte[] screenshotCompressed)
        {
            using (var memoryStreamInput = new System.IO.MemoryStream(screenshotCompressed))
            using (var memoryStreamOutput = new System.IO.MemoryStream())
            {
                using (var gs = new System.IO.Compression.GZipStream(memoryStreamInput, System.IO.Compression.CompressionMode.Decompress))
                {
                    copyTo(gs, memoryStreamOutput);
                }

                return memoryStreamOutput.ToArray();
            }
        }

        private static void copyTo(System.IO.Stream src, System.IO.Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}