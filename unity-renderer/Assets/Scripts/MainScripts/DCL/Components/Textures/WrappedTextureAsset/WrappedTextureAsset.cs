using System;
using System.Collections;
using DCL.Controllers.Gif;
using UnityEngine;

namespace DCL
{
    public static class WrappedTextureAssetFactory
    {
        static public IEnumerator Create(string contentType, byte[] bytes, Action<IWrappedTextureAsset> OnSuccess)
        {
            if (contentType == "image/gif")
            {
                var gif = new DCLGif();
                yield return gif.Load(bytes, () =>
                {
                    gif.Play();
                    OnSuccess?.Invoke(new WrappedGif(gif));
                });
            }
            else
            {
                var texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                OnSuccess?.Invoke(new WrappedImage(texture));
            }
        }
    }

    public interface IWrappedTextureAsset : IDisposable
    {
        Texture2D texture { get; }
        int width { get; }
        int height { get; }
    }

    public class WrappedImage : IWrappedTextureAsset
    {
        public Texture2D texture { get; private set; }
        public int width => texture.width;
        public int height => texture.height;

        public void Dispose()
        {
            if (texture != null)
            {
                UnityEngine.Object.Destroy(texture);
                texture = null;
            }
        }

        public WrappedImage(Texture2D t)
        {
            texture = t;
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
    }
}