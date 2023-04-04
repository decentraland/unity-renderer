using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using ThreeDISevenZeroR.UnityGifDecoder;
using ThreeDISevenZeroR.UnityGifDecoder.Model;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DCL
{
    public class GifWebRequestException : Exception
    {
        public GifWebRequestException(string message) : base(message) { }
    }

    public class GifDecoderProcessor : IGifProcessor
    {
        private struct RawImage
        {
            public readonly Color32[] colors;
            public readonly float delay;
            public RawImage(GifImage gifImage)
            {
                //(Kinerius): We have to clone the colors as the pointer gets modifier as we read the gif
                colors = gifImage.colors.Clone() as Color32[];
                delay = gifImage.SafeDelaySeconds;
            }
        }

        private bool isRunningInternal;
        private readonly ThrottlingCounter throttlingCounter = new ThrottlingCounter();

        private readonly string url;
        private readonly Stream stream;
        private readonly IWebRequestController webRequestController;
        private GifFrameData[] gifFrameData;

        public GifDecoderProcessor(string url, IWebRequestController webRequestController)
        {
            this.url = url;
            this.webRequestController = webRequestController;
        }

        public GifDecoderProcessor(Stream stream) { this.stream = stream; }

        public void DisposeGif()
        {
            gifFrameData = null;
            webRequestController.Dispose();
            stream?.Dispose();
        }

        public async UniTask Load(Action<GifFrameData[]> loadSuccsess, Action<Exception> fail, CancellationToken token)
        {
            try
            {
                await StartDecoding(loadSuccsess, fail, token);
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                fail(e);
            }
        }

        private async UniTask StartDecoding(Action<GifFrameData[]> loadSuccsess, Action<Exception> fail, CancellationToken token)
        {
            try
            {
                // (Kinerius): I noticed that overall the loading of multiple gifs at the same time is faster when only
                //          one is loading, this also avoids the "burst" of gifs loading at the same time, overall
                //          improving the smoothness and the experience, this could be further improved by prioritizing
                //          the processing of gifs whether im close or looking at them like GLFTs

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                GifStream gifStream;

                if (stream != null)
                {
                    gifStream = new GifStream(stream);
                }
                else
                {
                    gifStream = new GifStream( await DownloadGifAndReadStream(token));
                }

                var images = await ReadStream(gifStream, token);

                token.ThrowIfCancellationRequested();

                await TaskUtils.RunThrottledCoroutine(ProcessGifData(images, gifStream.Header.adjustedWidth, gifStream.Header.adjustedHeight), fail, throttlingCounter.EvaluateTimeBudget)
                               .AttachExternalCancellation(token);

                loadSuccsess(gifFrameData);

                stopwatch.Stop();
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                throw;
            }
        }

        private IEnumerator ProcessGifData(List<RawImage> rawImages, int width, int height)
        {
            gifFrameData = new GifFrameData[rawImages.Count];
            SkipFrameIfDepletedTimeBudget skipFrameIfDepletedTimeBudget = new SkipFrameIfDepletedTimeBudget();

            for (var i = 0; i < rawImages.Count; i++)
            {
                var frame = new Texture2D(
                    width,
                    height,
                    TextureFormat.ARGB32, false);

                frame.SetPixels32(rawImages[i].colors);
                frame.Compress(false);
                frame.Apply();

                gifFrameData[i] = new GifFrameData()
                {
                    texture = frame,
                    delay = rawImages[i].delay
                };

                yield return skipFrameIfDepletedTimeBudget;
            }
        }

        private async UniTask<byte[]> DownloadGifAndReadStream(CancellationToken token)
        {
            var operation = webRequestController.Get(url, timeout: 15, disposeOnCompleted: false);
            await operation.WithCancellation(token);

            if (!operation.isSucceeded)
            {
                throw new GifWebRequestException(url);
            }

            return operation.webRequest.downloadHandler.data;
        }

        private static UniTask<List<RawImage>> ReadStream(GifStream gifStream, CancellationToken token)
        {
            return TaskUtils.Run( () =>
            {
                var rawImages = new List<RawImage>();

                while (gifStream.HasMoreData)
                {
                    if (gifStream.CurrentToken == GifStream.Token.Image)
                    {
                        GifImage gifImage = gifStream.ReadImage();
                        rawImages.Add(new RawImage(gifImage));
                    }
                    else
                    {
                        gifStream.SkipToken();
                    }
                }

                return rawImages;
            }, token);
        }
    }
}
