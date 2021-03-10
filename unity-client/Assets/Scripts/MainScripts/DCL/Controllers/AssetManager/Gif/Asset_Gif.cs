using UnityEngine;

namespace DCL
{
    public class Asset_Gif : Asset, ITexture
    {
        public event System.Action OnCleanup;

        public Texture2D texture => frames?[0].texture;
        public int width => texture?.width ?? 0;
        public int height => texture?.height ?? 0;
        public GifFrameData[] frames { get; internal set; }

        internal GifProcessor processor;

        public void Dispose()
        {
            Cleanup();
        }

        public override void Cleanup()
        {
            OnCleanup?.Invoke();

            processor?.DisposeGif();

            if (frames is null)
            {
                return;
            }

            for (int i = 0; i < frames.Length; i++)
            {
                Object.Destroy(frames[i].texture);
            }

            frames = null;
        }
    }
}