namespace Altom.AltTester
{
    public class AltTextureScale
    {
        public class AltThreadData
        {
            public int start;
            public int end;
            public AltThreadData(int s, int e)
            {
                start = s;
                end = e;
            }
        }

        private static UnityEngine.Color32[] texColors;
        private static UnityEngine.Color32[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static System.Threading.Mutex mutex;

        public static void Point(UnityEngine.Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static void Bilinear(UnityEngine.Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(UnityEngine.Texture2D tex, int newWidth, int newHeight, bool useBilinear)
        {
            texColors = tex.GetPixels32();
            newColors = new UnityEngine.Color32[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else
            {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;

            var cores = UnityEngine.Mathf.Min(UnityEngine.SystemInfo.processorCount, newHeight);
            cores = UnityEngine.Mathf.Max(1, cores);
            var slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new System.Threading.Mutex(false);
            }
            if (cores > 1)
            {
                int i = 0;
                AltThreadData threadData;
                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new AltThreadData(slice * i, slice * (i + 1));
                    System.Threading.ParameterizedThreadStart ts = useBilinear ? new System.Threading.ParameterizedThreadStart(BilinearScale) : new System.Threading.ParameterizedThreadStart(PointScale);
                    System.Threading.Thread thread = new System.Threading.Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new AltThreadData(slice * i, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
                while (finishCount < cores)
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            else
            {
                AltThreadData threadData = new AltThreadData(0, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

#if UNITY_2021_2_OR_NEWER
            tex.Reinitialize(newWidth, newHeight);
#else
            tex.Resize(newWidth, newHeight);
#endif
            tex.SetPixels32(newColors);
            tex.Apply();

            texColors = null;
            newColors = null;
        }

        public static void BilinearScale(System.Object obj)
        {
            AltThreadData threadData = (AltThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)UnityEngine.Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++)
                {
                    int xFloor = (int)UnityEngine.Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                        ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                        y * ratioY - yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        public static void PointScale(System.Object obj)
        {
            AltThreadData threadData = (AltThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static UnityEngine.Color32 ColorLerpUnclamped(UnityEngine.Color32 c1, UnityEngine.Color32 c2, float value)
        {
            byte r = (byte)UnityEngine.Mathf.Lerp(c1.r, c2.r, value);
            byte g = (byte)UnityEngine.Mathf.Lerp(c1.g, c2.g, value);
            byte b = (byte)UnityEngine.Mathf.Lerp(c1.b, c2.b, value);
            byte a = (byte)UnityEngine.Mathf.Lerp(c1.a, c2.a, value);
            return new UnityEngine.Color32(r, g, b, a);
        }
    }
}