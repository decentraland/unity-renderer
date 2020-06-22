using System;
using System.Collections;
using DCL.Controllers.Gif;
using UnityEngine;

namespace DCL
{
    public enum WrappedTextureMaxSize { DONT_RESIZE = -1, _32 = 32, _64 = 64, _128 = 128, _256 = 256, _512 = 512, _1024 = 1024, _2048 = 2048 }
    public static class WrappedTextureAssetFactory
    {
        static public IEnumerator Create(string contentType, byte[] bytes, WrappedTextureMaxSize maxTextureSize, Action<IWrappedTextureAsset> OnSuccess)
        {
            if (contentType == "image/gif")
            {
                var gif = new DCLGif();
                yield return gif.Load(bytes, () =>
                {
                    var wrappedGif = new WrappedGif(gif);
                    wrappedGif.EnsureTextureMaxSize(maxTextureSize);
                    gif.Play();
                    OnSuccess?.Invoke(wrappedGif);
                });
            }
            else
            {
                var texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                var wrappedImage = new WrappedImage(texture);
                wrappedImage.EnsureTextureMaxSize(maxTextureSize);
                OnSuccess?.Invoke(wrappedImage);
            }
        }
    }

    public interface IWrappedTextureAsset : IDisposable
    {
        Texture2D texture { get; }
        int width { get; }
        int height { get; }
        void EnsureTextureMaxSize(WrappedTextureMaxSize maxTextureSize);
    }

    public class WrappedImage : IWrappedTextureAsset
    {
        public Texture2D texture { get { return texture2D; } }
        public int width => texture.width;
        public int height => texture.height;

        private Texture2D texture2D;

        public void Dispose()
        {
            if (texture2D != null)
            {
                UnityEngine.Object.Destroy(texture2D);
                texture2D = null;
            }
        }

        public WrappedImage(Texture2D t)
        {
            texture2D = t;
        }

        public void EnsureTextureMaxSize(WrappedTextureMaxSize maxTextureSize)
        {
            if (maxTextureSize != WrappedTextureMaxSize.DONT_RESIZE)
            {
                TextureHelpers.EnsureTexture2DMaxSize(ref texture2D, (int)maxTextureSize);
            }
        }
    }

    public class WrappedGif : IWrappedTextureAsset
    {
        DCLGif gif;
        Coroutine updateRoutine = null;

        public Texture2D texture => gif.texture;
        public int width => gif.textureWidth;
        public int height => gif.textureHeight;

        public void Dispose()
        {
            if (updateRoutine != null)
            {
                CoroutineStarter.Stop(updateRoutine);
            }
            if (gif != null)
            {
                gif.Dispose();
            }
        }

        public void SetUpdateTextureCallback(Action<Texture2D> callback)
        {
            gif.OnFrameTextureChanged += callback;

            if (updateRoutine != null)
            {
                CoroutineStarter.Stop(updateRoutine);
            }
            updateRoutine = CoroutineStarter.Start(gif.UpdateRoutine());
        }

        public WrappedGif(DCLGif gif)
        {
            this.gif = gif;
        }

        public void EnsureTextureMaxSize(WrappedTextureMaxSize maxTextureSize)
        {
            if (maxTextureSize != WrappedTextureMaxSize.DONT_RESIZE)
            {
                gif.SetMaxTextureSize((int)maxTextureSize);
            }
        }
    }
}